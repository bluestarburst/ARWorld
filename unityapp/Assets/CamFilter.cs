using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using unity UI
using UnityEngine.UI;
public class CamFilter : MonoBehaviour
{
    public bool isVisible = false;
    public Color color = Color.black;
    public float saturation = 1.0f;
    public float threshold = 0.5f;
    public bool isColor = false;

    public bool isNormal = true;

    public float opacity = 1.0f;

    

    private Material material;

    public bool isOverride = false;

    public void setVisible(bool visible) {
        isVisible = visible;
        GetComponent<Renderer>().enabled = isVisible;
    }

    // Start is called before the first frame update
    void Start()
    {
        // get the material component of this object
        material = GetComponent<Renderer>().material;
        // set the shader property
        material.SetColor("_Color", color);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_IsColor", isColor ? 1f : 0f);
        material.SetFloat("_Opacity", opacity);

        GetComponent<Renderer>().enabled = isVisible;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isVisible) {
            return;
        }

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
        material.SetFloat("_IsColor", isColor ? 1f : 0f);
        material.SetFloat("_Opacity", opacity);

        // Console.WriteLine("color: " + color.r + " " + color.g + " " + color.b);
        // Console.WriteLine("saturation: " + saturation);
        // Console.WriteLine("threshold: " + threshold);
        // Console.WriteLine("isColor: " + isColor);
        // Console.WriteLine("opacity: " + opacity);
        // Console.WriteLine("isNormal: " + isNormal);
        // }

    }

    // do during editing in the editor
    void OnValidate()
    {
        if (!isVisible) {
            return;
        }
        // get the material component of this object
        material = GetComponent<Renderer>().material;
        // set the shader property
        material.SetColor("_Color", color);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_IsColor", isColor ? 1f : 0f);
        material.SetFloat("_Opacity", opacity);
    }
}
