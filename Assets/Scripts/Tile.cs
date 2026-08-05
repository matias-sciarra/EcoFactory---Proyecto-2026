using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Renderer rend;

    public Material normalMaterial;
    public Material highlightMaterial;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material = normalMaterial;
    }

    public void Highlight()
    {
        rend.material = highlightMaterial;
    }

    public void UnHighlight()
    {
        rend.material = normalMaterial;
    }
}
