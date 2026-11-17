using System;
using UnityEngine;

public class BeltItem : MonoBehaviour
{
    public GameObject item;
    public string tipoMaterial = "";

    private void Awake()
    {
        item = gameObject;
    }
}