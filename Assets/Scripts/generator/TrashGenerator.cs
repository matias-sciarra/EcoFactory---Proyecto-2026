using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    //hace que se genere la basura despues de 2 segundos
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

    void cintralibre()
    {
        return primeraCinta != null
            &&!primeraCinta.isSpaceTaken()
            &&primeraCinta.beltItem == null;
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




}


