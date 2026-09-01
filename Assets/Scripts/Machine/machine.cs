using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class machine : MonoBehaviour
{
    private int cantidadrequerida = 4;
    public GameObject residuoproscesado;
    public Belt primeraCinta;
    private int contador = 0;


    //Recibe la basura en la maquina y suma uno al contador
    public void ReceiveTrash(Trash Basura)
    {
        contador+=1;
        Debug.Log("llego bien");
        Destroy(Basura.gameObject);
        if (contador >= cantidadrequerida){
            contador = 0;
            generarbasuraproscesada();
        }
    }

    //Cuando la maquina detecta que entro un objeto de basura, lo procesa y lo saca
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
