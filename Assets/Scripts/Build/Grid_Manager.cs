using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public float cellSize = 1f;
    public int gridWidth = 50;
    public int gridHeight = 50;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetGridPosition(Vector3 worldPosition)
    {
        float x = Mathf.Floor(worldPosition.x / cellSize) * cellSize;
        float y = worldPosition.y;
        float z = Mathf.Floor(worldPosition.z / cellSize) * cellSize;

        return new Vector3(x, y, z);
    }

    public Vector3 GetCellCenter(Vector3 worldPosition)
    {
        float x = Mathf.Floor(worldPosition.x / cellSize) * cellSize + cellSize / 2f;
        float y = worldPosition.y;
        float z = Mathf.Floor(worldPosition.z / cellSize) * cellSize + cellSize / 2f;

        return new Vector3(x, y, z);
    }

    public bool IsInsideGrid(Vector3 worldPosition)
    {
        float maxX = gridWidth * cellSize;
        float maxZ = gridHeight * cellSize;

        return worldPosition.x >= 0 &&
               worldPosition.x < maxX &&
               worldPosition.z >= 0 &&
               worldPosition.z < maxZ;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0, 0);
            Vector3 end = new Vector3(x * cellSize, 0, gridHeight * cellSize);

            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 start = new Vector3(0, 0, z * cellSize);
            Vector3 end = new Vector3(gridWidth * cellSize, 0, z * cellSize);

            Gizmos.DrawLine(start, end);
        }
    }
}
