using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFilter : MonoBehaviour
{
    public Color color = Color.black;
    public float saturation = 1.0f;
    public float threshold = 0.5f;
    public bool isColor = false;

    public bool isNormal = true;

    private Material material;



    // Start is called before the first frame update
    void Start()
    {
        // get the material component of this object
        material = GetComponent<Renderer>().material;
        // set the shader property
        material.SetColor("_Color", color);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Threshold", threshold);
        material.SetInt("_IsColor", isColor ? 1 : 0);
    }

    // Update is called once per frame
    void Update()
    {
        // if (isNormal)
        // {
        //     material.SetColor("_Color", Color.black);
        //     material.SetFloat("_Saturation", 1.0f);
        //     material.SetFloat("_Threshold", 0.5f);
        //     material.SetInt("_IsColor", 0);
        // }
        // else
        // {
        // get the material component of this object
        // Material material = GetComponent<Renderer>().material;
        // set the shader property
        material.SetColor("_Color", color);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Threshold", threshold);
        material.SetInt("_IsColor", isColor ? 1 : 0);
        // }

    }

    // do during editing in the editor
    void OnValidate()
    {
        // get the material component of this object
        material = GetComponent<Renderer>().material;
        // set the shader property
        material.SetColor("_Color", color);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Threshold", threshold);
        material.SetInt("_IsColor", isColor ? 1 : 0);
    }
}
