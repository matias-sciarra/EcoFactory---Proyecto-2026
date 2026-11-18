using UnityEngine;

public class BeltManager : MonoBehaviour
{
    public float speed = 2f;

    public static event System.Action OnPrecioMejoraCambiado;

    private int mejorasCompradas = 0;

    public int MejorasCompradas
    {
        get { return mejorasCompradas; }
    }

    public int PrecioMejora(float costoBase, float multiplicador)
    {
        return Mathf.RoundToInt(costoBase * Mathf.Pow(multiplicador, mejorasCompradas));
    }

    public bool ComprarMejora(economymanager manager, float costoBase, float multiplicador, float aumentoVelocidad)
    {
        if (manager == null)
            return false;

        if (!manager.Gastar(PrecioMejora(costoBase, multiplicador)))
            return false;

        mejorasCompradas += 1;
        speed += aumentoVelocidad;

        if (OnPrecioMejoraCambiado != null)
            OnPrecioMejoraCambiado();

        return true;
    }

    public static void ProponerMejora(Belt cinta, float distancia)
    {
        if (cinta == null)
            return;

        ArbitroMejoras.Proponer(cinta, distancia, cinta.mejorar);
    }
}
