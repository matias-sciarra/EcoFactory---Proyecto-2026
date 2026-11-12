using UnityEngine;

/// <summary>
/// Controla el paso del menú inicial al juego en la misma escena.
/// En esta primera etapa solo implementa Nueva partida.
/// </summary>
public class MenuInicio : MonoBehaviour
{
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject raizJugador;

    private void Start()
    {
        if (panelMenu != null)
            panelMenu.SetActive(true);

        if (raizJugador != null)
            raizJugador.SetActive(false);
        else
            Debug.LogError("MenuInicio: asigna el objeto raíz Player al campo Raiz Jugador.");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void NuevaPartida()
    {
        if (raizJugador == null)
        {
            Debug.LogError("MenuInicio: asigna el objeto raíz Player al campo Raiz Jugador.");
            return;
        }

        if (panelMenu != null)
            panelMenu.SetActive(false);

        raizJugador.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
