using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MachineSlot : MonoBehaviour
{
    public string acceptedItemId = "";
    public GameObject outputPrefab;
    public float processTime = 1.5f;
    public int cantidad = 3;
    public int actual = 0;
    private bool isProcessing;

    public Belt primeraCinta;
    public LayerMask capasconectadas;
    public float distanciamax = 123f;
    public float alturaRayo = 0.5f;

    public Button cartelmejora;
    public TextMeshProUGUI txtcostomejora;
    public float costomejora = 300f;
    private const float MULTIPLICADOR_MEJORA = 1.6f;
    private int cantidadMejoras = 0;

    public float radiodetection = 5f;
    public float distanciacerca = 2f;
    private bool jugadorcerca = false;

    public economymanager manager;
    public PlayerGrabber jugador;


    void Start()
    {
        if (manager == null) manager = FindObjectOfType<economymanager>();
        if (jugador == null) jugador = FindObjectOfType<PlayerGrabber>();

        if (manager == null) Debug.LogError($"[{name}] No se encontró el economymanager", this);
        if (jugador == null) Debug.LogError($"[{name}] No se encontró el PlayerGrabber", this);

        if (txtcostomejora != null) txtcostomejora.text = GetCostoMejora(cantidadMejoras).ToString();
        if (cartelmejora != null) cartelmejora.onClick.AddListener(mejorar);
    }

    void Update()
    {
        detectar();
    }

    public bool TryInsert(Grabbable item)
    {
        if (item == null)
        {
            Debug.Log("TryInsert: rechazado, item es null");
            return false;
        }
        if (isProcessing)
        {
            Debug.Log("TryInsert: rechazado, la máquina está procesando (isProcessing = true)");
            return false;
        }
        if (!string.IsNullOrEmpty(acceptedItemId) && item.itemId != acceptedItemId)
        {
            Debug.Log($"TryInsert: rechazado, itemId '{item.itemId}' no coincide con acceptedItemId '{acceptedItemId}'");
            return false;
        }

        item.OnConsumedByMachine();
        actual++;
        if (actual >= cantidad)
        {
            actual = 0;
            StartCoroutine(ProcessRoutine());
        }

        return true;
    }

    private IEnumerator ProcessRoutine()
    {
        isProcessing = true;
        yield return new WaitForSeconds(processTime);

        detectarbelt();
        while (!CintaLibre())
        {
            yield return null;
            detectarbelt();
        }

        SpawnOutput();
        isProcessing = false;
    }

    private bool CintaLibre()
    {
        return primeraCinta != null
            && !primeraCinta.isSpaceTaken
            && primeraCinta.beltItem == null;
    }

    private void SpawnOutput()
    {
        if (outputPrefab == null)
        {
            Debug.LogError($"[{name}] No hay outputPrefab asignado", this);
            return;
        }

        GameObject nueva = Instantiate(outputPrefab, primeraCinta.GetItemPosition(), Quaternion.identity);

        BeltItem itemcomponent = nueva.GetComponent<BeltItem>();
        if (itemcomponent == null)
            Debug.LogError($"[{name}] El outputPrefab no tiene BeltItem, la cinta no lo va a mover", this);

        primeraCinta.beltItem = itemcomponent;
        primeraCinta.isSpaceTaken = true;
    }

    public void detectar()
    {
        if (jugador == null) return;

        bool cerca = false;
        Collider[] objetosdetectados = Physics.OverlapSphere(transform.position, radiodetection);

        foreach (Collider col in objetosdetectados)
        {
            if (col.CompareTag("Player"))
            {
                float distancia = Vector3.Distance(transform.position, col.transform.position);
                if (distancia < distanciacerca && !jugador.IsHolding)
                    cerca = true;
            }
        }

        jugadorcerca = cerca;

        if (cartelmejora != null)
            cartelmejora.gameObject.SetActive(jugadorcerca);

        if (jugadorcerca && !BuildManager.BuildModeActivo && Input.GetKeyDown(KeyCode.Q))
            mejorar();
    }

    public void detectarbelt()
    {

        Vector3 inicio = transform.position + Vector3.up * alturaRayo;
        Vector3 direccion = transform.forward;

        Debug.DrawRay(inicio, direccion * distanciamax, Color.red, 0.5f);

        RaycastHit hit;
        if (Physics.Raycast(inicio, direccion, out hit, distanciamax, capasconectadas))
        {
            primeraCinta = hit.collider.GetComponentInParent<Belt>();
        }
    }

    public void mejorar()
    {
        if (manager == null)
        {
            Debug.LogWarning($"[{name}] No hay economymanager, no se puede comprar la mejora", this);
            return;
        }

        int costoActual = GetCostoMejora(cantidadMejoras);

        if (manager.Gastar(costoActual))
        {
            cantidadMejoras += 1;
            processTime = Mathf.Max(0f, processTime - 1f / 3f);

            if (txtcostomejora != null)
                txtcostomejora.text = GetCostoMejora(cantidadMejoras).ToString();
        }
    }

    private int GetCostoMejora(int mejoras)
    {
        return Mathf.RoundToInt(costomejora * Mathf.Pow(MULTIPLICADOR_MEJORA, mejoras));
    }
}