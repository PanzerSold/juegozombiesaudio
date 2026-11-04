using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject reticula;

    [Header("Audio")]
    [SerializeField] private AudioMixerSnapshot normalSnapshot;
    [SerializeField] private AudioMixerSnapshot pausedSnapshot;
    [SerializeField] private float transitionTime = 0.25f;

    public bool isPaused { get; private set; }

    private Coroutine pauseAudioCoroutine;

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        normalSnapshot.TransitionTo(0f);

        isPaused = false;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    private IEnumerator PausarAudio()
    {
        pausedSnapshot.TransitionTo(transitionTime);

        // Esperar usando tiempo real, incluso con Time.timeScale = 0.
        yield return new WaitForSecondsRealtime(transitionTime);

        AudioListener.pause = true;
        pauseAudioCoroutine = null;
    }

    private void ReanudarAudio()
    {
        // Cancelar una pausa de audio pendiente.
        if (pauseAudioCoroutine != null)
        {
            StopCoroutine(pauseAudioCoroutine);
            pauseAudioCoroutine = null;
        }

        AudioListener.pause = false;
        normalSnapshot.TransitionTo(transitionTime);
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        pauseAudioCoroutine = StartCoroutine(PausarAudio());

        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        reticula.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        ReanudarAudio();

        isPaused = false;
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        reticula.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void MainMenu()
    {
        if (pauseAudioCoroutine != null)
        {
            StopCoroutine(pauseAudioCoroutine);
            pauseAudioCoroutine = null;
        }

        AudioListener.pause = false;
        Time.timeScale = 1f;

        normalSnapshot.TransitionTo(0f);

        SceneManager.LoadScene("menudebenja");
    }
}