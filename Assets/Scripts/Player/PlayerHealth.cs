using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private int healthMax = 6;
    [SerializeField] private float invulnTime = 0.75f;

    [SerializeField] private ParticleSystem bloodParticle;

    [SerializeField] private AudioSource SFX_BrokenShield;//Sfx del escudo rompiendose
    [SerializeField] private AudioSource SFX_Gethurt; 
    [SerializeField] private AudioSource SFX_Die; 
    public int health = 6;
    private HealthBar healthBar;
    private Player player;
    private float invulnUntil = 0f;

    public bool IsInvulnerable => 0 < invulnUntil;

    private bool isDying = false;

    public bool IsDying => isDying;
    private Animator anim;

    private void Start()
    {
        health = healthMax;
        anim = GetComponentInChildren<Animator>();
        healthBar = GetComponent<HealthBar>();
        player = GetComponent<Player>();
    }

    public void GainHealth(int bonusHealth)
    {
        health += bonusHealth;
        if (health > healthMax)
        {
            health = healthMax;
        }
        healthBar.OnHealthChange(health);
    }
    private void Update()
    {
        if (invulnUntil > 0) invulnUntil -= Time.deltaTime;
    }

    public void LoseHealth(int damage = 1)
    {
        if (IsInvulnerable) return;
        invulnUntil = invulnTime;

        if (player.activePerks[0] && player.shieldAvailable)
        {
            player.shieldAvailable = false;
            UnityEvents.shielded.Invoke(false);
            player.StartCoroutine(player.Shield());
            SFX_BrokenShield.Play(); //Al recibir daño con el escudo, se reproduce el sonido del escudo rompiendose
            return;
        }
        if (bloodParticle != null) bloodParticle.Play();
        health = Mathf.Max(0, health - damage);
        healthBar.OnHealthChange(health);
        if (health <= 0)
        {
            SFX_Die.Play(); //Al morir, se reproduce el sonido de la muerte
            StartCoroutine(Dead());

        }
        SFX_Gethurt.pitch = Random.Range(0.8f, 1.2f); //Se le da un pitch random al sonido del daño para que no se escuche repetitivo
        SFX_Gethurt.Play(); //Al recibir daño, se reproduce el sonido del daño
    }

    private IEnumerator Dead()
    {
        isDying = true;
        anim.Play("Died", 0, 0f);
        anim.SetLayerWeight(1, 0f);

        yield return new WaitForEndOfFrame();

        float animationLength = anim.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animationLength);

        SceneManager.LoadScene("GameOverScene");
    }
}

