using UnityEngine;

public class Tile : MonoBehaviour
{
    public Material normalMaterial;
    public Material validMaterial;
    public Material invalidMaterial;

    public bool occupied = false;
    public GameObject currentBuilding;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material = normalMaterial;
    }

    public void SetNormal()
    {
        rend.material = normalMaterial;
    }

    public void SetValid()
    {
        rend.material = validMaterial;
    }

    public void SetInvalid()
    {
        rend.material = invalidMaterial;
    }
}