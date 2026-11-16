using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class machine : MonoBehaviour
{
    private int cantidadrequerida = 4;
    public GameObject residuoproscesado;
    public Belt primeraCinta;
    private int contador = 0;
    public economymanager manager;

    public int dineroPorResiduo = 100;

    public LayerMask capasconectadas;
    public float distanciamax = 7f;
    public float alturaRayo = 0f;
    public Transform puntoSalida;
    public bool autoConectar = true;
    public float intervaloRedSeguridad = 2f;
    public bool debugConexiones = false;
    public float angulorayo = 90f;

    public Button cartelmejora;
    public TextMeshProUGUI txtcostomejora;
    public float costomejora = 300f;
    public float aumentoVelocidadPorMejora = 0.25f;
    private const float MULTIPLICADOR_MEJORA = 1.6f;
    private int cantidadMejoras = 0;

    public float radiodetection = 5f;
    public float distanciacerca = 2f;
    private bool jugadorcerca = false;
    public PlayerGrabber jugador;

    public Animator animator;
    public AnimationClip animacion;
    public string estadoAnimacion = "Procesar";
    public string estadoReposo = "Idle";
    public float tiempoSinAnimacion = 0.5f;

    private bool ocupado;
    private bool salidaPendiente;
    private Coroutine rutinaProceso;
    private bool recalculoPendiente;
    private bool avisoCapasVacias;
    private float proximoChequeo;
    private bool avisoEstadoFaltante;
    private bool avisoReposoFaltante;
    private bool avisoSinCinta;

    public float VelocidadAnimacion
    {
        get { return Mathf.Max(0.01f, 1f + aumentoVelocidadPorMejora * cantidadMejoras); }
    }

    void Awake()
    {
        if (manager == null) manager = FindObjectOfType<economymanager>();
        if (jugador == null) jugador = FindObjectOfType<PlayerGrabber>();

        if (manager == null) Debug.LogError($"[{name}] No se encontró el economymanager", this);
        if (jugador == null) Debug.LogError($"[{name}] No se encontró el PlayerGrabber", this);
    }

    void OnEnable()
    {
        BuildManager.OnConstruccionCambiada += PedirRecalculo;
        PedirRecalculo();

        if (ocupado && rutinaProceso == null)
            rutinaProceso = StartCoroutine(RutinaProceso(false));
    }

    void OnDisable()
    {
        BuildManager.OnConstruccionCambiada -= PedirRecalculo;
        recalculoPendiente = false;
        rutinaProceso = null;
    }

    void Start()
    {
        MaquinaUtil.MostrarCosto(txtcostomejora, costomejora, MULTIPLICADOR_MEJORA, cantidadMejoras);
        if (cartelmejora != null) cartelmejora.onClick.AddListener(mejorar);

        if (animator != null)
        {
            ValidarAnimator();
            MaquinaUtil.VolverAReposo(animator, estadoReposo);
        }

        detectarbelt();
        proximoChequeo = Time.time + Random.Range(0f, Mathf.Max(0f, intervaloRedSeguridad));
    }

    private void ValidarAnimator()
    {
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[{name}] El Animator no tiene Animator Controller asignado, la máquina procesa sin animación", this);
            avisoEstadoFaltante = true;
            avisoReposoFaltante = true;
            return;
        }

        if (!MaquinaUtil.TieneEstado(animator, estadoAnimacion))
            MaquinaUtil.AvisarEstadoFaltante(this, estadoAnimacion, ref avisoEstadoFaltante);

        if (!MaquinaUtil.TieneEstado(animator, estadoReposo))
            MaquinaUtil.AvisarEstadoFaltante(this, estadoReposo, ref avisoReposoFaltante);

        if (animacion == null)
            Debug.LogWarning($"[{name}] Hay Animator pero no AnimationClip asignado, se usa tiempoSinAnimacion ({tiempoSinAnimacion}s) por residuo", this);
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

    public bool PuedeRecibir()
    {
        return isActiveAndEnabled && !ocupado;
    }

    public bool ReceiveTrash(Trash beltItem)
    {
        return beltItem != null && ReceiveTrash(beltItem.gameObject);
    }

    public bool ReceiveTrash(GameObject objeto)
    {
        if (objeto == null || !PuedeRecibir())
            return false;

        ocupado = true;
        contador += 1;
        Destroy(objeto);

        Pagar();

        if (contador >= cantidadrequerida)
        {
            contador = 0;
            salidaPendiente = true;
        }

        rutinaProceso = StartCoroutine(RutinaProceso(true));
        return true;
    }

    private void Pagar()
    {
        if (manager == null)
        {
            Debug.LogWarning(name + ": no hay economymanager en la escena, no se paga el residuo");
            return;
        }

        manager.Ganar(dineroPorResiduo);
    }

    private IEnumerator RutinaProceso(bool animar)
    {
        if (animar)
        {
            float duracion = IniciarAnimacion();
            if (duracion > 0f)
                yield return new WaitForSeconds(duracion);
        }

        if (animator != null)
            MaquinaUtil.VolverAReposo(animator, estadoReposo);

        while (salidaPendiente)
        {
            detectarbelt();
            if (MaquinaUtil.CintaLibre(primeraCinta))
            {
                generarbasuraproscesada();
                break;
            }

            if (primeraCinta == null && !avisoSinCinta)
            {
                avisoSinCinta = true;
                Debug.LogWarning($"[{name}] Residuo procesado listo pero no hay cinta de salida conectada, la máquina espera", this);
            }

            yield return null;
        }

        ocupado = false;
        rutinaProceso = null;
    }

    private float IniciarAnimacion()
    {
        float velocidad = VelocidadAnimacion;

        if (animator == null || animacion == null)
            return tiempoSinAnimacion / velocidad;

        MaquinaUtil.Reproducir(this, animator, estadoAnimacion, velocidad, ref avisoEstadoFaltante);
        return animacion.length / velocidad;
    }

    public void generarbasuraproscesada()
    {
        if (!MaquinaUtil.CintaLibre(primeraCinta))
        {
            Debug.LogWarning($"[{name}] La cinta de salida no está libre, no se suelta el residuo procesado", this);
            return;
        }

        MaquinaUtil.SoltarEnCinta(this, residuoproscesado, "residuoproscesado", primeraCinta);
        salidaPendiente = false;
        avisoSinCinta = false;
    }

    public void detectar()
    {
        if (jugador == null) return;

        jugadorcerca = MaquinaUtil.JugadorCerca(transform.position, radiodetection, distanciacerca, jugador);
        MaquinaUtil.MostrarCartel(cartelmejora, jugadorcerca);

        if (MaquinaUtil.PidioMejora(jugadorcerca))
            mejorar();
    }

    public void mejorar()
    {
        MaquinaUtil.ComprarMejora(this, manager, costomejora, MULTIPLICADOR_MEJORA, ref cantidadMejoras, txtcostomejora);
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
}
