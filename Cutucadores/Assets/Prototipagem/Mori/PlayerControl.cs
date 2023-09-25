using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControl : MonoBehaviour
{
    public int playerID;
    //Components
    [System.NonSerialized] public Rigidbody2D rb;
    [System.NonSerialized] public Collider2D[] colliders;
    [System.NonSerialized] public Animator animator;
    [System.NonSerialized] public SpriteRenderer spriteRenderer;
    [System.NonSerialized] public Drill drill;
    [System.NonSerialized] public SpriteRenderer hpSlots;
    [System.NonSerialized] public ParticleSystem deathParticle;
    //Variables
    public float moveSpeed;
    [System.NonSerialized] public float moveDirection;
    [System.NonSerialized] public float rotationDirection;
    public float moveSpeedMultiplier = 1f;
    [Space] public float rotationSpeed;

    public int hp = 3;

    //State Machine
    IState currentState;

    public DefaultState defaultState;
    public StunnedState stunnedState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultState = new DefaultState();
        stunnedState = new StunnedState();

        Collider2D[] collidersInPlayer = GetComponentsInChildren<Collider2D>();
        colliders = new Collider2D[collidersInPlayer.Length + 1];
        colliders[0] = GetComponent<Collider2D>();
        for (int i = 0; i < collidersInPlayer.Length; i++)
        {
            colliders[i + 1] = collidersInPlayer[i];
        }

        hpSlots = transform.Find("HP").GetChild(0).GetComponent<SpriteRenderer>();
        if (hpSlots == null)
        {
            hpSlots = transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>();
        }
        if (transform.Find("DeathExplosion").TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            deathParticle = ps;
        }
        else
        {
            if (transform.GetChild(4).TryGetComponent<ParticleSystem>(out ParticleSystem deathPs))
            {
                deathParticle = deathPs;
            }
        }
    }
    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = animator.GetComponent<SpriteRenderer>();
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
    public void RecoilOnHit(Vector3 direction)
    {
        StunTarget(0.125f);
        rb.AddForce(direction * (-20f), ForceMode2D.Impulse);
    }
    public void TakeDamage(Collision2D collision)
    {
        if (hp - 1 <= 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
            StartCoroutine(DeathAnimation());
        }
        else
        {
            Vector2 forceApplied = (collision.transform.position - transform.position).normalized;
            RecoilOnHit(forceApplied);

            hp--;
            UpdateHPBar();
            StartCoroutine(DamageTakenEffect());
        }
    }
    public float frac(float value)
    {
        return value - Mathf.Floor(value);
    }
    public IEnumerator LowHpVisual()
    {
        while (true)
        {
            spriteRenderer.color = new Color(1, 0.52f + frac(Time.time) * 0.48f, 0.52f + frac(Time.time) * 0.48f, 1);
            yield return null;
        }
    }
    public IEnumerator DeathAnimation()
    {
        deathParticle.Play();
        float timer = 0;
        while (timer < 1)
        {
            transform.rotation = Quaternion.Euler(0, 0, frac(Time.time) * 360);
            transform.localScale = new Vector3(1 - timer, 1 - timer, 1 - timer);
            yield return null;
            timer += Time.deltaTime;
        }
        gameObject.SetActive(false);
    }
    public IEnumerator FallAnimation(Vector3 holePos)
    {
        float timer = 0;
        Vector3 scale = transform.localScale;
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        while (timer < 1)
        {
            spriteRenderer.color = new Color(1f-timer, 1f - timer, 1f - timer, 1);
            transform.position = holePos;
            transform.rotation = Quaternion.Euler(0, 0, frac(Time.time) * 360);
            transform.localScale = new Vector3(1 - timer, 1 - timer, 1 - timer);
            yield return null;
            timer += Time.deltaTime;
        }
        yield return new WaitForSeconds(0.25f);
        transform.localScale = Vector3.zero;
        transform.position = GameManager.Instance.playerSpawnpoint[GameManager.Instance.myID].position;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = scale;
        timer = 0;
        while (timer < 0.75f)
        {
            transform.position = GameManager.Instance.playerSpawnpoint[GameManager.Instance.myID].position;
            spriteRenderer.color = new Color(1, 1, 1,Mathf.Abs(Mathf.Sin(timer*10f)));
            yield return null;
            timer += Time.deltaTime;
        }
        spriteRenderer.color = Color.white;
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }
    }
    public IEnumerator DamageTakenEffect()
    {
        Color color = spriteRenderer.color;
        spriteRenderer.color = new Color(1, 133 / 255, 133 / 255, 1);
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = color;
        if (hp.Equals(1))
        {
            StartCoroutine(LowHpVisual());
        }
    }
    public void UpdateHPBar()
    {
        hpSlots.transform.localScale = new Vector3(0.2f * hp, hpSlots.transform.localScale.y, hpSlots.transform.localScale.z);
        switch (hp)
        {
            case 1:
                hpSlots.color = Color.red;
                break;
            case 2:
                hpSlots.color = Color.yellow;
                break;
            case 3:
            default:
                hpSlots.color = Color.green;
                break;
        }
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
            TakeDamage(collision);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fall"))
        {
            StartCoroutine(FallAnimation(collision.transform.position));
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

        playerControl.StunTarget(0.15f);
        playerControl.rb.AddForce(forceApplied.normalized * (-10f), ForceMode2D.Impulse);
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
    public Color color;
    public void OnEnter(PlayerControl playerControl)
    {
        color = playerControl.spriteRenderer.color;
        playerControl.StartCoroutine(ApplyStun(playerControl));
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
        playerControl.spriteRenderer.color = color;
    }
    private IEnumerator ApplyStun(PlayerControl playerControl)
    {
        float timer = 0;
        float darkenAmount = 0.85f;
        while (timer <= stunDuration)
        {
            float darkenApplied = darkenAmount + timer / stunDuration * (1 - darkenAmount);
            playerControl.spriteRenderer.color = new Color(darkenApplied, darkenApplied, darkenApplied, 1);
            yield return null;
            timer += Time.deltaTime;
        }
        playerControl.ChangeState(playerControl.defaultState);
    }
}