using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TrashGenerator : MonoBehaviour

{
    public GameObject Prefabtrash;
    private float temporizador;
    public float intervalo = 2f;
    public Belt primeraCinta;   
    public LayerMask capasconectadas;
    public float distanciamax = 123f;
    public float alturaRayo = 0;
    public Transform puntoSalida;
    public bool autoConectar = true;
    public float intervaloRedSeguridad = 2f;
    public bool debugConexiones = false;
    public float angulorayo = 90f;
    private bool avisoCapasVacias;
    public PlayerGrabber jugador;
    public float radiodetection = 5f;
    public float distanciacerca = 2f;
    private bool jugadorcerca = false;
    public Button cartelmejora;
    public economymanager manager;
    public float costomejora = 50f;
    private const float MULTIPLICADOR_MEJORA = 2.1f;
    private int cantidadMejoras = 0;



    void Start()
    {
        if (jugador == null) jugador = FindObjectOfType<PlayerGrabber>();
        if (manager == null) manager = FindObjectOfType<economymanager>();
        if (cartelmejora != null) cartelmejora.onClick.AddListener(mejorar);

    }
    void Update()
    {
        temporizador+= Time.deltaTime;
        if (temporizador <  intervalo)
            return;
        
        detectarbelt();
        if (!CintaLibre())
            return;

        temporizador = 0f;
        GenerateTrash();

    }

    bool CintaLibre()
    {
        return primeraCinta != null
            && !primeraCinta.isSpaceTaken
            && primeraCinta.beltItem == null;
    }

    //Genera la basura arriba de la cinta para que se pueda ir hacia otra maquina u otro lugar
    void GenerateTrash()
    {
        Vector3 position = primeraCinta.GetItemPosition();
        Quaternion rotation = Quaternion.identity;
        GameObject nueva = Instantiate( Prefabtrash, position, rotation );
        Debug.Log("se genero");

        BeltItem itemcomponent = nueva.GetComponent<BeltItem>();
        primeraCinta.beltItem = itemcomponent;
        primeraCinta.isSpaceTaken = true;

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

    public void detectarbelt()
    {
        if (!autoConectar)
            return;

        Belt anterior = primeraCinta;
        primeraCinta = null;

        primeraCinta = DetectarSalida().belt;

        DetectorConexion.LogCambio(this, anterior, primeraCinta, debugConexiones);
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

    public void mejorar()
    {
        ///poner mejora bajando tiempo de intervalo 
        if (manager == null)
        {
            Debug.LogWarning(name + ": no hay economymanager, no se puede comprar la mejora");
            return;
        }
        int costoActual = GetCostoMejora(cantidadMejoras);

        if (manager.Gastar(costoActual))
        {
            cantidadMejoras++;
            intervalo *= 0.8f;
            Debug.Log(name + ": mejora comprada, nuevo intervalo: " + intervalo);
        }

    }

    private int GetCostoMejora(int mejoras)
    {
        return Mathf.RoundToInt(costomejora * Mathf.Pow(MULTIPLICADOR_MEJORA, mejoras));
    }




}


