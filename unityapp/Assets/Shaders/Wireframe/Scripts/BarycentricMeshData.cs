using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;

public class BarycentricMeshData : MonoBehaviour
{
    [SerializeField]
    ARMeshManager m_MeshManager;

    public ARMeshManager meshManager
    {
        get => m_MeshManager;
        set => m_MeshManager = value;
    }

    [SerializeField]
    BarycentricDataBuilder m_DataBuilder;

    public BarycentricDataBuilder dataBuilder
    {
        get => m_DataBuilder;
        set => m_DataBuilder = value;
    }

    List<MeshFilter> m_AddedMeshes = new List<MeshFilter>();
    List<MeshFilter> m_UpdatedMeshes = new List<MeshFilter>();

    [SerializeField]
    public LayerMask layersToInclude;

    [SerializeField]
    ARWorldMapController arWorldMapController;

    void OnEnable()
    {
        m_MeshManager.meshesChanged += MeshManagerOnmeshesChanged;
    }

    void OnDisable()
    {
        m_MeshManager.meshesChanged -= MeshManagerOnmeshesChanged;
    }

    private GameObject[] meshObjects = new GameObject[0];
    private bool meshLoaded = false;

    private MaterialPropertyBlock myBlock;
    private MeshRenderer[] renderers = new MeshRenderer[0];
    private Material sharedMat;

    private float opacity = 0;

    private float radius = 0;
    private Vector3 renderPosition = Vector3.zero;

    public void meshLoading()
    {
        meshObjects = GameObject.FindGameObjectsWithTag("Mesh");

        if (myBlock == null)
        {
            myBlock = new MaterialPropertyBlock();
        }

        if (meshObjects.Length == 0)
        {
            return;
        }

        renderers = new MeshRenderer[meshObjects.Length];
        for (int i = 0; i < meshObjects.Length; i++)
        {
            renderers[i] = meshObjects[i].GetComponent<MeshRenderer>();
        }

        meshLoaded = true;
    }

    void MeshManagerOnmeshesChanged(ARMeshesChangedEventArgs obj)
    {

        m_AddedMeshes = obj.added;
        m_UpdatedMeshes = obj.updated;


        foreach (MeshFilter filter in m_AddedMeshes)
        {
            m_DataBuilder.GenerateData(filter.mesh);
            meshLoading();
        }

        foreach (MeshFilter filter in m_UpdatedMeshes)
        {
            m_DataBuilder.GenerateData(filter.sharedMesh);
        }
        /*
                if (obj.updated.Count > 0)
                {
                    m_DataBuilder.GenerateData(obj.updated[0].sharedMesh);
                }
                */
    }

    private bool isMapLoaded = false;

    public void loadedMap() {
        opacity = 0;
        isMapLoaded = true;
    }

    void Update()
    {
        if (meshLoaded)
        {
            if (!isMapLoaded)
            {
                opacity = 1;
                radius = 100f;
                renderPosition = Camera.main.transform.position;
            }
            else
            {

                if (Input.touchCount < 1 && !Input.GetMouseButton(0))
                {
                    if (opacity > 0)
                    {
                        opacity -= 0.02f;
                    }
                    else
                    {
                        radius = 0;
                    }
                }
                else
                {

                    Vector3 positionI = Input.GetTouch(0).position;
                    var ray = Camera.main.ScreenPointToRay(positionI);
                    var hasHit = Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, layersToInclude);

                    if (hasHit)
                    {
                        if (radius < 0.5f)
                        {
                            radius += 0.05f;
                        }
                        opacity = 1;

                        renderPosition = hit.point;
                    }

                }
            }

            foreach (MeshRenderer renderer in renderers)
            {


                renderer.GetPropertyBlock(myBlock);
                myBlock.SetFloat("_Opacity", opacity);
                myBlock.SetFloat("_Radius", radius);
                myBlock.SetVector("_Pos", renderPosition);
                renderer.SetPropertyBlock(myBlock);
            }

        }
    }
}
