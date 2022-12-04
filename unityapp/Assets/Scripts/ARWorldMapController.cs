using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using System;
using System.Threading.Tasks;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif

public static class NativeArrayExtension
{
    public static byte[] ToRawBytes<T>(this NativeArray<T> arr) where T : struct
    {
        var slice = new NativeSlice<T>(arr).SliceConvert<byte>();
        var bytes = new byte[slice.Length];
        slice.CopyTo(bytes);
        return bytes;
    }

    public static void CopyFromRawBytes<T>(this NativeArray<T> arr, byte[] bytes) where T : struct
    {
        var byteArr = new NativeArray<byte>(bytes, Allocator.Temp);
        var slice = new NativeSlice<byte>(byteArr).SliceConvert<T>();

        UnityEngine.Debug.Assert(arr.Length == slice.Length);
        slice.CopyTo(arr);
    }
}

/// <summary>
/// Demonstrates the saving and loading of an
/// <a href="https://developer.apple.com/documentation/arkit/arworldmap">ARWorldMap</a>
/// </summary>
/// <remarks>
/// ARWorldMaps are only supported by ARKit, so this API is in the
/// <c>UntyEngine.XR.ARKit</c> namespace.
/// </remarks>
public class ARWorldMapController : MonoBehaviour
{
    [
        Tooltip(
            "The ARSession component controlling the session from which to generate ARWorldMaps.")
    ]
    [SerializeField]
    ARSession m_ARSession;

    /// <summary>
    /// The ARSession component controlling the session from which to generate ARWorldMaps.
    /// </summary>
    public ARSession arSession
    {
        get
        {
            return m_ARSession;
        }
        set
        {
            m_ARSession = value;
        }
    }

    [Tooltip("UI Text component to display error messages")]
    [SerializeField]
    Text m_ErrorText;

    /// <summary>
    /// The UI Text component used to display error messages
    /// </summary>
    public Text errorText
    {
        get
        {
            return m_ErrorText;
        }
        set
        {
            m_ErrorText = value;
        }
    }

    [Tooltip("The UI Text element used to display log messages.")]
    [SerializeField]
    Text m_LogText;

    /// <summary>
    /// The UI Text element used to display log messages.
    /// </summary>
    public Text logText
    {
        get
        {
            return m_LogText;
        }
        set
        {
            m_LogText = value;
        }
    }

    [
        Tooltip(
            "The UI Text element used to display the current AR world mapping status.")
    ]
    [SerializeField]
    Text m_MappingStatusText;

    /// <summary>
    /// The UI Text element used to display the current AR world mapping status.
    /// </summary>
    public Text mappingStatusText
    {
        get
        {
            return m_MappingStatusText;
        }
        set
        {
            m_MappingStatusText = value;
        }
    }

    [
        Tooltip(
            "A UI button component which will generate an ARWorldMap and save it to disk.")
    ]
    [SerializeField]
    Button m_SaveButton;

    /// <summary>
    /// A UI button component which will generate an ARWorldMap and save it to disk.
    /// </summary>
    public Button saveButton
    {
        get
        {
            return m_SaveButton;
        }
        set
        {
            m_SaveButton = value;
        }
    }

    [
        Tooltip(
            "A UI button component which will load a previously saved ARWorldMap from disk and apply it to the current session.")
    ]
    [SerializeField]
    Button m_LoadButton;

    /// <summary>
    /// A UI button component which will load a previously saved ARWorldMap from disk and apply it to the current session.
    /// </summary>
    public Button loadButton
    {
        get
        {
            return m_LoadButton;
        }
        set
        {
            m_LoadButton = value;
        }
    }

    public API api;

    /// <summary>
    /// Create an <c>ARWorldMap</c> and save it to disk.
    /// </summary>
    public void OnSaveButton()
    {
#if UNITY_IOS
        StartCoroutine(Save());
#endif
    }

