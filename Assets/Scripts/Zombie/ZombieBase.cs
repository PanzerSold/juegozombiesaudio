
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ZombieBase : MonoBehaviour
{
    [SerializeField] private float health = 10f;

    [Header("Probabilidad de crítico")]
    [SerializeField, Range(0f, 1f)]

    private float criticalChance = 0.33f;

    [Header("Probabilidad de curación")]
    [SerializeField, Range(0f, 1f)]

    private float healChance = 0.0667f;
    private bool isStunned = false;
    private Player player;
    [SerializeField] private PowerUpSpawner pus;
    [SerializeField] private GameObject zombieRagdoll;
    private PlayerShoot playerAmmo;
    private Transform playerTransform;


    [Header("Sonidos")]
    [SerializeField] private AudioSource SFX_ZombieDeath;

    private Rigidbody rb;

    private ZombieNavmesh nav;


    private void Awake()
    {
        nav = GetComponent<ZombieNavmesh>();
    }

    [SerializeField] private float attackRange = 1.5f;
    private PlayerHealth playerHealth;
    private Barricade ultimabarricada;

    private int barricadeContacts = 0;
    private bool isDead = false;
    private bool isBurning = false;

    [SerializeField] private int zombieDamage = 1;
    [SerializeField] private ParticleSystem ps;

    [SerializeField] private float[] speedValues = new float[3] { 1.6f, 1.8f, 2.4f };

    [SerializeField] private int speedIndex = 0;
    private bool destroyingBarricade = false;
    private RoundManager rm;

    private Animator anim;

    private bool isAttacking = false;

    [SerializeField] private AudioSource zombieAudio;

    [SerializeField] private bool gonza = false;
<<<<<<< Updated upstream
    private ZombieBase gonzaInstance;
=======

    [SerializeField] private float gonzaSpeed = 4f;
>>>>>>> Stashed changes
    private AudioSource bosstheme;
    public void ChangeSpeed(int index)
    {
        if (!gonza)
        {
            speedIndex = index;
            nav.SetSpeed(speedValues[index]);

        }

    }

    public void AsignMusic(AudioSource musicSource)
    {
        bosstheme = musicSource;
        bosstheme.Play();
    }    


    void Start()
    {
        if (gonza)
        {
            if (gonzaInstance == null) gonzaInstance = this;
            else Destroy(gameObject);
        }
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        GameObject playerObject = GameObject.Find("Player");
        player = playerObject.GetComponent<Player>();
        playerAmmo = playerObject.GetComponent<PlayerShoot>();
        playerTransform = playerObject.transform;
        playerHealth = playerObject.GetComponent<PlayerHealth>();
        rm = GameObject.FindWithTag("RoundManager").GetComponent<RoundManager>();
        if (!gonza)
        {
            health = rm.SetHealthBasedOnRound();
        }
        else
        {
            health = rm.SetGonzaHealthBasedOnRound();
        //----- NUEVO CODIGO ----- coroutina para q haga primero la anim de spawnear
        if (!gonza)
        {
            StartCoroutine(SpawnZombie());
        }
        else
        {
            nav.SetSpeed(gonzaSpeed);
        }
        //----- NUEVO CODIGO ----- Le pasa la velocidad al agente
        // nav.SetSpeed(speedValues[speedIndex]); - CODIGO QUITADO, ahora cuando termina la animacion de spawnear ahi recien le da la velocidad correcta
        //----- NUEVO CODIGO -----

    }

    private IEnumerator SpawnZombie()
    {
        nav.SetSpeed(0f);
        
        if(anim!= null)
        {
            anim.Play("rise", 0, 0f);
        }

        double inicio = AudioSettings.dspTime + Random.Range(0.1f, 0.5f);

        zombieAudio.PlayScheduled(inicio);

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        nav.SetSpeed(speedValues[speedIndex]);

    }


    private void Update()
    {
        bool blockedByBarricade = destroyingBarricade && ultimabarricada != null && !ultimabarricada.isOpen;
        nav.SetHolding(blockedByBarricade);

        if (!zombieAudio.isPlaying)
        {
            zombieAudio.pitch = Random.Range(0.8f, 1.2f);
            zombieAudio.Play();
        }
        if (blockedByBarricade || isStunned || isAttacking) return;

        Vector3 diff = playerTransform.position - transform.position;

        if (diff.magnitude <= attackRange && !playerHealth.IsDying)
        {
            StartCoroutine(Attack(playerHealth));
        }
    }

    private void FixedUpdate()
    {
        float actualSpeed = isStunned ? 0 : speedValues[speedIndex];
        if (anim != null)
        {
            anim.SetFloat("Speed", actualSpeed);
        }

    }


    public async void Stun()
    {
        isStunned = true;
        nav.SetStunned(true);
        await Awaitable.WaitForSecondsAsync(2f);
        if (this == null) return;
        isStunned = false;
        nav.SetStunned(false);
    }
    public void ReceiveDamage(bool isHeadshot, float damage)
    {
        if (health <= 0) return;
        if (isHeadshot)
        {
            if (player.HasPowerUp(Player.PlayerBonus.HeadShots))
            {
                health = 0;
            }
            else
            {
                if (player.activePerks[6])
                {
                    if (Random.value >= criticalChance) health -= damage * 5;
                    else health -= damage * 10;
                }
                else health -= damage * 5;
            }
        }
        if (player.activePerks[6])
        {
            if (Random.value >= criticalChance) health -= damage;
            else health -= damage * 2;
        }
        else health -= damage;
        if (health <= 0)
        {
            player.AwardPoints(60);
            if (isHeadshot)
            {
                player.AwardPoints(40);
            }
            Despawn();
            if (player.activePerks[4] && playerAmmo.Gun != null)
            {
                int bonusAmmo = playerAmmo.Gun.AmmoMax * 10 / 100;
                playerAmmo.Gun.GainAmmo(bonusAmmo);
            }
            if (player.activePerks[5])
            {
                PlayerHealth ph = GameObject.Find("Player").GetComponent<PlayerHealth>();
                if (Random.value >= healChance || ph.health >= 6) return;
                else ph.GainHealth(1);
            }
        }
        else
        {
            player.AwardPoints(10);
        }
    }

    private void Despawn()
    {
        //----- NUEVO CODIGO ----- Despawn corre una sola vez y libera la barricada
        if (isDead) return;
        isDead = true;
        ReleaseBarricade();
        //----- NUEVO CODIGO -----
        rm.RemoveZombie(this);
        pus.Spawn();
        SFX_ZombieDeath.Play();
        if (bosstheme != null)
        {
            bosstheme.Stop();
        }
        GameObject z_Ragdoll = Instantiate(zombieRagdoll, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void ReleaseBarricade()
    {
        if (!destroyingBarricade) return;
        destroyingBarricade = false;
        if (ultimabarricada != null)
        {
            ultimabarricada.zombiesClose = Mathf.Max(0, ultimabarricada.zombiesClose - 1);
        }
        ultimabarricada = null;
    }


    private IEnumerator Attack(PlayerHealth player)
    {
        isAttacking = true;

        if(anim != null)
        {
            anim.Play("attack", 0, 0f);
        }

        yield return new WaitForSeconds(0.3f);
        Vector3 diff = playerTransform.position - transform.position;
        bool disCloseEnough = diff.magnitude <= attackRange;
        bool blockedByBarricade = destroyingBarricade && ultimabarricada != null && !ultimabarricada.isOpen;

        if (!blockedByBarricade && disCloseEnough) { player.LoseHealth(zombieDamage); }

        yield return new WaitForSeconds(0.45f);

        isAttacking = false;

    }

    public void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag("ZombieBarricade"))
        {
            barricadeContacts++;
            if (!destroyingBarricade)
            {
                ultimabarricada = other.GetComponentInParent<Barricade>();
                ultimabarricada.zombiesClose++;
                if (anim != null)
                {
                    ultimabarricada.GetZombieAnim(anim);
                }
                destroyingBarricade = true;
            }
        }
            //----- NUEVO CODIGO ----- 
        if (other.CompareTag("Fire") && !isBurning && !gonza)
        {
            isBurning = true;
            StartCoroutine(Burn());
        }
    }

    IEnumerator Burn()
    {
        isStunned = true;
        nav.SetStunned(true);
        ps.Play();
        yield return new WaitForSeconds(1f);
        Despawn();
    }


    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZombieBarricade"))
        {
            barricadeContacts = Mathf.Max(0, barricadeContacts - 1);
            if (barricadeContacts == 0) ReleaseBarricade();
        }
    }
    public void goTo(Vector3 position)
    {
        nav.SetWaypoint(position);
    }

}
