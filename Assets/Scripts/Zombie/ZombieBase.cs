
using System.Collections;
using UnityEngine;

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

    //----- Codigo borrado ----- El movimiento por waypoint ahora lo maneja ZombieNavmesh
    // private bool scriptedMovement = false;
    // private Vector3 scriptedDir = Vector3.zero;
    //----- Codigo borrado -----

    private Rigidbody rb;

    //----- NUEVO CODIGO ----- Referencia al script que mueve al zombie con el NavMesh
    private ZombieNavmesh nav;
    //----- NUEVO CODIGO -----


    //----- NUEVO CODIGO ----- Awake porque RoundManager llama a goTo justo después del Instantiate, antes del Start
    private void Awake()
    {
        nav = GetComponent<ZombieNavmesh>();
    }
    //----- NUEVO CODIGO -----

    //----- NUEVO CODIGO ----- Distancia a la que el zombie le pega al jugador (tiene que ser mayor al Stopping Distance del NavMeshAgent)
    [SerializeField] private float attackRange = 1.5f;
    private PlayerHealth playerHealth;
    //----- NUEVO CODIGO -----
    private Barricade ultimabarricada;

    //----- NUEVO CODIGO ----- Colliders de barricada que toca el zombie, y banderas para no morir ni quemarse dos veces
    private int barricadeContacts = 0;
    private bool isDead = false;
    private bool isBurning = false; //por las dudas de que los zombies no golpeen la barricada y se quemen al mismo tiempo.
    //----- NUEVO CODIGO -----

    // [SerializeField] private float rotationSpeed = 10f; la rotacion depende del agente, no del rigidbody.

    [SerializeField] private int zombieDamage = 1;
    [SerializeField] private ParticleSystem ps;

    [SerializeField] private float[] speedValues = new float[3] { 1.6f, 1.8f, 2.4f };

    [SerializeField] private int speedIndex = 0;
    private bool destroyingBarricade = false;
    private RoundManager rm;

    private Animator anim;

    private bool isAttacking = false;

    [SerializeField] private AudioSource zombieAudio;

    public void ChangeSpeed(int index)
    {
        speedIndex = index;
        nav.SetSpeed(speedValues[index]);

    }

    // private float timerPos = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        GameObject playerObject = GameObject.Find("Player");
        player = playerObject.GetComponent<Player>();
        playerAmmo = playerObject.GetComponent<PlayerShoot>();
        playerTransform = playerObject.transform;
        //----- NUEVO CODIGO -----
        playerHealth = playerObject.GetComponent<PlayerHealth>();
        //----- NUEVO CODIGO -----
        rm = GameObject.FindWithTag("RoundManager").GetComponent<RoundManager>();
        health = rm.SetHealthBasedOnRound();
        //----- NUEVO CODIGO ----- coroutina para q haga primero la anim de spawnear
        StartCoroutine(SpawnZombie());
        //----- NUEVO CODIGO ----- Le pasa la velocidad al agente
        // nav.SetSpeed(speedValues[speedIndex]); - CODIGO QUITADO, ahora cuando termina la animacion de spawnear ahi recien le da la velocidad correcta
        //----- NUEVO CODIGO -----
    }

    //---NUEVO CODIGO--- spawnea el zombi sin velocidad, hace la anim de rise y cuando termina de hacerla pone la velocidad correcta :D
    private IEnumerator SpawnZombie()
    {
        nav.SetSpeed(0f);

        anim.Play("rise", 0, 0f);

        double inicio = AudioSettings.dspTime + Random.Range(0.1f, 0.5f);

        zombieAudio.PlayScheduled(inicio);

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        nav.SetSpeed(speedValues[speedIndex]);

    }


    //----- Codigo borrado ----- Con el Rigidbody kinematic esta colisión ya no se dispara
    // private void OnCollisionStay(Collision collision) => HitStay(collision);
    //----- Codigo borrado -----

    //----- NUEVO CODIGO ----- Frena al zombie si está en una barricada cerrada y ataca al jugador si está al alcance
    private void Update()
    {
        bool blockedByBarricade = destroyingBarricade && ultimabarricada != null && !ultimabarricada.isOpen;
        nav.SetHolding(blockedByBarricade);

        if(!zombieAudio.isPlaying)
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
    //----- NUEVO CODIGO -----

    private void FixedUpdate()
    {
        float actualSpeed = isStunned ? 0 : speedValues[speedIndex];

        anim.SetFloat("Speed", actualSpeed);

    }


    public async void Stun()
    {
        isStunned = true;
        //----- NUEVO CODIGO ----- Frena al agente mientras está aturdido
        nav.SetStunned(true);
        //----- NUEVO CODIGO -----
        await Awaitable.WaitForSecondsAsync(2f);
        if (this == null) return;
        isStunned = false;
        //----- NUEVO CODIGO ----- Lo vuelve a mover
        nav.SetStunned(false);
        //----- NUEVO CODIGO -----
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
        GameObject z_Ragdoll = Instantiate(zombieRagdoll, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    //----- NUEVO CODIGO ----- Resta un solo zombiesClose por zombie y nunca baja de 0
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
    //----- NUEVO CODIGO -----


    private IEnumerator Attack(PlayerHealth player)
    {
        isAttacking = true;

        anim.Play("attack", 0, 0f);

        yield return new WaitForSeconds(0.3f);
        Vector3 diff = playerTransform.position - transform.position;
        bool disCloseEnough = diff.magnitude <= attackRange;
        bool blockedByBarricade = destroyingBarricade && ultimabarricada != null && !ultimabarricada.isOpen;

        if (!blockedByBarricade && disCloseEnough) { player.LoseHealth(zombieDamage); }

        yield return new WaitForSeconds(0.45f);

        isAttacking = false;

    }

    /*private void OnCollisionEnter(Collision collision)
    {
        PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.LoseHealth(zombieDamage);
        }
    }*/
    public void OnTriggerEnter(Collider other)
    {
        //----- NUEVO CODIGO ----- Un zombie muerto no vuelve a sumarse a una barricada
        if (isDead) return;
        //----- NUEVO CODIGO -----
        if (other.CompareTag("ZombieBarricade"))
        {
            //----- NUEVO CODIGO ----- Solo el primer collider suma a zombiesClose; los demás solo se cuentan
            barricadeContacts++;
            if (!destroyingBarricade)
            {
                ultimabarricada = other.GetComponentInParent<Barricade>();
                ultimabarricada.zombiesClose++;
                ultimabarricada.GetZombieAnim(anim);
                destroyingBarricade = true;
            }
            //----- NUEVO CODIGO -----
        }
            //----- NUEVO CODIGO ----- 
        if (other.CompareTag("Fire") && !isBurning)
        {
            isBurning = true;
            StartCoroutine(Burn());
        }
            //----- NUEVO CODIGO -----
    }

    IEnumerator Burn()
    {
        isStunned = true;
        //----- NUEVO CODIGO ----- Frena al agente mientras se quema
        nav.SetStunned(true);
        //----- NUEVO CODIGO -----
        ps.Play();
        yield return new WaitForSeconds(1f);
        Despawn();
    }


    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZombieBarricade"))
        {
            //----- NUEVO CODIGO ----- Libera la barricada recién al salir del último collider
            barricadeContacts = Mathf.Max(0, barricadeContacts - 1);
            if (barricadeContacts == 0) ReleaseBarricade();
            //----- NUEVO CODIGO -----
        }
    }
    public void goTo(Vector3 position)
    {
        //----- Codigo borrado -----
        // scriptedMovement = true;
        // scriptedDir = position;
        //----- Codigo borrado -----

        //----- NUEVO CODIGO ----- Ahora el punto de la barricada lo maneja ZombieNavmesh
        nav.SetWaypoint(position);
        //----- NUEVO CODIGO -----
    }

}
