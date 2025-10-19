using FishNet;
using FishNet.Managing.Scened;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    public Image blackoutPanel;
    public TMP_Text loadingText;

    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(blackoutPanel.canvas.gameObject);

        // Ensure the loading canvas always appears on top. 
        blackoutPanel.canvas.sortingOrder = 999;
    }

    private void Start()
    {
        //Load the main menu scene by default.
        FadeIn();
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenuScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        StartCoroutine(WaitForSceneLoadAndActivate(asyncLoad));
    }


    /// <summary>
    /// Used for delayed loading of online scenes
    /// </summary>
    public IEnumerator DelayedOnlineSceneLoad(string newScene)
    {
        yield return new WaitForSeconds(fadeDuration + 0.2f);

        SceneLoadData sceneLoadData = new SceneLoadData(newScene);
        sceneLoadData.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
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
        blackoutPanel.gameObject.SetActive(true);
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
            blackoutPanel.gameObject.SetActive(false);
    }
}
