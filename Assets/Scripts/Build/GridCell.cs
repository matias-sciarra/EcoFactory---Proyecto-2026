using UnityEngine;  

public class GridCell : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public GameObject PlacedObject { get; private set; }
    public Vector3Int GridPosition { get; set; }

    public void Occupy(GameObject obj)
    {
        IsOccupied = true;
        PlacedObject = obj;
    }

    public void Clear()
    {
        IsOccupied = false;
        PlacedObject = null;
    }
}
