
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuInicio : MonoBehaviour
{
    [SerializeField] private GameObject panelMenu;
    [SerializeField] private GameObject raizJugador;
    [SerializeField] private Button botonContinuar;

    private readonly List<Behaviour> pausados = new List<Behaviour>();
    private readonly List<FPSController> controlesPropios = new List<FPSController>();
    private savemanager guardado;
    private bool avisoSinSavemanager;

    private void Awake()
    {
        savemanager.cargaDecididaPorMenu = true;
    }

    private void OnDestroy()
    {
        savemanager.cargaDecididaPorMenu = false;
    }

    private void Start()
    {
        if (panelMenu != null)
            panelMenu.SetActive(true);

        if (raizJugador == null)
            Debug.LogError("MenuInicio: asigna el objeto raíz Player.");

        guardado = FindObjectOfType<savemanager>();
        if (guardado == null)
            AvisarSinSavemanager();

        PausarJugador();
        ActualizarBotonContinuar();

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
        if (guardado != null)
            guardado.EmpezarDeCero();
        else
            AvisarSinSavemanager();

        EmpezarJuego();
    }

    public void Continuar()
    {
        if (!savemanager.HayPartidaGuardada())
        {
            Debug.Log("No hay ninguna partida guardada.");
            return;
        }

        if (raizJugador == null)
        {
            Debug.LogError("MenuInicio: asigna el objeto raíz Player.");
            return;
        }

        if (guardado == null)
        {
            AvisarSinSavemanager();
            return;
        }

        EmpezarJuego();

        // Aquí posteriormente cargaremos la posición
        // y el progreso de la partida.
        guardado.cargar();
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

    private void EmpezarJuego()
    {
        if (panelMenu != null)
            panelMenu.SetActive(false);

        raizJugador.SetActive(true);
        ReanudarJugador();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void PausarJugador()
    {
        pausados.Clear();
        controlesPropios.Clear();

        FirstPersonController[] controladores = raizJugador != null
            ? raizJugador.GetComponentsInChildren<FirstPersonController>(true)
            : FindObjectsOfType<FirstPersonController>();
        foreach (FirstPersonController controlador in controladores)
            Pausar(controlador);

        PlayerGrabber[] grabbers = raizJugador != null
            ? raizJugador.GetComponentsInChildren<PlayerGrabber>(true)
            : FindObjectsOfType<PlayerGrabber>();
        foreach (PlayerGrabber grabber in grabbers)
            Pausar(grabber);

        foreach (BuildManager build in FindObjectsOfType<BuildManager>())
            Pausar(build);

        FPSController[] propios = raizJugador != null
            ? raizJugador.GetComponentsInChildren<FPSController>(true)
            : FindObjectsOfType<FPSController>();
        foreach (FPSController propio in propios)
        {
            propio.SetControlEnabled(false);
            controlesPropios.Add(propio);
        }
    }

    private void Pausar(Behaviour componente)
    {
        if (componente != null && componente.enabled)
        {
            componente.enabled = false;
            pausados.Add(componente);
        }
    }

    private void ReanudarJugador()
    {
        foreach (Behaviour componente in pausados)
        {
            if (componente != null)
                componente.enabled = true;
        }
        pausados.Clear();

        foreach (FPSController propio in controlesPropios)
        {
            if (propio != null)
                propio.SetControlEnabled(true);
        }
        controlesPropios.Clear();
    }

    private void ActualizarBotonContinuar()
    {
        if (botonContinuar == null)
            botonContinuar = BuscarBotonContinuar();

        if (botonContinuar != null)
            botonContinuar.interactable = savemanager.HayPartidaGuardada();
        else
            Debug.LogWarning("MenuInicio: no se encontró el botón Continuar (ningún Button llama a MenuInicio.Continuar). Asignalo en el campo botonContinuar", this);
    }

    private Button BuscarBotonContinuar()
    {
        if (panelMenu != null)
        {
            Button enPanel = BuscarBotonQueLlama(panelMenu.GetComponentsInChildren<Button>(true), nameof(Continuar));
            if (enPanel != null)
                return enPanel;
        }

        return BuscarBotonQueLlama(FindObjectsOfType<Button>(), nameof(Continuar));
    }

    private Button BuscarBotonQueLlama(Button[] botones, string metodo)
    {
        foreach (Button boton in botones)
        {
            for (int i = 0; i < boton.onClick.GetPersistentEventCount(); i++)
            {
                if (boton.onClick.GetPersistentTarget(i) == this && boton.onClick.GetPersistentMethodName(i) == metodo)
                    return boton;
            }
        }

        return null;
    }

    private void AvisarSinSavemanager()
    {
        if (avisoSinSavemanager)
            return;

        avisoSinSavemanager = true;
        Debug.LogError("MenuInicio: no hay ningún savemanager en la escena. Agregá un GameObject con el componente savemanager: sin él no se guarda ni se carga la partida", this);
    }
}
