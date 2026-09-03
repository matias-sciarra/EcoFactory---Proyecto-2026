using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moneymachine : MonoBehaviour
{
    public moneymanager manager;

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.CompareTag("destroyable"))
        {
            Destroy(col.gameObject);
            manager.dinero += 100;
            manager.txtdinero.text = manager.dinero.ToString();
        };
    }
}
