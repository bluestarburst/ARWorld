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

namespace UnityEngine.XR.ARFoundation.Samples
{

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
        public string tempWorldMapId = "";

        public bool isWorldMapLoaded = false;

        public bool repeating = false;

        // create public unity object variables
        public GameObject AROrigin;
        public ARAnchorManager anchorManager;
        public GameObject ARCamera;
        public GameObject ChunkPrefab;

        public Dictionary<string, GameObject> chunks = new Dictionary<string, GameObject>();
        public Dictionary<string, string> chunksPos = new Dictionary<string, string>();
        public Dictionary<string, ARAnchor> anchors = new Dictionary<string, ARAnchor>();

        public ARPlaneManager planeManager;

        public string centerChunkIdToSave = "";
        public string centerChunkId = "";
        public GameObject centerChunk;

        public FirebaseFirestore db = FirebaseFirestore.GetInstance(FirebaseApp.Create());

        public GameObject posterPrefab;
        public GameObject spotlightPrefab;

        public int chunksToLoad = 0;

        public BarycentricMeshData bay;

        public CollaborativeSession collab;

        public bool showChunks = false;

        public float last_lat = 0;
        public float last_lon = 0;
        public float last_alt = 0;

        // create arraylist of strings to store potential chunk ids
        public List<(double, string)> potentialChunkIds = new List<(double, string)>();
        public Dictionary<string, GeoPoint> potentialChunkIdsPos = new Dictionary<string, GeoPoint>();
        public int potentialChunkIdsCurrent = 0;
        bool shouldGetNextPotentialChunkId = false;

        int trys = 0;

