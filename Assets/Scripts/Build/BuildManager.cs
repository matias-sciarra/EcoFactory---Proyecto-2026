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

// Define que pasa cuando tocas la b y llama a las funciones exitbuildmode y enterbuildmode depende que pase.
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

// Entra al modo build
void EnterBuildMode()
{
    buildMode = true;

    oldPlayerPosition = player.transform.position;
    oldPlayerRotation = player.transform.rotation;

    oldCameraLocalPosition = fpsCamera.localPosition;
    oldCameraLocalRotation = fpsCamera.localRotation;

    fpsController.enabled = false;

    player.transform.rotation = Quaternion.identity;

    fpsCamera.position = buildPosition.position;
    fpsCamera.rotation = Quaternion.Euler(90f, 0f, 0f);

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    placementController.StartPlacement(buildingPrefab);
}

void ExitBuildMode()
{
    buildMode = false;

    placementController.CancelPlacement();

    fpsController.enabled = false;

    player.transform.position = oldPlayerPosition;
    player.transform.rotation = oldPlayerRotation;

    fpsCamera.localPosition = oldCameraLocalPosition;
    fpsCamera.localRotation = oldCameraLocalRotation;

    CharacterController characterController =
        player.GetComponent<CharacterController>();

    if (characterController != null)
    {
        characterController.enabled = false;
        characterController.enabled = true;
    }

    fpsController.enabled = true;

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    }   
}