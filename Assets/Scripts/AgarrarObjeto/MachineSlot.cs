using UnityEngine;

public class MachineSlot : MonoBehaviour
{
    public string acceptedItemId = "";
    public GameObject outputPrefab;
    public Transform outputPoint;
    public float processTime = 1.5f;
    private bool isProcessing;

    //Funcion para poner el objeto en la maquina
    public bool TryInsert(Grabbable item)
    {
        if (item == null || isProcessing) return false;
        if (!string.IsNullOrEmpty(acceptedItemId) && item.itemId != acceptedItemId) return false;

        item.OnConsumedByMachine();
        StartCoroutine(ProcessRoutine());
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
        if (outputPrefab == null || outputPoint == null) return;

        GameObject result = Instantiate(outputPrefab, outputPoint.position, outputPoint.rotation);
        if (result.GetComponent<Grabbable>() == null)
            result.AddComponent<Grabbable>();
    }
}
