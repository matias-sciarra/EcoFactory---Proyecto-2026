using System;
using UnityEngine;

public class ArbitroMejoras : MonoBehaviour
{
    private static ArbitroMejoras instancia;
    private static Component candidato;
    private static Action accionCandidato;
    private static float distanciaCandidato = float.MaxValue;
    private static int frameCandidato = -1;

    public static void Proponer(Component duenio, float distancia, Action comprar)
    {
        if (duenio == null || comprar == null)
            return;

        AsegurarInstancia();

        if (frameCandidato != Time.frameCount)
        {
            frameCandidato = Time.frameCount;
            candidato = null;
            accionCandidato = null;
            distanciaCandidato = float.MaxValue;
        }

        bool esMasCercano = distancia < distanciaCandidato;
        bool empateYGana = candidato != null
            && Mathf.Approximately(distancia, distanciaCandidato)
            && duenio.GetInstanceID() < candidato.GetInstanceID();

        if (candidato == null || esMasCercano || empateYGana)
        {
            candidato = duenio;
            accionCandidato = comprar;
            distanciaCandidato = distancia;
        }
    }

    private static void AsegurarInstancia()
    {
        if (instancia != null)
            return;

        GameObject go = new GameObject("ArbitroMejoras");
        go.hideFlags = HideFlags.HideInHierarchy;
        instancia = go.AddComponent<ArbitroMejoras>();
    }

    private void LateUpdate()
    {
        if (frameCandidato != Time.frameCount)
            return;

        Component elegido = candidato;
        Action accion = accionCandidato;

        candidato = null;
        accionCandidato = null;
        distanciaCandidato = float.MaxValue;

        if (elegido != null && accion != null)
            accion();
    }
}
