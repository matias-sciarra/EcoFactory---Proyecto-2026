using UnityEngine;

public class PlacementController : MonoBehaviour
{
    public Camera playerCamera;
    public LayerMask groundLayer;
    public Grid_Manager gridManager;
    public GameObject buildingPrefab;

    private GridCell currentCell;
    private int rotationSteps;
    private bool buildMode;

    private GameObject previewObject;

    // Lo setea el BuildManager en su Awake, no hace falta arrastrarlo en el Inspector
    private BuildManager buildManager;

    public void SetBuildManager(BuildManager manager)
    {
        buildManager = manager;
    }

    void Update()
    {
        if (!buildMode)
            return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3Int gridPosition = gridManager.WorldToGrid(hit.point);
            GridCell cell = gridManager.GetCell(gridPosition);

            if (cell != currentCell)
            {
                if (currentCell != null)
                    currentCell.SetNormal();

                currentCell = cell;
            }

            if (currentCell != null)
            {
                if (currentCell.IsOccupied)
                    currentCell.SetInvalid();
                else
                    currentCell.SetValid();

                if (previewObject != null)
                {
                    Vector3 previewPos = gridManager.GridToWorld(currentCell.GridPosition);
                    previewPos.y = 0;
                    previewObject.transform.position = previewPos;
                    previewObject.SetActive(true);
                }
            }

            if (Input.GetMouseButtonDown(0) && currentCell != null && !currentCell.IsOccupied)
                PlaceBuilding();

            if (Input.GetMouseButtonDown(1) && currentCell != null && currentCell.IsOccupied)
                RemoveBuilding();
        }
        else
        {
            if (currentCell != null)
            {
                currentCell.SetNormal();
                currentCell = null;
            }

            if (previewObject != null)
                previewObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationSteps++;
            if (rotationSteps >= 4)
                rotationSteps = 0;

            if (previewObject != null)
                previewObject.transform.rotation = Quaternion.Euler(0, rotationSteps * 90f, 0);
        }
    }

    void PlaceBuilding()
    {
        // El BuildManager cobra la maquina seleccionada (precio escalado + descuento de plata).
        // Si no alcanza la plata devuelve false y no se coloca nada.
        if (buildManager != null && !buildManager.TryComprarSeleccionada())
            return;

        Vector3 position = gridManager.GridToWorld(currentCell.GridPosition);
        position.y = 0;

        Quaternion rotation = Quaternion.Euler(0, rotationSteps * 90f, 0);

        GameObject building = Instantiate(buildingPrefab, position, rotation);
        currentCell.Occupy(building);
    }

    void RemoveBuilding()
    {
        if (currentCell.PlacedObject != null)
            Destroy(currentCell.PlacedObject);

        currentCell.Clear();
    }

    public void StartPlacement(GameObject prefab)
    {
        buildingPrefab = prefab;
        rotationSteps = 0;
        buildMode = true;
        gridManager.SetBuildMode(true);

        if (previewObject != null)
            Destroy(previewObject);

        previewObject = Instantiate(buildingPrefab);
        previewObject.transform.rotation = Quaternion.identity;

        // apaga colliders del preview para que no interfiera con el raycast
        foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    public void CancelPlacement()
    {
        if (currentCell != null)
        {
            currentCell.SetNormal();
            currentCell = null;
        }

        rotationSteps = 0;
        buildMode = false;
        gridManager.SetBuildMode(false);

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }
    public void ChangeBuilding(GameObject prefab)
{
    buildingPrefab = prefab;

    if (previewObject != null)
        Destroy(previewObject);

    previewObject = Instantiate(buildingPrefab);
    previewObject.transform.rotation = Quaternion.Euler(0, rotationSteps * 90f, 0);

    foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
        col.enabled = false;

    if (currentCell == null)
        previewObject.SetActive(false);
}
}