using UnityEngine;

public class PlacementController : MonoBehaviour
{
    [Header("Refs")]
    public Camera playerCamera;
    public LayerMask groundLayer;
    public Grid_Manager gridManager;

    [Header("Ghost")]
    public Material validMaterial;
    public Material invalidMaterial;

    private GameObject currentPrefab;
    private GameObject ghostObject;
    private Renderer[] ghostRenderers;
    private int rotationSteps = 0; // 0,1,2,3 -> 0°,90°,180°,270°
    private Vector3Int currentGridPos;
    private bool isValidPlacement;

    public void StartPlacement(GameObject prefab)
    {
        currentPrefab = prefab;
        ghostObject = Instantiate(prefab);
        SetGhostMode(ghostObject);
        ghostRenderers = ghostObject.GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (ghostObject == null) return;

        HandleRotationInput();
        UpdateGhostPosition();

        if (Input.GetMouseButtonDown(0) && isValidPlacement)
            ConfirmPlacement();

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            CancelPlacement();
    }

    void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationSteps = (rotationSteps + 1) % 4;
            ghostObject.transform.rotation = Quaternion.Euler(0, rotationSteps * 90f, 0);
        }
    }

    void UpdateGhostPosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            currentGridPos = gridManager.WorldToGrid(hit.point);
            Vector3 snappedWorldPos = gridManager.GridToWorld(currentGridPos);

            ghostObject.transform.position = snappedWorldPos;

            isValidPlacement = ValidatePlacement(currentGridPos);
            SetGhostColor(isValidPlacement);
        }
        else
        {
            isValidPlacement = false;
            SetGhostColor(false);
        }
    }

    bool ValidatePlacement(Vector3Int gridPos)
    {
        if (!gridManager.IsWithinBounds(gridPos))
            return false;

        GridCell cell = gridManager.GetCell(gridPos);
        if (cell == null || cell.IsOccupied)
            return false;

        return true;
    }

    void ConfirmPlacement()
    {
        GameObject placed = Instantiate(currentPrefab, ghostObject.transform.position, ghostObject.transform.rotation);

        GridCell cell = gridManager.GetCell(currentGridPos);
        cell.Occupy(placed);

        CancelPlacement();
    }

    public void CancelPlacement()
    {
        if (ghostObject != null)
            Destroy(ghostObject);

        ghostObject = null;
        currentPrefab = null;
        rotationSteps = 0;
    }

    void SetGhostMode(GameObject obj)
    {
        foreach (var col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void SetGhostColor(bool valid)
    {
        Material mat = valid ? validMaterial : invalidMaterial;
        foreach (var r in ghostRenderers)
            r.material = mat;
    }
}