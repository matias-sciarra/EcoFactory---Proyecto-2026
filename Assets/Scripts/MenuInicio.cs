
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuInicio : MonoBehaviour
{
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject raizJugador;
    private FirstPersonController controladorFPS;

    private void Start()
    {
        if (panelMenu != null)
            panelMenu.SetActive(true);

        if (raizJugador != null)
        {
            // Dejamos la cámara activa para renderizar la escena, pero el
            // controlador no debe capturar el cursor mientras se usa el menú.
            controladorFPS = raizJugador.GetComponentInChildren<FirstPersonController>(true);
            if (controladorFPS != null)
                controladorFPS.enabled = false;
            else
                Debug.LogError("MenuInicio: no se encontró FirstPersonController dentro de Player.");
        }

        if (raizJugador == null)
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
        if (controladorFPS != null)
            controladorFPS.enabled = true;

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
        if (controladorFPS != null)
            controladorFPS.enabled = true;

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
