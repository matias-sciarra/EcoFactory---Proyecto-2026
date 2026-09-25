using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Unica billetera del juego: la usan el BuildManager (construir maquinas),
// las cintas y las MachineSlot (mejoras) y la moneymachine (ganancias).
public class economymanager : MonoBehaviour
{
    public TextMeshProUGUI txtdinero;
    public int dinero = 0;

    // Start is called before the first frame update
    void Start()
    {
        ActualizarTexto();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Devuelve true si la plata actual alcanza para pagar ese costo
    public bool TieneSuficiente(int costo)
    {
        return dinero >= costo;
    }

    // Descuenta la plata. Si no alcanza no toca nada y devuelve false.
    public bool Gastar(int costo)
    {
        if (!TieneSuficiente(costo))
            return false;

        dinero -= costo;
        ActualizarTexto();
        return true;
    }

    // Suma plata (ventas, maquinas que generan guita, etc.)
    public void Ganar(int cantidad)
    {
        dinero += cantidad;
        ActualizarTexto();
    }

    // Refresca el contador de plata en pantalla
    public void ActualizarTexto()
    {
        if (txtdinero != null)
            txtdinero.text = dinero.ToString();
    }
}
