using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenTransition : MonoBehaviour
{
    public static ScreenTransition Instance;

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private bool isTransitioning;

    private void Awake()
    {
        Debug.Log("[ScreenTransition] AWAKE");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[ScreenTransition] Instance assigned");
    }

    private void Start()
    {
        Debug.Log("[ScreenTransition] START");

        if (fadeCanvasGroup == null)
        {
            Debug.LogError(
                "[ScreenTransition] Fade Canvas Group is NULL!"
            );

            return;
        }

        FadeIn();
    }

    public void FadeIn()
    {
        Debug.Log("[ScreenTransition] FadeIn() CALLED");

        StartCoroutine(Fade(1f, 0f));
    }

    public void FadeOut()
    {
        Debug.Log("[ScreenTransition] FadeOut() CALLED");

        StartCoroutine(Fade(0f, 1f));
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning(
                "[ScreenTransition] Transition already in progress."
            );

            return;
        }

        StartCoroutine(
            LoadSceneRoutine(sceneName)
        );
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isTransitioning = true;

        Debug.Log(
            $"[ScreenTransition] Transition START -> {sceneName}"
        );

        yield return StartCoroutine(
            Fade(0f, 1f)
        );

        Debug.Log(
            "[ScreenTransition] FadeOut COMPLETE. Loading scene..."
        );

        yield return SceneManager.LoadSceneAsync(sceneName);

        Debug.Log(
            "[ScreenTransition] Scene loaded. Starting FadeIn..."
        );

        yield return StartCoroutine(
            Fade(1f, 0f)
        );

        Debug.Log(
            "[ScreenTransition] Transition COMPLETE"
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