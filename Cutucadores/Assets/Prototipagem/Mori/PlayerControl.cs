using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    public enum PlayerTypes
    {
        Input,
        Network
    }
    public PlayerTypes playerType;
    public int playerID;
    //Components
    [System.NonSerialized] public Rigidbody2D rb;
    [System.NonSerialized] public Collider2D[] colliders;
    [System.NonSerialized] public Animator animator;
    [System.NonSerialized] public SpriteRenderer spriteRenderer;
    [System.NonSerialized] public Drill drill;
    [System.NonSerialized] public SpriteRenderer hpSlots;
    [System.NonSerialized] public ParticleSystem deathParticle;
    [System.NonSerialized] public TrailRenderer speedBoostTrail;
    //Variables
    public float moveSpeed;
    [System.NonSerialized] public float moveDirection;
    [System.NonSerialized] public float rotationDirection;
    public float moveSpeedMultiplier = 1f;
    public float rotationSpeedMultiplier = 1f;
    [Space] public float rotationSpeed;

    public int hp = 3;

    //State Machine
    IState currentState;

    public DefaultState defaultState;
    public StunnedState stunnedState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
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
        if (transform.Find("SpeedTrail").TryGetComponent<TrailRenderer>(out TrailRenderer tr))
        {
            speedBoostTrail = tr;
        }
        else
        {
            if (transform.GetChild(5).TryGetComponent<TrailRenderer>(out TrailRenderer speedTr))
            {
                speedBoostTrail = speedTr;
            }
        }
    }
    private void Start()
    {
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
    public void ModifyHP(int newAmount)
    {
        if (newAmount <= 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
            StartCoroutine(DeathAnimation());
        }
        else
        {
            hp = newAmount;
            UpdateHPBar();
            StartCoroutine(DamageTakenEffect());
        }
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


    Coroutine SpeedBoostCoroutine;
    public IEnumerator SpeedBoost(float duration)
    {
        float trailDuration = speedBoostTrail.time;
        float defaultMovespeedMultiplier = moveSpeedMultiplier;
        float defaultRotationSpeedMultiplier = rotationSpeedMultiplier;
        float timer = 0;
        speedBoostTrail.time = 0;
        speedBoostTrail.enabled = true;

        while (timer < duration / 20f)
        {
            speedBoostTrail.time = trailDuration * (timer / (duration / 20f));
            moveSpeedMultiplier = defaultMovespeedMultiplier + 0.5f * (timer / (duration / 20f));
            rotationSpeedMultiplier = defaultRotationSpeedMultiplier + 0.25f * (timer / (duration / 20f));
            timer += Time.deltaTime;
            yield return null;
        }
        speedBoostTrail.time = trailDuration;
        moveSpeedMultiplier = 1.5f;
        rotationSpeedMultiplier = 1.25f;
        yield return new WaitForSeconds(duration / 20 * 18f);
        timer = 0;
        while (timer < duration / 20f)
        {
            speedBoostTrail.time = trailDuration * (1 - (timer / (duration / 20f)));
            moveSpeedMultiplier = defaultMovespeedMultiplier + 0.5f * (1 - (timer / (duration / 20f)));
            rotationSpeedMultiplier = defaultRotationSpeedMultiplier + 0.25f * (1 - (timer / (duration / 20f)));
            timer += Time.deltaTime;
            yield return null;
        }
        moveSpeedMultiplier = defaultMovespeedMultiplier;
        rotationSpeedMultiplier = defaultRotationSpeedMultiplier;
        speedBoostTrail.enabled = false;
        SpeedBoostCoroutine = null;
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
            spriteRenderer.color = new Color(1f - timer, 1f - timer, 1f - timer, 1);
            transform.position = holePos;
            transform.rotation = Quaternion.Euler(0, 0, frac(Time.time) * 360);
            transform.localScale = new Vector3(1 - timer, 1 - timer, 1 - timer);
            yield return null;
            timer += Time.deltaTime;
        }
        hp--;
        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            UpdateHPBar();
            yield return new WaitForSeconds(0.25f);
            transform.localScale = Vector3.zero;
            transform.position = GameManager.Instance.playerSpawnpoint[GameManager.Instance.myID].position;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = scale;
            timer = 0;
            while (timer < 0.75f)
            {
                transform.position = GameManager.Instance.playerSpawnpoint[GameManager.Instance.myID].position;
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Abs(Mathf.Sin(timer * 10f)));
                yield return null;
                timer += Time.deltaTime;
            }
            spriteRenderer.color = Color.white;
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
            }
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
        if (playerType.Equals(PlayerTypes.Input) && collision.CompareTag("Fall"))
        {
            StartCoroutine(FallAnimation(collision.transform.position));
            SendHoleFallInfo(collision.transform.position);
        }
        else if (collision.CompareTag("Speed"))
        {
            if (SpeedBoostCoroutine == null)
            {
                SpeedBoostCoroutine = StartCoroutine(SpeedBoost(5f));
                collision.gameObject.SetActive(false);
            }
        }
    }
    private void OnDisable()
    {
        GameManager.Instance.VirtualCamera.Follow = GameManager.Instance.players[Mathf.Abs(playerID - 1)].transform;
        GameManager.Instance.VirtualCamera.LookAt = GameManager.Instance.players[Mathf.Abs(playerID - 1)].transform;
        GameManager.Instance.ReturnToMenu(2f);
    }

    public void ReceiveOtherPlayerInteractionInfo()
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            Byte[] receiveBytes = Multiplayer.udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            string infoType = returnData.Substring(0, 5);
            Debug.Log(infoType);

            if (infoType == "PosPl")
            {
                if (returnData[5].ToString().Equals(playerID.ToString()))
                {

                }
            }
        }


    }
    public Coroutine SendTransformInfoCoroutine;
    [System.NonSerialized]public string connectedAdress;
    public IEnumerator SendTransformInfo()
    {
        while (true)
        {
            Vector3 roundPos = new Vector3(Mathf.Round(transform.position.x * 10000) / 10000, Mathf.Round(transform.position.y * 10000) / 10000, 0);
            Vector2 roundRot = new Vector2(Mathf.Round(transform.rotation.z * 10000) / 10000, Mathf.Round(transform.rotation.w * 10000) / 10000);
            Multiplayer.SendMessageToIP(connectedAdress, "PosPl" + playerID.ToString() + roundPos.x + "Y" + roundPos.y + "Z" + roundRot.x + "W" + roundRot.y);
            yield return new WaitForFixedUpdate();
        }
    }
    public void SendHitInfo()
    {

    }
    public void SendHoleFallInfo(Vector3 holePos)
    {
        StopCoroutine(SendTransformInfoCoroutine);
        Vector3 roundPos = new Vector3(Mathf.Round(holePos.x * 1000) / 1000, Mathf.Round(holePos.y * 1000) / 1000, 0);
        Multiplayer.SendMessageToIP(connectedAdress, "HFall" + playerID.ToString() + roundPos.x + "Y" + roundPos.y);
        StartCoroutine(SendTransformInfo());
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
            playerControl.rb.angularVelocity = playerControl.rotationSpeed * playerControl.rotationDirection * (-100) * playerControl.rotationSpeedMultiplier;
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
}