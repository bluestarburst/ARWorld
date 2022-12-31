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
        if (ARCamera == null)
        {
            return;
        }
        // get distance between camera and chunk
        float distance = Vector3.Distance(ARCamera.transform.position, transform.position);
        // if distance is greater than 100, destroy chunk
        if (!isLoaded && distance < 10)
        {
            LoadChunk();
            isLoaded = true;
        }

    }

    async void LoadChunk()
    {
        // get chunk from firestore
        //id is anchor name
        DocumentReference docRef = db.Collection("maps").Document("");
    }

}
