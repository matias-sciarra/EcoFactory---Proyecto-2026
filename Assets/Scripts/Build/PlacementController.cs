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

    // hace que el jugador pueda colocar y quitar edificios en la grilla
    void Update()
    {
        // si no está en modo construcción, no hace nada
        if (!buildMode)
            return;

        Ray ray = playerCamera.ScreenPointToRay(
            Input.mousePosition
        );

        // hace un raycast desde la cámara del jugador hacia el suelo para detectar la celda en la que está el mouse
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            groundLayer
        ))

        {   // si el raycast golpea el suelo, obtiene la posición de la celda en la grilla y la celda
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
        {   // si el raycast no golpea el suelo, resetea la celda actual
            if (currentCell != null)
            {
                currentCell.SetNormal();
                currentCell = null;
            }
        }

        // permite rotar la máquina con la tecla R
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationSteps++;

            if (rotationSteps >= 4)
                rotationSteps = 0;
        }
    }

    // coloca el edificio en la celda actual
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
        //building.transform.localScale *= 1.15f;

        currentCell.Occupy(building);
    }

    // elimina el edificio de la celda actual
    void RemoveBuilding()
    {
        if (currentCell.PlacedObject != null)
        {
            Destroy(currentCell.PlacedObject);
        }

        currentCell.Clear();
    }

    // inicia el modo construcción con el prefab del edificio a colocar
    public void StartPlacement(GameObject prefab)
    {
        buildingPrefab = prefab;

        rotationSteps = 0;

        buildMode = true;

        gridManager.SetBuildMode(true);
    }

    // cancela el modo construcción y resetea la celda actual
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