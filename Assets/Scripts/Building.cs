using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public GameObject player;
    public Transform buildPosition;

    private FPSController fpsController;

    private Vector3 oldPosition;
    private Quaternion oldRotation;

    private bool buildMode = false;

    void Start()
    {
        fpsController = player.GetComponent<FPSController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildMode = !buildMode;

            if (buildMode)
                EnterBuildMode();
            else
                ExitBuildMode();
        }
    }

    void EnterBuildMode()
    {
        oldPosition = player.transform.position;
        oldRotation = player.transform.rotation;

        fpsController.enabled = false;

        player.transform.position = buildPosition.position;
        player.transform.rotation = buildPosition.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitBuildMode()
    {
        player.transform.position = oldPosition;
        player.transform.rotation = oldRotation;

        fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}