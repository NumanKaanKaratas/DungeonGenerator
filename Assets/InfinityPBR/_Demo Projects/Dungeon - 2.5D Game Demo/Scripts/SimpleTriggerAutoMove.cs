using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTriggerAutoMove : MonoBehaviour
{
    private SimpleDemoControllerDungeon controller;
    private SimpleDemoCharacterMove movement;
    private Rigidbody rb;
    private Animator animator;
    public float speed = 6f;
    private Vector3 desiredPosition;
    private bool autoMoving = false;

    private float cooloff = 0f;

    private void Awake()
    {
        controller = GetComponent<SimpleDemoControllerDungeon>();
        movement = GetComponent<SimpleDemoCharacterMove>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        desiredPosition = transform.position;
    }

    void Update()
    {
        if (cooloff > 0)
            cooloff -= Time.deltaTime;
        
        if (!autoMoving)
            return;
        
        if (transform.position == desiredPosition)
        {
            if (controller)
                controller.enabled = true;
            if (movement)
                movement.enabled = true;
            if (rb)
                rb.isKinematic = false;
            if (animator)
                animator.enabled = true;
            autoMoving = false;
            cooloff = 3f;
        }
        else
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, desiredPosition, step);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (autoMoving || cooloff > 0)
            return;
        if (collider.GetComponent<SimpleTriggerDetails>())
        {
            autoMoving = true;
            if (animator)
                animator.enabled = false;
            if (controller)
                controller.enabled = false;
            if (movement)
                movement.enabled = false;
            if (rb)
                rb.isKinematic = true;
            desiredPosition = collider.GetComponent<SimpleTriggerDetails>().connection.position;

            RotatePlayer(collider.GetComponent<SimpleTriggerDetails>().rotationAmount);
        }
    }

    private void RotatePlayer(float value)
    {
        Vector3 newAngles = transform.eulerAngles;
        newAngles.y += value;
        transform.eulerAngles = newAngles;
    }
}
