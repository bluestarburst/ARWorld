using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine.UI;

public class Chunk : MonoBehaviour
{

    public GameObject ARCamera;
    public string id;
    public ARWorldMapController arWorldMapController;
    public bool isLoaded = false;

    public FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
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
            try {
                LoadChunk();
            } catch (System.Exception e) {
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

                // poster.transform.localPosition = new Vector3((float)posterData["x"], (float)posterData["y"], (float)posterData["z"]);

                arWorldMapController.Log(posterData["rx"].ToString());
                // poster.transform.localRotation = new Quaternion((float)posterData["qx"], (float)posterData["qy"], (float)posterData["qz"], (float)posterData["qw"]);
                poster.transform.localRotation = Quaternion.Euler((float)posterData["rx"], (float)posterData["ry"], (float)posterData["rz"]);

                // poster.transform.localScale = new Vector3((float)posterData["sx"], (float)posterData["sy"], (float)posterData["sz"]);

                arWorldMapController.Log("Loading poster users/" + posterData["user"] + "/posters/" + posterData["id"] + ".png");
                
                // get poster image
                StorageReference storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://ourworld-737cd.appspot.com");
                // get image data
                byte[] data = await storageRef.Child("users/" + posterData["user"] + "/posters/" + posterData["id"] + ".png").GetBytesAsync(1024 * 1024);
                
                // create texture
                Texture2D texture = new Texture2D(1, 1);
                // load texture
                texture.LoadImage(data);
                // set diffuse texture
                poster.GetComponent<MeshRenderer>().material.mainTexture = texture;


                
            }
            
        }
    }

}
