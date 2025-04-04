using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoCameraFollow : MonoBehaviour
{

    public Transform followThis;
    public Transform lookAtThis;
    public Vector3 positionDifference;

    public Vector2 minMax = new Vector2(-20f, 16f);
    

    // Update is called once per frame
    void Update()
    {
        Follow();
        Look();
    }

    private void Follow()
    {
        if (!followThis)
            return;

        Vector3 newPos = followThis.position + positionDifference;
        newPos.x = Mathf.Clamp(newPos.x, minMax.x, minMax.y);
        transform.position = newPos;
    }

    private void Look()
    {
        if (!lookAtThis)
            return;
        
        transform.LookAt(lookAtThis.position);
    }
}
