using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MaquinaUtil
{
    private static readonly HashSet<string> avisosSinBeltItem = new HashSet<string>();

    public static bool JugadorCerca(Vector3 centro, float radio, float distanciaCerca, PlayerGrabber jugador)
    {
        float distancia;
        return JugadorCerca(centro, radio, distanciaCerca, jugador, out distancia);
    }

    public static bool JugadorCerca(Vector3 centro, float radio, float distanciaCerca, PlayerGrabber jugador, out float distanciaMinima)
    {
        bool cerca = false;
        distanciaMinima = float.MaxValue;
        Collider[] objetosdetectados = Physics.OverlapSphere(centro, radio);

        foreach (Collider col in objetosdetectados)
        {
            if (col.CompareTag("Player"))
            {
                float distancia = Vector3.Distance(centro, col.transform.position);
                if (distancia < distanciaCerca && !jugador.IsHolding)
                {
                    cerca = true;
                    if (distancia < distanciaMinima)
                        distanciaMinima = distancia;
                }
            }
        }

        return cerca;
    }

    public static void ProponerMejoraSiPidio(Component duenio, bool jugadorcerca, float distancia, System.Action mejorar)
    {
        if (jugadorcerca && !BuildManager.BuildModeActivo && Input.GetKeyDown(KeyCode.Q))
            ArbitroMejoras.Proponer(duenio, distancia, mejorar);
    }

    public static void MostrarCartel(Button cartel, bool visible)
    {
        if (cartel != null)
            cartel.gameObject.SetActive(visible);
    }

    public static bool PidioMejora(bool jugadorcerca)
    {
        return jugadorcerca && !BuildManager.BuildModeActivo && Input.GetKeyDown(KeyCode.Q);
    }

    public static int CostoEscalado(float costoBase, float multiplicador, int mejoras)
    {
        return Mathf.RoundToInt(costoBase * Mathf.Pow(multiplicador, mejoras));
    }

    public static void MostrarCosto(TextMeshProUGUI txt, float costoBase, float multiplicador, int mejoras)
    {
        if (txt != null)
            txt.text = CostoEscalado(costoBase, multiplicador, mejoras).ToString();
    }

    public static bool ComprarMejora(Component duenio, economymanager manager, float costoBase, float multiplicador, ref int mejoras, TextMeshProUGUI txt)
    {
        if (manager == null)
        {
            Debug.LogWarning($"[{duenio.name}] No hay economymanager, no se puede comprar la mejora", duenio);
            return false;
        }

        if (!manager.Gastar(CostoEscalado(costoBase, multiplicador, mejoras)))
            return false;

        mejoras += 1;
        MostrarCosto(txt, costoBase, multiplicador, mejoras);
        return true;
    }

    public static void CalcularRayo(Transform propio, Transform puntoSalida, float alturaRayo, float angulo, out Vector3 inicio, out Vector3 direccion)
    {
        DetectorConexion.CalcularRayo(propio, puntoSalida, alturaRayo, out inicio, out direccion);
        direccion = Quaternion.AngleAxis(angulo, propio.up) * direccion;
    }

    public static ResultadoConexion DetectarSalida(Component propio, Transform puntoSalida, float alturaRayo, float angulo, float distancia, LayerMask capas, ref bool avisoCapasVacias)
    {
        Vector3 inicio, direccion;
        CalcularRayo(propio.transform, puntoSalida, alturaRayo, angulo, out inicio, out direccion);
        return DetectorConexion.Detectar(propio, inicio, direccion, distancia, capas, ref avisoCapasVacias);
    }

    public static void DibujarSalida(Component propio, Transform puntoSalida, float alturaRayo, float angulo, float distancia, LayerMask capas, ref bool avisoCapasVacias)
    {
        Vector3 inicio, direccion;
        CalcularRayo(propio.transform, puntoSalida, alturaRayo, angulo, out inicio, out direccion);
        ResultadoConexion r = DetectorConexion.Detectar(propio, inicio, direccion, distancia, capas, ref avisoCapasVacias);
        DetectorConexion.DibujarRayo(inicio, direccion, distancia, r, r.belt != null);
    }

    public static bool CintaLibre(Belt cinta)
    {
        return cinta != null
            && !cinta.isSpaceTaken
            && cinta.beltItem == null;
    }

    public static bool SoltarEnCinta(Component duenio, GameObject prefab, string nombreCampo, Belt cinta)
    {
        if (prefab == null)
        {
            Debug.LogError($"[{duenio.name}] No hay {nombreCampo} asignado", duenio);
            return false;
        }

        if (cinta == null)
        {
            Debug.LogError($"[{duenio.name}] No hay cinta de salida para soltar el {nombreCampo} '{prefab.name}'", duenio);
            return false;
        }

        if (!TieneBeltItem(duenio, prefab, nombreCampo))
            return false;

        GameObject nueva = Object.Instantiate(prefab, cinta.GetItemPosition(), Quaternion.identity);
        nueva.SetActive(true);

        BeltItem itemcomponent = nueva.GetComponent<BeltItem>();
        if (itemcomponent == null)
        {
            Debug.LogError($"[{duenio.name}] La instancia de '{prefab.name}' quedó sin BeltItem, se destruye y la cinta queda libre", duenio);
            Object.Destroy(nueva);
            return false;
        }

        cinta.beltItem = itemcomponent;
        cinta.isSpaceTaken = true;
        return true;
    }

    public static bool TieneBeltItem(Component duenio, GameObject prefab, string nombreCampo)
    {
        if (prefab != null && prefab.GetComponent<BeltItem>() != null)
            return true;

        string nombrePrefab = prefab != null ? prefab.name : "(ninguno)";
        string clave = duenio.GetInstanceID() + "|" + nombrePrefab;

        if (avisosSinBeltItem.Add(clave))
            Debug.LogError($"[{duenio.name}] El {nombreCampo} '{nombrePrefab}' no tiene el componente BeltItem en su raíz: no se suelta en la cinta y la cinta queda libre. Agregale BeltItem al prefab", duenio);

        return false;
    }

    public static bool TieneEstado(Animator animator, string estado)
    {
        return animator != null
            && !string.IsNullOrEmpty(estado)
            && animator.HasState(0, Animator.StringToHash(estado));
    }

    public static void VolverAReposo(Animator animator, string estadoReposo)
    {
        if (animator == null)
            return;

        animator.speed = 1f;
        if (TieneEstado(animator, estadoReposo))
            animator.Play(estadoReposo, 0, 0f);
    }

    public static bool Reproducir(Component duenio, Animator animator, string estado, float velocidad, ref bool avisoEstadoFaltante)
    {
        if (!TieneEstado(animator, estado))
        {
            AvisarEstadoFaltante(duenio, estado, ref avisoEstadoFaltante);
            return false;
        }

        animator.speed = velocidad;
        animator.Play(estado, 0, 0f);
        return true;
    }

    public static void AvisarEstadoFaltante(Component duenio, string estado, ref bool aviso)
    {
        if (aviso)
            return;

        aviso = true;
        Debug.LogWarning($"[{duenio.name}] No se encontró el estado de animación '{estado}' en el Animator", duenio);
    }
}
