using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject reticula;

    public bool isPaused { get; private set; }

    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        reticula.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
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
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("menudebenja");
    }
}