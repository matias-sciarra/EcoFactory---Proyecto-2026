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

    [Header("Animations")]
    public Animator animator;
    public AnimationClip animacion;
    public string estadoAnimacion = "Procesar";
    public string estadoReposo = "Idle";
    private bool avisoEstadoFaltante;

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

        MaquinaUtil.MostrarCosto(txtcostomejora, costomejora, MULTIPLICADOR_MEJORA, cantidadMejoras);
        if (cartelmejora != null) cartelmejora.onClick.AddListener(mejorar);

        if (animator != null)
            MaquinaUtil.VolverAReposo(animator, estadoReposo);

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
        reproduciranimacion();
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

        while (true)
        {
            detectarbelt();
            if (MaquinaUtil.CintaLibre(primeraCinta))
                break;
            yield return null;
        }

        MaquinaUtil.SoltarEnCinta(this, outputPrefab, "outputPrefab", primeraCinta);
        isProcessing = false;
    }

    public void detectar()
    {
        if (jugador == null) return;

        jugadorcerca = MaquinaUtil.JugadorCerca(transform.position, radiodetection, distanciacerca, jugador);
        MaquinaUtil.MostrarCartel(cartelmejora, jugadorcerca);

        if (MaquinaUtil.PidioMejora(jugadorcerca))
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
        primeraCinta = MaquinaUtil.DetectarSalida(this, puntoSalida, alturaRayo, angulorayo, distanciamax, capasconectadas, ref avisoCapasVacias).belt;

        DetectorConexion.LogCambio(this, anterior, primeraCinta, debugConexiones);
    }

    private void OnDrawGizmos()
    {
        MaquinaUtil.DibujarSalida(this, puntoSalida, alturaRayo, angulorayo, distanciamax, capasconectadas, ref avisoCapasVacias);
    }

    public void mejorar()
    {
        if (MaquinaUtil.ComprarMejora(this, manager, costomejora, MULTIPLICADOR_MEJORA, ref cantidadMejoras, txtcostomejora))
            processTime = Mathf.Max(0f, processTime - 1f / 3f);
    }

    public void reproduciranimacion()
    {
        MaquinaUtil.Reproducir(this, animator, estadoAnimacion, 1f, ref avisoEstadoFaltante);
    }
}
