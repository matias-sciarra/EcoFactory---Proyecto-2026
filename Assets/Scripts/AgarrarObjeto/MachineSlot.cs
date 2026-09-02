using UnityEngine;

public class MachineSlot : MonoBehaviour
{
    public string acceptedItemId = "";
    public GameObject outputPrefab;
    public float processTime = 1.5f;
    private bool isProcessing;
    public int cantidad = 3;
    public int actual = 0;
    public Belt primeraCinta;

    //Funcion para poner el objeto en la maquina
    public bool TryInsert(Grabbable item)
    {
        if (item == null || isProcessing) return false;
        if (!string.IsNullOrEmpty(acceptedItemId) && item.itemId != acceptedItemId) return false;

        item.OnConsumedByMachine();
        actual++;
        if(actual >= cantidad){
            actual = 0;
            StartCoroutine(ProcessRoutine());
        }

        return true;
    }

    //Proceso de transformacion de objeto mas tiempo de proceso
    private System.Collections.IEnumerator ProcessRoutine()
    {
        isProcessing = true;
        yield return new WaitForSeconds(processTime);

        SpawnOutput();
        isProcessing = false;
    }

    //Funcion de spawn del objeto procesado
    private void SpawnOutput()
    {
        if (outputPrefab == null || primeraCinta == null) return;
        Vector3 position = primeraCinta.GetItemPosition();
        Quaternion rotacion = Quaternion.identity;
        GameObject nueva = Instantiate(outputPrefab, position, rotacion);

        BeltItem itemcomponent = nueva.GetComponent<BeltItem>();
        primeraCinta.beltItem = itemcomponent;
    }
}