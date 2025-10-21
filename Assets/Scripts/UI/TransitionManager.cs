using FishNet;
using FishNet.Managing.Scened;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    public Canvas loadingCanvas;
    public Image blackoutPanel;
    public TMP_Text loadingText;

    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(loadingCanvas.gameObject);
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(loadingCanvas.gameObject);

        // Ensure the loading canvas is disabled and set to 0 alpha tranpsarency on load.
        // Ensure the loading canvas is always rendered on top of everything else.
        loadingCanvas.gameObject.SetActive(false);
        loadingCanvas.sortingOrder = 999;
        Color startingColour = blackoutPanel.color;
        startingColour.a = 0;
        blackoutPanel.color = startingColour;

        InstanceFinder.NetworkManager.SceneManager.OnLoadEnd += OnSceneLoadEnd;
    }

    /// <summary>
    /// Used when loading local non-networked scenes
    /// </summary>
    public IEnumerator WaitForSceneLoadAndActivate(AsyncOperation asyncLoad)
    {
        // Wait until the scene is 90% loaded
        // has to be 90 or less since setting allowSceneActivation to false causes the scene to stop loading at 90%
        while (asyncLoad.progress < 0.9f)
        { 
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        yield return null;

        FadeOut();
    }

    public void FadeIn()
    {
        loadingCanvas.gameObject.SetActive(true);
        StartCoroutine(Fade(1f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    { 
        Color blackoutColour = blackoutPanel.color;
        float startBlackoutAlpha = blackoutColour.a;

        Color loadingTextColour = loadingText.color;
        float startTextAlpha = loadingTextColour.a;

        float timer = 0f;

        while (timer < fadeDuration)
        { 
            timer += Time.deltaTime;
            blackoutColour.a = Mathf.Lerp(startBlackoutAlpha, targetAlpha, timer / fadeDuration);
            blackoutPanel.color = blackoutColour;

            loadingTextColour.a = Mathf.Lerp(startTextAlpha, targetAlpha, timer / fadeDuration);
            loadingText.color = loadingTextColour;

            yield return null;
        }

        blackoutColour.a = targetAlpha;
        blackoutPanel.color = blackoutColour;

        loadingTextColour.a = targetAlpha;
        loadingText.color = loadingTextColour;

        if (targetAlpha == 0f)
            loadingCanvas.gameObject.SetActive(false);
    }

    // Used so that whenever fishnet finishes loading a scene it automatically fades out. 
    public void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        FadeOut();
    }

    private void OnDestroy()
    {
        InstanceFinder.NetworkManager.SceneManager.OnLoadEnd -= OnSceneLoadEnd;
    }
}
