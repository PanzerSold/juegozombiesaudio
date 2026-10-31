using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieNavmesh : MonoBehaviour
{
    [Header("Navegación")]
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float waypointReachDistance = 1f;
    [SerializeField] private float rotationSpeed = 10f;
    //Pasos por intervalo (PlayOneShot), igual que el del player
    [Header("Sonidos")]
    [SerializeField] private AudioSource SFX_Steps;
    [SerializeField] private AudioClip stepsClip;
    [SerializeField] private float walkInterval = 0.6f;
    [SerializeField] private float minStepSpeed = 0.5f;
    private float walktimer = 0f;
    

    private NavMeshAgent agent;
    private Transform playerTransform;

    private Vector3 waypoint;
    private bool followingWaypoint = false;
    private bool isStunned = false;
    private bool isHolding = false;
    private float repathTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        if (isHolding)
        {
            LookAtPlayer();
            return;
        }

        if (isStunned) return;

        repathTimer -= Time.deltaTime;
        if (repathTimer > 0f) return;
        repathTimer = repathInterval;

        agent.SetDestination(GetTarget());
    }

    private Vector3 GetTarget()
    {
        if (followingWaypoint)
        {
            Vector3 flatDiff = waypoint - transform.position;
            flatDiff.y = 0f;

            if (flatDiff.magnitude < waypointReachDistance) followingWaypoint = false;
            else return waypoint;
        }

        return playerTransform.position;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private void ApplyStop()
    {
        if (!agent.isOnNavMesh) return;

        bool stop = isStunned || isHolding;
        agent.isStopped = stop;

        if (stop) agent.velocity = Vector3.zero;
        else repathTimer = 0f;
    }

    public void SetWaypoint(Vector3 position)
    {
        waypoint = position;
        followingWaypoint = true;

    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
        ApplyStop();
    }
    public void SetHolding(bool holding)
    {
        if (isHolding == holding) return;
        isHolding = holding;
        ApplyStop();
    }
    //Un paso cada walkInterval mientras el agente se mueve
    private void UpdateSteps()
    {
        if (SFX_Steps == null || stepsClip == null) return;

        if (agent.velocity.magnitude > minStepSpeed)
        {
            walktimer += Time.deltaTime;
            if (walktimer >= walkInterval)
            {
                SFX_Steps.pitch = Random.Range(0.8f, 1.2f);
                SFX_Steps.PlayOneShot(stepsClip);
                walktimer = 0f;
            }
        }
    }
}