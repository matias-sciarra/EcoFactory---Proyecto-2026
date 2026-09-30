using UnityEngine;

public struct ResultadoConexion
{
    public bool impacto;
    public Vector3 punto;
    public Belt belt;
    public MachineSlot machineSlot;
    public machine maquina;
    public moneymachine moneyMachine;

    public bool EsValido
    {
        get { return belt != null || machineSlot != null || maquina != null || moneyMachine != null; }
    }
}

public static class DetectorConexion
{
    private static readonly RaycastHit[] buffer = new RaycastHit[16];

    public static void CalcularRayo(Transform propio, Transform puntoSalida, float alturaRayo, out Vector3 inicio, out Vector3 direccion)
    {
        if (puntoSalida != null)
        {
            inicio = puntoSalida.position;
            direccion = puntoSalida.forward;
        }
        else
        {
            inicio = propio.position + Vector3.up * alturaRayo;
            direccion = propio.forward;
        }
    }

    public static ResultadoConexion Detectar(Component propio, Vector3 inicio, Vector3 direccion, float distancia, LayerMask capas, ref bool avisoCapasVacias)
    {
        ResultadoConexion mejor = new ResultadoConexion();

        if (capas.value == 0)
        {
            if (!avisoCapasVacias)
            {
                avisoCapasVacias = true;
                Debug.LogWarning(propio.name + ": el LayerMask de conexiones está en Nothing, no se va a conectar con nada", propio);
            }
            return mejor;
        }

        if (distancia <= 0f)
            return mejor;

        int cantidad = Physics.RaycastNonAlloc(inicio, direccion, buffer, distancia, capas);
        float mejorDistancia = float.MaxValue;

        for (int i = 0; i < cantidad; i++)
        {
            RaycastHit hit = buffer[i];
            if (hit.distance >= mejorDistancia)
                continue;

            ResultadoConexion candidato = Resolver(hit.collider);
            if (EsPropio(propio, hit.collider, candidato))
                continue;

            candidato.impacto = true;
            candidato.punto = hit.point;
            mejor = candidato;
            mejorDistancia = hit.distance;
        }

        return mejor;
    }

    private static ResultadoConexion Resolver(Collider col)
    {
        ResultadoConexion r = new ResultadoConexion();

        r.belt = col.GetComponentInParent<Belt>();
        if (r.belt != null) return r;

        r.machineSlot = col.GetComponentInParent<MachineSlot>();
        if (r.machineSlot != null) return r;

        r.maquina = col.GetComponentInParent<machine>();
        if (r.maquina != null) return r;

        r.moneyMachine = col.GetComponentInParent<moneymachine>();
        return r;
    }

    private static bool EsPropio(Component propio, Collider col, ResultadoConexion r)
    {
        GameObject yo = propio.gameObject;

        if (r.belt != null) return r.belt.gameObject == yo;
        if (r.machineSlot != null) return r.machineSlot.gameObject == yo;
        if (r.maquina != null) return r.maquina.gameObject == yo;
        if (r.moneyMachine != null) return r.moneyMachine.gameObject == yo;

        return col.transform.IsChildOf(propio.transform);
    }

    public static void LogCambio(Object propio, Object anterior, Object nuevo, bool debug)
    {
        if (!debug || ReferenceEquals(anterior, nuevo))
            return;

        if (!ReferenceEquals(anterior, null))
            Debug.Log(propio.name + " se desconectó de " + NombreDe(anterior), propio);

        if (!ReferenceEquals(nuevo, null))
            Debug.Log(propio.name + " se conectó a " + NombreDe(nuevo), propio);
    }

    private static string NombreDe(Object o)
    {
        return o != null ? o.name : "(objeto borrado)";
    }

    public static void DibujarRayo(Vector3 inicio, Vector3 direccion, float distancia, ResultadoConexion r, bool valido)
    {
        Gizmos.color = valido ? Color.green : Color.red;
        Vector3 fin = r.impacto ? r.punto : inicio + direccion.normalized * distancia;
        Gizmos.DrawLine(inicio, fin);
        Gizmos.DrawWireSphere(inicio, 0.05f);

        if (r.impacto)
            Gizmos.DrawSphere(fin, 0.08f);
    }
}
