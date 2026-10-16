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

    private class Construida
    {
        public GameObject objeto;
        public int precio;
        public int indice;
    }

    private List<Construida> construidas = new List<Construida>();

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
            else if (previewObject != null)
            {
                previewObject.SetActive(false);
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

        if (buildManager == null)
        {
            Debug.LogError("PlacementController: falta la referencia al BuildManager, no se puede construir");
            return;
        }

        int indice = buildManager.indiceSeleccionado;
        int precioPagado = buildManager.GetPrecioActual(indice);

        if (!buildManager.TryComprarSeleccionada())
            return;

        Vector3 position = gridManager.GridToWorld(currentCell.GridPosition);
        position.y = 2;

        Quaternion rotation = Quaternion.Euler(0, rotationSteps * 90f, 0);

        GameObject building = Instantiate(buildingPrefab, position, rotation);
        building.SetActive(true);
        currentCell.Occupy(building);

        construidas.Add(new Construida { objeto = building, precio = precioPagado, indice = indice });

        BuildManager.NotificarConstruccionCambiada();
    }

    void RemoveBuilding()
    {
        GameObject obj = currentCell.PlacedObject;

        if (obj != null)
        {
            for (int i = 0; i < construidas.Count; i++)
            {
                if (construidas[i].objeto == obj)
                {
                    if (buildManager != null)
                        buildManager.Devolver(construidas[i].indice, construidas[i].precio);

                    construidas.RemoveAt(i);
                    break;
                }
            }

            foreach (Collider col in obj.GetComponentsInChildren<Collider>())
                col.enabled = false;

            Destroy(obj);
        }

        currentCell.Clear();

        BuildManager.NotificarConstruccionCambiada();
    }

    public void StartPlacement(GameObject prefab)
    {
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
        CrearPreview();
    }

    // El preview nace escondido: si no, aparece en la posicion guardada en el prefab
    // (cerca del centro del piso) hasta que el mouse pasa por una celda valida
    void CrearPreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = Instantiate(buildingPrefab);
        previewObject.transform.rotation = Quaternion.Euler(0, rotationSteps * 90f, 0);

        // apaga colliders del preview para que no interfiera con el raycast
        foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (currentCell == null)
            previewObject.SetActive(false);
        else
            previewObject.transform.position = gridManager.GridToWorld(currentCell.GridPosition);
    }
}