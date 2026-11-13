
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
            Debug.LogError("MenuInicio: asigna el objeto raíz Player.");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void NuevaPartida()
    {
        if (raizJugador == null)
        {
            Debug.LogError("MenuInicio: asigna el objeto raíz Player.");
            return;
        }

        // Borra el progreso anterior
        PlayerPrefs.DeleteKey("PartidaGuardada");

        if (panelMenu != null)
            panelMenu.SetActive(false);

        raizJugador.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Continuar()
    {
        if (PlayerPrefs.GetInt("PartidaGuardada", 0) == 0)
        {
            Debug.Log("No hay ninguna partida guardada.");
            return;
        }

        if (panelMenu != null)
            panelMenu.SetActive(false);

        raizJugador.SetActive(true);

        // Aquí posteriormente cargaremos la posición
        // y el progreso de la partida.

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");

        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
