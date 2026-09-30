using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moneymachine : MonoBehaviour
{
    public economymanager manager;

    //Lo que paga cada objeto que entra a la maquina
    public int dineroPorObjeto = 100;

    void Awake()
    {
        //Las maquinas construidas en runtime salen de un prefab, que no puede guardar
        //una referencia a un objeto de la escena: hay que buscarlo aca
        if (manager == null)
            manager = FindObjectOfType<economymanager>();
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.CompareTag("destroyable"))
        {
            RecibirObjeto(col.gameObject);
        };
    }

    //La cinta llama esto cuando el objeto llega a la maquina: paga y lo destruye
    public void RecibirObjeto(GameObject objeto)
    {
        if (objeto == null)
            return;

        Destroy(objeto);

        if (manager == null)
        {
            Debug.LogWarning(name + ": no hay economymanager en la escena, no se paga el objeto");
            return;
        }

        manager.Ganar(dineroPorObjeto);
    }
}
