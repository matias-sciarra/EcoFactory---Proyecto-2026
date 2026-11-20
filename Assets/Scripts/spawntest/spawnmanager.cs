using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnmanager : MonoBehaviour
{
    public GameObject prefab;   // se usa solo si la lista "prefabs" esta vacia
    [Tooltip("Elemento 0 = nivel 1, elemento 1 = nivel 2, etc. (hasta 4)")]
    public GameObject[] prefabs;
    private const int NIVEL_MAXIMO = 4;
    private float spawnrate = 3f;
    private float tiempo = 0f;
    private  int contadordedestroyables = 0;
    private int limitededestruibles = 20;
    public float radiodetection = 30f;

    void Start()
    {
    
    }

    // Update is called once per frame  
    void Update()
        {
            tiempo += Time.deltaTime;

            if (tiempo >= spawnrate)
            {
                tiempo = 0f;
                detectar();

                if (contadordedestroyables < limitededestruibles)
                    spawn();
            }
        }

    void spawn()
    {
        GameObject Plano = GameObject.FindGameObjectWithTag("plano");
        Bounds b = Plano.GetComponent<Renderer>().bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float z = Random.Range(b.min.z, b.max.z);
        float y = 1f;

        GameObject elegido = ElegirPrefab();
        if (elegido == null)
            return;

        Instantiate(elegido, new Vector3(x, y, z), Quaternion.identity);
    }

    //Nivel 1 -> solo puede salir el prefab 0, nivel 2 -> prefabs 0 y 1, ... hasta el nivel 4
    private GameObject ElegirPrefab()
    {
        if (prefabs == null || prefabs.Length == 0)
            return prefab;

        int nivel = levelmanager.instance != null ? levelmanager.instance.currentLevel : 1;
        int disponibles = Mathf.Clamp(nivel, 1, Mathf.Min(NIVEL_MAXIMO, prefabs.Length));

        return prefabs[Random.Range(0, disponibles)];
    }

    void detectar()
    {
        contadordedestroyables = 0;
        HashSet<GameObject> unicos = new HashSet<GameObject>();

        Collider[] objetosdetectados = Physics.OverlapSphere(transform.position, radiodetection);
        foreach (Collider col in objetosdetectados)
        {
            if (col.CompareTag("destroyable"))
            {
                GameObject raiz = col.attachedRigidbody != null ? col.attachedRigidbody.gameObject : col.gameObject;
                unicos.Add(raiz);
            }
        }

        contadordedestroyables = unicos.Count;
    }
}
