using UnityEngine;

public class BuildManager : MonoBehaviour
{
public GameObject player;
public Behaviour fpsController;
public Transform fpsCamera;
public Transform buildPosition;
public PlacementController placementController;
public GameObject buildingPrefab;

private bool buildMode;

private Vector3 oldPlayerPosition;
private Quaternion oldPlayerRotation;

private Vector3 oldCameraLocalPosition;
private Quaternion oldCameraLocalRotation;

void Update()
{
    if (Input.GetKeyDown(KeyCode.B))
    {
        if (buildMode)
            ExitBuildMode();
        else
            EnterBuildMode();
    }
}

void EnterBuildMode()
{
    buildMode = true;

    oldPlayerPosition = player.transform.position;
    oldPlayerRotation = player.transform.rotation;

    oldCameraLocalPosition = fpsCamera.localPosition;
    oldCameraLocalRotation = fpsCamera.localRotation;

    fpsController.enabled = false;

    player.transform.position = buildPosition.position;

    player.transform.rotation = Quaternion.identity;

    fpsCamera.localPosition = Vector3.zero;
    fpsCamera.localRotation = Quaternion.Euler(90f, 0f, 0f);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    placementController.StartPlacement(buildingPrefab);
}

void ExitBuildMode()
{
    buildMode = false;

    placementController.CancelPlacement();

    fpsCamera.localPosition = oldCameraLocalPosition;
    fpsCamera.localRotation = oldCameraLocalRotation;

    player.transform.position = oldPlayerPosition;
    player.transform.rotation = oldPlayerRotation;

    fpsController.enabled = true;

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

}