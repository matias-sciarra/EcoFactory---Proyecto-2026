using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject player;
    public Transform buildPosition;
    public GameObject buildingPrefab;
    public LayerMask gridLayer;
    private Tile lastPlacedTile;
    private Tile lastRemovedTile;

    [Header("Distancia del Raycast")]
    public float rayDistance = 100f;

    private FPSController fpsController;

    private Vector3 oldPosition;
    private Quaternion oldRotation;

    private bool buildMode = false;

    private Tile currentTile;

    void Start()
    {
        fpsController = player.GetComponent<FPSController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildMode = !buildMode;

            if (buildMode)
                EnterBuildMode();
            else
                ExitBuildMode();
        }

        if (!buildMode)
            return;

HandleTileSelection();
HandleBuildingPlacement();
HandleBuildingRemoval();
    }

    void EnterBuildMode()
    {
        oldPosition = player.transform.position;
        oldRotation = player.transform.rotation;

        fpsController.enabled = false;

        player.transform.position = buildPosition.position;
        player.transform.rotation = buildPosition.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitBuildMode()
    {
        if (currentTile != null)
        {
            currentTile.SetNormal();
            currentTile = null;
        }

        player.transform.position = oldPosition;
        player.transform.rotation = oldRotation;

        fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void HandleTileSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, gridLayer))
        {
            Tile tile = hit.collider.GetComponent<Tile>();

            if (tile != null)
            {
                if (currentTile != tile)
                {
                    if (currentTile != null)
                        currentTile.SetNormal();

                    currentTile = tile;

                    if (tile.occupied)
                        tile.SetInvalid();
                    else
                        tile.SetValid();
                }
            }
        }
        else
        {
            if (currentTile != null)
            {
                currentTile.SetNormal();
                currentTile = null;
            }
        }
    }

    void HandleBuildingPlacement()
{
    if (currentTile == null)
        return;

    if (currentTile.occupied)
        return;

    if (Input.GetMouseButton(0))
    {
        if (currentTile == lastPlacedTile)
            return;

        GameObject building = Instantiate(
    buildingPrefab,
    currentTile.transform.position + Vector3.up * 0.55f,
    Quaternion.identity
);

currentTile.currentBuilding = building;
currentTile.occupied = true;
currentTile.SetInvalid();

        lastPlacedTile = currentTile;
    }

    if (Input.GetMouseButtonUp(0))
    {
        lastPlacedTile = null;
        }
    }

    void HandleBuildingRemoval()
{
    if (currentTile == null)
        return;

    if (!currentTile.occupied)
        return;

    if (Input.GetMouseButton(1))
    {
        if (currentTile == lastRemovedTile)
            return;

        Destroy(currentTile.currentBuilding);

        currentTile.currentBuilding = null;
        currentTile.occupied = false;
        currentTile.SetValid();

        lastRemovedTile = currentTile;
    }

    if (Input.GetMouseButtonUp(1))
    {
        lastRemovedTile = null;
    }
}
}