using UnityEngine;

public class Tile : MonoBehaviour
{
    public Material normalMaterial;
    public Material validMaterial;
    public Material invalidMaterial;
    public bool occupied = false;
    public GameObject currentBuilding;
    private Renderer rend;

    //Aplica el mesh renderer, para que se vea la tile, a penas empieza la escena (antes del update y start)
    void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material = normalMaterial;
    }

    //Deja sin construcciones esas casillas
    public void SetNormal()
    {
        rend.material = normalMaterial;
    }
    
    //Pone en verde la casilla ya que no hay otra construccion y se puede construir ahi
    public void SetValid()
    {
        rend.material = validMaterial;
    }

    //Pone en rojo la casilla porque no se puede construir ahi ya que hay otra construccion de antes
    public void SetInvalid()
    {
        rend.material = invalidMaterial;
    }
}