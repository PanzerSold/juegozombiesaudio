using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private static float moveSpeed = 4f;
    private float wingsSpeed = moveSpeed * 1.5f;
    private float superSpeed = moveSpeed * 1.7f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float friction = 5f;
    private bool hasWings = false;
    public bool shieldAvailable = true;
    [SerializeField] private float shieldCooldown = 30f;
    private float shieldCooldownTimer;
    private int timeLeft;
    private int lastTimeLeft;

    [SerializeField] private TextMeshProUGUI textPoints;
    [SerializeField] private AudioSource BarrierBuildSfx;
    [SerializeField] private AudioSource SFX_PowerUp; //Sfx del power up
    [SerializeField] private AudioSource SFX_Steps; //Sfx de los pasos
    [SerializeField] private AudioClip stepsClip; //Sfx de los pasos
    private PlayerShoot playerAmmo;
    private string pointsStart;
    public int points = 0;
    public enum PlayerBonus
    {
        DoublePoints,
        HeadShots,
        Discount,
        Wings,
        MaxAmmo
    }

    public enum PlayerPerks
    {
        Shielded,
        Speed,
        Haste,
        Rebirth,
        Scavenger,
        Vampirism,
        CriticalShots,
        ExplosiveBullets
    }

    public float[] activePowerUps;
    [SerializeField] private Dictionary<PlayerBonus, Coroutine> powerUpTimers = new Dictionary<PlayerBonus, Coroutine>();
    public bool HasPowerUp(PlayerBonus bonus)
    {
        return powerUpTimers.ContainsKey(bonus);
    }
    public bool[] activePerks;
    private float horizontalMovement;
    private float forwardMovement;
    private float verticalMovement = -2f;
    private CharacterController cc;

    private float walktimer = 0f;

    [SerializeField] private float walkInterval = 0.5f;
    [SerializeField] private PlayerInputs pi;
    [SerializeField] private Inventory inv;
    [SerializeField] private PlayerHealth ph;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private ParticleSystem PlayerFire;
    [SerializeField] private ParticleSystem psWings;
    [SerializeField] private PauseMenu pauseMenu;

    private Animator anim;

    public bool IsVaulting { get; set; }

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        ph = GetComponent<PlayerHealth>();
        cc = GetComponent<CharacterController>();
        pi = GetComponent<PlayerInputs>();
        playerAmmo = GetComponent<PlayerShoot>();
        pointsStart = textPoints.text;
        textPoints.text = pointsStart + " " + points;
        shieldCooldownTimer = shieldCooldown;
        lastTimeLeft = -1;
    }

    private void Update()
    {
        if (pi.pauseMenu.WasPressedThisFrame()) pauseMenu.TogglePause();
        if (pauseMenu.isPaused) return;

        shieldCooldownTimer += Time.deltaTime;
        timeLeft = Mathf.CeilToInt(shieldCooldown - shieldCooldownTimer);
        bool isGrounded = cc.isGrounded;
        Vector3 playerMovement;

        if (!shieldAvailable)
        {
            if (timeLeft != lastTimeLeft)
            {
                healthBar.shieldTime.gameObject.SetActive(true);
                healthBar.shieldTimeText.gameObject.SetActive(true);
                healthBar.shieldTimeText.text = timeLeft + "s";
                lastTimeLeft = timeLeft;
            }
        }
        if (playerAmmo.InventoryOpen || IsVaulting)
        {
            anim.SetFloat("Xspeed", 0f);
            anim.SetFloat("Zspeed", 0f);
            return;
        }
        if (isGrounded)
        {
            if (!ph.IsDying) { anim.SetLayerWeight(1, 1f); }
            anim.SetBool("IsGrounded", true);
            playerMovement = pi.moveAction.ReadValue<Vector2>();
            bool hasSpeed = activePerks[(int)PlayerPerks.Speed];
            float currentSpeed;
            if (!hasSpeed && !hasWings) currentSpeed = moveSpeed;
            else if ((hasSpeed && !hasWings) || (!hasSpeed && hasWings)) currentSpeed = wingsSpeed;
            else (currentSpeed) = superSpeed;
            if (ph.IsDying) { currentSpeed = 0; }
            horizontalMovement = Mathf.MoveTowards(horizontalMovement, playerMovement.x * currentSpeed, friction * Time.deltaTime);
            forwardMovement = Mathf.MoveTowards(forwardMovement, playerMovement.y * currentSpeed, friction * Time.deltaTime); verticalMovement = -2f;
            anim.SetFloat("Xspeed", horizontalMovement);
            anim.SetFloat("Zspeed", forwardMovement);
            playerMovement = new Vector3(horizontalMovement, verticalMovement, forwardMovement);
            Quaternion rotacion = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            playerMovement = rotacion * playerMovement;
        }
        else
        {
            anim.SetLayerWeight(1, 0f);
            anim.SetBool("IsGrounded", false);
            verticalMovement += gravity * Time.deltaTime;
            playerMovement = new Vector3(0, verticalMovement, 0);
        }

        cc.Move(playerMovement * Time.deltaTime);
        if (playerMovement.magnitude > 2f && isGrounded && !ph.IsDying)
        {

            walktimer += Time.deltaTime;
            if (walktimer >= walkInterval)
            {
                SFX_Steps.pitch = Random.Range(0.8f, 1.2f); SFX_Steps.PlayOneShot(stepsClip);
                walktimer = 0f;

            }
        }
        else
        {
            walktimer = 0f;
        }
    }

    public IEnumerator Shield()
    {
        shieldCooldownTimer = 0;
        yield return new WaitForSeconds(shieldCooldown);
        healthBar.shieldTime.gameObject.SetActive(false);
        healthBar.shieldTimeText.gameObject.SetActive(false);
        shieldAvailable = true;
        UnityEvents.shielded.Invoke(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interaction"))
        {
            other.GetComponent<Interaction>().PlayerEntered();
        }
        if (other.CompareTag("PowerUp"))
        {
            PowerUp pu = other.GetComponent<PowerUp>();
            if (pu != null)
            {
                PlayerBonus bonus = pu.pu;
                if (SFX_PowerUp != null) SFX_PowerUp.Play(); //Suena al agarrar cualquier mejora (En teoria)
                ActivatePowerUp(bonus);
            }
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Fire") && !PlayerFire.isEmitting)
        {
            ph.LoseHealth();
            PlayerFire.Play();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Interaction"))
        {
            other.GetComponent<Interaction>().PlayerStay();
        }
    }


    private void ActivatePowerUp(PlayerBonus bonus)
    {
        if (bonus == PlayerBonus.DoublePoints) StartCoroutine(healthBar.VisualPowerUpText("Double points"));

        if (bonus == PlayerBonus.Discount) StartCoroutine(healthBar.VisualPowerUpText("Everything's on discount"));

        if (bonus == PlayerBonus.HeadShots) StartCoroutine(healthBar.VisualPowerUpText("Headshots are deadly"));

        if (bonus == PlayerBonus.MaxAmmo)
        {
            Inventory.itemIndex[] weapons = inv.getAllWeapons();

            foreach (Inventory.itemIndex weapon in weapons)
            {
                if (weapon == Inventory.itemIndex.None) continue;

                Inventory.itemIndex ammoType = Inventory.GetAmmoFor(weapon);

                if (ammoType != Inventory.itemIndex.None) inv.FindAndFillFirst(ammoType);
            }

            if (playerAmmo.Gun != null) playerAmmo.Gun.FillMagazine();

            StartCoroutine(healthBar.VisualPowerUpText("All weapons filled"));
            return;
        }
        if (bonus == PlayerBonus.Wings)
        {
            hasWings = true;
            StartCoroutine(healthBar.VisualPowerUpText("You feel faster"));
            if (psWings != null)
            {
                psWings.Play();
            }
        }

        bool alreadyActive = powerUpTimers.TryGetValue(bonus, out Coroutine existingTimer);

        if (alreadyActive) StopCoroutine(existingTimer);

        int index = (int)bonus;

        if (index >= activePowerUps.Length) return;

        float duration = activePowerUps[index];

        powerUpTimers[bonus] = StartCoroutine(RemovePowerUpAfterTime(bonus, duration));

        UnityEvents.onPowerUpTriggered.Invoke();
    }

    private IEnumerator RemovePowerUpAfterTime(PlayerBonus bonus, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (bonus == PlayerBonus.Wings)
        {
            hasWings = false;

            if (psWings != null)
            {
                psWings.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        powerUpTimers.Remove(bonus);

        UnityEvents.onPowerUpTriggered.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interaction"))
        {
            other.GetComponent<Interaction>().PlayerExit();
        }
    }

    public void AwardPoints(int pts)
    {
        if (HasPowerUp(PlayerBonus.DoublePoints)) points += pts;
        points += pts;
        textPoints.text = pointsStart + " " + points;
    }
    public void RemovePoints(int pts)
    {
        points -= pts;
        textPoints.text = pointsStart + " " + points;
    }
    public void PlayBarrierSound()
    {
        BarrierBuildSfx.Play();
    }
}
