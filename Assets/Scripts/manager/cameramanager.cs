using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameramanager : MonoBehaviour
{
    public Camera fpscamera;
    public Camera topdown;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            cambiarcamara();
        }
    }
    
    void cambiarcamara()
    {
    bool topDownActiva = topdown.enabled;

    topdown.enabled = !topDownActiva;
    fpscamera.enabled = topDownActiva;
    }
}
