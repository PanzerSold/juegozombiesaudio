using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private RectTransform mainMenu;
    [SerializeField] private RectTransform optionsMenu;
    [SerializeField] private RectTransform informationMenu;
    [SerializeField] private RectTransform creditsMenu;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float screenWidth = 1920f;

    private RectTransform currentMenu;
    private bool isTransitioning;

    private void Start()
    {
        currentMenu = mainMenu;

        mainMenu.anchoredPosition = Vector2.zero;
        informationMenu.anchoredPosition = Vector2.right * screenWidth;
        optionsMenu.anchoredPosition = Vector2.left * screenWidth;

        informationMenu.gameObject.SetActive(false);
        optionsMenu.gameObject.SetActive(false);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("mainGameLoop");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void OpenInfo()
    {
        TransitionTo(informationMenu);
    }
    public void OpenOptions()
    {
        TransitionTo(optionsMenu);
    }

    public void BackToMenu()
    {
        TransitionTo(mainMenu);
    }

    public void BackToOptions()
    {
        TransitionTo(optionsMenu);
    }

    public void OpenCredits()
    {
        TransitionTo(creditsMenu);
    }

    private void TransitionTo(RectTransform nextMenu)
    {
        if (isTransitioning || nextMenu == currentMenu)
            return;

        StartCoroutine(AnimateTransition(nextMenu));
    }
    private IEnumerator AnimateTransition(RectTransform nextMenu)
    {
        isTransitioning = true;

        RectTransform previousMenu = currentMenu;

        float direction;

        if (nextMenu == informationMenu) direction = 1f;
        else direction = 1f;

        nextMenu.gameObject.SetActive(true);

        Vector2 nextStart = Vector2.right * screenWidth * direction;
        Vector2 previousEnd = Vector2.left * screenWidth * direction;

        nextMenu.anchoredPosition = nextStart;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp01(elapsed / transitionDuration));

            previousMenu.anchoredPosition =
                Vector2.Lerp(Vector2.zero, previousEnd, t);

            nextMenu.anchoredPosition =
                Vector2.Lerp(nextStart, Vector2.zero, t);

            yield return null;
        }

        previousMenu.gameObject.SetActive(false);
        previousMenu.anchoredPosition = Vector2.zero;
        nextMenu.anchoredPosition = Vector2.zero;

        currentMenu = nextMenu;
        isTransitioning = false;
    }
}