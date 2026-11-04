using UnityEngine;

public sealed class AdaptiveMusic : MonoBehaviour
{
    [System.Serializable]
    private struct Track
    {
        public AudioSource Source;
        [Range(0f, 1f)] public float Volume;
        [Min(0f)] public float TransitionSeconds;
    }

    [SerializeField] private RoundManager rounds;
    [Tooltip("Track volumes are authored here. Audio Source volumes show the current gain during Play Mode.")]
    [SerializeField] private Track respite;
    [SerializeField] private Track combat;
    [SerializeField] private Track boss;

    private AudioSource currentSource;
    private Vector3 weights;
    private Vector3 startWeights;
    private Vector3 targetWeights;
    private float elapsed;
    private float duration;

    private void Start()
    {
        var group = respite.Source.outputAudioMixerGroup;
        if (group != null)
        {
            GameSettings.ApplyMixerVolume(group.audioMixer, "MasterVolume", GameSettings.MasterVolume);
            GameSettings.ApplyMixerVolume(group.audioMixer, "MusicVolume", GameSettings.MusicVolume);
        }

        currentSource = respite.Source;
        weights = targetWeights = startWeights = Vector3.right;
        ApplyVolumes();
        currentSource.Play();
    }

    private void Update()
    {
        var phase = rounds != null ? rounds.Phase : RoundManager.WavePhase.Respite;
        if (phase == RoundManager.WavePhase.Boss)
            TransitionTo(boss, Vector3.forward);
        else if (phase == RoundManager.WavePhase.Combat)
            TransitionTo(combat, Vector3.up);
        else
            TransitionTo(respite, Vector3.right);

        elapsed += Time.unscaledDeltaTime;
        weights = Vector3.Lerp(startWeights, targetWeights, duration > 0f ? elapsed / duration : 1f);
        ApplyVolumes();

        if (elapsed >= duration)
        {
            StopInactive(respite.Source);
            StopInactive(combat.Source);
            StopInactive(boss.Source);
        }
    }

    private void ApplyVolumes()
    {
        respite.Source.volume = weights.x * respite.Volume;
        combat.Source.volume = weights.y * combat.Volume;
        boss.Source.volume = weights.z * boss.Volume;
    }

    private void StopInactive(AudioSource source)
    {
        if (source != currentSource && source.isPlaying) source.Stop();
    }

    private void TransitionTo(Track track, Vector3 target)
    {
        if (currentSource == track.Source) return;

        currentSource = track.Source;
        startWeights = weights;
        targetWeights = target;
        duration = track.TransitionSeconds;
        elapsed = 0f;

        if (!currentSource.isPlaying)
        {
            currentSource.volume = 0f;
            currentSource.Play();
        }
    }
}
