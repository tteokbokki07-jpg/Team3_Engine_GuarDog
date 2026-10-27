using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFade : MonoBehaviour
{
    public static SceneFade Instance;

    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작할 때 검은 화면에서 서서히 밝아짐
        StartCoroutine(FadeIn());
    }

    // 페이드아웃: 화면을 검은색으로 전환
    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(Fade(0f, 1f));
    }

    // 페이드인: 검은 화면에서 원래 화면으로 전환
    public IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1f, 0f));
    }

    // 실제 페이드 처리
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;

        fadeImage.raycastTarget = true;

        Color color = fadeImage.color;
        color.a = startAlpha;
        fadeImage.color = color;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                time / fadeDuration
            );

            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;

        // 완전히 투명해지면 클릭 방지 해제
        if (endAlpha == 0f)
        {
            fadeImage.raycastTarget = false;
        }
    }

    // 씬 전환 함수
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneRoutine(sceneName));
    }

    private IEnumerator ChangeSceneRoutine(string sceneName)
    {
        // 1. 화면을 검게 만듦
        yield return StartCoroutine(FadeOut());

        // 2. 씬 전환
        SceneManager.LoadScene(sceneName);

        // 3. 새 씬을 검은 화면에서 서서히 보여 줌
        yield return StartCoroutine(FadeIn());
    }
}
