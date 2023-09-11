using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour
{
    public int playerID;
    //Components
    [System.NonSerialized]public Rigidbody2D rb;
    [System.NonSerialized] public Animator animator;
    [System.NonSerialized] public Drill drill;
    //Variables
    public float moveSpeed;
    [System.NonSerialized] public float moveDirection;
    [System.NonSerialized] public float rotationDirection;
    public float moveSpeedMultiplier = 1f;
    [Space] public float rotationSpeed;

    //State Machine
    IState currentState;

    public DefaultState defaultState;
    public StunnedState stunnedState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultState = new DefaultState();
        stunnedState = new StunnedState();
    }
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        drill = GetComponentInChildren<Drill>();
        currentState = defaultState;
    }
    public void StunTarget(float stunDurationInSeconds)
    {
        stunnedState.stunDuration = stunDurationInSeconds;
        ChangeState(stunnedState);
    }
    public void ChangeState(IState state)
    {
        currentState.OnExit(this);
        currentState = state;
        currentState.OnEnter(this);
    }
    public void TakeDamage()
    {
        Destroy(gameObject);
    }
    public void OnDirectDrillHit()
    {
        currentState.OnDirectDrillHit(this);
    }
    public void OnMoveInputReceive()
    {
        currentState.OnMovementInputReceive(this);
    }
    private void Update()
    {
        currentState.OnUpdate(this);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Drill"))
        {
            //TakeDamage();
        }
    }
}
public interface IState
{
    public void OnEnter(PlayerControl playerControl);
    public void OnDirectDrillHit(PlayerControl playerControl);
    public void OnMovementInputReceive(PlayerControl playerControl);
    public void OnUpdate(PlayerControl playerControl);
    public void OnExit(PlayerControl playerControl);
}
public class DefaultState : IState
{
    public void OnEnter(PlayerControl playerControl)
    {

    }
    public void OnDirectDrillHit(PlayerControl playerControl)
    {
        
        playerControl.StunTarget(0.5f);
    }
    public void OnMovementInputReceive(PlayerControl playerControl)
    {
        playerControl.animator.SetFloat("Speed", playerControl.moveDirection);
    }
    public void OnUpdate(PlayerControl playerControl)
    {
        playerControl.rb.angularVelocity = playerControl.rotationSpeed * playerControl.rotationDirection * (-100);
        playerControl.rb.velocity = playerControl.transform.up * playerControl.moveSpeedMultiplier * playerControl.moveSpeed * playerControl.moveDirection;
    }
    public void OnExit(PlayerControl playerControl)
    {
        playerControl.animator.SetFloat("Speed", 0);
    }
}
public class StunnedState : IState
{
    public float stunDuration;
    public void OnEnter(PlayerControl playerControl)
    {
        
        playerControl.StartCoroutine(LeaveStunStateCooldown(playerControl));
    }
    public void OnDirectDrillHit(PlayerControl playerControl)
    {
       
    }
    public void OnMovementInputReceive(PlayerControl playerControl)
    {

    }
    public void OnUpdate(PlayerControl playerControl)
    {

    }
    public void OnExit(PlayerControl playerControl)
    {

    }
    private IEnumerator LeaveStunStateCooldown(PlayerControl playerControl)
    {
        yield return new WaitForSeconds(stunDuration);
        playerControl.ChangeState(playerControl.defaultState);
    }
}