using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    //Components
    private Rigidbody2D rb;
    private Animator animator;
    //Variables
    [SerializeField] private float moveSpeed;
    [System.NonSerialized] public float moveDirection;
    [System.NonSerialized] public float rotationDirection;
    [SerializeField] public float moveSpeedMultiplier = 1f;
    [Space, SerializeField] private float rotationSpeed;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }
    public void OnMoving()
    {
        animator.SetBool("isMoving", !moveDirection.Equals(0));
    }
    private void FixedUpdate()
    {
        rb.rotation = rb.rotation + rotationSpeed * rotationDirection * (-1);
    }
    private void Update()
    {
        rb.velocity = transform.up * moveSpeedMultiplier * moveSpeed * moveDirection;
    }
}