    /// <summary>
    /// Load an <c>ARWorldMap</c> from disk and apply it
    /// to the current session.
    /// </summary>
    public void OnLoadButton()
    {
#if UNITY_IOS
        StartCoroutine(Load());
#endif
    }

    /// <summary>
    /// Reset the <c>ARSession</c>, destroying any existing trackables,
    /// such as planes. Upon loading a saved <c>ARWorldMap</c>, saved
    /// trackables will be restored.
    /// </summary>
    public void OnResetButton()
    {
        m_ARSession.Reset();
    }


#if UNITY_IOS
    IEnumerator Save()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            Log("No session subsystem available. Could not save.");
            yield break;
        }

        var request = sessionSubsystem.GetARWorldMapAsync();

        while (!request.status.IsDone()) yield return null;

        if (request.status.IsError())
        {
            Log(string
                .Format("Session serialization failed with status {0}",
                request.status));
            yield break;
        }

        var worldMap = request.GetWorldMap();
        request.Dispose();

        SaveAndDisposeWorldMap(worldMap);
    }

    IEnumerator Load()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            Log("No session subsystem available. Could not load.");
            yield break;
        }

        var file = File.Open(path, FileMode.Open);
        if (file == null)
        {
            Log(string.Format("File {0} does not exist.", path));
            yield break;
        }

        Log(string.Format("Reading {0}...", path));

        int bytesPerFrame = 1024 * 10;
        var bytesRemaining = file.Length;
        var binaryReader = new BinaryReader(file);
        var allBytes = new List<byte>();
        while (bytesRemaining > 0)
        {
            var bytes = binaryReader.ReadBytes(bytesPerFrame);
            allBytes.AddRange(bytes);
            bytesRemaining -= bytesPerFrame;
            yield return null;
        }

        var data = new NativeArray<byte>(allBytes.Count, Allocator.Temp);
        data.CopyFrom(allBytes.ToArray());

        Log(string.Format("Deserializing to ARWorldMap...", path));
        ARWorldMap worldMap;
        if (ARWorldMap.TryDeserialize(data, out worldMap)) data.Dispose();

        if (worldMap.valid)
        {
            Log("Deserialized successfully.");
        }
        else
        {
            Debug.LogError("Data is not a valid ARWorldMap.");
            yield break;
        }

        Log("Apply ARWorldMap to current session.");
        sessionSubsystem.ApplyWorldMap(worldMap);
    }

    void SaveAndDisposeWorldMap(ARWorldMap worldMap)
    {
        Log("Serializing ARWorldMap to byte array...");
        var data = worldMap.Serialize(Allocator.Temp);
        Log(string.Format("ARWorldMap has {0} bytes.", data.Length));

        var file = File.Open(path, FileMode.Create);
        var writer = new BinaryWriter(file);
        writer.Write(data.ToArray());
        writer.Close();
        // create a firestore location
        var location = new GeoPoint(37.7853889, -122.4056973);






        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        Dictionary<string, object> docData = new Dictionary<string, object>
        {
            { "location", location },
            { "altitude", 0 },
            { "creator", "bryant" },
            { "timestamp", DateTime.Now }
        };

        DocumentReference addedDocRef = db.Collection("maps").Document();

        addedDocRef.SetAsync(docData).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error adding document: " + task.Exception);
            }
            else
            {
                Debug.Log("Document added with ID: " + addedDocRef.Id);

                FirebaseStorage storage = FirebaseStorage.DefaultInstance;
                StorageReference storageRef = storage.RootReference;
                StorageReference mapsRef = storageRef.Child("maps");
                Debug.Log("Uploading to: " + mapsRef.Path);
                Debug.Log("BLUESTARBURST: " + addedDocRef.Id + ".worldmap");
                StorageReference mapRef = mapsRef.Child(addedDocRef.Id + ".worldmap");

                mapRef.PutBytesAsync(data.ToArray())
                .ContinueWith((Task<StorageMetadata> task) =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            Debug.Log(task.Exception.ToString());
                            data.Dispose();
                            // Uh-oh, an error occurred!
                        }
                        else
                        {
                            // Metadata contains file metadata such as size, content-type, and md5hash.
                            StorageMetadata metadata = task.Result;
                            string md5Hash = metadata.Md5Hash;
                            Debug.Log("Finished uploading...");
                            Debug.Log("md5 hash = " + md5Hash);
                            addedDocRef.UpdateAsync("md5", md5Hash);
                            addedDocRef.UpdateAsync("url", mapRef.Path);
                            data.Dispose();
                        }
                    });
            }
        });



        // var dataArray = data.ToArray();
        // // split the dataArray into chunks of 1048487 bytes
        // int chunkSize = 1048487;
        // int numberOfChunks = (int)Math.Ceiling((double)dataArray.Length / chunkSize);
        // int start = 0;
        // int end = 0;

        // for (int i = 0; i < numberOfChunks; i++)
        // {
        //     start = i * chunkSize;
        //     end = (i + 1) * chunkSize;
        //     if (end > dataArray.Length)
        //     {
        //         end = dataArray.Length;
        //     }
        //     byte[] chunk = new byte[end - start];
        //     Array.Copy(dataArray, start, chunk, 0, end - start);
        //     // create a firestore document
        //     docData.Add("chunk" + i, chunk);

        //     // Dictionary<string, object> docData = new Dictionary<string, object>
        //     // {
        //     //     { "chunk", chunk },
        //     //     { "chunkSize", chunkSize },
        //     //     { "numberOfChunks", numberOfChunks },
        //     //     { "start", start },
        //     //     { "end", end },
        //     //     { "location", location }
        //     // };
        //     // // add the document to the collection
        //     // db.Collection("worldMaps").Document("worldMap").SetAsync(docData);
        // }



        // db.Collection("maps").Document().SetAsync(docData);

        
        worldMap.Dispose();
        Log(string.Format("ARWorldMap written to {0}", path));



    }
