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

    [Tooltip("The UI Text element used to display log messages.")]
    [SerializeField]
    public Text m_LogText;

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
    public GameObject ARCamera;
    public bool isLoaded = false;

    public FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // get distance between camera and chunk
        float distance = Vector3.Distance(ARCamera.transform.position, transform.position);
        // if distance is greater than 100, destroy chunk
        if (!isLoaded && distance < 10)
        {
            LoadChunk();
            Log("Loading chunk " + gameObject.name);
            isLoaded = true;
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
    }

    async void LoadChunk()
    {
        // get chunk from firestore
        //id is anchor name
        DocumentReference docRef = db.Collection("chunks").Document(gameObject.name);
    }

    void Log(string logMessage)
    {
        m_LogMessages.Add(logMessage);
    }

    static void SetText(Text text, string value)
    {
        if (text != null) text.text = value;
    }

    List<string> m_LogMessages;
}
