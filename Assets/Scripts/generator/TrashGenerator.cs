using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashGenerator : MonoBehaviour

{
    public GameObject Prefabtrash;
    private float temporizador;
    public float intervalo = 2f;
    public Belt primeraCinta;       



    void update()
    {
        temporizador+= Time.deltaTime;
        if (temporizador >= intervalo)
        {
            temporizador = 0f;
            GenerateTrash()
        }

       void GenerateTrash()
       {


       }

    }




}


