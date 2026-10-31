
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] AudioSource RoundPassSFX;
    [SerializeField] GameObject ZombieSpawnerMain;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] GameObject zombieObj;

    private bool restartingRound = false;
    private List<SpawnerZombie> SZSpawners = new List<SpawnerZombie>();
    private List<ZombieBase> zombieList = new List<ZombieBase>();
    private int zombiesForCurrentRound = 4;
    private int currentRound = 1;

    public int mapActivity = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in ZombieSpawnerMain.transform)
        {
            SZSpawners.Add(child.GetComponent<SpawnerZombie>());
        }
    }
    public void AddZombie(Transform spawnTransform, Vector3 nearestBarricadePos)
    {
        if (zombieList.Count < 24 && zombiesForCurrentRound > 0)
        {
            GameObject newZombie = Instantiate(zombieObj, spawnTransform.position, spawnTransform.rotation);

            ZombieBase newZombieScript = newZombie.GetComponent<ZombieBase>();
            float minIndex = 0;
            float maxIndex = 0;
            if (currentRound >= 3 && currentRound < 6) maxIndex = 1.1f;
            if (currentRound >= 6) maxIndex = 2.1f;
            int indexRandom = (int)Random.Range(minIndex, maxIndex);
            Debug.Log("Spawn con random " + indexRandom + "/" + maxIndex);
            newZombieScript.ChangeSpeed(indexRandom);
            newZombieScript.goTo(nearestBarricadePos);
            zombieList.Add(newZombieScript);
            zombiesForCurrentRound--;   
        }
    }

    public async void RestartRound()
    {
        if (restartingRound) return;
        restartingRound = true;
        RoundPassSFX.Play();
        await Awaitable.WaitForSecondsAsync(1f);
        currentRound++;
        roundText.text = "Round " + currentRound;
        await Awaitable.WaitForSecondsAsync(3f);
        if (this == null) return;
        zombiesForCurrentRound = currentRound*4;
        foreach (SpawnerZombie sz in SZSpawners)
        {
            sz.SetSpawnTimer();
        }
        restartingRound = false;
    }
    public int SetHealthBasedOnRound()
    {
        return 8 + 2 * currentRound + currentRound - 1;
    }
    public void RemoveZombie(ZombieBase zombie)
    {
        zombieList.Remove(zombie);
        if (zombieList.Count == 0 && zombiesForCurrentRound == 0)
        {
            RestartRound();
        }
    }

    public void IncreaseMapIndex()
    {
        mapActivity++;
    }
    
}
