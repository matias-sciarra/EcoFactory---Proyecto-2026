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

    void Update()
    {
        if (!buildMode)
            return;

        Ray ray = playerCamera.ScreenPointToRay(
            Input.mousePosition
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            groundLayer
        ))
        {
            Vector3Int gridPosition =
                gridManager.WorldToGrid(hit.point);

            GridCell cell =
                gridManager.GetCell(gridPosition);

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
            }

            if (
                Input.GetMouseButtonDown(0) &&
                currentCell != null &&
                !currentCell.IsOccupied
            )
            {
                PlaceBuilding();
            }

            if (
                Input.GetMouseButtonDown(1) &&
                currentCell != null &&
                currentCell.IsOccupied
            )
            {
                RemoveBuilding();
            }
        }
        else
        {
            if (currentCell != null)
            {
                currentCell.SetNormal();
                currentCell = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationSteps++;

            if (rotationSteps >= 4)
                rotationSteps = 0;
        }
    }

    void PlaceBuilding()
    {
        Vector3 position =
            gridManager.GridToWorld(
                currentCell.GridPosition
            );

        position.y = 0;

        Quaternion rotation =
            Quaternion.Euler(
                0,
                rotationSteps * 90f,
                0
            );

        GameObject building =
            Instantiate(
                buildingPrefab,
                position,
                rotation
            );

        // Hace la máquina un poco más grande.
        building.transform.localScale *= 1.15f;

        currentCell.Occupy(building);
    }

    void RemoveBuilding()
    {
        if (currentCell.PlacedObject != null)
        {
            Destroy(currentCell.PlacedObject);
        }

        currentCell.Clear();
    }

    public void StartPlacement(GameObject prefab)
    {
        buildingPrefab = prefab;

        rotationSteps = 0;

        buildMode = true;

        gridManager.SetBuildMode(true);
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
    }
}