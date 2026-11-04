using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Referencias Generales")]
    [SerializeField] AudioSource RoundPassSFX;
    [SerializeField] GameObject ZombieSpawnerMain;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] GameObject zombieObj;
    [SerializeField] GameObject gonzaPrefab;
    [SerializeField] AudioSource musicSource;

    [Header("Sistema de Ambiente (Bloque D)")]
    [SerializeField] AudioSource sourcePajaros;
    [SerializeField] AudioSource sourceTormenta;
    [SerializeField] float duracionTransicion = 2.0f;
    [SerializeField] float volumenMaximoPajaros = 1.0f;  
    [SerializeField] float volumenMaximoTormenta = 0.4f;   
    private Coroutine rutinaTransicionAmbiente;

    [Header("Configuración de Rondas y Jefes")]
    [SerializeField] int gonzaBaseHealth = 100;
    private bool gonzaSpawned = false;
    private bool restartingRound = false;
    private List<SpawnerZombie> SZSpawners = new List<SpawnerZombie>();
    private List<ZombieBase> zombieList = new List<ZombieBase>();
    private int zombiesForCurrentRound = 4;
    [SerializeField] private int currentRound = 1;

    public int mapActivity = 0;
    public enum WavePhase { Respite, Combat, Boss }
    public WavePhase Phase { get; private set; }


    void Start()
    {
        foreach (Transform child in ZombieSpawnerMain.transform)
        {
            SZSpawners.Add(child.GetComponent<SpawnerZombie>());
        }

        // Bloque D: inicializacion de ambiente
        if (sourcePajaros != null && sourceTormenta != null)
        {
            sourcePajaros.volume = volumenMaximoPajaros;
            sourceTormenta.volume = 0f;
            if (!sourcePajaros.isPlaying) sourcePajaros.Play();
            if (!sourceTormenta.isPlaying) sourceTormenta.Play();
        }
    }

    public void AddZombie(Transform spawnTransform, Vector3 nearestBarricadePos)
    {
        if (currentRound % 10 == 0)
        {
            if (!gonzaSpawned && zombieList.Count < 24)
            {
                SpawnGonza(nearestBarricadePos);
            }

            return;
        }
        if (zombieList.Count < 24 && zombiesForCurrentRound > 0)
        {
            GameObject newZombie = Instantiate(zombieObj, spawnTransform.position, spawnTransform.rotation);

            ZombieBase newZombieScript = newZombie.GetComponent<ZombieBase>();

            bool shouldntRandom = currentRound < 3 || currentRound > 5;
            int roundRange = currentRound < 6 ? 0 : 1;
            int indexRandom = shouldntRandom ? roundRange : Random.Range(0, 2) + roundRange;

            newZombieScript.ChangeSpeed(indexRandom);
            newZombieScript.goTo(nearestBarricadePos);
            zombieList.Add(newZombieScript);
            zombiesForCurrentRound--;
        }
    }

    private void SpawnGonza(Vector3 nearestBarricadePos)
    {
        if (gonzaSpawned) return;
        int randomIndex = Random.Range(0, SZSpawners.Count);
        SpawnerZombie selectedSpawner = SZSpawners[randomIndex];

        GameObject newGonza = Instantiate(gonzaPrefab, selectedSpawner.transform.position, selectedSpawner.transform.rotation);

        ZombieBase gonzaScript = newGonza.GetComponent<ZombieBase>();

        if (!musicSource.TryGetComponent(out AdaptiveMusic adaptiveMusic) || !adaptiveMusic.isActiveAndEnabled)
        {
            gonzaScript.AsignMusic(musicSource);
        }
        gonzaScript.goTo(nearestBarricadePos);
        zombieList.Add(gonzaScript);

        gonzaSpawned = true;
        Phase = WavePhase.Boss;
    }

    public async void RestartRound()
    {
        if (restartingRound) return;
        restartingRound = true;
        Phase = WavePhase.Respite;
        RoundPassSFX.Play();
        await Awaitable.WaitForSecondsAsync(1f);
        currentRound++;
        roundText.text = "Round " + currentRound;

        GestionarAmbientePorRonda();

        await Awaitable.WaitForSecondsAsync(3f);
        if (this == null) return;
        zombiesForCurrentRound = currentRound * 4;
        foreach (SpawnerZombie sz in SZSpawners)
        {
            sz.SetSpawnTimer();
        }
        restartingRound = false;
        gonzaSpawned = false;
    }

    private void GestionarAmbientePorRonda()
    {
        if (sourcePajaros == null || sourceTormenta == null) return;

        bool esRondaJefe = (currentRound % 10 == 0);

        if (rutinaTransicionAmbiente != null)
        {
            StopCoroutine(rutinaTransicionAmbiente);
        }

        if (esRondaJefe)
        {
            rutinaTransicionAmbiente = StartCoroutine(TransicionAudio(sourcePajaros, sourceTormenta, volumenMaximoTormenta));
        }
        else
        {
            rutinaTransicionAmbiente = StartCoroutine(TransicionAudio(sourceTormenta, sourcePajaros, volumenMaximoPajaros));
        }
    }

    public int SetHealthBasedOnRound()
    {
        return 8 + 2 * currentRound + currentRound - 1;
    }

    public int SetGonzaHealthBasedOnRound()
    {
        return currentRound * gonzaBaseHealth;
    }

    public void RemoveZombie(ZombieBase zombie)
    {
        zombieList.Remove(zombie);

        if (currentRound % 10 == 0 && gonzaSpawned && zombieList.Count == 0)
        {
            RestartRound();
            return;
        }

        if (zombieList.Count == 0 && zombiesForCurrentRound == 0)
        {
            RestartRound();
        }
    }

    public void IncreaseMapIndex()
    {
        mapActivity++;
    }
    //Bloque D:
    private IEnumerator TransicionAudio(AudioSource saliente, AudioSource entrante, float volumenObjetivoEntrante)
    {
        float tiempo = 0f;
        float volInicialSaliente = saliente.volume;
        float volInicialEntrante = entrante.volume;

        if (!entrante.isPlaying) entrante.Play();

        while (tiempo < duracionTransicion)
        {
            tiempo += Time.deltaTime;
            float porcentaje = tiempo / duracionTransicion;
            saliente.volume = Mathf.Lerp(volInicialSaliente, 0f, porcentaje);
            entrante.volume = Mathf.Lerp(volInicialEntrante, volumenObjetivoEntrante, porcentaje);

            yield return null;
        }

        saliente.volume = 0f;
        entrante.volume = volumenObjetivoEntrante;
    }
}
