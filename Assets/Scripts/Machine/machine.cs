using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class machine : MonoBehaviour
{
    private int cantidadrequerida = 4;
    public GameObject residuoproscesado;
    public Belt primeraCinta;
    private int contador = 0;
    public economymanager manager;

    //Lo que paga cada residuo que entra y se destruye en la maquina
    public int dineroPorResiduo = 100;

    void Awake()
    {
        //Se puede dejar vacio en el Inspector: las maquinas construidas en runtime salen de
        //un prefab, y un prefab no puede guardar una referencia a un objeto de la escena
        if (manager == null)
            manager = FindObjectOfType<economymanager>();
    }

    //Recibe la basura en la maquina y suma uno al contador
    public void ReceiveTrash(Trash beltItem)
    {
        ReceiveTrash(beltItem.gameObject);
    }

    //Version que acepta cualquier objeto, asi paga aunque el objeto no tenga el componente Trash
    public void ReceiveTrash(GameObject objeto)
    {
        contador+=1;
        Debug.Log("llego bien");
        Destroy(objeto);

        //Un pago por cada residuo destruido
        Pagar();

        if (contador >= cantidadrequerida){
            contador = 0;
            generarbasuraproscesada();
        }
    }

    //Suma la plata del residuo. Si no hay economymanager no rompe, solo avisa
    private void Pagar()
    {
        if (manager == null)
        {
            Debug.LogWarning(name + ": no hay economymanager en la escena, no se paga el residuo");
            return;
        }

        manager.Ganar(dineroPorResiduo);
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
