
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
        public Vector3Int celda;
        public int rotacion;
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

        GameObject building =
            Construir(buildingPrefab, currentCell, rotationSteps);

        construidas.Add(new construida
        {   
            objeto = building,
            precio = preciopagado,
            indice = indice,
            celda = currentCell.GridPosition,
            rotacion = rotationSteps
        });

        BuildManager.NotificarConstruccionCambiada();
    }

    private GameObject Construir(GameObject prefab, GridCell cell, int rot)
    {
        Vector3 position = gridManager.GridToWorld(cell.GridPosition);

        position.y = 0;

        Quaternion rotation = Quaternion.Euler(0, rot * 90f, 0);

        GameObject building = Instantiate(prefab, position, rotation);

        building.SetActive(true);
        cell.Occupy(building);

        return building;
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

    public List<MaquinaGuardada> ExportarConstrucciones()
    {
        List<MaquinaGuardada> lista = new List<MaquinaGuardada>();

        foreach (construida c in construidas)
        {
            if (c.objeto == null)
                continue;

            lista.Add(new MaquinaGuardada
            {
                tipo = c.indice,
                x = c.celda.x,
                y = c.celda.y,
                z = c.celda.z,
                rotacion = c.rotacion,
                precio = c.precio
            });
        }

        return lista;
    }

    public void ColocarSinCobrar(MaquinaGuardada m)
    {
        if (m == null || buildManager == null || gridManager == null)
            return;

        GameObject prefab = buildManager.GetPrefab(m.tipo);
        GridCell cell = gridManager.GetCell(new Vector3Int(m.x, m.y, m.z));

        if (prefab == null || cell == null || cell.IsOccupied)
        {
            Debug.LogWarning(
                "No se restaura la maquina tipo " + m.tipo +
                " en la celda " + m.x + ", " + m.z +
                ": falta el prefab, no hay celda o esta ocupada."
            );
            return;
        }

        GameObject building = Construir(prefab, cell, m.rotacion);

        construidas.Add(new construida
        {
            objeto = building,
            precio = m.precio,
            indice = m.tipo,
            celda = cell.GridPosition,
            rotacion = m.rotacion
        });
    }
}
