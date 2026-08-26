using UnityEngine;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;

    public Transform holdPoint;
    public float pickUpDistance = 3f;

    private Trash heldTrash;

    void Awake()
    {
        Instance = this;
    }

    void Update()
{
    if (heldTrash != null)
    {
        heldTrash.transform.position = holdPoint.position;
        heldTrash.transform.rotation = holdPoint.rotation;
    }

    if (Input.GetKeyDown(KeyCode.E))
    {
        if (heldTrash == null)
            TryPickUp();
        else
            TryDeposit();
    }

    if (Input.GetKeyDown(KeyCode.Q))
    {
        DropOnGround();
    }
}

    void TryPickUp()
    {
        if (heldTrash != null)
            return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickUpDistance))
{
    Trash trash = hit.collider.GetComponent<Trash>();

    if (trash != null)
    {
        heldTrash = trash;
        trash.isHeld = true;

        Collider col = trash.GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        Rigidbody rb = trash.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}
    }

    public Trash GetHeldTrash()
    {
        return heldTrash;
    }

    public void ClearHeldTrash()
    {
    heldTrash = null;
    }

    void TryDeposit()
{
    Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));

    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, pickUpDistance))
    {
        Machine machine = hit.collider.GetComponent<Machine>();

        if (machine != null)
        {
            machine.ReceiveTrash(heldTrash);
        }
    }
}

void DropOnGround()
{
    if (heldTrash == null)
        return;

    heldTrash.isHeld = false;

    heldTrash.transform.position =
        holdPoint.position + Camera.main.transform.forward;

    heldTrash.transform.parent = null;
    Collider col = heldTrash.GetComponent<Collider>();

    if (col != null)
    col.enabled = true;

    Rigidbody rb = heldTrash.GetComponent<Rigidbody>();

    if (rb != null)
    {   
    rb.isKinematic = false;
    rb.useGravity = true;
    }

    heldTrash = null;
    }
}
