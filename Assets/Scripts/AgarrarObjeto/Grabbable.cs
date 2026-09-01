using UnityEngine;

public class Grabbable : MonoBehaviour
{
    //Id del item que queres agarrar, no hace falta pero se puede usar
    public string itemId = "Pilon de botellas";

    public bool IsHeld { get; private set; }

    private Rigidbody rb;
    private Collider col;
    private Transform originalParent;

    //Pone Rigidbody y Collider antes de que empiece el update y start para que funcione y no le falte a ningun getcomponent
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalParent = transform.parent;
    }

    //Funcion del momento donde tenes agarrado el objeto
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

    //Funcion para dropear el objeto, o cuando se dropea
    public void OnDropped()
    {
        IsHeld = false;
        transform.SetParent(originalParent);

        rb.isKinematic = false;
        rb.useGravity = true;
        if (col != null) col.enabled = true;
    }

    //Funcion para cuando la maquina agarra el objeto
    public void OnConsumedByMachine()
    {
        IsHeld = false;
        Destroy(gameObject);
    }
}
