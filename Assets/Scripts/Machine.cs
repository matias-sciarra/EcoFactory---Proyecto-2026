using UnityEngine;

public class Machine : MonoBehaviour
{
    public Transform inputPoint;

    public GameObject trashPrefab;
    public Transform spawnPoint;

    public void ReceiveTrash(Trash trash)
    {
        trash.transform.position = inputPoint.position;
        trash.transform.rotation = inputPoint.rotation;

        trash.isHeld = false;

        TrashManager.Instance.ClearHeldTrash();

        Instantiate(trashPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}