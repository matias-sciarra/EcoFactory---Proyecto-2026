using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashGenerator : MonoBehaviour

{
    public GameObject Prefabtrash;
    private float temporizador;
    public float intervalo = 2f;
    public Belt primeraCinta;       



    void Update()
    {
        temporizador+= Time.deltaTime;
        if (temporizador >= intervalo)
        {
            temporizador = 0f;
        
            GenerateTrash();
        }

    }
    void GenerateTrash()
    {
        Vector3 position = primeraCinta.GetItemPosition();
        Quaternion rotation = Quaternion.identity;
        GameObject nueva = Instantiate( Prefabtrash, position, rotation );
        Debug.Log("se genero");

        BeltItem itemcomponent = nueva.GetComponent<BeltItem>();
        primeraCinta.beltItem = itemcomponent;

    }




}


