using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


[System.Serializable]
public class MaquinaGuardada
{
    public int tipo;
    public int x,y,z;
    public int rotacion;
    public int precio;
}

[System.Serializable]
public class Partida
{
    public int dinero;
    public int nivel;
    public int[] vecesComprada;
    public List<MaquinaGuardada> maquinas = new List<MaquinaGuardada>();
}

public class savemanager : MonoBehaviour
{
    public economymanager economia;
    public BuildManager buildManager;
    public PlacementController placement;
    public float intervalo = 60f;

    private string ruta;
    private float proximoGuardado;



    void Awake()
    {
        ruta = Path.Combine(Application.persistentDataPath, "partida.json");
    }

    IEnumerator Start()
    {
        if(economia == null) economia = FindObjectOfType<economymanager>();
        if(buildManager == null) buildManager = FindObjectOfType<BuildManager>();
        if(placement == null) placement = FindObjectOfType<PlacementController>();

            yield return null;

        cargar();
        proximoGuardado = Time.time + intervalo;
    }


    void Update()
    {
        if(intervalo > 0 && Time.time >= proximoGuardado)
        {
            guardar();
            proximoGuardado = Time.time + intervalo;
        }
    }

    void OnApplicationQuit()
    {
        guardar();
    }

    public void guardar()
    {
        if(economia == null || buildManager == null || placement == null)
        {
            Debug.LogWarning("No se puede guardar la partida, faltan referencias.");
            return;
        }

        Partida p = new Partida();
        p.dinero = economia.dinero;
        p.nivel = levelmanager.instance != null ? levelmanager.instance.currentLevel : 1;

        p.vecesComprada = new int[buildManager.maquinas.Length];
        for (int i = 0; i < buildManager.maquinas.Length; i++)
            p.vecesComprada[i] = buildManager.maquinas[i].vecesComprada;
 
        p.maquinas = placement.ExportarConstrucciones();
 
        File.WriteAllText(ruta, JsonUtility.ToJson(p, true));
        Debug.Log("Partida guardada en " + ruta);
    }

    public void cargar()
    {
        if (!File.Exists(ruta))
        {
            Debug.Log("No hay partida guardada, se arranca de cero");
            return;
        }
 
        if (economia == null || buildManager == null || placement == null)
        {
            Debug.LogError("SaveManager: faltan referencias, no se puede cargar", this);
            return;
        }
 
        Partida p = JsonUtility.FromJson<Partida>(File.ReadAllText(ruta));
 
        economia.dinero = p.dinero;
 
        if (levelmanager.instance != null)
            levelmanager.instance.currentLevel = p.nivel;
 
        if (p.vecesComprada != null)
        {
            for (int i = 0; i < p.vecesComprada.Length && i < buildManager.maquinas.Length; i++)
                buildManager.maquinas[i].vecesComprada = p.vecesComprada[i];
        }
 
        if (p.maquinas != null)
        {
            foreach (MaquinaGuardada m in p.maquinas)
                placement.ColocarSinCobrar(m);
        }
 
        BuildManager.NotificarConstruccionCambiada();
        Debug.Log("Partida cargada desde " + ruta);
    }

}
