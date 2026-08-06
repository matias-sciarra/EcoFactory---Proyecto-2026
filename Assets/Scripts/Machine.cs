using UnityEngine;

public class Machine : MonoBehaviour
{
    public Transform inputPoint;

    public void ReceiveTrash(Trash trash)
    {
        trash.transform.position = inputPoint.position;
        trash.transform.rotation = inputPoint.rotation;

        trash.isHeld = false;

        TrashManager.Instance.ClearHeldTrash();
    }
}
