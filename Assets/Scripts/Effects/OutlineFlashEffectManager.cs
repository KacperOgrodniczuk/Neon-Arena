using System.Collections;
using UnityEngine;

public class OutlineFlashEffectManager : MonoBehaviour
{
    public static OutlineFlashEffectManager Instance { get; private set; }

    // Assign the colour pallete Scriptable Object here
    public OutlineFlashColourPallete colourPallete;

    // Assign fullscreen outline material here
    public Material outlineMaterial;

    public AnimationCurve flashIntesityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Default curve;
    public float defaultFlashDuration = 0.2f;

    // Coroutine reference to stop/start new effects
    private Coroutine flashCoroutine;

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

    public void TriggerOutlineEffect(Color color, float duration)
    {
        // Stop any currently running effect to ensure the new one takes priority
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(ColorFlashRoutine(color, duration));
    }

    private IEnumerator ColorFlashRoutine(Color targetColor, float duration)
    {
        float timer = 0f;

        //  Set the Colour
        outlineMaterial.SetColor("_Effect_Colour", targetColor);

        // Wait timer duration and fade intensity back to 0.0
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration; 

            float intensity = flashIntesityCurve.Evaluate(t);

            outlineMaterial.SetFloat("_Effect_Intensity", intensity);
            yield return null;
        }

        outlineMaterial.SetFloat("_Effect_Intensity", 0.0f);
        flashCoroutine = null;
    }
}
