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

    void Awake()
    {
        CreateVisual();
    }

    void CreateVisual()
    {
        visual = GameObject.CreatePrimitive(PrimitiveType.Quad);

        visual.name = "CellVisual";

        visual.transform.SetParent(transform);

        visual.transform.localPosition = new Vector3(0, 0.01f, 0);

        visual.transform.localRotation = Quaternion.Euler(90, 0, 0);

        // Tamaño de la celda visual.
        // 0.80 = más separación entre celdas.
        visual.transform.localScale = Vector3.one * 0.80f;

        Collider collider = visual.GetComponent<Collider>();

        if (collider != null)
            Destroy(collider);

        visualRenderer = visual.GetComponent<Renderer>();

        normalMaterial = new Material(Shader.Find("Unlit/Color"));
        normalMaterial.color = new Color(1f, 1f, 1f, 0f);

        validMaterial = new Material(Shader.Find("Unlit/Color"));
        validMaterial.color = Color.green;

        invalidMaterial = new Material(Shader.Find("Unlit/Color"));
        invalidMaterial.color = Color.red;

        SetBuildMode(false);
    }

    public void SetBuildMode(bool active)
    {
        if (visual != null)
            visual.SetActive(active);

        if (active)
            SetNormal();
    }

    public void SetNormal()
    {
        if (visualRenderer != null)
            visualRenderer.material = normalMaterial;
    }

    public void SetValid()
    {
        if (visualRenderer != null)
            visualRenderer.material = validMaterial;
    }

    public void SetInvalid()
    {
        if (visualRenderer != null)
            visualRenderer.material = invalidMaterial;
    }

    public void Occupy(GameObject obj)
    {
        IsOccupied = true;
        PlacedObject = obj;
    }

    public void Clear()
    {
        IsOccupied = false;
        PlacedObject = null;
    }
}