using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
