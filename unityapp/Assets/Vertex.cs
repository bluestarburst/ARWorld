

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[ExecuteInEditMode]
public class Vertex : MonoBehaviour
{
    public Mesh originalMesh;
    Mesh clonedMesh;
    MeshFilter meshFilter;
    int[] triangles;

    // [HideInInspector]
    public Vector3[] vertices;
    public Vector3[] prev_vertices;

    [HideInInspector]
    public bool isCloned = false;

    // For Editor
    public List<int>[] connectedVertices;
    public List<Vector3[]> allTriangleList;
    public bool moveVertexPoint = true;

    public float topRadius = 1f;
    public float bottomRadius = 1f;

    void Start()
    {
        InitMesh();
        getTopVertices();
        getBottomVertices();
    }

    public void InitMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        // originalMesh = meshFilter.sharedMesh; //1
        clonedMesh = new Mesh(); //2

        clonedMesh.name = "clone";
        clonedMesh.vertices = originalMesh.vertices;
        clonedMesh.triangles = originalMesh.triangles;
        clonedMesh.normals = originalMesh.normals;
        clonedMesh.uv = originalMesh.uv;
        meshFilter.mesh = clonedMesh;  //3

        vertices = clonedMesh.vertices; //4
        prev_vertices = clonedMesh.vertices;
        triangles = clonedMesh.triangles;
        isCloned = true; //5
        Debug.Log("Init & Cloned");
    }

    public void Reset()
    {
        if (clonedMesh != null && originalMesh != null) //1
        {
            clonedMesh.vertices = originalMesh.vertices; //2
            clonedMesh.triangles = originalMesh.triangles;
            clonedMesh.normals = originalMesh.normals;
            clonedMesh.uv = originalMesh.uv;
            meshFilter.mesh = clonedMesh; //3

            vertices = clonedMesh.vertices; //4
            triangles = clonedMesh.triangles;
        }
    }

    public void GetConnectedVertices()
    {
        connectedVertices = new List<int>[vertices.Length];
    }

    public void DoAction(int index, Vector3 localPos)
    {
        PullSimilarVertices(index, localPos);
    }

    // returns List of int that is related to the targetPt.
    private List<int> FindRelatedVertices(Vector3 targetPt, bool findConnected)
    {
        // list of int
        List<int> relatedVertices = new List<int>();

        int idx = 0;
        Vector3 pos;

        // loop through triangle array of indices
        for (int t = 0; t < triangles.Length; t++)
        {
            // current idx return from tris
            idx = triangles[t];
            // current pos of the vertex
            pos = vertices[idx];
            // if current pos is same as targetPt
            if (pos == targetPt)
            {
                // add to list
                relatedVertices.Add(idx);
                // if find connected vertices
                if (findConnected)
                {
                    // min
                    // - prevent running out of count
                    if (t == 0)
                    {
                        relatedVertices.Add(triangles[t + 1]);
                    }
                    // max 
                    // - prevent runnign out of count
                    if (t == triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                    }
                    // between 1 ~ max-1 
                    // - add idx from triangles before t and after t 
                    if (t > 0 && t < triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                        relatedVertices.Add(triangles[t + 1]);
                    }
                }
            }
        }
        // return compiled list of int
        return relatedVertices;
    }

    public void BuildTriangleList()
    {
    }

    public void ShowTriangle(int idx)
    {
    }

    // Pulling only one vertex pt, results in broken mesh.
    private void PullOneVertex(int index, Vector3 newPos)
    {
        vertices[index] = newPos; //1
        clonedMesh.vertices = vertices; //2
        clonedMesh.RecalculateNormals(); //3
    }

    private void PullSimilarVertices(int index, Vector3 newPos)
    {
        Vector3 targetVertexPos = vertices[index]; //1
        List<int> relatedVertices = FindRelatedVertices(targetVertexPos, false); //2
        foreach (int i in relatedVertices) //3
        {
            vertices[i] = newPos;
        }
        clonedMesh.vertices = vertices; //4
        clonedMesh.RecalculateNormals();
    }

    // To test Reset function
    public void EditMesh()
    {
        vertices[2] = new Vector3(2, 3, 4);
        vertices[3] = new Vector3(1, 2, 4);
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    public List<int> topVertices = new List<int>();
    public void getTopVertices()
    {
        topVertices.Clear();
        topVertices = new List<int>();
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y >= 1f)
            {
                topVertices.Add(i);
            }
        }
        Debug.Log("Top Vertices: " + topVertices.Count);

        // multiply x and z by 2
        for (int i = 0; i < topVertices.Count; i++)
        {
            vertices[topVertices[i]] = new Vector3(prev_vertices[topVertices[i]].x * 2, prev_vertices[topVertices[i]].y, prev_vertices[topVertices[i]].z * 2);
        }
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    public void updateTopVertices()
    {
        for (int i = 0; i < topVertices.Count; i++)
        {
            vertices[topVertices[i]] = new Vector3(prev_vertices[topVertices[i]].x * topRadius, prev_vertices[topVertices[i]].y, prev_vertices[topVertices[i]].z * topRadius);
        }
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    public List<int> bottomVertices = new List<int>();

    public void getBottomVertices()
    {
        bottomVertices.Clear();
        bottomVertices = new List<int>();
        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y <= -1f)
            {
                bottomVertices.Add(i);
            }
        }
        Debug.Log("Bottom Vertices: " + bottomVertices.Count);

        // multiply x and z by 2
        for (int i = 0; i < bottomVertices.Count; i++)
        {
            vertices[bottomVertices[i]] = new Vector3(prev_vertices[bottomVertices[i]].x * 2, prev_vertices[bottomVertices[i]].y, prev_vertices[bottomVertices[i]].z * 2);
        }
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    public void updateBottomVertices()
    {
        for (int i = 0; i < bottomVertices.Count; i++)
        {
            vertices[bottomVertices[i]] = new Vector3(prev_vertices[bottomVertices[i]].x * bottomRadius, prev_vertices[bottomVertices[i]].y, prev_vertices[bottomVertices[i]].z * bottomRadius);
        }
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    public Light light;
    public GameObject lightObj;

    void Update()
    {
        updateTopVertices();
        updateBottomVertices();

        light.range = transform.localScale.y * 2 + 0.2f;

        // get radius of the bottom and find the angle
        float angle = (Mathf.Atan((bottomRadius * 0.25f) / light.range) * Mathf.Rad2Deg) * 2;

        light.spotAngle = angle + 5;
        light.innerSpotAngle = angle;

    }
}
