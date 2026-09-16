using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{
    public Camera topdown;
    public Camera jugador;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            cambiodecamara();
        }
    }

    public void cambiodecamara()
    {
        bool topdownactiva = topdown.enabled;

        topdown.enabled = !topdownactiva;
        jugador.enabled = topdownactiva;
    }

}
