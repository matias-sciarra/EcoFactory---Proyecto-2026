using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class machine : MonoBehaviour
{
    private int canridadrequerida = 4;
    public GameObject residuoproscesado;
    public Belt primeraCinta;
    private int contador = 0;



    public void ReceiveTrash(Trash Basura)
    {
        contador+=1;
        Debug.Log("llego bien");
        Destroy(Basura.gameObject);
        if (contador >= canridadrequerida){
            contador = 0;
            generarbasuraproscesada();
        }
    }

    public void generarbasuraproscesada()
    {
        Vector3 position = primeraCinta.GetItemPosition();
        Quaternion rotacion = Quaternion.identity; 
        GameObject nueva = Instantiate(residuoproscesado, position, rotacion);
        Debug.Log("produciodo");

        BeltItem itemcomponent = nueva.GetComponent<BeltItem>();
        primeraCinta.beltItem = itemcomponent;

        Debug.Log("en la cinta");
    }
}
