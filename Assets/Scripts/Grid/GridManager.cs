using UnityEngine;
using System.Collections.Generic;

public class Grid_Manager : MonoBehaviour
{
    public static Grid_Manager Instance;

    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;

    private Dictionary<Vector2Int, MachineBase> occupied = new Dictionary<Vector2Int, MachineBase>();

    void Awake()
    {
        Instance = this;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.z / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize + cellSize / 2f, 0, gridPos.y * cellSize + cellSize / 2f);
    }

    public bool IsCellFree(Vector2Int cell)
    {
        if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height) return false;
        return !occupied.ContainsKey(cell);
    }

    public void Occupy(Vector2Int cell, MachineBase machine)
    {
        occupied[cell] = machine;
    }

    public void Free(Vector2Int cell)
    {
        occupied.Remove(cell);
    }
}