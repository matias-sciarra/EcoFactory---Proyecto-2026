using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{
    // Una entrada por cada tipo de maquina que se puede construir.
    // Se configura desde el Inspector: prefab + precio base (el contador arranca en 0).
    [System.Serializable]
    public class MaquinaConstruible
    {
        public GameObject prefab;
        public Sprite imagenUI;
        public int precioBase = 100;
        public int vecesComprada = 0;
    }

    public const int CANTIDAD_MAQUINAS = 2;
    private const float MULTIPLICADOR_PRECIO = 1.7f;

    public Image imgMaquina;
    public GameObject player;
    public Behaviour fpsController;
    public Transform fpsCamera;
    public Transform buildPosition;
    public PlacementController placementController;
    public economymanager economyManager;   
    public GameObject panelInfoMaquina;
    public TextMeshProUGUI txtNombreMaquina;
    public TextMeshProUGUI txtPrecioMaquina;
    public TextMeshProUGUI txtMaquina;
    private int ultimoDinero = -1;
    private const float PORCENTAJE_DEVOLUCION = 0.7f;

    // Array fijo de 3 maquinas, con sus precios base 100 / 150 / 200
    public MaquinaConstruible[] maquinas = new MaquinaConstruible[CANTIDAD_MAQUINAS]
    {
        new MaquinaConstruible { precioBase = 100 },
        new MaquinaConstruible { precioBase = 150 },
    };

    public int indiceSeleccionado;

    private bool buildMode;

    // Lo leen los otros scripts (PlayerGrabber con E, Belt y MachineSlot con Q) para no
    // dispararse mientras estamos en modo construccion.
    private static bool buildModeActivo;
    public static bool BuildModeActivo { get { return buildModeActivo; } }

    public static event System.Action OnConstruccionCambiada;

    public static void NotificarConstruccionCambiada()
    {
        if (OnConstruccionCambiada != null)
            OnConstruccionCambiada();
    }

    public void Devolver(int indice, int precioPagado)
    {
        int devolucion = Mathf.RoundToInt(precioPagado * PORCENTAJE_DEVOLUCION);

        if (economyManager != null)
            economyManager.Ganar(devolucion);

        if (IndiceValido(indice) && maquinas[indice].vecesComprada > 0)
            maquinas[indice].vecesComprada--;

        Debug.Log("Maquina " + indice + " vendida: se devuelven " + devolucion + ". Proximo precio: " + GetPrecioActual(indice));
        ActualizarUIMaquina();
    }

    private CharacterController characterController;

    private Vector3 oldPlayerPosition;
    private Quaternion oldPlayerRotation;
    private Vector3 oldCameraLocalPosition;
    private Quaternion oldCameraLocalRotation;

    void Awake()
    {
        // Todas las referencias se cachean aca, nunca en Update
        if (economyManager == null)
            economyManager = FindObjectOfType<economymanager>();

        if (placementController == null)
            placementController = FindObjectOfType<PlacementController>();

        if (placementController != null)
            placementController.SetBuildManager(this);

        if (player != null)
            characterController = player.GetComponent<CharacterController>();

        buildModeActivo = false;
        if (txtMaquina != null) txtMaquina.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (buildMode)
                ExitBuildMode();
            else
                EnterBuildMode();
        }

        // Fuera del modo construccion Q y E no hacen nada aca:
        // los usa el resto del juego (E para agarrar basura, Q para mejorar cintas)
        if (!buildMode)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            SeleccionarMaquina(indiceSeleccionado - 1);
        else if (Input.GetKeyDown(KeyCode.E))
            SeleccionarMaquina(indiceSeleccionado + 1);

        // Atajos numericos 1 / 2 / 3 (los que ya estaban)
        for (int i = 0; i < maquinas.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SeleccionarMaquina(i);
        }
        if (economyManager != null && economyManager.dinero != ultimoDinero)
        {
            ultimoDinero = economyManager.dinero;
            ActualizarUIMaquina();
        }
    }

    // ---------- Seleccion de maquina ----------

    // Cambia la maquina seleccionada. El indice es circular: -1 pasa a la ultima
    // y el que se pasa del final vuelve a 0.
    public void SeleccionarMaquina(int indice)
    {
        if (maquinas == null || maquinas.Length == 0)
            return;

        indiceSeleccionado = Envolver(indice);

        ActualizarPreview();

        Debug.Log("Maquina seleccionada: " + indiceSeleccionado + " - precio actual: " + GetPrecioActual(indiceSeleccionado));
        if (txtMaquina != null) txtMaquina.gameObject.SetActive(true);
        ActualizarUIMaquina();
    }

    private int Envolver(int indice)
    {
        int cantidad = maquinas.Length;

        if (indice < 0)
            return cantidad - 1;

        if (indice >= cantidad)
            return 0;

        return indice;
    }

    private bool IndiceValido(int indice)
    {
        return maquinas != null
            && indice >= 0
            && indice < maquinas.Length
            && maquinas[indice] != null;
    }

    private void ActualizarPreview()
    {
        if (!buildMode || placementController == null)
            return;

        GameObject prefab = GetPrefab(indiceSeleccionado);
        if (prefab != null)
            placementController.ChangeBuilding(prefab);
    }

    public GameObject GetPrefab(int indice)
    {
        if (!IndiceValido(indice))
            return null;

        return maquinas[indice].prefab;
    }

    // ---------- Precios ----------

    // precioActual = precioBase * 1.7 ^ vecesComprada  (la primera compra sale el precio base)
    public int GetPrecioActual(int indice)
    {
        if (!IndiceValido(indice))
            return 0;

        MaquinaConstruible maquina = maquinas[indice];
        return Mathf.RoundToInt(maquina.precioBase * Mathf.Pow(MULTIPLICADOR_PRECIO, maquina.vecesComprada));
    }

    public int GetVecesComprada(int indice)
    {
        if (!IndiceValido(indice))
            return 0;

        return maquinas[indice].vecesComprada;
    }

    // ---------- Compra ----------

    // La llama el PlacementController justo antes de instanciar la maquina.
    // Si devuelve false no hay plata y no se coloca nada.
    public bool TryComprarSeleccionada()
    {
        return TryComprarMaquina(indiceSeleccionado);
    }

    public bool TryComprarMaquina(int indice)
    {
        if (!IndiceValido(indice))
        {
            Debug.Log("BuildManager: indice de maquina invalido (" + indice + ")");
            return false;
        }

        if (economyManager == null)
        {
            Debug.Log("BuildManager: falta la referencia al economymanager, no se puede cobrar la maquina");
            return false;
        }

        int precio = GetPrecioActual(indice);

        if (!economyManager.TieneSuficiente(precio))
        {
            Debug.Log("No hay plata suficiente para la maquina " + indice + ": cuesta " + precio + " y hay " + economyManager.dinero);
            // TODO UI: enganchar aca el aviso en pantalla de "plata insuficiente"
            return false;
        }

        economyManager.Gastar(precio);
        maquinas[indice].vecesComprada++;

        Debug.Log("Maquina " + indice + " comprada por " + precio + ". Proximo precio: " + GetPrecioActual(indice));
        // TODO UI: refrescar aca el precio que muestra la UI para este tipo de maquina
        return true;
    }

    // ---------- Modo construccion ----------

    
