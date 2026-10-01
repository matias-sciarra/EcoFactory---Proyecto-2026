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
    public float alturaRayo = 0;
    public Transform puntoSalida;
    public bool autoConectar = true;
    public float intervaloRedSeguridad = 2f;
    public bool debugConexiones = false;
    public float angulorayo = 90f;


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

    private bool recalculoPendiente;
    private bool avisoCapasVacias;
    private float proximoChequeo;

    void OnEnable()
    {
        BuildManager.OnConstruccionCambiada += PedirRecalculo;
        PedirRecalculo();
    }

    void OnDisable()
    {
        BuildManager.OnConstruccionCambiada -= PedirRecalculo;
        recalculoPendiente = false;
    }

    void Start()
    {
        if (manager == null) manager = FindObjectOfType<economymanager>();
        if (jugador == null) jugador = FindObjectOfType<PlayerGrabber>();

        if (manager == null) Debug.LogError($"[{name}] No se encontró el economymanager", this);
        if (jugador == null) Debug.LogError($"[{name}] No se encontró el PlayerGrabber", this);

        if (txtcostomejora != null) txtcostomejora.text = GetCostoMejora(cantidadMejoras).ToString();
        if (cartelmejora != null) cartelmejora.onClick.AddListener(mejorar);

        detectarbelt();
        proximoChequeo = Time.time + Random.Range(0f, Mathf.Max(0f, intervaloRedSeguridad));
    }

    void Update()
    {
        detectar();

        if (autoConectar && intervaloRedSeguridad > 0f && Time.time >= proximoChequeo)
        {
            proximoChequeo = Time.time + intervaloRedSeguridad;
            detectarbelt();
        }
    }

    public bool PuedeAceptar(Grabbable item)
    {
        return MotivoRechazo(item) == null;
    }

    private string MotivoRechazo(Grabbable item)
    {
        if (item == null)
            return "item es null";
        if (isProcessing)
            return "la máquina está procesando (isProcessing = true)";
        if (!string.IsNullOrEmpty(acceptedItemId) && item.itemId != acceptedItemId)
            return $"itemId '{item.itemId}' no coincide con acceptedItemId '{acceptedItemId}'";
        return null;
    }

    public bool TryInsert(Grabbable item)
    {
        string motivo = MotivoRechazo(item);
        if (motivo != null)
        {
            Debug.Log("TryInsert: rechazado, " + motivo);
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
        while (true)
        {
            if (CintaLibre())
            {
                detectarbelt();
                if (CintaLibre())
                    break;
            }
            yield return null;
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

    private void PedirRecalculo()
    {
        if (recalculoPendiente || !isActiveAndEnabled)
            return;

        recalculoPendiente = true;
        StartCoroutine(RecalcularFrameSiguiente());
    }

    private IEnumerator RecalcularFrameSiguiente()
    {
        yield return null;
        recalculoPendiente = false;
        detectarbelt();
    }

    public void detectarbelt()
    {
        if (!autoConectar)
            return;

        Belt anterior = primeraCinta;
        primeraCinta = null;

        primeraCinta = DetectarSalida().belt;

        DetectorConexion.LogCambio(this, anterior, primeraCinta, debugConexiones);
    }

    private ResultadoConexion DetectarSalida()
    {
        Vector3 inicio, direccion;
        DetectorConexion.CalcularRayo(transform, puntoSalida, alturaRayo, out inicio, out direccion);
        direccion = Quaternion.AngleAxis(angulorayo, transform.up) * direccion;
        return DetectorConexion.Detectar(this, inicio, direccion, distanciamax, capasconectadas, ref avisoCapasVacias);
    }

    private void OnDrawGizmos()
    {
        Vector3 inicio, direccion;
        DetectorConexion.CalcularRayo(transform, puntoSalida, alturaRayo, out inicio, out direccion);
        ResultadoConexion r = DetectorConexion.Detectar(this, inicio, direccion, distanciamax, capasconectadas, ref avisoCapasVacias);
        direccion = Quaternion.AngleAxis(angulorayo, transform.up) * direccion;
        DetectorConexion.DibujarRayo(inicio, direccion, distanciamax, r, r.belt != null);
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