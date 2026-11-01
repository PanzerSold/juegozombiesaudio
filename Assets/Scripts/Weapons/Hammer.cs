using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Hammer : MonoBehaviour
{
    [SerializeField] private float hammerRadius = 5.5f;
    [SerializeField] private float coolDown = 15.0f;
    [SerializeField] private bool isOnCooldown = false;
    [SerializeField] private RawImage hammerImage;
    [SerializeField] private TextMeshProUGUI HammerText;
    [SerializeField] private PlayerInputs pi;

    [Header("Sounds")]
    [SerializeField] private AudioSource SFX_Hammer;
    [SerializeField] private AudioSource SFX_HammerOnCooldown;

    [SerializeField] private ParticleSystem hammerParticle;
    private PlayerHealth ph;
    private Color originalColor;
    private float cooldownTimer;
    private int timeLeft;
    private int lastTimeLeft;
    void Awake()
    {
        pi = GetComponent<PlayerInputs>();
        ph = GetComponent<PlayerHealth>();
        cooldownTimer = coolDown;
        lastTimeLeft = -1;

        if (hammerImage != null) originalColor = hammerImage.color;
    }

    private void OnEnable()
    {
        UnityEvents.onCooldownChanged.AddListener(ChangeColor);
    }

    private void OnDisable()
    {
        UnityEvents.onCooldownChanged.RemoveListener(ChangeColor);
    }

    private void ChangeColor(bool onCooldown)
    {
        hammerImage.color = onCooldown ? Color.black : originalColor;
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        timeLeft = Mathf.CeilToInt(coolDown - cooldownTimer);

        if (timeLeft <= 0)
        {
            if (isOnCooldown)
            {
                isOnCooldown = false;
                UnityEvents.onCooldownChanged.Invoke(false);
            }

            HammerText.text = "Ready";
        }
        else
        {
            if (timeLeft != lastTimeLeft)
            {
                HammerText.text = timeLeft + "s";
                lastTimeLeft = timeLeft;
            }
        }

        if (pi.skillAction.WasPressedThisFrame() && !ph.IsDying)
        {
            if (!isOnCooldown)
            {
                HammerStun();
                SFX_Hammer.Play();
                if (hammerParticle != null) hammerParticle.Play();
                isOnCooldown = true;
                cooldownTimer = 0;
                UnityEvents.onCooldownChanged.Invoke(true);
            }
            else
            {
                SFX_HammerOnCooldown.Play();
            }
        }



    }

    void HammerStun()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, hammerRadius);
        foreach (Collider collider in colliders)
        {

            ZombieBase zombie = collider.GetComponent<ZombieBase>();
            if (zombie != null)
            {
                zombie.Stun();
            }
        }

    }


    void SonidoCooldown()
    {
        
    }



}
