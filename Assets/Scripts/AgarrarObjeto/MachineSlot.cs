using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MachineSlot : MonoBehaviour
{
    public string acceptedItemId = "";
    public GameObject outputPrefab;
    public float processTime = 1.5f;
    private bool isProcessing;
    public int cantidad = 3;
    public int actual = 0;
    public Belt primeraCinta;
    public float radiodetection = 5f;
    private bool jugadorcerca = false;
    public Button cartelmejora;
    public float distanciacerca = 2f;
    public moneymanager manager;
    public PlayerGrabber jugador;
    public float costomejora = 300f;
    private int cantidadMejoras = 0;
    public TextMeshProUGUI txtcostomejora;


    void Start()
    {
        txtcostomejora.text = costomejora.ToString();
        cartelmejora.onClick.AddListener(mejorar);
    }

    void Update()
    {
        detectar();
    }

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

    public void detectar()
    {
        Vector3 inicio = transform.position;

        Collider[] objetosdetectados = Physics.OverlapSphere(inicio, radiodetection);

        foreach (Collider col in  objetosdetectados)
        {
            if(col.CompareTag("Player"))
            {
                float distancia = Vector3.Distance(transform.position, col.transform.position);
                if(distancia < distanciacerca && !jugador.IsHolding)
                {
                    jugadorcerca = true;
                    cartelmejora.gameObject.SetActive(true);
                }
                else
                {
                    jugadorcerca = false;
                    cartelmejora.gameObject.SetActive(false);
                }

                if(jugadorcerca && Input.GetKeyDown(KeyCode.Q))
                {
                    mejorar();
                }
            }
        };

    }

    public void mejorar()
    {
        float costoActual = costomejora * Mathf.Pow(1.6f, cantidadMejoras);

        if (manager.dinero >= costoActual)
        {
            manager.dinero -= costoActual;
            manager.txtdinero.text = manager.dinero.ToString();
            cantidadMejoras += 1;
            processTime = Mathf.Max(0f, processTime - 1f / 3f);

            float costoSiguiente = costomejora * Mathf.Pow(2.3f, cantidadMejoras);
            txtcostomejora.text = costoSiguiente.ToString();
        }
    }

}