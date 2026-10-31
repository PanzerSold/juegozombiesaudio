using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;

public class HealthBar : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Player player;

    
    [Header("Barra de vida")]
    private RawImage[] hearts;
    [SerializeField] private RawImage heart1;
    [SerializeField] private RawImage heart2;
    [SerializeField] private RawImage heart3;
    [SerializeField] private RawImage heart4;
    [SerializeField] private RawImage heart5;
    [SerializeField] private RawImage heart6;
    [SerializeField] private Gradient colorByHealth;
    private Coroutine rebirthCoroutine;
    [SerializeField] private float rebirthDuration = 3f;
    [SerializeField] private Color rebirthStartColor = new Color(0.5f, 0.9f, 1f, 1f);
    [SerializeField] private TextMeshProUGUI textRebirth;

    [Header("Power Ups")]
    private RawImage[] powerUps;
    [SerializeField] private RawImage doublePoints;
    [SerializeField] private RawImage headShots;
    [SerializeField] private RawImage discount;
    [SerializeField] private RawImage wings;

    [Header("Perks")]
    private RawImage[] perks;
    [SerializeField] private RawImage shielded;
    [SerializeField] private RawImage speed;
    [SerializeField] private RawImage haste;
    [SerializeField] private RawImage rebirth;
    [SerializeField] private RawImage scavenger;
    [SerializeField] private RawImage vampirism;
    [SerializeField] private RawImage criticalShots;
    [SerializeField] private RawImage explosiveBullets;

    [Header("Otros visuales")]
    [SerializeField] private TextMeshProUGUI textMaxAmmo;
    [SerializeField] private RawImage shield;
    public RawImage shieldTime;
    public TextMeshProUGUI shieldTimeText;

    [Header("Particulas")]

    [SerializeField] private ParticleSystem rebirthParticle;

    //Codigo nuevo 
    [Header("Sonidos")]
    [SerializeField] private AudioSource SFX_RebirthSound; //Sfx del rebirth
    [SerializeField] private AudioSource SFX_Newshield; //Sfx del escudo nuevo


    //tengo sueño y me da flojera escribir el comentario todo largo de "Codigo nuevo" 

    private void Start()
    {
        perks = new RawImage[] { shielded, speed, haste, rebirth, scavenger, vampirism, criticalShots, explosiveBullets };
        powerUps = new RawImage[] { doublePoints, headShots, discount, wings };
        hearts = new RawImage[] { heart1, heart2, heart3, heart4, heart5, heart6 };
        playerHealth = GetComponent<PlayerHealth>();
        player = GetComponent<Player>();
        UnityEvents.onHealthChanged.AddListener(OnHealthChange);
        UnityEvents.shielded.AddListener(Shielded);
        UnityEvents.onPowerUpTriggered.AddListener(VisualPowerUps);
        UnityEvents.onPerkActive.AddListener(VisualPerks);
        OnHealthChange(playerHealth.health);
    }

    private void OnDeath()
    {
        if (player.activePerks[3])
        {
            playerHealth.GainHealth(6);
            player.activePerks[3] = false;

            foreach (RawImage heart in hearts)
            {
                if (heart != null) heart.gameObject.SetActive(true);
            }

            if (rebirthCoroutine != null) StopCoroutine(rebirthCoroutine);
            if (rebirthParticle != null) rebirthParticle.Play();
            SFX_RebirthSound.Play(); //Al recibir el rebirth, se reproduce el sonido del rebirth
            rebirthCoroutine = StartCoroutine(VisualRebirth());
            StartCoroutine(VisualRebirthText());
        }

        UnityEvents.onPerkActive.Invoke();
    }
    public void OnHealthChange(int health)
    {
        playerHealth.health = health;

        if (health <= 0 && player.activePerks[3])
        {
            OnDeath();
            return;
        }

        Color healthColor = GetHealthColor(health);

        for (int i = 0; i < hearts.Length; i++)
        {
            bool check = health > i;

            if (hearts[i] != null)
            {
                hearts[i].gameObject.SetActive(check);
                if (rebirthCoroutine == null) hearts[i].color = healthColor;
            }
        }
    }
    public void Shielded(bool isShielded)
    {
        if (shield != null) shield.gameObject.SetActive(isShielded);
        SFX_Newshield.Play(); //Al recibir el escudo, se reproduce el sonido del escudo

    }

    public void VisualPowerUps()
    {
        for (int i = 0; i < powerUps.Length; i++)
        {
            if (powerUps[i] == null) continue;
            PlayerBonus bonus = (PlayerBonus)i;
            powerUps[i].gameObject.SetActive(player.HasPowerUp(bonus));
        }
    }

    public void VisualPerks()
    {
        for (int i = 0; i < perks.Length; i++)
        {
            if(perks[i] == null) continue;
            PlayerPerks perk = (PlayerPerks)i;
            perks[i].gameObject.SetActive(player.activePerks[(int)perk]);
        }
    }

    public void VisualMaxAmmoText()
    {
        StartCoroutine(VisualMaxAmmo());
    }

    private IEnumerator VisualMaxAmmo()
    {
        textMaxAmmo.text = "All weapons filled";
        textMaxAmmo.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        textMaxAmmo.gameObject.SetActive(false);
    }

    private IEnumerator VisualRebirthText()
    {
        textRebirth.text = "Rebirth!";
        textRebirth.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        textRebirth.gameObject.SetActive(false);
    }

    private IEnumerator VisualRebirth()
    {
        float elapsed = 0f;

        foreach (RawImage heart in hearts)
        {
            if (heart != null) heart.color = rebirthStartColor;
        }

        while (elapsed < rebirthDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / rebirthDuration);
            Color healthColor = GetHealthColor(playerHealth.health);
            Color currentColor = Color.Lerp(rebirthStartColor, healthColor, progress);

            foreach (RawImage heart in hearts)
            {
                if (heart != null) heart.color = currentColor;
            }
            yield return null;
        }

        Color finalColor = GetHealthColor(playerHealth.health);

        foreach (RawImage heart in hearts)
        {
            if (heart != null) heart.color = finalColor;
        }
        rebirthCoroutine = null;
    }
    private Color GetHealthColor(int health)
    {
        return colorByHealth.Evaluate(health / 6f);
    }
}
