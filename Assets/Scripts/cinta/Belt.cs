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

    private void Start()
    {
    _beltManager = FindObjectOfType<BeltManager>();
    gameObject.name = $"Belt: {_beltID++}";
    }

    private void Update()
    {


        if (beltItem != null && beltItem.item != null && !isMoving)
        {
            StartCoroutine(StartBeltMove());
        }
    }

    public Vector3 GetItemPosition()
    {
        var padding = 0.3f;
        var position = transform.position;
        return new Vector3(position.x, position.y + padding, position.z);
    }

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
