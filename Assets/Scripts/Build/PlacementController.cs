using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementController : MonoBehaviour
{
    public GameObject machinePrefab;
    public Camera cam;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector2Int cell = Grid_Manager.Instance.WorldToGrid(hit.point);

            if (!Grid_Manager.Instance.IsCellFree(cell))
            {
                Debug.Log("Celda ocupada, no se puede colocar ahí.");
                return;
            }

            Vector3 worldPos = Grid_Manager.Instance.GridToWorld(cell);
            GameObject go = Instantiate(machinePrefab, worldPos, Quaternion.identity);

            MachineBase m = go.GetComponent<MachineBase>();
            if (m != null) m.gridPosition = cell;

            Grid_Manager.Instance.Occupy(cell, m);
        }
    }
}
