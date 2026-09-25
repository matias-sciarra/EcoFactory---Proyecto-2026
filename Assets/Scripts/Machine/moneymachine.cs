using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moneymachine : MonoBehaviour
{
    public economymanager manager;

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.CompareTag("destroyable"))
        {
            Destroy(col.gameObject);
            manager.Ganar(100);
        };
    }
}
