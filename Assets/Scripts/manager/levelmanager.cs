using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levelmanager : MonoBehaviour
{
    public int currentLevel = 1;
    public economyManager economyManager;

    [header("estandares de niveles")]
    public int nivel1 = 0;
    public int nivel2 = 2000;
    public int nivel3 = 10000;
    public int nivel4 = 50000;


    void Start()
    {
        if(economyManager == null)
        {
            economyManager = FindObjectOfType<economyManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (economyManager.money >= nivel1 && economyManager.money < nivel2)
        {
            currentLevel = 1;
        }
        else if (economyManager.money >= nivel2 && economyManager.money < nivel3)
        {
            currentLevel = 2;
        }
        else if (economyManager.money >= nivel3 && economyManager.money < nivel4)
        {
            currentLevel = 3;
        }
        else if (economyManager.money >= nivel4)
        {
            currentLevel = 4;
        }
    }
}
