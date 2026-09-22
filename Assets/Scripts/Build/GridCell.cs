using UnityEngine;

public class GridCell : MonoBehaviour
{
    public bool IsOccupied { get; private set; }

    public GameObject PlacedObject { get; private set; }

    public Vector3Int GridPosition { get; set; }

    private GameObject visual;
    private Renderer visualRenderer;

    private Material normalMaterial;
    private Material validMaterial;
    private Material invalidMaterial;

    // crea la visualización de la celda al iniciar el juego
    void Awake()
    {
        CreateVisual();
    }

    // funcion que crea la visualización de la celda
    void CreateVisual()
    {
        // crea un cuadrado para representar la celda en el plano
        visual = GameObject.CreatePrimitive(PrimitiveType.Quad);

        // elimina el collider del cuadrado para que no interfiera con la interacción del jugador
        visual.name = "CellVisual";

        // hace que el cuadrado sea hijo de la celda para que se mueva con ella
        visual.transform.SetParent(transform);

        // posiciona el cuadrado ligeramente por encima del plano para que no se vea cortado por el suelo
        visual.transform.localPosition = new Vector3(0, 0.01f, 0);

        // rota el cuadrado para que quede paralelo al plano del suelo
        visual.transform.localRotation = Quaternion.Euler(90, 0, 0);

        // Tamaño de la celda visual.
        // separacion entre celdas
        visual.transform.localScale = Vector3.one * 1.2f;

        // elimina el collider del cuadrado para que no interfiera con la interacción del jugador
        Collider collider = visual.GetComponent<Collider>();

        // elimina el collider del cuadrado para que no interfiera con la interacción del jugador
        if (collider != null)
            Destroy(collider);

        visualRenderer = visual.GetComponent<Renderer>();

        // crea materiales para representar los diferentes estados de la celda
        normalMaterial = new Material(Shader.Find("Unlit/Color"));
        normalMaterial.color = new Color(1f, 1f, 1f, 0f);

        // crea materiales para representar los diferentes estados de la celda
        validMaterial = new Material(Shader.Find("Unlit/Color"));
        validMaterial.color = Color.green;

        // crea materiales para representar los diferentes estados de la celda
        invalidMaterial = new Material(Shader.Find("Unlit/Color"));
        invalidMaterial.color = Color.red;

        SetBuildMode(false);
    }

    // activa o desactiva el modo de construcción en la celda
    public void SetBuildMode(bool active)
    {
        if (visual != null)
            visual.SetActive(active);

        if (active)
            SetNormal();
    }

    // cambia el material de la celda a normal
    public void SetNormal()
    {
        if (visualRenderer != null)
            visualRenderer.material = normalMaterial;
    }

    // cambia el material de la celda a válido
    public void SetValid()
    {
        if (visualRenderer != null)
            visualRenderer.material = validMaterial;
    }

    // cambia el material de la celda a inválido
    public void SetInvalid()
    {
        if (visualRenderer != null)
            visualRenderer.material = invalidMaterial;
    }

    // ocupa la celda con un objeto
    public void Occupy(GameObject obj)
    {
        IsOccupied = true;
        PlacedObject = obj;
    }

    // libera la celda de un objeto
    public void Clear()
    {
        IsOccupied = false;
        PlacedObject = null;
    }
}