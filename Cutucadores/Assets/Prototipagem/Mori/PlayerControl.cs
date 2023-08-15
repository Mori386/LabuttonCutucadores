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
        animator.SetFloat("Speed", moveDirection);
    }
    private void Update()
    {
        rb.angularVelocity = rotationSpeed * rotationDirection * (-100);
        rb.velocity = transform.up * moveSpeedMultiplier * moveSpeed * moveDirection;
    }
}
