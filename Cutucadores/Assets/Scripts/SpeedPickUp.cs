using Fusion;
using UnityEngine;

public class SpeedPickUp : NetworkBehaviour
{
    [Header("Components")]
    private CapsuleCollider pickupCollider;
    public AudioSource playAudioSourceSpeedPickUp;

    [Header("Respawn")]
    public readonly float respawnTime = 15f;

    [Header("Item")]
    public Transform itemVisual;
    public float itemRotationSpeed;
    public float itemYDelta;
    public float itemYDeltaFrequency;

    [Header("PickupArea")]
    public Transform pickupAreaVisual;
    Vector3 startingPosition;

    [Networked] TickTimer respawnTickTimer { get; set; }
    [Networked] TickTimer respawningTickTimer { get; set; }
    private void Awake()
    {
        pickupCollider = GetComponent<CapsuleCollider>();
    }
    public override void Spawned()
    {
        base.Spawned();
        startingPosition = itemVisual.localPosition;
    }
    private void FixedUpdate()
    {
        //Rotate over time and wiggle up and down
        itemVisual.Rotate(0, itemRotationSpeed, 0, Space.Self);
        itemVisual.localPosition = new Vector3(itemVisual.localPosition.x,
            startingPosition.y + Mathf.Abs(Mathf.Sin(Time.time * itemYDeltaFrequency)) * itemYDelta
            , itemVisual.localPosition.z);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.parent.TryGetComponent(out NetworkCharacterDrillController drillController))
            {
                GetPowerUp();
                drillController.StartSpeedBoost();
            }
        }
    }
    public void GetPowerUp()
    {
        pickupCollider.enabled = false;
        itemVisual.gameObject.SetActive(false);
        pickupAreaVisual.gameObject.SetActive(false);

        PlayOnPickUpAudio();

        //Inicia um timer na network para o respawn
        respawnTickTimer = TickTimer.CreateFromSeconds(Runner, respawnTime);
    }

    #region Respawn 
    public override void FixedUpdateNetwork()
    {
        if (respawnTickTimer.Expired(Runner))
        {
            Respawn();
        }
        else if (respawningTickTimer.Expired(Runner))
        {
            ReEnablePowerUp();
        }
    }
    public void Respawn()
    {
        pickupAreaVisual.gameObject.SetActive(true);
        respawnTickTimer = TickTimer.None;
        respawningTickTimer = TickTimer.CreateFromSeconds(Runner, 0.25f);
    }
    public void ReEnablePowerUp()
    {
        itemVisual.gameObject.SetActive(true);
        pickupCollider.enabled = true;
        respawningTickTimer = TickTimer.None;
    }
    #endregion

    public virtual void PlayOnPickUpAudio()
    {
        playAudioSourceSpeedPickUp.Play();
    }
}
