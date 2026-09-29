
using System;
using System.Collections;
using System.Collections.Generic;
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

    public float radiodetection = 5f;
    private bool jugadorcerca = false;
    public Button cartelmejora;
    public float distanciacerca = 2f;
    public economymanager manager;
    public PlayerGrabber jugador;
    public float costomejora = 50f;
    private const float MULTIPLICADOR_MEJORA = 2.1f;
    private int cantidadMejoras = 0;
    public TextMeshProUGUI txtcostomejora;
    public float distanciamaximabelt = 2f;
    public bool maquinadisponible = false;

    //Velocidad de la cinta. Si por lo que sea no hay BeltManager no rompe ni devuelve 0
    private float Velocidad
    {
        get { return _beltManager != null ? _beltManager.speed : 2f; }
    }

    private void Start()
    {
        _beltManager = ObtenerBeltManager();
        gameObject.name = $"Belt: {_beltID++}";

        //Las cintas colocadas en runtime salen de un prefab, y un prefab no puede guardar
        //referencias a objetos de la escena: hay que resolverlas aca
        if (manager == null) manager = FindObjectOfType<economymanager>();
        if (jugador == null) jugador = FindObjectOfType<PlayerGrabber>();
        if ( beltItem == null) beltItem = FindObjectOfType<BeltItem>();
        if (beltInSequence == null && maquinadisponible == true) beltInSequence = FindNextBelt();

        if (txtcostomejora != null) txtcostomejora.text = costomejora.ToString();
        if (cartelmejora != null) cartelmejora.onClick.AddListener(mejorar);
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

        if (beltItem != null && beltItem.item != null && !isMoving)
        {
            StartCoroutine(StartBeltMove());
        }
    }

    //Detecta si el jugador esta cerca (y sin nada agarrado) para mostrar el cartel de mejora
    public void detectar()
    {
        //Sin jugador no hay nada que detectar. Sin este corte, una excepcion aca aborta
        //el Update entero y la cinta nunca llega a mover el objeto
        if (jugador == null)
            return;

        Vector3 inicio = transform.position;

        Collider[] objetosdetectados = Physics.OverlapSphere(inicio, radiodetection);

        foreach (Collider col in objetosdetectados)
        {
            if(col.CompareTag("Player"))
            {
                float distancia = Vector3.Distance(transform.position, col.transform.position);
                jugadorcerca = distancia < distanciacerca && !jugador.IsHolding;

                if (cartelmejora != null)
                    cartelmejora.gameObject.SetActive(jugadorcerca);

                if(jugadorcerca && !BuildManager.BuildModeActivo && Input.GetKeyDown(KeyCode.Q))
                {
                    mejorar();
                }
            }

            if(col.CompareTag("belt"))
            {
                float distanciabelt = Vector3.Distance(transform.position, col.transform.position);

                if(distanciabelt < distanciamaximabelt)
                {
                    maquinadisponible = true;
                }

            }
        };
    }

    //Compra una mejora de velocidad para la cinta (velocidad compartida via BeltManager)
    public void mejorar()
    {
        if (manager == null)
        {
            Debug.LogWarning(name + ": no hay economymanager, no se puede comprar la mejora");
            return;
        }

        int costoActual = GetCostoMejora(cantidadMejoras);

        // Gastar descuenta la plata y refresca el texto del dinero; devuelve false si no alcanza
        if (manager.Gastar(costoActual))
        {
            cantidadMejoras += 1;

            if (_beltManager != null)
                _beltManager.speed += 1f / 3f;

            if (txtcostomejora != null)
                txtcostomejora.text = GetCostoMejora(cantidadMejoras).ToString();
        }
    }

    // Lo que sale la mejora numero "mejoras": costomejora * 2.1 ^ mejoras.
    // Se cobra y se muestra con esta misma cuenta para que no se desfasen.
    private int GetCostoMejora(int mejoras)
    {
        return Mathf.RoundToInt(costomejora * Mathf.Pow(MULTIPLICADOR_MEJORA, mejoras));
    }

    //Le da la posicion en la cinta al objeto salido de la maquina
    public Vector3 GetItemPosition()
    {
        var padding = 0.3f;
        var position = transform.position;
        return new Vector3(position.x, position.y + padding, position.z);
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

            if (beltInSequence != null && beltInSequence.isSpaceTaken == false)
            {
                Vector3 toPosition = beltInSequence.GetItemPosition();
                beltInSequence.isSpaceTaken = true;
                var step = Velocidad * Time.deltaTime;

                while (beltItem.item.transform.position != toPosition)
                {
                    beltItem.item.transform.position =
                        Vector3.MoveTowards(beltItem.item.transform.position, toPosition, step);
                    yield return null;
                }

                isSpaceTaken = false;
                beltInSequence.beltItem = beltItem;
                beltItem = null;
            }
            else if (beltInSequence == null &&
                (moneyMachineInSequence != null || machineSlotInSequence != null || MachineInSequence != null))
            {
                Transform machineTransform = moneyMachineInSequence != null
                    ? moneyMachineInSequence.transform
                    : machineSlotInSequence != null
                        ? machineSlotInSequence.transform
                        : MachineInSequence.transform;

                Vector3 toPosition = machineTransform.position;
                GameObject item = beltItem.item;
                var step = Velocidad * Time.deltaTime;

                //Igual que el movimiento hacia otra cinta, pero el destino es la maquina
                while (item != null && item.transform.position != toPosition)
                {
                    item.transform.position =
                        Vector3.MoveTowards(item.transform.position, toPosition, step);
                    yield return null;
                }

                if (item != null)
                {
                    Trash trash = MachineInSequence != null ? item.GetComponent<Trash>() : null;

                    if (trash != null)
                    {
                        MachineInSequence.ReceiveTrash(trash);
                    }
                    else
                    {
                        //Si la maquina no lo destruyo por colision (ej: no tiene Rigidbody), lo destruimos igual al llegar
                        Destroy(item);
                    }
                }

                beltItem = null;
            }
        }
        finally
        {
            isMoving = false;
        }
    }

    //Pasa el objeto de un objeto a otra, (ya que no es toda una cinta en conjunto, son varias partes)
    private Belt FindNextBelt()
    {
        Transform currentBeltTransform = transform;
        RaycastHit hit;

        var forward = transform.forward;

        Ray ray = new Ray(currentBeltTransform.position, forward);

        if (Physics.Raycast(ray, out hit, 1f))
        {
            Belt belt = hit.collider.GetComponent<Belt>();

            if (belt != null)
                return belt;
        }

        return null;
    }
}
