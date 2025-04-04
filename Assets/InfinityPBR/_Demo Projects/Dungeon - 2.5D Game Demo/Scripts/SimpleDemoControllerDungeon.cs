using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDemoControllerDungeon : MonoBehaviour
{
    public float speed = 2f;
    public float speedRamp = 2f;
    private float moveSpeed = 0f;
    private float moveHor;
    private float moveVer;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void LateUpdate()
    {
        moveVer = 0;
        moveHor = 0;
        
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            moveHor = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            moveHor = 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            moveVer = 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            moveVer = -1;

        if (moveVer != 0 || moveHor != 0)
            moveSpeed = Mathf.MoveTowards(moveSpeed, speed, speedRamp * Time.deltaTime);
        else
            moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, speedRamp * Time.deltaTime);

        //moveHor *= moveSpeed;
       // moveVer *= moveSpeed;

        //transform.Translate(moveHor, 0, moveVer);
        
        Vector3 tempVect = new Vector3(moveHor, 0, moveVer);
        tempVect = tempVect.normalized * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + tempVect);
    }
}
