using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine.UI;
using System;
using Firebase.Extensions;
using Siccity.GLTFUtility;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace UnityEngine.XR.ARFoundation.Samples
{

    public class Chunk : MonoBehaviour
    {

        public GameObject ARCamera;
        public string id;
        public ARWorldMapController arWorldMapController;
        public bool isLoaded = false;

        public int cx = 0;
        public int cy = 0;

        public FirebaseFirestore db;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (ARCamera == null || arWorldMapController == null || id == null)
            {
                return;
            }
            // get distance between camera and chunk
            float distance = Vector3.Distance(ARCamera.transform.position, transform.position);
            // if distance is greater than 100, destroy chunk
            if (!isLoaded && distance < 10)
            {
                preFilePath = $"{Application.persistentDataPath}/Files";
                arWorldMapController.Log("Loading chunk " + id);
                try
                {
                    LoadChunk();
                }
                catch (System.Exception e)
                {
                    arWorldMapController.Log("Error loading chunk " + id + ": " + e.Message);
                }
                isLoaded = true;
            }

        }

        async void LoadChunk()
        {
            // get chunk from firestore
            //id is anchor name
            DocumentReference docRef = db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            cx = Convert.ToInt32(snapshot.GetValue<double>("cx"));
            cy = Convert.ToInt32(snapshot.GetValue<double>("cy"));
            int[] chunkPos = new int[] { cx, cy };
            arWorldMapController.chunksPos[cx + "-" + cy] = id;

            if (snapshot.Exists)
            {
                // get chunk data
                Dictionary<string, object> chunkData = snapshot.ToDictionary();

                // get chunk data
                arWorldMapController.Log("Loading chunk " + chunkData["updated"]);

                // get posters from chunk
                CollectionReference postersRef = db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(id).Collection("posters");
                QuerySnapshot postersSnapshot = await postersRef.GetSnapshotAsync();
                // go through posters and add them to the chunk
                foreach (DocumentSnapshot posterSnapshot in postersSnapshot.Documents)
                {
                    Dictionary<string, object> posterData = posterSnapshot.ToDictionary();
                    arWorldMapController.Log("Loading poster " + posterData["id"] + " from chunk " + id);
                    // create poster
                    GameObject poster = Instantiate(arWorldMapController.posterPrefab, transform);

                    // NOT WORKING :(


                    float x = Convert.ToSingle(posterSnapshot.GetValue<double>("x"));
                    float y = Convert.ToSingle(posterSnapshot.GetValue<double>("y"));
                    float z = Convert.ToSingle(posterSnapshot.GetValue<double>("z"));

                    poster.transform.localPosition = new Vector3(x, y, z);

                    float rx = Convert.ToSingle(posterSnapshot.GetValue<double>("rx"));
                    float ry = Convert.ToSingle(posterSnapshot.GetValue<double>("ry"));
                    float rz = Convert.ToSingle(posterSnapshot.GetValue<double>("rz"));

                    poster.transform.localRotation = Quaternion.Euler(rx, ry, rz);

                    float sx = Convert.ToSingle(posterSnapshot.GetValue<double>("sx"));
                    float sy = Convert.ToSingle(posterSnapshot.GetValue<double>("sy"));
                    float sz = Convert.ToSingle(posterSnapshot.GetValue<double>("sz"));

                    poster.transform.localScale = new Vector3(sx, sy, sz);

                    arWorldMapController.Log("Loading poster users/" + posterData["user"] + "/posters/" + posterData["id"] + ".jpg");



                    // get poster image
                    StorageReference storageRef = FirebaseStorage.GetInstance(FirebaseApp.DefaultInstance).GetReferenceFromUrl("gs://ourworld-737cd.appspot.com");
                    // get image data
                    // byte[] data = await storageRef.Child("users/" + posterData["user"] + "/posters/" + posterData["id"] + ".png").GetBytesAsync(1024 * 1024);

                    await storageRef.Root.Child("users/" + posterData["user"] + "/" + posterData["type"] + "/" + posterData["id"] + ".jpg").GetDownloadUrlAsync().ContinueWithOnMainThread(async task2 =>
                    {
                        if (task2.IsFaulted || task2.IsCanceled)
                        {
                            Console.WriteLine("FAULTED PNG");

                            byte[] data = await storageRef.Child("users/" + posterData["user"] + "/" + posterData["type"] + "/" + posterData["id"] + ".png").GetBytesAsync(1024 * 1024);
                            // create texture
                            Texture2D texture = new Texture2D(1, 1);
                            // load texture
                            texture.LoadImage(data);
                            // set diffuse texture
                            poster.GetComponent<MeshRenderer>().material.mainTexture = texture;
                            // get width and height of image and set scale of poster
                        }
                        else
                        {
                            Console.WriteLine("WORKING JPG");

                            byte[] data = await storageRef.Child("users/" + posterData["user"] + "/" + posterData["type"] + "/" + posterData["id"] + ".jpg").GetBytesAsync(1024 * 1024);
                            // create texture
                            Texture2D texture = new Texture2D(1, 1);
                            // load texture
                            texture.LoadImage(data);
                            // set diffuse texture
                            poster.GetComponent<MeshRenderer>().material.mainTexture = texture;
                            // get width and height of image and set scale of poster
                        }
                    });



                }


                // get posters from chunk
                CollectionReference objectsRef = db.Collection("maps").Document(arWorldMapController.worldMapId).Collection("chunks").Document(id).Collection("objects");
                QuerySnapshot objectsSnapshot = await objectsRef.GetSnapshotAsync();
                // go through posters and add them to the chunk
                foreach (DocumentSnapshot objectSnapshot in objectsSnapshot.Documents)
                {
                    Dictionary<string, object> posterData = objectSnapshot.ToDictionary();
                    arWorldMapController.Log("Loading object " + posterData["id"] + " from chunk " + id);
                    // create poster

                    arWorldMapController.Log("getting locations");

                    float x = Convert.ToSingle(objectSnapshot.GetValue<double>("x"));
                    float y = Convert.ToSingle(objectSnapshot.GetValue<double>("y"));
                    float z = Convert.ToSingle(objectSnapshot.GetValue<double>("z"));


                    float rx = Convert.ToSingle(objectSnapshot.GetValue<double>("rx"));
                    float ry = Convert.ToSingle(objectSnapshot.GetValue<double>("ry"));
                    float rz = Convert.ToSingle(objectSnapshot.GetValue<double>("rz"));


                    float sx = Convert.ToSingle(objectSnapshot.GetValue<double>("sx"));
                    float sy = Convert.ToSingle(objectSnapshot.GetValue<double>("sy"));
                    float sz = Convert.ToSingle(objectSnapshot.GetValue<double>("sz"));

                    arWorldMapController.Log("getting storage ref");

                    // get poster image
                    StorageReference storageRef = FirebaseStorage.GetInstance(FirebaseApp.DefaultInstance).GetReferenceFromUrl("gs://ourworld-737cd.appspot.com");
                    // get image data
                    // byte[] data = await storageRef.Child("users/" + posterData["user"] + "/posters/" + posterData["id"] + ".png").GetBytesAsync(1024 * 1024);
                    arWorldMapController.Log("URL");

                    string url = "users/" + posterData["user"] + "/" + posterData["type"] + "/" + posterData["id"] + ".glb";

                    arWorldMapController.Log("exists");

                    if (File.Exists(preFilePath + url))
                    {
                        // File.Delete(preFilePath + url);
                        LoadModel(preFilePath + url, x, y, z, rx, ry, rz, sx, sy, sz);
                        continue;
                    }

                    arWorldMapController.Log("creating new file");

                    // get glb file and instantiate object
                    // await storageRef.Child("users/" + user + "/" + type + "/" + id + ".glb").GetFileAsync(preFilePath + url);
                    await storageRef.Child("users/" + posterData["user"] + "/" + posterData["type"] + "/" + posterData["id"] + ".glb").GetDownloadUrlAsync().ContinueWith((Task<Uri> task) =>
                    {
                        if (!task.IsFaulted && !task.IsCanceled)
                        {
                            arWorldMapController.Log("WORKING GLB");
                            arWorldMapController.Log(task.Result.ToString());
                            DownloadFile(task.Result.ToString(), preFilePath + url, x, y, z, rx, ry, rz, sx, sy, sz);
                        }
                        else
                        {
                            arWorldMapController.Log(task.Exception.ToString());
                        }
                    });





                }

            }
        }

        async public void DownloadFile(string url, string filePath, float x, float y, float z, float rx, float ry, float rz, float sx, float sy, float sz)
        {

            if (File.Exists(filePath))
            {
                arWorldMapController.Log("Found the same file locally, Loading!!!");

                LoadModel(filePath, x, y, z, rx, ry, rz, sx, sy, sz);

                return;
            }

            StartCoroutine(GetFileRequest(url, filePath, (UnityWebRequest req) =>
            {
                if (req.isNetworkError || req.isHttpError)
                {
                    //Logging any errors that may happen
                    arWorldMapController.Log($"{req.error} : {req.downloadHandler.text}");
                }

                else
                {
                    //Save the model fetched from firebase into spaceShip 
                    LoadModel(filePath, x, y, z, rx, ry, rz, sx, sy, sz);

                }
            }

            ));
        }

        private string preFilePath = "";

        void LoadModel(string path, float x, float y, float z, float rx, float ry, float rz, float sx, float sy, float sz)
        {
            GameObject obj = Importer.LoadFromFile(path);
            obj.transform.SetParent(transform);
            
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.localRotation = Quaternion.Euler(rx, ry, rz);
            obj.transform.localScale = new Vector3(sx, sy, sz);
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