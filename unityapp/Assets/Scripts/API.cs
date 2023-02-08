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
#if PLATFORM_IOS
using UnityEngine.iOS;
using UnityEngine.Apple.ReplayKit;

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

        [DllImport("__Internal")]
        public static extern void ElementOptions(string type, string id, string chunkId, string storageId, string user, string createdBy);

        [DllImport("__Internal")]
        public static extern void SetPersistentDataPath(string path);
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
        public BarycentricMeshData bay;
        public AROcclusionManager occlusionManager;
        public AddObj addObj;
        public float lat = 0.0f;
        public float lon = 0.0f;
        public float alt = 0.0f;

        public bool recievedData = false;

        public FirebaseApp app = FirebaseApp.Create();

        public bool finishedStart = false;

        public GameObject logs;

        public float topR = 0.5f;
        public float botR = 1.5f;

        public Color inputColor = Color.black;
        public float inputSaturation = 1.0f;
        public float inputThreshold = 0.5f;
        public bool inputIsColor = false;

        public string status = "";

        public Color spotColor = Color.white;

        void Awake()
        {
            if (!finishedStart)
            {
                Application.targetFrameRate = 30;
                // half the target resolution of the camera
                Screen.SetResolution(Screen.width / 2, Screen.height / 2, true);
                finishedStart = true;
            }

            HostNativeAPI.SetPersistentDataPath(Application.persistentDataPath);
        }

        void OnApplicationFocus(bool focusStatus)
        {

        }

        void Start()
        {
            logs.SetActive(false);
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
                    recievedData = true;
                    _UpdateVars(serializedMessage);
                    break;
                case "change-color":
                    _UpdateCubeColor(serializedMessage);
                    break;
                case "save-map":
                    worldMapController.ResetAndWaitUntilMappedSave();
                    // _SaveMap(serializedMessage);
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
                case "change-settings":
                    _ChangeSettings(serializedMessage);
                    break;
                case "change-radius":
                    _ChangeRadius(serializedMessage);
                    break;
                case "change-filter":
                    _ChangeFilter(serializedMessage);
                    break;
                case "next-step-filter":
                    addObj.AddFilter();
                    break;
                case "delete-obj":
                    _DeleteObj(serializedMessage);
                    break;
                case "take-pic":
                    _TakePic(serializedMessage);
                    break;
                default:
                    Debug.LogError("Unrecognized message '" + header.type + "'");
                    break;
            }
        }

        public void _TakePic(string serialized)
        {
            ScreenCapture.CaptureScreenshot("screenshot.png");
            Console.WriteLine("Screenshot taken");
        }

        public void _TakeVideo(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<string>>(serialized);
            if (msg.data != null) {
                if (msg.data == "start") {
                    // start recording
                    ReplayKit.StartRecording(true, true);

                } else if (msg.data == "stop") {
                    // stop recording
                    ReplayKit.StopRecording();
                    ReplayKit.Preview();
                }
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

        public string dtype = "";
        public string did = "";
        public string dchunkId = "";
        public bool shouldDelete = false;

        public void _DeleteObj(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<string[]>>(serialized);
            if (msg.data != null)
            {
                dtype = msg.data[0];
                did = msg.data[1];
                dchunkId = msg.data[2];
                shouldDelete = true;
            }
        }

        public void _ChangeFilter(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<float[]>>(serialized);
            if (msg.data != null && msg.data.Length >= 4)
            {
                inputColor = new Color(msg.data[0], msg.data[1], msg.data[2]);
                inputSaturation = msg.data[3];
                inputThreshold = msg.data[4];
                inputIsColor = msg.data[5] >= 0.5f;
                Console.WriteLine("inputIsColor: " + inputIsColor);
            }
        }

        public void _ChangeRadius(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<float[]>>(serialized);
            if (msg.data != null && msg.data.Length >= 2)
            {
                topR = msg.data[0];
                botR = msg.data[1];
                spotColor = new Color(msg.data[2], msg.data[3], msg.data[4]);
            }
        }

        public void _ChangeSettings(string serialized)
        {
            var msg = JsonConvert.DeserializeObject<MessageWithData<string>>(serialized);
            if (msg.data != null)
            {
                switch (msg.data)
                {
                    case "mesh-on":
                        bay.showMesh = true;
                        occlusionManager.enabled = false;
                        break;
                    case "mesh-off":
                        bay.showMesh = false;
                        occlusionManager.enabled = true;
                        break;
                    case "logs-on":
                        logs.SetActive(true);
                        break;
                    case "logs-off":
                        logs.SetActive(false);
                        break;
                    case "chunks-on":
                        worldMapController.showChunks = true;
                        break;
                    case "chunks-off":
                        worldMapController.showChunks = false;
                        break;
                    default:
                        break;
                }
            }
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
                worldMapController.OnLoadButton();
            }
        }
    }
}
#endif