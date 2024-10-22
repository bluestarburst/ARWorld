using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class filter : MonoBehaviour
{
    public float radius = 0.4f;
    private float radius2 = 0.01f;
    public GameObject camera;
    public CamFilter innerFilter;

    // public bool isVisible = false;
    public Color color = Color.black;
    public float saturation = 1.0f;
    public float contrast = 1.0f;
    public float hue = 0.0f;
    public float threshold = 0.5f;
    public bool isColor = false;

    private Material material;

    private bool outside = false;

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
        material.SetFloat("_Contrast", contrast);
        material.SetFloat("_Hue", hue);

        // GetComponent<Renderer>().enabled = isVisible;
    }

    // Update is called once per frame
    void Update()
    {
        if (innerFilter == null || camera == null || innerFilter.isOverride)
        {
            return;
        }

        // get the material component of this object
        material = GetComponent<Renderer>().material;
        // set the shader property
        material.SetColor("_Color", color);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_IsColor", isColor ? 1f : 0f);
        material.SetFloat("_Contrast", contrast);
        material.SetFloat("_Hue", hue);

        if (Vector3.Distance(camera.transform.position, transform.position) < (transform.localScale.x / 2 + radius2) + radius)
        {
            outside = false;
            innerFilter.setVisible(true);
            innerFilter.color = color;
            innerFilter.saturation = saturation;
            innerFilter.threshold = threshold;
            innerFilter.isColor = isColor;
            innerFilter.contrast = contrast;
            innerFilter.hue = hue;

            if (Vector3.Distance(camera.transform.position, transform.position) < (transform.localScale.x / 2 + radius2))
                innerFilter.opacity = 1.0f;
            else
                innerFilter.opacity = 1.0f - (Vector3.Distance(camera.transform.position, transform.position) - (transform.localScale.x / 2 + radius2)) / radius;

            // innerFilter.opacity = 1.0f - (Vector3.Distance(camera.transform.position, transform.position) - transform.localScale.x/2) / radius;
        }
        else
        {
            if (!outside)
            {
                innerFilter.setVisible(false);
                outside = true;
            }
        }

    }
}
