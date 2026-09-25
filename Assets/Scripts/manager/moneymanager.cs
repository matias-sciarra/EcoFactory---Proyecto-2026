using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// OBSOLETO: la plata del juego ahora la maneja economymanager (Assets/Scripts/economy).
// Ya no lo usa ningun script ni ninguna escena, se puede borrar este archivo.
public class moneymanager : MonoBehaviour
{
    public TextMeshProUGUI txtdinero;
    public float dinero = 0f;
    void Start()
    {  
       txtdinero.text = dinero.ToString(); 
    }

    void Update()
    {
        
    }
}
