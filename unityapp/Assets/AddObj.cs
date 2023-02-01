using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARSubsystems;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Firebase.Extensions;
using Siccity.GLTFUtility;
using System.IO;
using UnityEngine.Networking;

namespace UnityEngine.XR.ARFoundation.Samples
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class AddObj : PressInputBase
    {
        [SerializeField]
        [Tooltip("Instantiates this prefab on a plane at the touch location.")]
        GameObject m_PlacedPrefab;
        [SerializeField]
        [Tooltip("Instantiates this prefab on a plane at the touch location.")]
        GameObject m_PosterPrefab;

        /// <summary>
        /// The prefab to instantiate on touch.
        /// </summary>
        public GameObject placedPrefab
        {
            get { return m_PlacedPrefab; }
            set { m_PlacedPrefab = value; }
        }

        public GameObject posterPrefab
        {
            get { return m_PosterPrefab; }
            set { m_PosterPrefab = value; }
        }

        /// <summary>
        /// The object instantiated as a result of a successful raycast intersection with a plane.
        /// </summary>
        public GameObject spawnedObject { get; private set; }

        /// <summary>
        /// Invoked whenever an object is placed in on a plane.
        /// </summary>
        public static event Action onPlacedObject;

        ARRaycastManager m_RaycastManager;

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        public bool isAdding = false;

        public string type = "";
        public string user = "";
        public string id = "";

        public string change = "move";

        public ARWorldMapController arWorldMapController;
        public GameObject centerChunk = null;
        public GameObject currentChunk = null;

        public API api;

        private float ratioY = 1.0f;
        private float ratioX = 1.0f;

        public GameObject MoveComponentPrefab;
        public LayerMask selectedLayers;

        public CamFilter innerFilter;

        protected override void Awake()
        {
            base.Awake();
            m_RaycastManager = GetComponent<ARRaycastManager>();
            preFilePath = $"{Application.persistentDataPath}/Files";

            // if directory exists, delete all files inside it
            if (Directory.Exists(preFilePath))
            {
                string[] files = Directory.GetFiles(preFilePath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(preFilePath);
            }
        }

        // protected override void OnPress

        protected override void OnPress(Vector3 position)
        {
            // if (!isAdding)
            // {
            //     Add();
            //     return;
            // }

        }

        bool isPrev = false;

        public void CreateObjectInFrontOfCamera(string type, string user, string id)
        {
            if (!isAdding)
            {

                if (arWorldMapController.centerChunk == null)
                {
                    return;
                }

                centerChunk = arWorldMapController.centerChunk;

                this.type = type;
                this.user = user;
                this.id = id;

                if (type.Equals("posters") || type.Equals("stickers") || type.Equals("images"))
                {
                    HostNativeAPI.addingObj("adding");
                    AddPoster(type, user, id);
                }
                else if (type.Equals("objects"))
                {
                    HostNativeAPI.addingObj("adding");
                    AddObject(type, user, id);
                }
                else if (type.Equals("spotlights"))
                {
                    HostNativeAPI.addingObj("adding");
                    AddLight(type);
                }
                else if (type.Equals("filters"))
                {
                    isPrev = true;
                    innerFilter.transform.GetChild(0).gameObject.SetActive(true);
                    HostNativeAPI.addingObj("preview");
                    AddFilter(type);
                }
                return;
            }
        }

        private GameObject moveChild = null;
        async void AddPoster(string type, string user, string id)
        {
            isAdding = true;
            Debug.Log("Adding: users/" + user + "/posters/" + id + ".jpg");
            Debug.Log("type: " + type);


            // raycast directly in front of camera to place object 0.5 units above plane hit relative to plane normal. If there is no plane hit, place object 0.5 units above camera
            if (m_RaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;

                // the rotation of the object is relative to the plane normal

                Console.WriteLine("PLANE HIT");

                if (type.Equals("posters") || type.Equals("stickers") || type.Equals("images"))
                {
                    Console.WriteLine("POSTER");

                    // flip rotation because texture is upside down
                    Quaternion newRotation = Quaternion.Euler(hitPose.rotation.eulerAngles.x + 180, hitPose.rotation.eulerAngles.y, hitPose.rotation.eulerAngles.z);

                    // create poster prefab parallel to plane hit normal and 0.5 units above plane hit
                    spawnedObject = Instantiate(m_PosterPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.1f, newRotation);
                    // get poster image
                    StorageReference storageRef = FirebaseStorage.GetInstance(FirebaseApp.DefaultInstance).GetReferenceFromUrl("gs://ourworld-737cd.appspot.com");

                    await storageRef.Root.Child("users/" + user + "/" + type + "/" + id + ".jpg").GetDownloadUrlAsync().ContinueWithOnMainThread(async task2 =>
                    {
                        if (task2.IsFaulted || task2.IsCanceled)
                        {
                            Console.WriteLine("FAULTED PNG");

                            //get plane normal up vector
                            wallNormalUp = s_Hits[0].pose.up;

                            byte[] data = await storageRef.Child("users/" + user + "/" + type + "/" + id + ".png").GetBytesAsync(1024 * 1024);
                            Console.WriteLine("Data");
                            // create texture
                            Texture2D texture = new Texture2D(1, 1);
                            // load texture
                            texture.LoadImage(data);
                            // set diffuse texture
                            spawnedObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
                            // get width and height of image and set scale of poster

                            float maxWidth = 0.1f;
                            float maxHeight = 0.1f;

                            float width = texture.width;
                            float height = texture.height;

                            // maintain aspect ratio and scale width and height
                            if (width > height)
                            {
                                float ratio = width / height;
                                width = maxWidth;
                                height = width / ratio;
                            }
                            else
                            {
                                float ratio = height / width;
                                height = maxHeight;
                                width = height / ratio;
                            }

                            spawnedObject.transform.localScale = new Vector3(width, 1, height);

                            ratioX = width;
                            ratioY = height;
                        }
                        else
                        {
                            Console.WriteLine("WORKING JPG");

                            byte[] data = await storageRef.Child("users/" + user + "/" + type + "/" + id + ".jpg").GetBytesAsync(1024 * 1024);
                            // create texture
                            Texture2D texture = new Texture2D(1, 1);
                            // load texture
                            texture.LoadImage(data);
                            // set diffuse texture
                            spawnedObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
                            // get width and height of image and set scale of poster
                            float maxWidth = 0.1f;
                            float maxHeight = 0.1f;

                            float width = texture.width;
                            float height = texture.height;

                            // maintain aspect ratio and scale width and height
                            if (width > height)
                            {
                                float ratio = width / height;
                                width = maxWidth;
                                height = width / ratio;
                            }
                            else
                            {
                                float ratio = height / width;
                                height = maxHeight;
                                width = height / ratio;
                            }

                            spawnedObject.transform.localScale = new Vector3(width, 1, height);

                            ratioX = width;
                            ratioY = height;
                        }
                    });


                }
                else
                {
                    spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.1f, transform.rotation * Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y + 180, 0));
                }

                if (onPlacedObject != null)
                {
                    onPlacedObject();
                }
            }
            else
            {
                Console.WriteLine("NOT HIT");
                if (type.Equals("posters") || type.Equals("stickers") || type.Equals("images"))
                {
                    spawnedObject = Instantiate(m_PosterPrefab, Camera.main.transform.position + Camera.main.transform.forward * 2f, transform.rotation * Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y + 180, 0));
                    // get poster image
                    StorageReference storageRef = FirebaseStorage.GetInstance(FirebaseApp.DefaultInstance).GetReferenceFromUrl("gs://ourworld-737cd.appspot.com");

                    await storageRef.Root.Child("users/" + user + "/" + type + "/" + id + ".jpg").GetDownloadUrlAsync().ContinueWithOnMainThread(async task2 =>
                    {
                        if (task2.IsFaulted || task2.IsCanceled)
                        {
                            Console.WriteLine("FAULTED PNG");

                            byte[] data = await storageRef.Child("users/" + user + "/" + type + "/" + id + ".png").GetBytesAsync(1024 * 1024);
                            // create texture
                            Texture2D texture = new Texture2D(1, 1);
                            // load texture
                            texture.LoadImage(data);
                            // set diffuse texture
                            spawnedObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
                            // get width and height of image and set scale of poster
                            float maxWidth = 0.1f;
                            float maxHeight = 0.1f;

                            float width = texture.width;
                            float height = texture.height;

                            // maintain aspect ratio and scale width and height
                            if (width > height)
                            {
                                float ratio = width / height;
                                width = maxWidth;
                                height = width / ratio;
                            }
                            else
                            {
                                float ratio = height / width;
                                height = maxHeight;
                                width = height / ratio;
                            }

                            spawnedObject.transform.localScale = new Vector3(width, 1, height);

                            ratioX = width;
                            ratioY = height;
                        }
                        else
                        {
                            Console.WriteLine("WORKING JPG");

                            byte[] data = await storageRef.Child("users/" + user + "/" + type + "/" + id + ".jpg").GetBytesAsync(1024 * 1024);
                            // create texture
                            Texture2D texture = new Texture2D(1, 1);
                            // load texture
                            texture.LoadImage(data);
                            // set diffuse texture
                            spawnedObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
                            // get width and height of image and set scale of poster
                            float maxWidth = 0.1f;
                            float maxHeight = 0.1f;

                            float width = texture.width;
                            float height = texture.height;

                            // maintain aspect ratio and scale width and height
                            if (width > height)
                            {
                                float ratio = width / height;
                                width = maxWidth;
                                height = width / ratio;
                            }
                            else
                            {
                                float ratio = height / width;
                                height = maxHeight;
                                width = height / ratio;
                            }

                            spawnedObject.transform.localScale = new Vector3(width, 1, height);

                            ratioX = width;
                            ratioY = height;
                        }
                    });
                }
                else
                {
                    spawnedObject = Instantiate(m_PlacedPrefab, Camera.main.transform.position + Camera.main.transform.forward * 2f, transform.rotation * Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y + 180, 0));
                }

                if (onPlacedObject != null)
                {
                    onPlacedObject();
                }
            }

            arWorldMapController.Log("before center chunk");
            // get distance between spawned object and center chunk
            float distanceToCenterChunk = Vector3.Distance(spawnedObject.transform.position, centerChunk.transform.position);
            arWorldMapController.Log("centerChunk");

            // get components of distance to center chunk in the direction of center chunk forward
            float distanceToCenterChunkForward = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.forward);

            // get components of distance to center chunk in the direction of center chunk right
            float distanceToCenterChunkRight = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.right);

            // round down to nearest 1 unit
            int roundedDistanceToCenterChunkForward = (int)Math.Round(distanceToCenterChunkForward);
            int roundedDistanceToCenterChunkRight = (int)Math.Round(distanceToCenterChunkRight);

            // convert to coordinates relative to the world map

            Vector3 centerChunkCoordinates = centerChunk.transform.position;

            if (currentChunk == null)
            {
                currentChunk = Instantiate(arWorldMapController.ChunkPrefab, centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight, centerChunk.transform.rotation);
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }
            else
            {
                currentChunk.transform.position = centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight;
                currentChunk.transform.rotation = centerChunk.transform.rotation;
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }

            arWorldMapController.Log("currentChunk");

            moveChild = Instantiate(MoveComponentPrefab, spawnedObject.transform.position, Quaternion.identity);
        }

        Vector3 tempPos = new Vector3(0, 0, 0);
        async void AddObject(string type, string user, string id)
        {
            isAdding = true;
            arWorldMapController.Log("Adding: users/" + user + "/objects/" + id + ".glb");
            arWorldMapController.Log("type: " + type);


            // raycast directly in front of camera to place object 0.5 units above plane hit relative to plane normal. If there is no plane hit, place object 0.5 units above camera
            if (m_RaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;
                tempPos = hitPose.position;

                StorageReference storageRef = FirebaseStorage.GetInstance(FirebaseApp.DefaultInstance).GetReferenceFromUrl("gs://ourworld-737cd.appspot.com");

                string url = "users/" + user + "/" + type + "/" + id + ".glb";

                arWorldMapController.Log("creating new file");

                if (File.Exists(preFilePath + url))
                {
                    // File.Delete(preFilePath + url);
                    LoadModel(preFilePath + url);
                }
                else
                {

                    // get glb file and instantiate object
                    // await storageRef.Child("users/" + user + "/" + type + "/" + id + ".glb").GetFileAsync(preFilePath + url);
                    await storageRef.Child("users/" + user + "/" + type + "/" + id + ".glb").GetDownloadUrlAsync().ContinueWith((Task<Uri> task) =>
                    {
                        if (!task.IsFaulted && !task.IsCanceled)
                        {
                            arWorldMapController.Log("WORKING GLB");
                            arWorldMapController.Log(task.Result.ToString());
                            DownloadFile(task.Result.ToString(), preFilePath + url);
                        }
                        else
                        {
                            arWorldMapController.Log(task.Exception.ToString());
                        }
                    });
                }

                if (onPlacedObject != null)
                {
                    onPlacedObject();
                }

                // spawnedObject = Importer.LoadFromFile(preFilePath + url);

                // spawnedObject.transform.position = hitPose.position;

            }

            arWorldMapController.Log("before center chunk");
            // get distance between spawned object and center chunk
            float distanceToCenterChunk = Vector3.Distance(spawnedObject.transform.position, centerChunk.transform.position);
            arWorldMapController.Log("centerChunk");

            // get components of distance to center chunk in the direction of center chunk forward
            float distanceToCenterChunkForward = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.forward);

            // get components of distance to center chunk in the direction of center chunk right
            float distanceToCenterChunkRight = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.right);

            // round down to nearest 1 unit
            int roundedDistanceToCenterChunkForward = (int)Math.Round(distanceToCenterChunkForward);
            int roundedDistanceToCenterChunkRight = (int)Math.Round(distanceToCenterChunkRight);

            // convert to coordinates relative to the world map

            Vector3 centerChunkCoordinates = centerChunk.transform.position;

            if (currentChunk == null)
            {
                currentChunk = Instantiate(arWorldMapController.ChunkPrefab, centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight, centerChunk.transform.rotation);
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }
            else
            {
                currentChunk.transform.position = centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight;
                currentChunk.transform.rotation = centerChunk.transform.rotation;
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }

            arWorldMapController.Log("currentChunk");
            change = "move";

            // moveChild = Instantiate(MoveComponentPrefab, spawnedObject.transform.position, Quaternion.identity);
        }

        async void AddLight(String type)
        {
            isAdding = true;


            // raycast directly in front of camera to place object 0.5 units above plane hit relative to plane normal. If there is no plane hit, place object 0.5 units above camera
            if (m_RaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;
                tempPos = hitPose.position;

                switch (type)
                {
                    case "spotlights":
                        spawnedObject = Instantiate(arWorldMapController.spotlightPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.5f, Quaternion.identity);
                        break;
                    case "pointlights":
                        // spawnedObject = Instantiate(arWorldMapController.pointlightPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.5f, Quaternion.identity);
                        break;
                    case "directionallights":
                        // spawnedObject = Instantiate(arWorldMapController.directionallightPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.5f, Quaternion.identity);
                        break;
                }
                // spawnedObject = Instantiate(arWorldMapController.spotlightPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.5f, Quaternion.identity);

                if (onPlacedObject != null)
                {
                    onPlacedObject();
                }

                // spawnedObject = Importer.LoadFromFile(preFilePath + url);

                // spawnedObject.transform.position = hitPose.position;

            }

            arWorldMapController.Log("before center chunk");
            // get distance between spawned object and center chunk
            float distanceToCenterChunk = Vector3.Distance(spawnedObject.transform.position, centerChunk.transform.position);
            arWorldMapController.Log("centerChunk");

            // get components of distance to center chunk in the direction of center chunk forward
            float distanceToCenterChunkForward = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.forward);

            // get components of distance to center chunk in the direction of center chunk right
            float distanceToCenterChunkRight = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.right);

            // round down to nearest 1 unit
            int roundedDistanceToCenterChunkForward = (int)Math.Round(distanceToCenterChunkForward);
            int roundedDistanceToCenterChunkRight = (int)Math.Round(distanceToCenterChunkRight);

            // convert to coordinates relative to the world map

            Vector3 centerChunkCoordinates = centerChunk.transform.position;

            if (currentChunk == null)
            {
                currentChunk = Instantiate(arWorldMapController.ChunkPrefab, centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight, centerChunk.transform.rotation);
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }
            else
            {
                currentChunk.transform.position = centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight;
                currentChunk.transform.rotation = centerChunk.transform.rotation;
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }

            arWorldMapController.Log("currentChunk");
            change = "move";

            moveChild = Instantiate(MoveComponentPrefab, spawnedObject.transform.position, Quaternion.identity);
        }

        async void AddFilter(String type)
        {
            isAdding = true;


            // raycast directly in front of camera to place object 0.5 units above plane hit relative to plane normal. If there is no plane hit, place object 0.5 units above camera
            if (m_RaycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;
                tempPos = hitPose.position;


                spawnedObject = Instantiate(arWorldMapController.filterPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.5f, Quaternion.identity);

                // spawnedObject = Instantiate(arWorldMapController.spotlightPrefab, hitPose.position + hitPose.rotation * Vector3.up * 0.5f, Quaternion.identity);

                if (onPlacedObject != null)
                {
                    onPlacedObject();
                }

                // spawnedObject = Importer.LoadFromFile(preFilePath + url);

                // spawnedObject.transform.position = hitPose.position;

            }

            arWorldMapController.Log("before center chunk");
            // get distance between spawned object and center chunk
            float distanceToCenterChunk = Vector3.Distance(spawnedObject.transform.position, centerChunk.transform.position);
            arWorldMapController.Log("centerChunk");

            // get components of distance to center chunk in the direction of center chunk forward
            float distanceToCenterChunkForward = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.forward);

            // get components of distance to center chunk in the direction of center chunk right
            float distanceToCenterChunkRight = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.right);

            // round down to nearest 1 unit
            int roundedDistanceToCenterChunkForward = (int)Math.Round(distanceToCenterChunkForward);
            int roundedDistanceToCenterChunkRight = (int)Math.Round(distanceToCenterChunkRight);

            // convert to coordinates relative to the world map

            Vector3 centerChunkCoordinates = centerChunk.transform.position;

            if (currentChunk == null)
            {
                currentChunk = Instantiate(arWorldMapController.ChunkPrefab, centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight, centerChunk.transform.rotation);
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }
            else
            {
                currentChunk.transform.position = centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight;
                currentChunk.transform.rotation = centerChunk.transform.rotation;
                chunkPos[0] = roundedDistanceToCenterChunkForward;
                chunkPos[1] = roundedDistanceToCenterChunkRight;
            }

            arWorldMapController.Log("currentChunk");
            change = "move";

            moveChild = Instantiate(MoveComponentPrefab, spawnedObject.transform.position, Quaternion.identity);
        }
        private Quaternion trueRot = Quaternion.identity;
        private bool rotating = false;
        private string lastRotation = "none";

        private bool locked = false;
        private Vector2 lastTouch = Vector2.zero;
        private Vector3 previousRotation = Vector3.zero;

        private Vector3 wallNormalUp = Vector3.up;

        private int roundTo = 15;

        private int[] chunkPos = new int[2] { 0, 0 };

        private bool pressAndHold = false;
        private Vector3 pressPosition = Vector3.zero;
        private string axisMove = "X";

        private Plane fakePlane;

        private float moveOffset = 0.1f;

        private bool notMoveTool = false;
        private void Update()
        {
            if (isPrev && !change.Equals("delete")) {
                innerFilter.color = api.inputColor;
                innerFilter.saturation = api.inputSaturation;
                innerFilter.threshold = api.inputThreshold;
                innerFilter.isColor = api.inputIsColor;
                return;
            }

            if (change.Equals("delete"))
            {
                innerFilter.transform.GetChild(0).gameObject.SetActive(false);
                if (moveChild != null)
                {
                    Destroy(moveChild);
                    moveChild = null;
                }
                if (spawnedObject != null)
                {
                    change = "move";
                    Destroy(spawnedObject);
                    spawnedObject = null;

                    Destroy(currentChunk);
                    currentChunk = null;
                    isAdding = false;
                }

            }
            else if (change.Equals("save"))
            {
                if (moveChild != null)
                {
                    Destroy(moveChild);
                    moveChild = null;
                }
                if (spawnedObject != null)
                {

                    string tempType = "posters";
                    if (type.Equals("objects"))
                    {
                        tempType = "objects";
                    }


                    change = "move";
                    Vector3 localPosition = currentChunk.transform.InverseTransformPoint(spawnedObject.transform.position);
                    // save object to world map

                    // check if the chunk already exists
                    isAdding = false;

                    // increment the number of creations by one
                    arWorldMapController.db.Collection(type).Document(id).UpdateAsync("creations", FieldValue.Increment(1));

                    if (arWorldMapController.chunksPos.ContainsKey(chunkPos[0] + "-" + chunkPos[1]))
                    {
                        Destroy(currentChunk);


                        currentChunk = arWorldMapController.chunks[arWorldMapController.chunksPos[chunkPos[0] + "-" + chunkPos[1]]];
                        Chunk chunkScript = currentChunk.GetComponent<Chunk>();
                        // make spawned object a child of the chunk

                        spawnedObject.transform.parent = currentChunk.transform;
                        // save the local position of the spawned object relative to the chunk
                        spawnedObject.transform.localPosition = localPosition;

                        if (type.Equals("spotlights"))
                        {
                            arWorldMapController.db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(chunkScript.id).Collection(tempType).Document().SetAsync(new
                            {
                                user = user,
                                type = type,
                                id = id,
                                x = spawnedObject.transform.localPosition.x,
                                y = spawnedObject.transform.localPosition.y,
                                z = spawnedObject.transform.localPosition.z,
                                rx = spawnedObject.transform.localRotation.eulerAngles.x,
                                ry = spawnedObject.transform.localRotation.eulerAngles.y,
                                rz = spawnedObject.transform.localRotation.eulerAngles.z,
                                sx = spawnedObject.transform.localScale.x,
                                sy = spawnedObject.transform.localScale.y,
                                sz = spawnedObject.transform.localScale.z,
                                topR = api.topR,
                                botR = api.botR,
                            });
                        }
                        else
                        {
                            arWorldMapController.db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(chunkScript.id).Collection(tempType).Document().SetAsync(new
                            {
                                user = user,
                                type = type,
                                id = id,
                                x = spawnedObject.transform.localPosition.x,
                                y = spawnedObject.transform.localPosition.y,
                                z = spawnedObject.transform.localPosition.z,
                                rx = spawnedObject.transform.localRotation.eulerAngles.x,
                                ry = spawnedObject.transform.localRotation.eulerAngles.y,
                                rz = spawnedObject.transform.localRotation.eulerAngles.z,
                                sx = spawnedObject.transform.localScale.x,
                                sy = spawnedObject.transform.localScale.y,
                                sz = spawnedObject.transform.localScale.z,
                            });
                        }
                    }
                    else
                    {
                        spawnedObject.transform.parent = currentChunk.transform;
                        // save the local position of the spawned object relative to the chunk
                        spawnedObject.transform.localPosition = localPosition;

                        Chunk chunkScript = currentChunk.GetComponent<Chunk>();
                        var anchor = currentChunk.AddComponent<ARAnchor>();
                        chunkScript.db = arWorldMapController.db;
                        chunkScript.ARCamera = arWorldMapController.ARCamera;
                        chunkScript.arWorldMapController = arWorldMapController;
                        chunkScript.id = anchor.trackableId.ToString();

                        arWorldMapController.db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(chunkScript.id).SetAsync(new Dictionary<string, object>{
                            { "x", currentChunk.transform.position.x },
                            { "y", currentChunk.transform.position.y },
                            { "z", currentChunk.transform.position.z },
                            { "cx", chunkPos[0] },
                            { "cy", chunkPos[1] },
                            { "size", 0 },
                            { "updated", DateTime.Now },
                            { "worldMapId", arWorldMapController.worldMapId }
                        });

                        if (type.Equals("spotlights"))
                        {
                            arWorldMapController.db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(chunkScript.id).Collection(tempType).Document().SetAsync(new
                            {
                                user = user,
                                type = type,
                                id = id,
                                x = spawnedObject.transform.localPosition.x,
                                y = spawnedObject.transform.localPosition.y,
                                z = spawnedObject.transform.localPosition.z,
                                rx = spawnedObject.transform.localRotation.eulerAngles.x,
                                ry = spawnedObject.transform.localRotation.eulerAngles.y,
                                rz = spawnedObject.transform.localRotation.eulerAngles.z,
                                sx = spawnedObject.transform.localScale.x,
                                sy = spawnedObject.transform.localScale.y,
                                sz = spawnedObject.transform.localScale.z,
                                topR = api.topR,
                                botR = api.botR,
                            });
                        }
                        else
                        {
                            arWorldMapController.db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(chunkScript.id).Collection(tempType).Document().SetAsync(new
                            {
                                user = user,
                                type = type,
                                id = id,
                                x = spawnedObject.transform.localPosition.x,
                                y = spawnedObject.transform.localPosition.y,
                                z = spawnedObject.transform.localPosition.z,
                                rx = spawnedObject.transform.localRotation.eulerAngles.x,
                                ry = spawnedObject.transform.localRotation.eulerAngles.y,
                                rz = spawnedObject.transform.localRotation.eulerAngles.z,
                                sx = spawnedObject.transform.localScale.x,
                                sy = spawnedObject.transform.localScale.y,
                                sz = spawnedObject.transform.localScale.z,
                            });
                        }

                        arWorldMapController.chunks.Add(anchor.trackableId.ToString(), currentChunk);
                        arWorldMapController.anchors.Add(anchor.trackableId.ToString(), anchor);
                    }
                    currentChunk = null;
                    spawnedObject = null;
                }

                arWorldMapController.OnSaveButtonDelay(3);
            }

            if (type.Equals("spotlights"))
            {
                spawnedObject.GetComponent<Vertex>().topRadius = api.topR;
                spawnedObject.GetComponent<Vertex>().bottomRadius = api.botR;
            }

            if (moveChild != null)
            {
                if (change.Equals("move"))
                {
                    moveChild.SetActive(true);
                }
                else
                {
                    moveChild.SetActive(false);
                }
            }

            if (Input.touchCount < 1 && !Input.GetMouseButton(0))
            {
                rotating = false;
                locked = false;
                pressAndHold = false;
                notMoveTool = false;
                if (moveChild != null)
                {
                    moveChild.transform.position = spawnedObject.transform.position;
                }
                return;
            }

            Vector3 position = Vector3.zero;

            position = Input.GetTouch(0).position;

            if (change.Equals("move"))
            {
                // if touch is on screen
                if (position.x > 0 && position.x < Screen.width && position.y > 0 && position.y < Screen.height)
                {

                    if (arWorldMapController.centerChunk == null)
                    {
                        return;
                    }

                    if (centerChunk == null || centerChunk != arWorldMapController.centerChunk)
                    {
                        centerChunk = arWorldMapController.centerChunk;
                    }

                    if (spawnedObject == null)
                    {
                        return;
                    }

                    if (!notMoveTool)
                    {

                        var ray = Camera.main.ScreenPointToRay(position);

                        // get if hit the plane
                        if (pressAndHold)
                        {
                            if (fakePlane.Raycast(ray, out float enter))
                            {
                                switch (axisMove)
                                {
                                    case "X":
                                        moveChild.transform.position = new Vector3(ray.GetPoint(enter).x + moveOffset, pressPosition.y, pressPosition.z);

                                        break;
                                    case "Y":
                                        float cameraRotationY = Camera.main.transform.eulerAngles.y;
                                        Vector3 planeDir = Quaternion.Euler(0, cameraRotationY, 0) * Vector3.forward;
                                        fakePlane = new Plane(planeDir, pressPosition);
                                        moveChild.transform.position = new Vector3(pressPosition.x, ray.GetPoint(enter).y + moveOffset, pressPosition.z);

                                        break;
                                    case "Z":
                                        moveChild.transform.position = new Vector3(pressPosition.x, pressPosition.y, ray.GetPoint(enter).z + moveOffset);
                                        break;
                                    default:
                                        moveChild.transform.position = ray.GetPoint(enter);
                                        break;
                                }
                                spawnedObject.transform.position = moveChild.transform.position;

                                // get distance between spawned object and center chunk
                                float distanceToCenterChunks = Vector3.Distance(spawnedObject.transform.position, centerChunk.transform.position);

                                // get components of distance to center chunk in the direction of center chunk forward
                                float distanceToCenterChunkForwards = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.forward);

                                // get components of distance to center chunk in the direction of center chunk right
                                float distanceToCenterChunkRights = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.right);

                                // round down to nearest 1 unit
                                int roundedDistanceToCenterChunkForwards = (int)Math.Round(distanceToCenterChunkForwards);
                                int roundedDistanceToCenterChunkRights = (int)Math.Round(distanceToCenterChunkRights);

                                // convert to coordinates relative to the world map

                                Vector3 centerChunkCoordinatess = centerChunk.transform.position;

                                if (currentChunk == null)
                                {
                                    currentChunk = Instantiate(arWorldMapController.ChunkPrefab, centerChunkCoordinatess + centerChunk.transform.forward * roundedDistanceToCenterChunkForwards + centerChunk.transform.right * roundedDistanceToCenterChunkRights, centerChunk.transform.rotation);
                                    chunkPos[0] = roundedDistanceToCenterChunkForwards;
                                    chunkPos[1] = roundedDistanceToCenterChunkRights;
                                }
                                else
                                {
                                    currentChunk.transform.position = centerChunkCoordinatess + centerChunk.transform.forward * roundedDistanceToCenterChunkForwards + centerChunk.transform.right * roundedDistanceToCenterChunkRights;
                                    currentChunk.transform.rotation = centerChunk.transform.rotation;
                                    chunkPos[0] = roundedDistanceToCenterChunkForwards;
                                    chunkPos[1] = roundedDistanceToCenterChunkRights;
                                }

                                Vector3 eulerRots = spawnedObject.transform.rotation.eulerAngles;
                                spawnedObject.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Round(eulerRots.x / roundTo) * roundTo, Mathf.Round(eulerRots.y / roundTo) * roundTo, Mathf.Round(eulerRots.z / roundTo) * roundTo));
                            }
                            return;
                        }


                        var hasHit = Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, selectedLayers);
                        if (hasHit)
                        {

                            if (!pressAndHold)
                            {
                                pressPosition = spawnedObject.transform.position;
                                pressAndHold = true;
                                if (hit.collider.gameObject.name == "X")
                                {
                                    arWorldMapController.Log("X");
                                    axisMove = "X";
                                    fakePlane = new Plane(Vector3.up, hit.point);

                                    moveOffset = spawnedObject.transform.position.x - hit.point.x;
                                }
                                else if (hit.collider.gameObject.name == "Y")
                                {
                                    arWorldMapController.Log("Y");
                                    axisMove = "Y";

                                    //Get camera rotation around y-axis 
                                    float cameraRotationY = Camera.main.transform.eulerAngles.y;
                                    Vector3 planeDir = Quaternion.Euler(0, cameraRotationY, 0) * Vector3.forward;

                                    fakePlane = new Plane(planeDir, hit.point);

                                    moveOffset = spawnedObject.transform.position.y - hit.point.y;
                                }
                                else if (hit.collider.gameObject.name == "Z")
                                {
                                    arWorldMapController.Log("Z");
                                    axisMove = "Z";
                                    fakePlane = new Plane(Vector3.up, hit.point);

                                    moveOffset = spawnedObject.transform.position.z - hit.point.z;
                                }
                                moveChild.transform.position = spawnedObject.transform.position;


                                return;
                            }

                        }
                    }

                    // arWorldMapController.Log("position");

                    if (m_RaycastManager.Raycast(position, s_Hits, TrackableType.PlaneWithinPolygon))
                    {
                        notMoveTool = true;
                        Pose hitPose = s_Hits[0].pose;
                        wallNormalUp = s_Hits[0].pose.up;

                        // spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);

                        // get axis of the plane and print it out
                        Vector3 planeNormal = hitPose.rotation * Vector3.up;

                        // // get if object is behind plane
                        // bool isBehindPlane = Vector3.Dot(spawnedObject.transform.position - hitPose.position, planeNormal) < 0;

                        // get distance between object and plane only in the direction of the plane normal
                        float distance = Vector3.Dot(spawnedObject.transform.position - hitPose.position, planeNormal);

                        // move object to the plane


                        // if poster, make the rotation same as the plane normal
                        // if (type.Equals("posters") || type.Equals("stickers") || type.Equals("images"))
                        // {
                        //     Console.WriteLine("POSTER");
                        // spawnedObject.transform.rotation = hitPose.rotation;
                        spawnedObject.transform.position = hitPose.position + hitPose.rotation * Vector3.up * 0.1f;
                        moveChild.transform.position = spawnedObject.transform.position;
                        // }
                        // else
                        // {
                        //     spawnedObject.transform.position = hitPose.position + hitPose.rotation * Vector3.up * Math.Max(distance, 0.1f);
                        // }

                    }
                    else
                    {
                        // if no plane is hit, move object to 0.5 units in front of camera at position of touch
                        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 1.5f));
                        spawnedObject.transform.position = touchPosition;
                        moveChild.transform.position = spawnedObject.transform.position;
                    }


                    // get distance between spawned object and center chunk
                    float distanceToCenterChunk = Vector3.Distance(spawnedObject.transform.position, centerChunk.transform.position);

                    // get components of distance to center chunk in the direction of center chunk forward
                    float distanceToCenterChunkForward = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.forward);

                    // get components of distance to center chunk in the direction of center chunk right
                    float distanceToCenterChunkRight = Vector3.Dot(spawnedObject.transform.position - centerChunk.transform.position, centerChunk.transform.right);

                    // round down to nearest 1 unit
                    int roundedDistanceToCenterChunkForward = (int)Math.Round(distanceToCenterChunkForward);
                    int roundedDistanceToCenterChunkRight = (int)Math.Round(distanceToCenterChunkRight);

                    // convert to coordinates relative to the world map

                    Vector3 centerChunkCoordinates = centerChunk.transform.position;

                    if (currentChunk == null)
                    {
                        currentChunk = Instantiate(arWorldMapController.ChunkPrefab, centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight, centerChunk.transform.rotation);
                        chunkPos[0] = roundedDistanceToCenterChunkForward;
                        chunkPos[1] = roundedDistanceToCenterChunkRight;
                    }
                    else
                    {
                        currentChunk.transform.position = centerChunkCoordinates + centerChunk.transform.forward * roundedDistanceToCenterChunkForward + centerChunk.transform.right * roundedDistanceToCenterChunkRight;
                        currentChunk.transform.rotation = centerChunk.transform.rotation;
                        chunkPos[0] = roundedDistanceToCenterChunkForward;
                        chunkPos[1] = roundedDistanceToCenterChunkRight;
                    }

                    Vector3 eulerRot = spawnedObject.transform.rotation.eulerAngles;
                    spawnedObject.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Round(eulerRot.x / roundTo) * roundTo, Mathf.Round(eulerRot.y / roundTo) * roundTo, Mathf.Round(eulerRot.z / roundTo) * roundTo));

                }

            }
            else if (change.Equals("rotate"))
            {
                if (rotating == false)
                {
                    rotating = true;
                    lastTouch = position;
                    trueRot = spawnedObject.transform.rotation;
                    locked = false;
                }

                if (locked == false)
                {
                    if (Math.Abs(lastTouch.x - position.x) > Math.Abs(lastTouch.y - position.y))
                    {
                        lastRotation = "x";
                    }
                    else
                    {
                        lastRotation = "y";
                    }
                }

                if (spawnedObject != null)
                {

                    if (Input.touchCount > 1)
                    {

                        if (locked == false)
                        {
                            lastRotation = "z";
                            locked = true;
                        }

                        if (lastRotation == "z")
                        {

                            // Touch touchZero = Input.GetTouch(0);
                            // Touch touchOne = Input.GetTouch(1);

                            // Vector2 deltaZero = Input.GetTouch(0).deltaPosition;
                            // Vector2 deltaOne = Input.GetTouch(1).deltaPosition;

                            // // get avg mag of delta positions
                            // float avgDeltaMag = (deltaZero.magnitude + deltaOne.magnitude) / 2;
                            // int clockwise = 1;

                            // // get if clockwise or counterclockwise
                            // if (deltaZero.x * deltaOne.y - deltaZero.y * deltaOne.x < 0)
                            // {
                            //     clockwise = -1;
                            // }

                            Vector2 prevPos1 = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;  // Generate previous frame's finger positions
                            Vector2 prevPos2 = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;

                            Vector2 prevDir = prevPos2 - prevPos1;
                            Vector2 currDir = Input.GetTouch(1).position - Input.GetTouch(0).position;
                            float angle = Vector2.Angle(prevDir, currDir);

                            int clockwise = Vector3.Cross(prevDir, currDir).z > 0 ? 1 : -1;

                            float avgDeltaMag = angle;

                            arWorldMapController.Log("CLOCKWISE: " + clockwise);


                            // get spawned object rotation around its up axis                           

                            Vector3 roundedRotW = new Vector3(0, trueRot.eulerAngles.y, 0);

                            // get world forward vector rotated by roundedRot
                            Vector3 worldForward = Quaternion.Euler(roundedRotW) * Vector3.forward;
                            Vector3 worldRight = Quaternion.Euler(roundedRotW) * Vector3.right;

                            // get if camera is facing parallel to world forward
                            bool isCameraFacingParallel = Vector3.Dot(Camera.main.transform.forward, worldForward) > 0.5f;
                            bool isCameraFacingParallelBack = Vector3.Dot(Camera.main.transform.forward, worldForward) < -0.5f;

                            // get if camera is facing parallel to world right
                            bool isCameraFacingParallelRight = Vector3.Dot(Camera.main.transform.forward, worldRight) > 0.5f;
                            bool isCameraFacingParallelLeft = Vector3.Dot(Camera.main.transform.forward, worldRight) < -0.5f;

                            // trueRot = Quaternion.AngleAxis(avgDeltaMag * clockwise * 0.1f, worldForward) * trueRot;

                            if (isCameraFacingParallel)
                            {
                                // swipe delta y rotates about x axis of object
                                // spawnedObject.transform.Rotate(Vector3.right, delta.y * 0.1f, Space.World);
                                // trueRot = Quaternion.AngleAxis(delta.y * 0.15f, worldRight) * trueRot;
                                arWorldMapController.Log("FRONT");
                                trueRot = Quaternion.AngleAxis(avgDeltaMag * clockwise * 0.5f, worldForward) * trueRot;
                            }
                            else if (isCameraFacingParallelBack)
                            {
                                // swipe delta y rotates about x axis of object
                                // spawnedObject.transform.Rotate(Vector3.right, -delta.y * 0.1f, Space.World);
                                // trueRot = Quaternion.AngleAxis(-delta.y * 0.15f, worldRight) * trueRot;
                                arWorldMapController.Log("BACK");
                                trueRot = Quaternion.AngleAxis(avgDeltaMag * -clockwise * 0.5f, worldForward) * trueRot;
                            }
                            else if (isCameraFacingParallelLeft)
                            {
                                // swipe delta y rotates about z axis of object
                                // spawnedObject.transform.Rotate(Vector3.forward, -delta.y * 0.1f, Space.World);
                                // trueRot = Quaternion.AngleAxis(delta.y * 0.15f, worldForward) * trueRot;
                                arWorldMapController.Log("LEFT");
                                trueRot = Quaternion.AngleAxis(avgDeltaMag * -clockwise * 0.5f, worldRight) * trueRot;
                            }
                            else if (isCameraFacingParallelRight)
                            {
                                // swipe delta y rotates about z axis of object
                                // spawnedObject.transform.Rotate(Vector3.forward, delta.y * 0.1f, Space.World);
                                // trueRot = Quaternion.AngleAxis(-delta.y * 0.15f, worldForward) * trueRot;
                                arWorldMapController.Log("RIGHT");
                                trueRot = Quaternion.AngleAxis(avgDeltaMag * clockwise * 0.5f, worldRight) * trueRot;
                            }

                            // spawnedObject.transform.Rotate(Vector3.up, deltaMagnitudeDiff * 0.01f, Space.World);
                        }
                    }
                    else
                    {
                        Vector2 delta = Input.GetTouch(0).deltaPosition;

                        // swipe delta x rotates about y axis of object
                        // spawnedObject.transform.Rotate(Vector3.up, -delta.x * 0.1f, Space.World);
                        if (lastRotation == "x")
                        {
                            trueRot = Quaternion.AngleAxis(-delta.x * 0.15f, Vector3.up) * trueRot;
                        }


                        if (lastRotation == "y")
                        {
                            // get only y axis rotation of spawned object
                            Vector3 roundedRotW = new Vector3(0, trueRot.eulerAngles.y, 0);

                            // get world forward vector rotated by roundedRot
                            Vector3 worldForward = Quaternion.Euler(roundedRotW) * Vector3.forward;
                            Vector3 worldRight = Quaternion.Euler(roundedRotW) * Vector3.right;

                            // get if camera is facing parallel to world forward
                            bool isCameraFacingParallel = Vector3.Dot(Camera.main.transform.forward, worldForward) > 0.5f;
                            bool isCameraFacingParallelBack = Vector3.Dot(Camera.main.transform.forward, worldForward) < -0.5f;

                            // get if camera is facing parallel to world right
                            bool isCameraFacingParallelRight = Vector3.Dot(Camera.main.transform.forward, worldRight) > 0.5f;
                            bool isCameraFacingParallelLeft = Vector3.Dot(Camera.main.transform.forward, worldRight) < -0.5f;

                            if (isCameraFacingParallel)
                            {
                                // swipe delta y rotates about x axis of object
                                // spawnedObject.transform.Rotate(Vector3.right, delta.y * 0.1f, Space.World);
                                trueRot = Quaternion.AngleAxis(delta.y * 0.15f, worldRight) * trueRot;
                            }
                            else if (isCameraFacingParallelBack)
                            {
                                // swipe delta y rotates about x axis of object
                                // spawnedObject.transform.Rotate(Vector3.right, -delta.y * 0.1f, Space.World);
                                trueRot = Quaternion.AngleAxis(-delta.y * 0.15f, worldRight) * trueRot;
                            }
                            else if (isCameraFacingParallelLeft)
                            {
                                // swipe delta y rotates about z axis of object
                                // spawnedObject.transform.Rotate(Vector3.forward, -delta.y * 0.1f, Space.World);
                                trueRot = Quaternion.AngleAxis(delta.y * 0.15f, worldForward) * trueRot;
                            }
                            else if (isCameraFacingParallelRight)
                            {
                                // swipe delta y rotates about z axis of object
                                // spawnedObject.transform.Rotate(Vector3.forward, delta.y * 0.1f, Space.World);
                                trueRot = Quaternion.AngleAxis(-delta.y * 0.15f, worldForward) * trueRot;
                            }
                        }
                    }



                    // snap to closest 15 degrees when rotating object with trueRot
                    Vector3 eulerRot = trueRot.eulerAngles;
                    Vector3 roundedRot = new Vector3(Mathf.Round(eulerRot.x / roundTo) * roundTo, Mathf.Round(eulerRot.y / roundTo) * roundTo, Mathf.Round(eulerRot.z / roundTo) * roundTo);

                    if (previousRotation != roundedRot)
                    {
                        locked = true;
                        previousRotation = roundedRot;
                        // rotate object
                        spawnedObject.transform.rotation = Quaternion.Euler(roundedRot);
                        trueRot = Quaternion.Euler(roundedRot);
                    }
                }

            }
            else if (change.Equals("scale"))
            {

                // scale object using two finger pinch
                if (spawnedObject != null)
                {
                    if (Input.touchCount == 2)
                    {
                        Touch touchZero = Input.GetTouch(0);
                        Touch touchOne = Input.GetTouch(1);

                        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                        // scale object
                        if (type.Equals("objects") || type.Equals("filters"))
                        {
                            spawnedObject.transform.localScale -= new Vector3(deltaMagnitudeDiff * 0.001f, deltaMagnitudeDiff * 0.001f, deltaMagnitudeDiff * 0.001f);
                            // moveChild.transform.localScale -= new Vector3(deltaMagnitudeDiff * 0.001f, deltaMagnitudeDiff * 0.001f, deltaMagnitudeDiff * 0.001f);
                        }
                        else if (type.Equals("spotlights"))
                        {
                            spawnedObject.transform.localScale -= new Vector3(0, deltaMagnitudeDiff * 0.001f, 0);
                        }
                        else
                        {
                            spawnedObject.transform.localScale -= new Vector3(deltaMagnitudeDiff * 0.001f * ratioX, 1, deltaMagnitudeDiff * 0.001f * ratioY);
                        }

                    }
                }
            }


        }





        async public void DownloadFile(string url, string filePath)
        {

            if (File.Exists(filePath))
            {
                Debug.Log("Found the same file locally, Loading!!!");

                LoadModel(filePath);

                return;
            }

            StartCoroutine(GetFileRequest(url, filePath, (UnityWebRequest req) =>
            {
                if (req.isNetworkError || req.isHttpError)
                {
                    //Logging any errors that may happen
                    Debug.Log($"{req.error} : {req.downloadHandler.text}");
                }

                else
                {
                    //Save the model fetched from firebase into spaceShip 
                    LoadModel(filePath);

                }
            }

            ));
        }

        private string preFilePath = "";

        // string GetFilePath(string url)
        // {
        //     string[] pieces = url.Split('/');
        //     string filename = pieces[pieces.Length - 1];

        //     return $"{filePath}{filename}";
        // }

        void LoadModel(string path)
        {
            Console.WriteLine("Loading glb");
            // spawnedObject = Importer.LoadFromFile(path);
            // Importer.ImportGLBAsync(path, new ImportSettings(), OnFinishAsync);
            byte[] arr = File.ReadAllBytes(path);
            Console.WriteLine("Loading glb from bytes");
            spawnedObject = Importer.LoadFromBytes(arr);
            // Importer.ImportGLBAsync(File.ReadAllBytes(path), new ImportSettings(), OnFinishAsync);

            Console.WriteLine("Finished importing");
            // spawnedObject = result;
            spawnedObject.transform.position = tempPos;
            spawnedObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            moveChild = Instantiate(MoveComponentPrefab, spawnedObject.transform.position, Quaternion.identity);


        }

        void OnFinishAsync(GameObject result, AnimationClip[] animations)
        {
            Console.WriteLine("Finished importing " + result.name);
            spawnedObject = result;
            spawnedObject.transform.position = tempPos;
            spawnedObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            moveChild = Instantiate(MoveComponentPrefab, spawnedObject.transform.position, Quaternion.identity);
        }

        IEnumerator GetFileRequest(string url, string path, Action<UnityWebRequest> callback)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.downloadHandler = new DownloadHandlerFile(path);

                yield return req.SendWebRequest();

                callback(req);
            }
        }

    }
}
