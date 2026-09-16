
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Belt : MonoBehaviour
{
    private static int _beltID = 0;

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
    public moneymanager manager;
    public PlayerGrabber jugador;
    public float costomejora = 50f;
    private int cantidadMejoras = 0;
    public TextMeshProUGUI txtcostomejora;


    private void Start()
    {
    _beltManager = FindObjectOfType<BeltManager>();
    gameObject.name = $"Belt: {_beltID++}";

    txtcostomejora.text = costomejora.ToString();
    cartelmejora.onClick.AddListener(mejorar);
    }

    //Controla cuando la cinta empieza a mover un objeto y cuando no
    private void Update()
    {
        detectar();

        if (beltItem != null && beltItem.item != null && !isMoving)
        {
            StartCoroutine(StartBeltMove());
        }
        else if (beltItem != null)
        {
            Debug.Log($"{name}: no arranca. item={beltItem.item}, isMoving={isMoving}");
        }
    }

    //Detecta si el jugador esta cerca (y sin nada agarrado) para mostrar el cartel de mejora
    public void detectar()
    {
        Vector3 inicio = transform.position;

        Collider[] objetosdetectados = Physics.OverlapSphere(inicio, radiodetection);

        foreach (Collider col in objetosdetectados)
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

    //Compra una mejora de velocidad para la cinta (velocidad compartida via BeltManager)
    public void mejorar()
    {
        float costoActual = costomejora * Mathf.Pow(1.6f, cantidadMejoras);

        if (manager.dinero >= costoActual)
        {
            manager.dinero -= costoActual;
            manager.txtdinero.text = manager.dinero.ToString();
            cantidadMejoras += 1;
            _beltManager.speed += 1f / 3f;

            float costoSiguiente = costomejora * Mathf.Pow(1.6f, cantidadMejoras);
            txtcostomejora.text = costoSiguiente.ToString();
        }
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

    if (beltItem.item != null && beltInSequence != null && beltInSequence.isSpaceTaken == false)
    {
        Vector3 toPosition = beltInSequence.GetItemPosition();
        beltInSequence.isSpaceTaken = true;
        var step = _beltManager.speed * Time.deltaTime;

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
    else if (beltItem.item != null && beltInSequence == null &&
        (moneyMachineInSequence != null || machineSlotInSequence != null || MachineInSequence != null))
    {
        Transform machineTransform = moneyMachineInSequence != null
            ? moneyMachineInSequence.transform
            : machineSlotInSequence != null
                ? machineSlotInSequence.transform
                : MachineInSequence.transform;

        Vector3 toPosition = machineTransform.position;
        GameObject item = beltItem.item;
        var step = _beltManager.speed * Time.deltaTime;

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

    isMoving = false;
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
