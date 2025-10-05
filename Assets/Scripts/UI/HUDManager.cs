using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Hit Marker")]
    public Image hitMarkerImage;
    public TMP_Text hitText;
    public float hitMarkerDuration = 0.2f;

    private Coroutine hitMarkerCoroutine;
    private float defaultHitMarkerAlpha;
    private float defaultHitTextAlpha;

    [Header("Timer")]
    public TMP_Text timerText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Hit Marker
    public void TriggerHitMarkerEffect()
    {
        if (hitMarkerCoroutine != null)
        { 
            StopCoroutine(hitMarkerCoroutine);
        }

        hitMarkerCoroutine = StartCoroutine(HitMarkerEffect());
    }

    private IEnumerator HitMarkerEffect()
    {
        Color hitMarkerColour = hitMarkerImage.color;
        defaultHitMarkerAlpha = hitMarkerColour.a;
        hitMarkerColour.a = 1f;
        hitMarkerImage.color = hitMarkerColour;

        Color hitTextColour = hitText.color;
        defaultHitTextAlpha = hitTextColour.a;
        hitTextColour.a = 1f;
        hitText.color = hitTextColour;

        yield return new WaitForSeconds(hitMarkerDuration);

        hitMarkerColour.a = defaultHitMarkerAlpha;
        hitMarkerImage.color = hitMarkerColour;

        hitTextColour.a = defaultHitTextAlpha;
        hitText.color = hitTextColour;

        hitMarkerCoroutine = null;
    }

    #endregion

    #region Timer

    public void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    #endregion
}
