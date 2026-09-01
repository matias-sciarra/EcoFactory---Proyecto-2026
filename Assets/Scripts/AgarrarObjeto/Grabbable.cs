using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public string itemId = "cubo_negro";

    public bool IsHeld { get; private set; }

    private Rigidbody rb;
    private Collider col;
    private Transform originalParent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalParent = transform.parent;
    }

    public void OnGrabbed(Transform holdPoint)
    {
        IsHeld = true;

        rb.isKinematic = true;
        rb.useGravity = false;
        if (col != null) col.enabled = false; // así el raycast no choca contra lo que sostenemos

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDropped()
    {
        IsHeld = false;
        transform.SetParent(originalParent);

        rb.isKinematic = false;
        rb.useGravity = true;
        if (col != null) col.enabled = true;
    }

    public void OnConsumedByMachine()
    {
        IsHeld = false;
        Destroy(gameObject);
    }
}
