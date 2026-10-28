using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levelmanager : MonoBehaviour
{
    public int currentLevel = 1;
    public economymanager economymanager;
    public int nivelpordinero;
    public static levelmanager instance;

    [Header("estandares de niveles")]
    public int nivel1 = 0;
    public int nivel2 = 2000;
    public int nivel3 = 10000;
    public int nivel4 = 50000;

    void awake()
    {
        instance = this;
    }
    void Start()
    {
        if(economymanager == null)
        {
            economymanager = FindObjectOfType<economymanager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (economymanager.dinero >= nivel1 && economymanager.dinero < nivel2)
        {
            currentLevel = 1;
        }
        else if (economymanager.dinero >= nivel2 && economymanager.dinero < nivel3)
        {
            currentLevel = 2;
        }
        else if (economymanager.dinero >= nivel3 && economymanager.dinero < nivel4)
        {
            currentLevel = 3;
        }
        else if (economymanager.dinero >= nivel4)
        {
            currentLevel = 4;
        }


        if(currentLevel < nivelpordinero)
        {
            currentLevel = nivelpordinero;
        }
    }

    
}
