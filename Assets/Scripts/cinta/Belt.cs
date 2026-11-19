using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Belt : MonoBehaviour
{
    private static int _beltID = 0;

    //Compartido por todas las cintas: se busca una sola vez por escena
    private static BeltManager _sharedBeltManager;

    public Belt beltInSequence;
    public BeltItem beltItem;
    public machine MachineInSequence;
    public moneymachine moneyMachineInSequence;
    public MachineSlot machineSlotInSequence;
    public bool isSpaceTaken;
    private bool isMoving = false;
    private BeltManager _beltManager;
    public LayerMask capasconectadas;
    public float radiodetection = 5f;
    private bool jugadorcerca = false;
    public Button cartelmejora;
    public float distanciacerca = 2f;
    public economymanager manager;
    public PlayerGrabber jugador;
    public float costomejora = 50f;
    private const float MULTIPLICADOR_MEJORA = 2.1f;
    public TextMeshProUGUI txtcostomejora;
    public Transform puntoItem;
    public bool autoConectar = true;
    public Transform puntoSalida;
    public Transform puntoEntrada;
    public bool debugRecorrido = false;
    private const float DURACION_DEBUG_RECORRIDO = 4f;
    private GameObject itemKinematico;
    public float alturaRayo = 0.5f;
    public float distanciaRayo = 7f;
    public float intervaloRedSeguridad = 2f;
    public bool debugConexiones = false;
    public float angulorayo = 90f;
    private Belt destinoReservado;
    private bool recalculoPendiente;
    private bool avisoCapasVacias;
    private bool avisoSinGrabbable;
    private float proximoChequeo;
    private bool avisosinpuntoitem;

    //Velocidad de la cinta. Si por lo que sea no hay BeltManager no rompe ni devuelve 0
    private float Velocidad
    {
        get { return _beltManager != null ? _beltManager.speed : 2f; }
    }

    private void OnEnable()
    {
        BuildManager.OnConstruccionCambiada += PedirRecalculo;
        BeltManager.OnPrecioMejoraCambiado += RefrescarPrecio;
        PedirRecalculo();
    }

    private void OnDisable()
    {
        BuildManager.OnConstruccionCambiada -= PedirRecalculo;
        BeltManager.OnPrecioMejoraCambiado -= RefrescarPrecio;
        StopAllCoroutines();
        recalculoPendiente = false;
        isMoving = false;
        LiberarReserva();
    }

    private void Start()
    {
        _beltManager = ObtenerBeltManager();
        gameObject.name = $"Belt: {_beltID++}";

      

        //Las cintas colocadas en runtime salen de un prefab, y un prefab no puede guardar
        //referencias a objetos de la escena: hay que resolverlas aca
        if (manager == null) manager = FindObjectOfType<economymanager>();
        if (jugador == null) jugador = FindObjectOfType<PlayerGrabber>();
        RefrescarPrecio();
        if (cartelmejora != null) cartelmejora.onClick.AddListener(mejorar);

        //Si en el Inspector se arrastro un prefab (asset) en vez de un objeto de la escena,
        //la cinta intentaria destruir el asset. Un objeto de la escena tiene scene valida, un prefab no
        if (beltItem != null && !beltItem.gameObject.scene.IsValid())
        {
            Debug.LogWarning(name + ": beltItem apunta a un prefab, no a un objeto de la escena. Se ignora", this);
            beltItem = null;
        }

        detectarbelt();
        proximoChequeo = Time.time + Random.Range(0f, Mathf.Max(0f, intervaloRedSeguridad));
    }

    //Busca el BeltManager de la escena. Si no hay ninguno lo crea, porque sin el
    //las cintas no se pueden mover y se quedan trabadas para siempre
    private static BeltManager ObtenerBeltManager()
    {
        if (_sharedBeltManager != null)
            return _sharedBeltManager;

        _sharedBeltManager = FindObjectOfType<BeltManager>();

        if (_sharedBeltManager == null)
        {
            GameObject go = new GameObject("BeltManager (auto)");
            _sharedBeltManager = go.AddComponent<BeltManager>();
            Debug.LogWarning("No habia BeltManager en la escena, se creo uno con speed = " + _sharedBeltManager.speed);
        }

        return _sharedBeltManager;
    }

    //Controla cuando la cinta empieza a mover un objeto y cuando no
    private void Update()
    {
        detectar();

        if (autoConectar && intervaloRedSeguridad > 0f && Time.time >= proximoChequeo)
        {
            proximoChequeo = Time.time + intervaloRedSeguridad;
            detectarbelt();
        }

        if (beltItem != null && beltItem.item != null && !isMoving)
        {
            StartCoroutine(StartBeltMove());
        }
    }

    //Detecta si el jugador esta cerca (y sin nada agarrado) para mostrar el cartel de mejora
    public void detectar()
    {
        if (jugador == null)
            return;

        float distanciaJugador;
        jugadorcerca = MaquinaUtil.JugadorCerca(transform.position, radiodetection, distanciacerca, jugador, out distanciaJugador);

        if (cartelmejora != null)
            cartelmejora.gameObject.SetActive(jugadorcerca);

        if (jugadorcerca && !BuildManager.BuildModeActivo && Input.GetKeyDown(KeyCode.Q))
            BeltManager.ProponerMejora(this, distanciaJugador);
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

        Object anterior = ObjetivoActual();

        beltInSequence = null;
        machineSlotInSequence = null;
        MachineInSequence = null;
        moneyMachineInSequence = null;

        ResultadoConexion r = DetectarSalida();
        beltInSequence = r.belt;
        machineSlotInSequence = r.machineSlot;
        MachineInSequence = r.maquina;
        moneyMachineInSequence = r.moneyMachine;

        DetectorConexion.LogCambio(this, anterior, ObjetivoActual(), debugConexiones);
    }

    private ResultadoConexion DetectarSalida()
    {
        Vector3 inicio, direccion;
        DetectorConexion.CalcularRayo(transform, puntoSalida, alturaRayo, out inicio, out direccion);
        direccion = Quaternion.AngleAxis(angulorayo, transform.up) * direccion;
        return DetectorConexion.Detectar(this, inicio, direccion, distanciaRayo, capasconectadas, ref avisoCapasVacias);
    }

    private Object ObjetivoActual()
    {
        if (!ReferenceEquals(beltInSequence, null)) return beltInSequence;
        if (!ReferenceEquals(moneyMachineInSequence, null)) return moneyMachineInSequence;
        if (!ReferenceEquals(machineSlotInSequence, null)) return machineSlotInSequence;
        if (!ReferenceEquals(MachineInSequence, null)) return MachineInSequence;
        return null;
    }

    private void LiberarReserva()
    {
        if (destinoReservado != null && destinoReservado.beltItem == null)
            destinoReservado.isSpaceTaken = false;

        destinoReservado = null;
    }

    private void OnDrawGizmos()
    {
        Vector3 inicio, direccion;
        DetectorConexion.CalcularRayo(transform, puntoSalida, alturaRayo, out inicio, out direccion);
        direccion = Quaternion.AngleAxis(angulorayo, transform.up) * direccion;
        ResultadoConexion r = DetectorConexion.Detectar(this, inicio, direccion, distanciaRayo, capasconectadas, ref avisoCapasVacias);
        DetectorConexion.DibujarRayo(inicio, direccion, distanciaRayo, r, r.EsValido);
    }

    public void mejorar()
    {
        if (manager == null)
        {
            Debug.LogWarning(name + ": no hay economymanager, no se puede comprar la mejora");
            return;
        }

        if (_beltManager == null)
            _beltManager = ObtenerBeltManager();

        _beltManager.ComprarMejora(manager, costomejora, MULTIPLICADOR_MEJORA, 1f / 3f);
    }

    private void RefrescarPrecio()
    {
        if (txtcostomejora == null || _beltManager == null)
            return;

        txtcostomejora.text = _beltManager.PrecioMejora(costomejora, MULTIPLICADOR_MEJORA).ToString();
    }

    //Le da la posicion en la cinta al objeto salido de la maquina
    public Vector3 GetItemPosition()
    {
        if(puntoItem != null)
            return puntoItem.position;
        
        if(!avisosinpuntoitem)
        {
            avisosinpuntoitem = true;
            Debug.LogError(name + ": no hay puntoItem asignado, se devuelve la posicion de la cinta", this);
        }

        return transform.position;


    }

    public Vector3 GetItemPosition(GameObject item)
    {
        return GetItemPosition() + Vector3.up * OffsetBase(item);
    }

    public static float OffsetBase(GameObject item)
    {
        if (item == null)
            return 0f;

        Renderer[] renderers = item.GetComponentsInChildren<Renderer>();
        bool hayBounds = false;
        Bounds bounds = new Bounds();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
                continue;

            if (!hayBounds)
            {
                bounds = r.bounds;
                hayBounds = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        if (!hayBounds)
            return 0f;

        return item.transform.position.y - bounds.min.y;
    }

    public static void FijarKinematico(GameObject item)
    {
        if (item == null)
            return;

        Rigidbody[] cuerpos = item.GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < cuerpos.Length; i++)
        {
            Rigidbody rb = cuerpos[i];
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            rb.useGravity = false;
        }
    }

    //Hace que la cinta empiece el movimiento de objetos
    private IEnumerator StartBeltMove()
    {
        isMoving = true;

        //El finally es lo que evita que la cinta se trabe: si algo tira una excepcion
        //adentro (por ejemplo la maquina al recibir la basura), isMoving quedaba en true
        //para siempre y esta cinta no volvia a arrancar nunca mas
        try
        {
            if (beltItem == null || beltItem.item == null)
                yield break;

            BeltItem itemActual = beltItem;
            GameObject item = beltItem.item;

            //Nunca tocar un prefab (asset): solo objetos que estan en la escena
            if (!item.scene.IsValid())
            {
                beltItem = null;
                yield break;
            }

            if (item != itemKinematico)
            {
                FijarKinematico(item);
                itemKinematico = item;
            }

            Belt destinoBelt = beltInSequence;
            moneymachine destinoMoney = moneyMachineInSequence;
            MachineSlot destinoSlot = machineSlotInSequence;
            machine destinoMaquina = MachineInSequence;

            if (destinoBelt != null)
            {
                if (destinoBelt.isSpaceTaken)
                    yield break;

                destinoBelt.isSpaceTaken = true;
                destinoReservado = destinoBelt;

                Vector3 desfase = Vector3.up * OffsetBase(item);

                Transform[] puntos = null;
                if (puntoItem != null && puntoSalida != null && destinoBelt.puntoEntrada != null)
                    puntos = new Transform[] { puntoItem, puntoSalida, destinoBelt.puntoEntrada };

                int total = puntos != null ? puntos.Length : 0;
                int paso = 0;

                while (item != null && destinoBelt != null)
                {
                    Vector3 objetivo;

                    if (paso < total)
                    {
                        if (puntos[paso] == null)
                        {
                            paso++;
                            continue;
                        }

                        objetivo = puntos[paso].position + desfase;
                    }
                    else
                    {
                        objetivo = destinoBelt.GetItemPosition() + desfase;
                    }

                    Vector3 posicionActual = item.transform.position;

                    if (posicionActual == objetivo)
                    {
                        if (paso >= total)
                            break;

                        paso++;
                        continue;
                    }

                    Vector3 posicionNueva = Vector3.MoveTowards(posicionActual, objetivo, Velocidad * Time.deltaTime);
                    item.transform.position = posicionNueva;

                    if (debugRecorrido)
                        Debug.DrawLine(posicionActual, posicionNueva, Color.cyan, DURACION_DEBUG_RECORRIDO);

                    yield return null;
                }

                destinoReservado = null;

                if (item == null)
                {
                    beltItem = null;
                    isSpaceTaken = false;
                    if (destinoBelt != null && destinoBelt.beltItem == null)
                        destinoBelt.isSpaceTaken = false;
                    yield break;
                }

                if (destinoBelt == null)
                    yield break;

                isSpaceTaken = false;
                destinoBelt.beltItem = itemActual;
                beltItem = null;
            }
            else if (destinoMoney != null || destinoSlot != null || destinoMaquina != null)
            {
                Grabbable grabbable = null;

                if (destinoMoney == null && destinoSlot != null)
                {
                    grabbable = item.GetComponent<Grabbable>();
                    if (grabbable == null)
                    {
                        if (!avisoSinGrabbable)
                        {
                            avisoSinGrabbable = true;
                            Debug.LogWarning(name + ": el item " + item.name + " no tiene Grabbable, no se puede meter en " + destinoSlot.name, this);
                        }
                        yield break;
                    }

                    if (!destinoSlot.PuedeAceptar(grabbable))
                        yield break;
                }
                else if (destinoMoney == null && destinoMaquina != null)
                {
                    if (!destinoMaquina.PuedeRecibir())
                        yield break;
                }

                Transform machineTransform = destinoMoney != null
                    ? destinoMoney.transform
                    : destinoSlot != null
                        ? destinoSlot.transform
                        : destinoMaquina.transform;

                Transform puntoDestino = null;
                if (destinoMoney == null && destinoSlot == null && destinoMaquina != null)
                    puntoDestino = destinoMaquina.puntoEntrada;

                Vector3 toPosition = puntoDestino != null ? puntoDestino.position : machineTransform.position;
                

                //Igual que el movimiento hacia otra cinta, pero el destino es la maquina
                while (item != null && machineTransform != null && item.transform.position != toPosition)
                {
                    item.transform.position =
                        Vector3.MoveTowards(item.transform.position, toPosition, Velocidad * Time.deltaTime);
                    yield return null;
                }

                if (item == null)
                {
                    beltItem = null;
                    isSpaceTaken = false;
                    yield break;
                }

                if (machineTransform == null)
                    yield break;

                //Mismo orden que al elegir el destino: la maquina que recibe es la que paga
                bool entregado = true;

                if (destinoMoney != null && itemActual != null)
                    destinoMoney.RecibirObjeto(itemActual);
                else if (destinoMoney != null)
                    destinoMoney.RecibirObjeto(item);
                else if (destinoSlot != null)
                    entregado = destinoSlot.TryInsert(grabbable);
                else
                    entregado = destinoMaquina.ReceiveTrash(item);

                if (entregado)
                {
                    beltItem = null;
                    isSpaceTaken = false;
                }
                else
                {
                    item.transform.position = GetItemPosition(item);
                }
            }
        }
        finally
        {
            isMoving = false;
        }
    }
}
