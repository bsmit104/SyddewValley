using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    private Image fadeImage;
    public float fadeDuration = 0.5f; // Adjust speed of fade

    void Awake()
    {
        // Singleton for global access
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        fadeImage = GetComponent<Image>();
    }

    void Start()
    {
        // Fade in on scene load
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        float t = fadeDuration;
        while (t > 0)
        {
            t -= Time.deltaTime;
            float a = t / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
        fadeImage.color = Color.clear;
    }

    public IEnumerator FadeOutAndLoad(string sceneName)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = t / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }

        // Load the next scene
        SceneManager.LoadScene(sceneName);

        // Fade back in on new scene
        StartCoroutine(FadeIn());
    }
}
