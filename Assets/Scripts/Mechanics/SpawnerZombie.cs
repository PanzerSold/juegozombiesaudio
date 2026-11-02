using UnityEngine;

public class SpawnerZombie : MonoBehaviour
{
    [SerializeField] private float zombieSpawnCooldown = 5f;
    [SerializeField] private Transform nearestBarricadePos;
    [SerializeField] public int mapIndex = 0;

    [SerializeField] private bool disabled = true;
    private RoundManager rm;

    private float zombieSpawnTimer;

    void Start()
    {
        zombieSpawnTimer = Random.Range(0f,3f);
        rm = GameObject.FindWithTag("RoundManager").GetComponent<RoundManager>();
    }

    void Update()
    {
        if (!disabled)
        {
            zombieSpawnTimer += Time.deltaTime;
            if (zombieSpawnTimer > zombieSpawnCooldown)
            {
                SpawnZombie();
            }
        }
        else
        {
            if (mapIndex <= rm.mapActivity) disabled = false;
        }
        
    }

    void SpawnZombie()
    {
        zombieSpawnTimer = 0;
        rm.AddZombie(transform,nearestBarricadePos.position);
    }
    public void SetSpawnTimer()
    {
        zombieSpawnTimer = zombieSpawnTimer-Random.Range(0f,3.2f);
    }
}
