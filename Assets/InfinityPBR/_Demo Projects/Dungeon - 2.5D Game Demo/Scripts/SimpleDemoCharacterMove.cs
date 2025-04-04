using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDemoCharacterMove : MonoBehaviour
{
    
    public float speed = 2f;
    public float speedRamp = 2f;
    public float rotationSpeed = 180f;
    private float moveSpeed = 0f;
    private float moveHor;
    private float moveVer;
    private Rigidbody rb;
    public Animator animator;
    public string animatorLocomotion = "Locomotion";

    void Awake()
    { 
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (GetComponent<SimpleDemoControllerDungeon>())
            GetComponent<SimpleDemoControllerDungeon>().enabled = false;
    }
  
    void LateUpdate()
    {
        DoRotation();
        DoMovement();
    }

    private void DoMovement()
    {
        moveVer = 0;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            moveVer = 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            moveVer = -1;
        
        if (moveVer != 0)
            moveSpeed = Mathf.MoveTowards(moveSpeed, speed * moveVer, speedRamp * Time.deltaTime);
        else
            moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, speedRamp * Time.deltaTime);
        
        //if (!useRootMotion)
        //    transform.position += transform.forward * Time.deltaTime * moveSpeed;
        if (animator)
            animator.SetFloat(animatorLocomotion, moveSpeed);
    }

    private void DoRotation()
    {
        moveHor = 0;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            moveHor = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            moveHor = 1;

        float rotationAmount = moveHor * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotationAmount, 0f, Space.Self);
        
    }
}
