using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine;
using AOT;
using Newtonsoft.Json;
using Unity.Collections;
using Firebase;
using Firebase.Auth;

namespace UnityEngine.XR.ARFoundation.Samples
{

    /// <summary>
    /// C-API exposed by the Host, i.e., Unity -> Host API.
    /// </summary>
    public class HostNativeAPI
    {
        public delegate void TestDelegate(string name);

        [DllImport("__Internal")]
        public static extern void sendUnityStateUpdate(string state);

        [DllImport("__Internal")]
        public static extern void setTestDelegate(TestDelegate cb);

        // // byte array marshalling: https://stackoverflow.com/questions/10010873/how-to-pass-byte-array-to-c-sharp-dll
        [DllImport("__Internal")]
        public static extern void mapStatus(string status);

        [DllImport("__Internal")]
        public static extern void addingObj(string status);
    }

    /// <summary>
    /// C-API exposed by Unity, i.e., Host -> Unity API.
    /// </summary>
    public class UnityNativeAPI
    {

        [MonoPInvokeCallback(typeof(HostNativeAPI.TestDelegate))]
        public static void test(string name)
        {
            Debug.Log("This static function has been called from iOS!");
            Debug.Log(name);
        }

    }

    /// <summary>
    /// This structure holds the type of an incoming message.
    /// Based on the type, we will parse the extra provided data.
    /// </summary>
    public struct Message
    {
        public string type;
    }

    /// <summary>
    /// This structure holds the type of an incoming message, as well
    /// as some data.
    /// </summary>
    public struct MessageWithData<T>
    {
        [JsonProperty(Required = Newtonsoft.Json.Required.AllowNull)]
        public string type;

        [JsonProperty(Required = Newtonsoft.Json.Required.AllowNull)]
        public T data;
    }

    public class API : MonoBehaviour
    {
        public GameObject cube;
        public ARWorldMapController worldMapController;
        public AddObj addObj;
        public float lat = 0.0f;
        public float lon = 0.0f;
        public float alt = 0.0f;

        public FirebaseApp app = FirebaseApp.Create();

        void Start()
        {
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    app = FirebaseApp.Create();
                }
            });
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                HostNativeAPI.setTestDelegate(UnityNativeAPI.test);
                HostNativeAPI.sendUnityStateUpdate("ready");
            }
#endif
        }

        void ReceiveMessage(string serializedMessage)
        {
            var header = JsonConvert.DeserializeObject<Message>(serializedMessage);
            switch (header.type)
            {
                case "update-vars":
                    _UpdateVars(serializedMessage);
                    break;
                case "change-color":
                    _UpdateCubeColor(serializedMessage);
                    break;
                case "save-map":
                    _SaveMap(serializedMessage);
                    // HostNativeAPI.saveMap("hehehe");
                    break;
                case "load-map":
                    _LoadMap(serializedMessage);
                    break;
                case "add-object":
                    _AddObject(serializedMessage);
                    break;
                case "change-transform":
                    _ChangeTransform(serializedMessage);
                    break;
                default:
                    Debug.LogError("Unrecognized message '" + header.type + "'");
                    break;
            }
        }

        public void _UpdateVars(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<float[]>>(serialized);
            if (msg.data != null && msg.data.Length >= 3)
            {
                // print("lat: " + msg.data[0]);
                // print("lon: " + msg.data[1]);
                // print("alt: " + msg.data[2]);
                lat = msg.data[0];
                lon = msg.data[1];
                alt = msg.data[2];
            }
            // print("updateVars");
        }

        public void _ChangeTransform(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<string>>(serialized);
            if (msg.data != null)
            {
                addObj.change = msg.data;
            }
        }

        public void _AddObject(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<string[]>>(serialized);
            if (msg.data != null)
            {
                addObj.CreateObjectInFrontOfCamera(msg.data[0], msg.data[1], msg.data[2]);
            }
        }

        private void _UpdateCubeColor(string serialized)
        {
            if (cube == null)
            {
                Debug.LogError("Cube is null");
                return;
            }
            var msg = JsonConvert.DeserializeObject<MessageWithData<float[]>>(serialized);
            if (msg.data != null && msg.data.Length >= 3)
            {
                var color = new Color(msg.data[0], msg.data[1], msg.data[2]);
                Debug.Log("Setting Color = " + color);
                var material = cube.GetComponent<MeshRenderer>()?.sharedMaterial;
                material?.SetColor("_Color", color);
            }
        }

        private void _SaveMap(string serialized)
        {
            if (worldMapController == null)
            {
                Debug.LogError("WorldMapController is null");
                return;
            }
            var msg = JsonConvert.DeserializeObject<MessageWithData<float[]>>(serialized);
            if (msg.data != null)
            {
                lat = msg.data[0];
                lon = msg.data[1];
                alt = msg.data[2];
                Debug.Log("Saving Map = " + msg.data);
                worldMapController.OnSaveButton();
            }
        }

        private void _LoadMap(string serialized)
        {
            if (worldMapController == null)
            {
                Debug.LogError("WorldMapController is null");
                return;
            }
            var msg = JsonConvert.DeserializeObject<MessageWithData<float[]>>(serialized);
            if (msg.data != null)
            {
                lat = msg.data[0];
                lon = msg.data[1];
                alt = msg.data[2];
                Debug.Log("Loading Map = " + msg.data);
                worldMapController.OnLoadButton();
            }
        }
    }

}