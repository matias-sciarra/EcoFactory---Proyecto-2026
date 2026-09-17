using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public Transform tilesParent;

    private Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

    void Awake()
    {
        Instance = this;
        RegisterExistingTiles();
    }

    void RegisterExistingTiles()
    {
        Tile[] existingTiles = tilesParent.GetComponentsInChildren<Tile>();

        foreach (Tile tile in existingTiles)
        {
            Vector3Int gridPos = WorldToGrid(tile.transform.position);
            tiles[gridPos] = tile;
        }
    }

    public Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPos.x),
            0,
            Mathf.RoundToInt(worldPos.z)
        );
    }

    public Tile GetTileAt(Vector3 worldPos)
    {
        Vector3Int gridPos = WorldToGrid(worldPos);
        tiles.TryGetValue(gridPos, out Tile tile);
        return tile;
    }

    public bool IsOccupied(Vector3 worldPos)
    {
        Tile tile = GetTileAt(worldPos);
        return tile != null && tile.occupied;
    }
}