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
    public float InputMultiplier = 1;
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
        ApplyConcussion(playerControl, 0.5f);
    }
    public void OnMovementInputReceive(PlayerControl playerControl)
    {
        playerControl.animator.SetFloat("Speed", playerControl.moveDirection);
    }
    public void OnUpdate(PlayerControl playerControl)
    {
        playerControl.rb.angularVelocity = playerControl.rotationSpeed * playerControl.rotationDirection * (-100) * playerControl.InputMultiplier;
        playerControl.rb.AddForce(playerControl.transform.up * playerControl.moveSpeedMultiplier * playerControl.moveSpeed * playerControl.moveDirection * playerControl.InputMultiplier, ForceMode2D.Force);
    }
    public void OnExit(PlayerControl playerControl)
    {
        playerControl.animator.SetFloat("Speed", 0);
    }
    private IEnumerator ApplyConcussion(PlayerControl playerControl, float concussionDuration)
    {
        float timer = 0;
        while (timer < concussionDuration)
        {
            playerControl.InputMultiplier = (timer / concussionDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        playerControl.InputMultiplier = 1;
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