        /// <summary>
        /// Create an <c>ARWorldMap</c> and save it to disk.
        /// </summary>
        public void OnSaveButton()
        {
#if UNITY_IOS
            try
            {
                HostNativeAPI.mapStatus("saving");
                StartCoroutine(Save());
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

#endif
        }

        public void OnSaveButtonDelay(int delay)
        {

#if UNITY_IOS
            try
            {
                HostNativeAPI.mapStatus("saving");
                Invoke("OnSaveButton", delay);
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

            if (chunks.Count < chunksToLoad && worldMapId.Length != 0)
            {
                Log("Not enough chunks loaded to save.");
                yield break;
            }

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

            var tempData = Compress(worldMapData.ToArray());

            // check size of worldmap is greater than 55 mb
            if (tempData.Length > 55000000)
            {
                Log("World map is too large to save.");
                yield break;
            }

            SaveAndDisposeWorldMap(tempData);
        }

        public bool firstLoadMap = false;
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

        async void WaitUntilMappedSave()
        {
            tempWorldMapId = worldMapId;
            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            while (sessionSubsystem.worldMappingStatus != ARWorldMappingStatus.Mapped)
            {
                await Task.Delay(1000);
            }
            sessionSubsystem.SetCoachingActive(false, ARCoachingOverlayTransition.Animated);
            await createChunks(1, 0);
            OnSaveButton();
        }



        async void retrieveFirestoreMap(ARKitSessionSubsystem sessionSubsystem)
        {
            CancelInvoke("getNextPotentialChunkId");
            CollectionReference mapsRef = db.Collection("maps");

            // create a boudning box around (api.lat,api.lon) current location within 50 meters
            GeoPoint southwest = new GeoPoint(api.lat - 0.00045, api.lon - 0.00045);
            GeoPoint northeast = new GeoPoint(api.lat + 0.00045, api.lon + 0.00045);

            Query query = mapsRef.WhereGreaterThanOrEqualTo("location", southwest).WhereLessThanOrEqualTo("location", northeast);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            var error = Double.MaxValue;
            var newId = "";
            var tempCenterChunkId = "";

            potentialChunkIds.Clear();
            potentialChunkIdsPos.Clear();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                Console.WriteLine("Document {0} returned by query maps", documentSnapshot.Id);
                var tempAlt = documentSnapshot.GetValue<double>("altitude");
                var tempErr = Math.Abs(tempAlt - api.alt);

                var location = documentSnapshot.GetValue<GeoPoint>("location");

                var locError = Math.Abs(location.Latitude - api.lat) + Math.Abs(location.Longitude - api.lon) + Math.Abs(tempAlt - api.alt);
                var locErrorInMeters = locError * 111000;
                var locErrorInFeet = locErrorInMeters * 3.28084;

                // put potential chunk ids into arraylist and sort by error in altitude and location error
                potentialChunkIds.Add((locErrorInMeters, documentSnapshot.Id));
                potentialChunkIdsPos.Add(documentSnapshot.Id, documentSnapshot.GetValue<GeoPoint>("location"));

                Log("Location error is " + locErrorInMeters);
                if (tempErr < error)
                {
                    error = tempErr;
                    newId = documentSnapshot.Id;
                    chunksToLoad = documentSnapshot.GetValue<int>("chunks");
                    tempCenterChunkId = documentSnapshot.GetValue<string>("centerChunkId");
                }
            }

            potentialChunkIds.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            if (newId == worldMapId && newId != "")
            {
                Log("Already loaded map " + newId);
                return;
            }

            centerChunk = null;
            centerChunkId = tempCenterChunkId;
            repeating = false;
            chunks.Clear();
            anchors.Clear();

            if (newId == "")
            {
                Log("No nearby maps found");
                Log("Saving current map");

                // OnSaveButton();
                WaitUntilMappedSave();
                return;
            }

            collab.serviceType = newId;
            collab.enabled = true;

            Console.WriteLine("Closest map is " + newId + " with error " + error);
            Log("Loading map " + newId);



            FirebaseStorage storage = FirebaseStorage.GetInstance(api.app);
            StorageReference storageRef = storage.RootReference;
            StorageReference mapsdbRef = storageRef.Child("maps");
            StorageReference mapRef = mapsdbRef.Child(newId + ".worldmap");
            var data = await mapRef.GetBytesAsync(1024 * 1024 * 55);

            data = Decompress(data);

            // byte[] to native array
            var nativeData = new NativeArray<byte>(data.Length, Allocator.Temp);
            nativeData.CopyFrom(data);

            ARWorldMap worldMap;
            if (ARWorldMap.TryDeserialize(nativeData, out worldMap)) nativeData.Dispose();

            if (worldMap.valid)
            {
                Log("Deserialized successfully.");
                tempWorldMapId = newId;
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
            Invoke("getNextPotentialChunkId", 10f);
            // OnSaveButton();

        }


        void getNextPotentialChunkId()
        {
            getNextPotentialChunkIdAsync();
        }
        async void getNextPotentialChunkIdAsync()
        {
            // if (!shouldGetNextPotentialChunkId)
            // {
            //     return;
            // }

            if (chunks.Count != 0)
            {
                return;
            }

            if (potentialChunkIds.Count == 0)
            {
                Log("No nearby maps found");
                Log("Saving current map");
                WaitUntilMappedSave();
                return;
            }

            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            sessionSubsystem.Reset();
            await WaitUntilWorldMapReady();

            if (potentialChunkIdsCurrent >= potentialChunkIds.Count)
            {
                potentialChunkIdsCurrent = 0;
                // OnSaveButton();
                if (shouldGetNextPotentialChunkId)
                {
                    trys += 1;
                }

                Log("No nearby maps found... RETRYING");

                if (trys > 1)
                {
                    Log("No nearby maps found");
                    Log("Saving current map");
                    CancelInvoke("getNextPotentialChunkId");
                    if (worldMapId == "")
                    {
                        sessionSubsystem.Reset();
                        WaitUntilMappedSave();
                        // OnSaveButton();
                    }

                    return;
                }

                // 
                CancelInvoke("getNextPotentialChunkId");
                OnLoadButton();
                return;
            }

            // double err = potentialChunkIds[potentialChunkIdsCurrent].Item1;

            var newId = potentialChunkIds[potentialChunkIdsCurrent].Item2;
            var error = potentialChunkIds[potentialChunkIdsCurrent].Item1;
            potentialChunkIdsCurrent++;

            centerChunk = null;
            centerChunkId = newId;

            collab.serviceType = newId;
            collab.enabled = true;

            Console.WriteLine("Closest map is " + newId + " with error " + error);
            Log("Loading new map " + newId);

            FirebaseStorage storage = FirebaseStorage.GetInstance(api.app);
            StorageReference storageRef = storage.RootReference;
            StorageReference mapsdbRef = storageRef.Child("maps");
            StorageReference mapRef = mapsdbRef.Child(newId + ".worldmap");
            var data = await mapRef.GetBytesAsync(1024 * 1024 * 55);

            data = Decompress(data);

            // byte[] to native array
            var nativeData = new NativeArray<byte>(data.Length, Allocator.Temp);
            nativeData.CopyFrom(data);

            ARWorldMap worldMap;
            if (ARWorldMap.TryDeserialize(nativeData, out worldMap)) nativeData.Dispose();

            if (worldMap.valid)
            {
                Log("Deserialized successfully.");
                tempWorldMapId = newId;
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

            await WaitUntilMapped();
            Invoke("getNextPotentialChunkId", 10f);
            // OnSaveButton();

        }

        // an async method that waits for the world map to be ready and returns a boolean
        async Task<bool> WaitUntilWorldMapReady()
        {
            while (!m_ARSession.subsystem.running)
            {
                await Task.Delay(100);
            }
            return true;
        }

        // an async method that waits for the world map to be mapped and returns a boolean
        async Task<bool> WaitUntilMapped()
        {
            var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
            while (sessionSubsystem.worldMappingStatus != ARWorldMappingStatus.Mapped)
            {
                await Task.Delay(100);
            }
            return true;
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
                    { "chunks", 1 },
                    { "id", docRef.Id },
                    { "centerChunkId", centerChunkId },
                });
                worldMapId = docRef.Id;
                collab.serviceType = docRef.Id;
                collab.enabled = true;
                Debug.Log("Added document with ID: " + docRef.Id);
            }
            else
            {
                docRef = db.Collection("maps").Document(worldMapId);
                await docRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "updated", DateTime.Now },
                    { "altitude", api.alt },
                    { "location", location },
                });

                Debug.Log("Updated document with ID: " + docRef.Id);

            }

            if (centerChunkIdToSave.Length > 0)
            {
                var chunk = chunks[centerChunkIdToSave];
                DocumentReference chunkRef = db.Collection("maps").Document(worldMapId).Collection("chunks").Document(centerChunkIdToSave);
                await chunkRef.SetAsync(new Dictionary<string, object>{
                    { "x", chunk.transform.position.x },
                    { "y", chunk.transform.position.y },
                    { "z", chunk.transform.position.z },
                    { "cx", 0 },
                    { "cy", 0},
                    { "size", 0 },
                    { "updated", DateTime.Now },
                    { "worldMapId", worldMapId }
                });
                centerChunkIdToSave = "";
            }

            // Debug.Log("Uploading world map to storage");
            Debug.Log(worldMapId + ".worldmap");

            FirebaseStorage storage = FirebaseStorage.GetInstance(FirebaseApp.DefaultInstance);
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
                HostNativeAPI.mapStatus("mapped");
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

            db = FirebaseFirestore.GetInstance(api.app);
        }

        private bool firstLoad = true;

        private void AnchorManager_anchorsChanged(ARAnchorsChangedEventArgs obj)
        {
            // throw new NotImplementedException();
            Debug.Log("anchors changed");
            if (obj.added.Count > 0)
            {

                Debug.Log("added");
                foreach (var anchor in obj.added)
                {

                    if (worldMapId.Length == 0 && tempWorldMapId.Length != 0)
                    {
                        worldMapId = tempWorldMapId;
                    }

                    if (firstLoad)
                    {
                        bay.loadedMap();
                        firstLoad = false;
                    }

                    if (!repeating)
                    {
                        CancelInvoke("getNextPotentialChunkId");
                        InvokeRepeating("OnSaveButton", 15, 15);
                        // OnSaveButton();
                        // InvokeRepeating("OnLoadButton", 10, 10);
                        repeating = true;
                        isWorldMapLoaded = true;
                    }

                    Log("ANCHOR NAME: " + anchor.name);
                    Log("TRACKABLE NAME: " + anchor.trackableId.ToString());
                    var chunk = Instantiate(ChunkPrefab, anchor.transform.position, anchor.transform.rotation);
                    Chunk chunkScript = chunk.GetComponent<Chunk>();
                    chunkScript.db = db;
                    chunkScript.ARCamera = ARCamera;
                    chunkScript.arWorldMapController = this;
                    chunkScript.id = anchor.trackableId.ToString();

                    chunks.Add(anchor.trackableId.ToString(), chunk);
                    Log("added chunk to chunks");
                    anchors.Add(anchor.trackableId.ToString(), anchor);



                    if (centerChunk == null && centerChunkId == anchor.trackableId.ToString())
                    {

#if UNITY_IOS
                        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
#else
                        XRSessionSubsystem sessionSubsystem = null;
#endif
                        Console.WriteLine("SETTING CENTER CHUNK");
                        centerChunk = chunk;
                        chunksPos["0-0"] = anchor.trackableId.ToString();
                        sessionSubsystem.SetCoachingActive(false, ARCoachingOverlayTransition.Animated);
                        centerChunkId = "";
                        HostNativeAPI.mapStatus("mapped");
                    }

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

        public void Log(string logMessage)
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


            // var locError = Math.Abs(last_lat - api.lat) + Math.Abs(last_lon - api.lon) + Math.Abs(last_alt - api.alt);
            // var locErrorInMeters = locError * 111000;
            // var locErrorInFeet = locErrorInMeters * 3.28084;
            // if distance between last lat, long, and altitude and current lat, long, and altitude is greater than 5 meters, load new map
            // if (anchors.Count == 0 && locErrorInMeters > 30)
            // {
            //     last_lat = api.lat;
            //     last_lon = api.lon;
            //     last_alt = api.alt;
            //     OnLoadButton();
            //     // load new map
            //     // if (isWorldMapLoaded == false)
            //     // {
            //     //     preventReload = false;
            //     //     LoadMap();
            //     // }
            // }

            if (!firstLoadMap && api.lat != 0.0f && api.lon != 0.0f && api.alt != 0.0f)
            {
                Log("first load map");
                Log("lat: " + api.lat + " lon: " + api.lon + " alt: " + api.alt);
                OnLoadButton();
                firstLoadMap = true;
            }


            if (chunks.Count > 0)
            {

                bool check = false;
                // find gameobjects with tag "chunk" and add them to chunks
                foreach (GameObject chunk in chunks.Values)
                {
                    // get x and z position of chunk
                    Vector3 chunkPos = new Vector3(chunk.transform.position.x, 0, chunk.transform.position.z);
                    // get camera x z position
                    Vector3 cameraPos = new Vector3(ARCamera.transform.position.x, 0, ARCamera.transform.position.z);

                    var dist = Vector3.Distance(chunkPos, cameraPos);
                    if (dist < 1)
                    {
                        if (planeManager.enabled == false)
                        {
                            Log("enabling plane manager");
                            planeManager.enabled = true;
                        }
                        check = true;
                        break;
                    }
                }
                if (!check && planeManager.enabled == true)
                {
                    Log("disabling plane manager");
                    planeManager.enabled = false;
                }
            }


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

            // if (isWorldMapLoaded == false && preventReload == false && sessionSubsystem.worldMappingStatus == ARWorldMappingStatus.Mapped)
            // {
            //     preventReload = true;
            //     // sessionSubsystem.SetCoachingActive(false, ARCoachingOverlayTransition.Animated);
            //     InvokeRepeating("OnLoadButton", 0, 60);
            // }

            // if (preventReload == false && api.lat != 0 && api.lon != 0)
            // {
            //     preventReload = true;

            // }

            if (sessionSubsystem.worldMappingStatus == ARWorldMappingStatus.Mapped)
            {
                shouldGetNextPotentialChunkId = true;
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

            // create chunks around ar session origin

            for (int x = -num; x <= num; x++)
            {
                for (int z = -num; z <= num; z++)
                {
                    // DocumentReference docRef = db.Collection("chunks").Document();
                    var chunk = Instantiate(ChunkPrefab, ARCamera.transform.position + new Vector3(x * size, minY + 0.5f, z * size), Quaternion.identity);
                    // chunk.name = docRef.Id;
                    var anchor = chunk.AddComponent<ARAnchor>();

                    // create firebase document

                    Log("ANCHOR NAME: " + anchor.name);
                    Log("TRACKABLE NAME: " + anchor.trackableId.ToString());

                    // save the id to the anchor so we can find it after reloading the world map

                    Chunk chunkScript = chunk.GetComponent<Chunk>();
                    chunkScript.db = db;
                    chunkScript.ARCamera = ARCamera;
                    chunkScript.arWorldMapController = this;
                    chunkScript.id = anchor.trackableId.ToString();



                    chunks.Add(anchor.trackableId.ToString(), chunk);
                    anchors.Add(anchor.trackableId.ToString(), anchor);

                    if (x == 0 && z == 0)
                    {

#if UNITY_IOS
                        var sessionSubsystem = (ARKitSessionSubsystem)m_ARSession.subsystem;
#else
        XRSessionSubsystem sessionSubsystem = null;
#endif

                        centerChunkIdToSave = anchor.trackableId.ToString();
                        centerChunkId = anchor.trackableId.ToString();
                        centerChunk = chunk;
                        sessionSubsystem.SetCoachingActive(false, ARCoachingOverlayTransition.Animated);
                        HostNativeAPI.mapStatus("mapped");
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

            chunksToLoad = chunks.Count;

        }


        List<string> m_LogMessages;
    }

}