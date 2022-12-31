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
using System.IO.Compression;
#if UNITY_IOS
using UnityEngine.XR.ARKit;
#endif


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

    public API api;


    public string worldMapId = "";

    public bool isWorldMapLoaded = false;

    public bool repeating = false;

    // create public unity object variables
    public GameObject AROrigin;
    public ARAnchorManager anchorManager;
    public GameObject ARCamera;
    public GameObject ChunkPrefab;

    public Dictionary<string, GameObject> chunks = new Dictionary<string, GameObject>();
    public Dictionary<string, ARAnchor> anchors = new Dictionary<string, ARAnchor>();

    public ARPlaneManager planeManager;

    public string centerChunkId = "";

    public FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

    /// <summary>
    /// Create an <c>ARWorldMap</c> and save it to disk.
    /// </summary>
    public void OnSaveButton()
    {
#if UNITY_IOS
        try
        {
            StartCoroutine(Save());
        }
        catch (Exception e)
        {
            Log(e.Message);
        }

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

    public byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        Log(string.Format("ARWorldMap has {0} bytes.", output.ToArray().Length));
        return output.ToArray();
    }

    public byte[] Decompress(byte[] data)
    {
        MemoryStream input = new MemoryStream(data);
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
        {
            dstream.CopyTo(output);
        }
        return output.ToArray();
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

        Log(worldMap.valid ? "World map is valid." : "World map is invalid.");

        Log("Serializing ARWorldMap to byte array...");

        var worldMapData = worldMap.Serialize(Allocator.Temp);

        SaveAndDisposeWorldMap(worldMapData.ToArray());
    }

    IEnumerator Load()
    {
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        if (sessionSubsystem == null)
        {
            Log("No session subsystem available. Could not load.");
            yield break;
        }

        retrieveFirestoreMap(sessionSubsystem);

    }

    async void retrieveFirestoreMap(ARKitSessionSubsystem sessionSubsystem)
    {
        CollectionReference mapsRef = db.Collection("maps");
        Query query = mapsRef.OrderBy("location").Limit(5);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
        var error = Double.MaxValue;
        var newId = "";
        foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
        {
            Console.WriteLine("Document {0} returned by query maps", documentSnapshot.Id);
            var tempAlt = documentSnapshot.GetValue<double>("altitude");
            var tempErr = Math.Abs(tempAlt - api.alt);

            var location = documentSnapshot.GetValue<GeoPoint>("location");

            var locError = Math.Abs(location.Latitude - api.lat) + Math.Abs(location.Longitude - api.lon);
            var locErrorInMeters = locError * 111000;
            var locErrorInFeet = locErrorInMeters * 3.28084;

            Log("Location error is " + locErrorInMeters);
            if (tempErr < error && locErrorInMeters < 10)
            {
                error = tempErr;
                newId = documentSnapshot.Id;
            }
        }

        if (newId == "")
        {
            Log("No nearby maps found");
            Log("Saving current map");

            await createChunks(1, 0);
            //hi


            if (!repeating)
            {
                InvokeRepeating("OnSaveButton", 15, 15);
                repeating = true;
            }

            OnSaveButton();
            return;
        }

        Console.WriteLine("Closest map is " + newId + " with error " + error);
        Log("Loading map " + newId);

        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        StorageReference storageRef = storage.RootReference;
        StorageReference mapsdbRef = storageRef.Child("maps");
        StorageReference mapRef = mapsdbRef.Child(newId + ".worldmap");
        var data = await mapRef.GetBytesAsync(1024 * 1024 * 10);

        // data = Decompress(data);

        // byte[] to native array
        var nativeData = new NativeArray<byte>(data.Length, Allocator.Temp);
        nativeData.CopyFrom(data);

        ARWorldMap worldMap;
        if (ARWorldMap.TryDeserialize(nativeData, out worldMap)) nativeData.Dispose();

        if (worldMap.valid)
        {
            Log("Deserialized successfully.");
            isWorldMapLoaded = true;
            worldMapId = newId;

        }
        else
        {
            Debug.LogError("Data is not a valid ARWorldMap.");
            Log("not valid world map");
            return;
            // yield break;
        }



        Log("Apply ARWorldMap to current session.");
        sessionSubsystem.ApplyWorldMap(worldMap);

        if (!repeating)
        {
            InvokeRepeating("OnSaveButton", 15, 15);
            repeating = true;
        }

    }


    async void SaveAndDisposeWorldMap(byte[] data)
    {

        var location = new GeoPoint(api.lat, api.lon);

        DocumentReference docRef = db.Collection("maps").Document("null");

        if (worldMapId.Length == 0)
        {


            // Add a new document with a generated ID
            docRef = db.Collection("maps").Document();
            Debug.Log("New document created");
            await docRef.SetAsync(new Dictionary<string, object>
        {
            { "location", location },
            { "altitude", api.alt },
            { "creator", "bryant" },
            { "updated", DateTime.Now },
            { "created", DateTime.Now },
            { "name", "test" },
            { "public", true },
            { "id", docRef.Id }
        });
            worldMapId = docRef.Id;
            Debug.Log("Added document with ID: " + docRef.Id);
        }
        else
        {
            docRef = db.Collection("maps").Document(worldMapId);
            await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "updated", DateTime.Now },
            { "altitude", api.alt },
            { "location", location }
        });
            Debug.Log("Updated document with ID: " + docRef.Id);

        }

        if (centerChunkId.Length > 0)
        {
            DocumentReference chunkRef = db.Collection("chunks").Document(centerChunkId);
            await chunkRef.UpdateAsync(new Dictionary<string, object>{
            { "worldMapId", worldMapId },
            { "updated", DateTime.Now },
            });
        }

        // Debug.Log("Uploading world map to storage");
        Debug.Log(worldMapId + ".worldmap");

        FirebaseStorage storage = FirebaseStorage.DefaultInstance;
        StorageReference storageRef = storage.RootReference;
        StorageReference mapsRef = storageRef.Child("maps");
        StorageReference mapRef = mapsRef.Child(worldMapId + ".worldmap");

        // // Debug.Log("Reference created");

        // // Upload the file to the path "maps/<worldMapId>.worldmap"

        // // use unity to compress byte array


        try
        {
            await mapRef.PutBytesAsync(data);
            Log("Upload complete?");
        }
        catch (System.Exception)
        {
            Log("Upload failed");
            throw;
        }

        isWorldMapLoaded = true;

        // data.Dispose();
        // worldMap.Dispose();
        // // Log(string.Format("ARWorldMap written to {0}", path));



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
        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
        sessionSubsystem.SetCoachingActive(true, ARCoachingOverlayTransition.Animated);

        // create a method on anchor change anchor manager
        anchorManager.anchorsChanged += AnchorManager_anchorsChanged;
    }

    private void AnchorManager_anchorsChanged(ARAnchorsChangedEventArgs obj)
    {
        // throw new NotImplementedException();
        Debug.Log("anchors changed");
        if (obj.added.Count > 0)
        {
            Debug.Log("added");
            foreach (var anchor in obj.added)
            {
                Log("ANCHOR NAME: " + anchor.name);
                var chunk = Instantiate(ChunkPrefab, anchor.transform.position, anchor.transform.rotation);
                chunk.GetComponent<Chunk>().ARCamera = ARCamera;
                chunk.GetComponent<Chunk>().m_LogText = m_LogText;
            }
        }
        if (obj.updated.Count > 0)
        {
            Debug.Log("updated");
            foreach (var anchor in obj.updated)
            {
                Debug.Log(anchor.name);

            }
        }
        if (obj.removed.Count > 0)
        {
            Debug.Log("removed");
            foreach (var anchor in obj.removed)
            {
                Debug.Log(anchor.name);
            }
        }
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

    private bool preventReload = false;

    void Update()
    {
        if (supported)
        {
            SetActive(errorText, false);
            SetActive(mappingStatusText, true);
        }
        else
        {
            SetActive(errorText, true);
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

        if (isWorldMapLoaded == false && preventReload == false && sessionSubsystem.worldMappingStatus == ARWorldMappingStatus.Mapped)
        {
            preventReload = true;
            sessionSubsystem.SetCoachingActive(false, ARCoachingOverlayTransition.Animated);
            OnLoadButton();
        }


#endif
    }

    async Task createChunks(float size, int num)
    {
        // get plane with lowest y value from plane manager with an upward facing normal
        var plane = planeManager.trackables;
        float minY = float.MaxValue;

        foreach (var p in planeManager.trackables)
        {
            if (p.transform.position.y < minY && p.transform.up.y > 0.5f)
            {
                minY = p.transform.position.y;
            }
        }

        // create chunks around arcamera

        for (int x = -num; x <= num; x++)
        {
            for (int z = -num; z <= num; z++)
            {
                var chunk = Instantiate(ChunkPrefab, ARCamera.transform.position + new Vector3(x * size, minY + 0.5f, z * size), Quaternion.identity);
                var anchor = chunk.AddComponent<ARAnchor>();

                // create firebase document
                DocumentReference docRef = db.Collection("chunks").Document();
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "x", chunk.transform.position.x },
                    { "y", chunk.transform.position.y },
                    { "z", chunk.transform.position.z },
                    { "size", size },
                    { "updated", DateTime.Now },
                    { "worldMapId", worldMapId }
                });

                anchor.name = docRef.Id;
                Log("ANCHOR NAME: " + anchor.name)
                chunk.name = docRef.Id;

                // save the id to the anchor so we can find it after reloading the world map
                
                

                chunks.Add(docRef.Id, chunk);
                anchors.Add(docRef.Id, anchor);

                if (x == 0 && z == 0)
                {
                    centerChunkId = docRef.Id;
                }

            }
        }

        foreach (ARAnchor anchor in anchors.Values)
        {
            while (anchor.pending)
            {
                await Task.Delay(100);
            }
        }

    }


    List<string> m_LogMessages;
}
