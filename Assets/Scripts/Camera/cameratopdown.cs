using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameratopdown : MonoBehaviour
{
    public GameObject player;
    public float upset = 5f;
    void Start()
    {
        
    }

    void LateUpdate()
    {
        getplayerposition();
    }

    void getplayerposition()
    {
        Vector3 playerPosition = player.transform.position;
        transform.position = new Vector3(playerPosition.x, playerPosition.y + upset, playerPosition.z);
    }
}
