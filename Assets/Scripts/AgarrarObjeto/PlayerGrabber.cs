using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    [Header("Referencias")]
    public Transform rayOrigin;
    public Transform holdPoint;

    [Header("Configuración")]
    public float grabDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    private Grabbable heldItem;

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (heldItem == null)
                TryGrab();
            else
                TryDropOrInsert();
        }
    }

    private void TryGrab()
    {
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, grabDistance))
        {
            Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
            if (grabbable != null && !grabbable.IsHeld)
            {
                heldItem = grabbable;
                heldItem.OnGrabbed(holdPoint);
            }
        }
    }

    private void TryDropOrInsert()
    {
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, grabDistance))
        {
            Debug.Log("El rayo pegó contra: " + hit.collider.gameObject.name);

            MachineSlot machine = hit.collider.GetComponent<MachineSlot>();
            if (machine == null)
            {
                Debug.Log("Ese objeto NO tiene el componente MachineSlot");
            }
            else if (machine.TryInsert(heldItem))
            {
                Debug.Log("Insertado con éxito");
                heldItem = null;
                return;
            }
            else
            {
                Debug.Log("La máquina rechazó el item (TryInsert devolvió false)");
            }
        }
        else
        {
            Debug.Log("El rayo no pegó contra nada");
        }

        Vector3 dropPos = rayOrigin.position + rayOrigin.forward * 1.2f;
        heldItem.transform.position = dropPos;
        heldItem.OnDropped();
        heldItem = null;
    }
}