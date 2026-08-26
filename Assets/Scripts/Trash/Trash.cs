using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{
    public enum TrashType
    {
    Carton,
    Plastico,
    Vidrio,
    Metal
       
    }
    public TrashType trashType;
    public bool isHeld = false;
}