using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnmanager : MonoBehaviour
{
    public GameObject prefab;
    private float spawnrate = 5f;
    private float tiempo = 0f;
    private  int contadordedestroyables = 0;
    private int limitededestruibles = 5;
    public float radiodetection = 5f;

    void Start()
    {
    
    }

    // Update is called once per frame  
    void Update()
    {
         tiempo += Time.deltaTime;
       if(tiempo >= spawnrate && contadordedestroyables < limitededestruibles)
       {
            spawn();
            tiempo = 0;

       }


    }

    void spawn()
    {
        GameObject Plano = GameObject.FindGameObjectWithTag("plano");
        Bounds b = Plano.GetComponent<Renderer>().bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float z = Random.Range(b.min.z, b.max.z);
        float y = 0;

        Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
    }

    void detectar()
    {
        Vector3 inicio = transform.position;
        Collider[] objetosdetectados = Physics.OverlapSphere(inicio, radiodetection);
        foreach (Collider col in  objetosdetectados)
        {
            if(col.CompareTag("destroyable"))
            {
                contadordedestroyables+=1;
            }
        }
    }
}
