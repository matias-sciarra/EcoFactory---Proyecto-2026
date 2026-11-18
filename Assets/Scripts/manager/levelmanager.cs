using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class levelmanager : MonoBehaviour
{
    public int currentLevel = 1;
    public economymanager economymanager;
    public static levelmanager instance;
    public TextMeshProUGUI txtnivel;
    private int nivelMostrado = -1;

    [Header("estandares de niveles")]
    public int nivel1 = 0;
    public int nivel2 = 2000;
    public int nivel3 = 10000;
    public int nivel4 = 50000;

    void Awake()
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
    if (economymanager == null)
        return;

    int d = economymanager.dinero;
    int nivelPorDinero;

    if (d >= nivel4)
        nivelPorDinero = 4;
    else if (d >= nivel3)
        nivelPorDinero = 3;
    else if (d >= nivel2)
        nivelPorDinero = 2;
    else
        nivelPorDinero = 1;

    if (nivelPorDinero > currentLevel)
     {
        currentLevel = nivelPorDinero;
     }

    if (currentLevel != nivelMostrado)
    {
        nivelMostrado = currentLevel;
        Actualizartexto();
    }
        

    }

    void Actualizartexto()
    {
        if (txtnivel != null)
            txtnivel.text = currentLevel.ToString();

    }

    

    
}
