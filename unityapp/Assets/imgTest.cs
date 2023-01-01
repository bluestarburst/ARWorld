using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;

public class imgTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ChangeMaterial();
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    async void ChangeMaterial()
    {
        // get the material of the mesh renderer
        Material material = GetComponent<MeshRenderer>().material;
        Debug.Log(material.mainTexture);
        // get the main texture of the material

        StorageReference storageRef = FirebaseStorage.DefaultInstance.GetReferenceFromUrl("gs://ourworld-737cd.appspot.com");
        // get image data with get file async
        byte[] data = await storageRef.Child("posters/dog.png").GetBytesAsync(1024 * 1024);
        Debug.Log(data);

        // create texture
        Texture2D texture = new Texture2D(1, 1);
        // load texture
        texture.LoadImage(data);
        // set diffuse texture
        material.mainTexture = texture;
    }
}
