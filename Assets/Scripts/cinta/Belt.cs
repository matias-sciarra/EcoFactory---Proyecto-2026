using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : MonoBehaviour
{
    private static int _beltID = 0;

    public Belt beltInSequence;
    public BeltItem beltItem;
    public machine MachineInSequence;
    public bool isSpaceTaken;
    private bool isMoving = false;
    private BeltManager _beltManager;

    // Busca en la escena un objeto que tenga un script beltmanager o un componente como ese y lo asigna a _beltmanager 
    // Para facilitar aplicar funciones
    private void Start()
    {
    _beltManager = FindObjectOfType<BeltManager>();
    gameObject.name = $"Belt: {_beltID++}";
    }

    //Controla cuando la cinta empieza a mover un objeto y cuando no
    private void Update()
    {

        if (beltItem != null && beltItem.item != null && !isMoving)
        {
            StartCoroutine(StartBeltMove());
        }
    }

    //Le da la posicion en la cinta al objeto salido de la maquina
    public Vector3 GetItemPosition()
    {
        var padding = 0.3f;
        var position = transform.position;
        return new Vector3(position.x, position.y + padding, position.z);
    }

    //Hace que la cinta empiece el movimiento de objetos
private IEnumerator StartBeltMove()
{
    isMoving = true;
    isSpaceTaken = true;


    if (beltItem.item != null && beltInSequence != null && beltInSequence.isSpaceTaken == false)
    {

        Vector3 toPosition = beltInSequence.GetItemPosition();
        beltInSequence.isSpaceTaken = true;
        var step = _beltManager.speed * Time.deltaTime;

        while (beltItem.item.transform.position != toPosition)
        {
            beltItem.item.transform.position = 
                Vector3.MoveTowards(beltItem.item.transform.position, toPosition, step);
            yield return null;
        }

        isSpaceTaken = false;
        beltInSequence.beltItem = beltItem;
        beltItem = null;
    }
    else
    {
    }

    isMoving = false;
}

    //Pasa el objeto de un objeto a otra, (ya que no es toda una cinta en conjunto, son varias partes)
    private Belt FindNextBelt()
    {
        Transform currentBeltTransform = transform;
        RaycastHit hit;

        var forward = transform.forward;

        Ray ray = new Ray(currentBeltTransform.position, forward);

        if (Physics.Raycast(ray, out hit, 1f))
        {
            Belt belt = hit.collider.GetComponent<Belt>();

            if (belt != null)
                return belt;
        }

        return null;
    }
}
