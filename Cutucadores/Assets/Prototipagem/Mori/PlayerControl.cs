using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour
{
    public int playerID;
    //Components
    [System.NonSerialized] public Rigidbody2D rb;
    [System.NonSerialized] public Animator animator;
    [System.NonSerialized] public Drill drill;
    //Variables
    public float moveSpeed;
    [System.NonSerialized] public float moveDirection;
    [System.NonSerialized] public float rotationDirection;
    public float moveSpeedMultiplier = 1f;
    public float slowMultiplier = 1;
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
    public void OnDrilltoDrillHit(Transform otherPlayer)
    {
        currentState.OnDirectDrillHit(this, otherPlayer.position);
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
    public void OnDirectDrillHit(PlayerControl playerControl, Vector3 otherPosition);
    public void OnMovementInputReceive(PlayerControl playerControl);
    public void OnUpdate(PlayerControl playerControl);
    public void OnExit(PlayerControl playerControl);
}
public class DefaultState : IState
{
    public void OnEnter(PlayerControl playerControl)
    {

    }
    public void OnDirectDrillHit(PlayerControl playerControl, Vector3 otherPosition)
    {
        Vector2 forceApplied = otherPosition - playerControl.transform.position;
        Debug.Log(forceApplied.normalized);
        playerControl.rb.AddForce(forceApplied.normalized * (-10f), ForceMode2D.Impulse);
        SlowApplied(playerControl, 3);
    }
    public void OnMovementInputReceive(PlayerControl playerControl)
    {
        playerControl.animator.SetFloat("Speed", playerControl.moveDirection);
    }
    public void OnUpdate(PlayerControl playerControl)
    {
        playerControl.rb.angularVelocity = playerControl.rotationSpeed * playerControl.rotationDirection * (-100) * playerControl.slowMultiplier;
        playerControl.rb.velocity = playerControl.transform.up * playerControl.moveSpeedMultiplier * playerControl.moveSpeed * playerControl.moveDirection * playerControl.slowMultiplier;
    }
    public void OnExit(PlayerControl playerControl)
    {
        playerControl.animator.SetFloat("Speed", 0);
    }
    private IEnumerator SlowApplied(PlayerControl playerControl,float slowDuration)
    {
        yield return new WaitForSeconds(slowDuration);
        
    }
}
public class StunnedState : IState
{
    public float stunDuration;
    public void OnEnter(PlayerControl playerControl)
    {
    }
    public void OnDirectDrillHit(PlayerControl playerControl, Vector3 otherPosition)
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
}