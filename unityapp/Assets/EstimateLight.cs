using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class EstimateLight : MonoBehaviour
{
    public ARCameraManager arcamman;
    Light our_light;
    void OnEnable()
    {
        arcamman.frameReceived += getlight;
    }

    void OnDisable()
    {
        arcamman.frameReceived -= getlight;
    }
    // Start is called before the first frame update
    void Start()
    {
        our_light = GetComponent<Light>();

    }

    void getlight(ARCameraFrameEventArgs args)
    {
        if (args.lightEstimation.mainLightColor.HasValue)
        {
            //brightness.text=$"Color_value:{args.lightEstimation.mainLightColor.Value}";
            our_light.color = args.lightEstimation.mainLightColor.Value;
            float average_brightness = 0.2126f * our_light.color.r + 0.7152f * our_light.color.g + 0.0722f * our_light.color.b;
        }
    }

}