using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenTransition : MonoBehaviour
{
    public static ScreenTransition Instance;

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 2f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError(
                "ScreenTransition: Fade Canvas Group is not assigned."
            );

            return;
        }

        FadeIn();
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(0f, 1f));
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(
            LoadSceneRoutine(sceneName)
        );
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;

        yield return StartCoroutine(
            Fade(0f, 1f)
        );

        yield return SceneManager.LoadSceneAsync(sceneName);

        yield return StartCoroutine(
            Fade(1f, 0f)
        );

        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float time = 0f;

        fadeCanvasGroup.alpha = from;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                time / fadeDuration;

            fadeCanvasGroup.alpha =
                Mathf.Lerp(
                    from,
                    to,
                    progress
                );

            yield return null;
        }

        fadeCanvasGroup.alpha = to;
    }
}