void EnterBuildMode()
{
    buildMode = true;
    buildModeActivo = true;

    oldPlayerPosition = player.transform.position;
    oldPlayerRotation = player.transform.rotation;
    oldCameraLocalPosition = fpsCamera.localPosition;
    oldCameraLocalRotation = fpsCamera.localRotation;

    fpsController.enabled = false;
    player.transform.rotation = Quaternion.identity;

    fpsCamera.position = buildPosition.position;
    fpsCamera.rotation = Quaternion.Euler(90f, 0f, 0f);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    indiceSeleccionado = 0;

    GameObject prefab = GetPrefab(indiceSeleccionado);

    if (prefab != null)
    {
        placementController.StartPlacement(prefab);

        if (panelInfoMaquina != null)
            panelInfoMaquina.SetActive(true);
        else
            Debug.LogError("Falta asignar Panel Info Maquina en el Inspector.");

        ActualizarUIMaquina();
    }
    else
    {
        Debug.LogError(
            "La primera máquina no tiene prefab asignado en el Inspector."
        );
    }
}


    void ExitBuildMode()
    {
        buildMode = false;
        buildModeActivo = false;

        placementController.CancelPlacement();

        fpsController.enabled = false;

        player.transform.position = oldPlayerPosition;
        player.transform.rotation = oldPlayerRotation;
        fpsCamera.localPosition = oldCameraLocalPosition;
        fpsCamera.localRotation = oldCameraLocalRotation;

        if (characterController != null)
        {
            characterController.enabled = false;
            characterController.enabled = true;
        }

        fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (txtMaquina != null) txtMaquina.gameObject.SetActive(false);

        if (panelInfoMaquina != null)
        panelInfoMaquina.SetActive(false);
    }

    // Si el objeto se desactiva o se destruye estando en modo construccion,
    // que la bandera no quede trabada en true para el resto de los scripts.
    void OnDisable()
    {
        buildModeActivo = false;
    }

    
private void ActualizarUIMaquina()
{
    if (!IndiceValido(indiceSeleccionado))
        return;

    int precio = GetPrecioActual(indiceSeleccionado);

    bool alcanza = economyManager != null &&
                   economyManager.TieneSuficiente(precio);

    GameObject prefab = maquinas[indiceSeleccionado].prefab;

    string nombre = prefab != null
        ? prefab.name
        : "Maquina " + (indiceSeleccionado + 1);

    if (txtNombreMaquina != null)
        txtNombreMaquina.text = nombre;

    if (txtPrecioMaquina != null)
    {
        txtPrecioMaquina.text = "$" + precio;
        txtPrecioMaquina.color = alcanza
            ? Color.white
            : Color.red;
    }

    // Actualizar la imagen independientemente del texto del precio
    if (imgMaquina != null)
{
    imgMaquina.sprite = maquinas[indiceSeleccionado].imagenUI;

    Debug.Log("Índice seleccionado: " + indiceSeleccionado);
    Debug.Log("Sprite asignado: " +
        (imgMaquina.sprite != null
            ? imgMaquina.sprite.name
            : "NULL"));
}
else
{
    Debug.LogError("imgMaquina sigue siendo NULL");
}
}
}