#endif

    string path
    {
        get
        {
            return Path
                .Combine(Application.persistentDataPath, "my_session.worldmap");
        }
    }

    bool supported
    {
        get
        {
#if UNITY_IOS
            return m_ARSession.subsystem is ARKitSessionSubsystem &&
            ARKitSessionSubsystem.worldMapSupported;
#else
            return false;
#endif
        }
    }

    void Awake()
    {
        m_LogMessages = new List<string>();
    }

    void Log(string logMessage)
    {
        m_LogMessages.Add(logMessage);
    }

    static void SetActive(Button button, bool active)
    {
        if (button != null) button.gameObject.SetActive(active);
    }

    static void SetActive(Text text, bool active)
    {
        if (text != null) text.gameObject.SetActive(active);
    }

    static void SetText(Text text, string value)
    {
        if (text != null) text.text = value;
    }

    void Update()
    {
        if (supported)
        {
            SetActive(errorText, false);
            SetActive(saveButton, true);
            SetActive(loadButton, true);
            SetActive(mappingStatusText, true);
        }
        else
        {
            SetActive(errorText, true);
            SetActive(saveButton, false);
            SetActive(loadButton, false);
            SetActive(mappingStatusText, false);
        }


#if UNITY_IOS
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
#else
        XRSessionSubsystem sessionSubsystem = null;
#endif


        if (sessionSubsystem == null) return;

        var numLogsToShow = 20;
        string msg = "";
        for (
            int i = Mathf.Max(0, m_LogMessages.Count - numLogsToShow);
            i < m_LogMessages.Count;
            ++i
        )
        {
            msg += m_LogMessages[i];
            msg += "\n";
        }
        SetText(logText, msg);


#if UNITY_IOS
        SetText(mappingStatusText,
        string
            .Format("Mapping Status: {0}",
            sessionSubsystem.worldMappingStatus));
#endif
    }

    List<string> m_LogMessages;
}
