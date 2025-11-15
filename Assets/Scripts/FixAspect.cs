using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixAspect : MonoBehaviour
{
    float targetAspect = 9f /16f;
    float windowAspect;
    float previusAspect;
    float scaleHeight;
    float scaleWidth;
    Camera camera;
    

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        
    }

    // Update is called once per frame
    void Update()
    {
        windowAspect = (float)Screen.width / (float)Screen.height;
        if (previusAspect != windowAspect)
            Adjust();


    }

    void Adjust() {
        scaleHeight = windowAspect / targetAspect;

        if(scaleHeight < 1f) {
            Rect rect = camera.rect;

            rect.width = 1f;
            rect.height =  scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) /2f;

            camera.rect = rect;
        }
        else {
            scaleWidth = 1f/ scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) /2f;
            rect.y = 0f;

            camera.rect = rect;
        }

        previusAspect = windowAspect;
        Debug.Log("ADJUSTED");
    }
}
