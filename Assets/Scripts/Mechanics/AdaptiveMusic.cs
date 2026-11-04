using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class AdaptiveMusic : MonoBehaviour
{
    [System.Serializable]
    private struct Mix
    {
        [Range(0f, 1f)] public float Volume;
        [Min(0f)] public float TransitionSeconds;
    }

    [SerializeField] private RoundManager rounds;
    [SerializeField] private AudioClip respiteClip;
    [SerializeField] private AudioClip combatClip;
    [SerializeField] private Mix respite;
    [SerializeField] private Mix combat;
    [SerializeField] private Mix boss;

    private AudioSource bossSource;
    private AudioSource respiteSource;
    private AudioSource combatSource;
    private AudioSource currentSource;
    private Vector3 startGains;
    private Vector3 targetGains;
    private float elapsed;
    private float duration;

    private void Awake()
    {
        bossSource = GetComponent<AudioSource>();
        bossSource.volume = 0f;
        respiteSource = CreateLayer(respiteClip);
        combatSource = CreateLayer(combatClip);
    }

    private AudioSource CreateLayer(AudioClip clip)
    {
        AudioSource layer = gameObject.AddComponent<AudioSource>();
        layer.playOnAwake = false;
        layer.loop = true;
        layer.clip = clip;
        layer.outputAudioMixerGroup = bossSource.outputAudioMixerGroup;
        layer.volume = 0f;
        return layer;
    }

    private void Update()
    {
        if (rounds.Phase == RoundManager.WavePhase.Boss)
            TransitionTo(bossSource, boss, new Vector3(0f, 0f, boss.Volume));
        else if (rounds.Phase == RoundManager.WavePhase.Combat)
            TransitionTo(combatSource, combat, new Vector3(0f, combat.Volume, 0f));
        else
            TransitionTo(respiteSource, respite, new Vector3(respite.Volume, 0f, 0f));

        elapsed += Time.unscaledDeltaTime;
        Vector3 gains = Vector3.Lerp(startGains, targetGains, duration > 0f ? elapsed / duration : 1f);
        respiteSource.volume = gains.x;
        combatSource.volume = gains.y;
        bossSource.volume = gains.z;

        if (elapsed >= duration)
        {
            if (currentSource != respiteSource && respiteSource.isPlaying) respiteSource.Stop();
            if (currentSource != combatSource && combatSource.isPlaying) combatSource.Stop();
            if (currentSource != bossSource && bossSource.isPlaying) bossSource.Stop();
        }
    }

    private void TransitionTo(AudioSource source, Mix mix, Vector3 target)
    {
        if (currentSource == source) return;

        currentSource = source;
        startGains = new Vector3(respiteSource.volume, combatSource.volume, bossSource.volume);
        targetGains = target;
        duration = mix.TransitionSeconds;
        elapsed = 0f;

        if (!source.isPlaying)
        {
            source.volume = 0f;
            source.Play();
        }
    }
}