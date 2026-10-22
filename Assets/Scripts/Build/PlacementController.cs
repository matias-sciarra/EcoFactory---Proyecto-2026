
using UnityEngine;
using System.Collections.Generic;

public class PlacementController : MonoBehaviour
{
    public Camera playerCamera;
    public LayerMask groundLayer;
    public Grid_Manager gridManager;
    public GameObject buildingPrefab;

    private GridCell currentCell;
    private int rotationSteps;
    private bool buildMode;

    private class construida
    {
        public GameObject objeto;
        public int precio;
        public int indice;
    }

    private List<construida> construidas =
        new List<construida>();

    private GameObject previewObject;
    private BuildManager buildManager;

    public void SetBuildManager(BuildManager manager)
    {
        buildManager = manager;
    }

    void Update()
    {
        if (!buildMode)
            return;

        if (playerCamera == null || gridManager == null)
            return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3Int gridPosition =
                gridManager.WorldToGrid(hit.point);

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
                    Vector3 previewPos =
                        gridManager.GridToWorld(currentCell.GridPosition);

                    previewPos.y = 0;
                    previewObject.transform.position = previewPos;
                    previewObject.SetActive(true);
                }
            }
            else if (previewObject != null)
            {
                previewObject.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0) &&
                currentCell != null &&
                !currentCell.IsOccupied)
            {
                PlaceBuilding();
            }

            if (Input.GetMouseButtonDown(1) &&
                currentCell != null &&
                currentCell.IsOccupied)
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

            if (previewObject != null)
                previewObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationSteps = (rotationSteps + 1) % 4;

            if (previewObject != null)
            {
                previewObject.transform.rotation =
                    Quaternion.Euler(0, rotationSteps * 90f, 0);
            }
        }
    }

    // ---------------- COLOCAR MÁQUINA ----------------

    private void PlaceBuilding()
    {
        if (buildManager == null ||
            currentCell == null ||
            buildingPrefab == null)
        {
            Debug.LogWarning(
                "No se puede construir: falta el manager, " +
                "la celda o el prefab."
            );
            return;
        }

        int indice = buildManager.indiceSeleccionado;
        int preciopagado = buildManager.GetPrecioActual(indice);

        // Importante: cobrar una sola vez.
        if (!buildManager.TryComprarSeleccionada())
            return;

        Vector3 position =
            gridManager.GridToWorld(currentCell.GridPosition);

        position.y = 0  ;

        Quaternion rotation =
            Quaternion.Euler(0, rotationSteps * 90f, 0);

        GameObject building =
            Instantiate(buildingPrefab, position, rotation);

        building.SetActive(true);
        currentCell.Occupy(building);

        construidas.Add(new construida
        {   
            objeto = building,
            precio = preciopagado,
            indice = indice
        });

        BuildManager.NotificarConstruccionCambiada();
    }

    // ---------------- ELIMINAR MÁQUINA ----------------

    private void RemoveBuilding()
    {
        GameObject obj = currentCell.PlacedObject;

        if (obj != null)
        {
            for (int i = 0; i < construidas.Count; i++)
            {
                if (construidas[i].objeto == obj)
                {
                    if (buildManager != null)
                    {
                        buildManager.Devolver(
                            construidas[i].indice,
                            construidas[i].precio
                        );
                    }

                    construidas.RemoveAt(i);
                    break;
                }
            }

            foreach (Collider col in
                     obj.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            Destroy(obj);
        }

        currentCell.Clear();

        BuildManager.NotificarConstruccionCambiada();
    }

    // ---------------- ACTIVAR CONSTRUCCIÓN ----------------

    public void StartPlacement(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("StartPlacement recibió un prefab nulo.");
            return;
        }

        buildingPrefab = prefab;
        rotationSteps = 0;
        buildMode = true;

        gridManager.SetBuildMode(true);

        CrearPreview();
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

        if (gridManager != null)
            gridManager.SetBuildMode(false);

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    // ---------------- CAMBIAR MÁQUINA ----------------

    public void ChangeBuilding(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning(
                "ChangeBuilding recibió un prefab nulo."
            );
            return;
        }

        buildingPrefab = prefab;
        CrearPreview();
    }

    // ---------------- PREVISUALIZACIÓN ----------------

    private void CrearPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        if (buildingPrefab == null)
            return;

        previewObject = Instantiate(buildingPrefab);

        previewObject.transform.rotation =
            Quaternion.Euler(0, rotationSteps * 90f, 0);

        foreach (Collider col in
                 previewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        if (currentCell == null)
        {
            previewObject.SetActive(false);
        }
        else
        {
            Vector3 position =
                gridManager.GridToWorld(currentCell.GridPosition);

            position.y = 0;

            previewObject.transform.position = position;
            previewObject.SetActive(true);
        }
    }
}
