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

            arWorldMapController.Log("1");

            cx = Convert.ToInt32(snapshot.GetValue<double>("cx"));
            cy = Convert.ToInt32(snapshot.GetValue<double>("cy"));
            int[] chunkPos = new int[] { cx, cy };
            arWorldMapController.chunksPos[cx + "-" + cy] = id;

            if (snapshot.Exists)
            {
                arWorldMapController.Log("2");
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
                    arWorldMapController.Log("3");
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

            }
        }

    }
}