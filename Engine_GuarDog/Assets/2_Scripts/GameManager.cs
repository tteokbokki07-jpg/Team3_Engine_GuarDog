using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    //public static GameManager Instance; // 싱글톤 인스턴스
    [Header("낮밤 설정")]
    public Light directionalLight;
    public float transitionSpeed = 1f;
    public bool isDefence = true;
    public float Wave = 1;
    public UpgradeManager upgradeManager;
    [Header("게임 적 관리 설정")]
    public float EnemyCount = 0f; // 현재 씬에 존재하는 적의 수
    public float EnemySpawnerCount = 0f; // 현재 씬에 존재하는 적 스포너의 수
    public int KillCount = 0; // 플레이어가 처치한 적의 수
    public UImanager uimanager;
    [Header("게임 설정")]
    // 타이틀 텍스트 페이드아웃 코루틴 참조
    private Coroutine titleFadeRoutine;

    [Header("타이틀 텍스트 설정")]
    public TextMeshProUGUI titleText;
    public float titleDisplayDuration = 3f;
    public float titleFadeInDuration = 1f;
    public float titleFadeOutDuration = 1f;


    // 내부 상태
    private bool isDay = true;
    private bool lastIsDay;
    private Color targetColor;
    private float targetIntensity;

    // 낮 전환 후 GameClear 호출 코루틴 참조
    private Coroutine dayClearRoutine;

    void Awake()
    {
        //Instance = this;
    }

    void Start()
    {
        titleText.text = "집을 지켜내라\n" + Wave.ToString() + "번째 밤";
        titleText.color = new Color(1, 1, 0); // 노란색
        ShowTitleText();

        // 초기 적 수 계산
        EnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // 라이트 초기 타깃 값 설정
        if (directionalLight != null)
        {
            targetColor = directionalLight.color;
            targetIntensity = directionalLight.intensity;
        }

        // isDefence 값에 따라 낮/밤 결정 (isDefence == true -> 밤, false -> 낮)
        isDay = !isDefence;
        lastIsDay = !isDay;
        ApplyDayNightImmediate(isDay);
    }

    void Update()
    {
        // 적 수 갱신
        EnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // 적 스포너 수 갱신
        EnemySpawnerCount = GameObject.FindGameObjectsWithTag("EnemySpawner").Length;

        if (uimanager != null)
            uimanager.GetComponent<UImanager>();

        // 적과 적 스포너가 모두 없으면 낮으로 전환
        if (EnemyCount <= 0 && EnemySpawnerCount <= 0 && isDefence)
        {
            titleText.text = "밤동안 무사히\n집을 지켜냈다!";
            titleText.color = new Color(0.6f, 1f, 0.7f); // 녹색
            ShowTitleText();
            isDefence = false;
            Wave++;
            Invoke(nameof(CallUpgrade), 4f);
            Invoke(nameof(CallNextWave), 6f);
        }

        // isDefence 값에 따라 낮/밤 결정
        // isDefence == true  -> 밤
        // isDefence == false -> 낮
        bool newIsDay = !isDefence;

        if (newIsDay != lastIsDay)
        {
            lastIsDay = newIsDay;
            isDay = newIsDay;
            ApplyDayNight(isDay);
        }

        // directional light가 있으면 서서히 보간
        if (directionalLight != null)
        {
            directionalLight.color = Color.Lerp(
                directionalLight.color,
                targetColor,
                Time.deltaTime * transitionSpeed
            );

            directionalLight.intensity = Mathf.MoveTowards(
                directionalLight.intensity,
                targetIntensity,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    private void CallUpgrade() //강화 부르기
    {
        uimanager.GameStopNoPanel();
        upgradeManager.UpgradeNumbers();
    }
    private void CallNextWave()
    {
        titleText.text = "집을 지켜내라\n" + Wave.ToString() + "번째 밤";
        titleText.color = new Color(1, 1, 0); // 노란색
        ShowTitleText();
    }

    // 낮/밤 전환 요청 (서서히 보간될 타깃 값만 변경)
    void ApplyDayNight(bool toDay)
    {
        if (toDay) SetDayTargets();
        else SetNightTargets();
    }

    // 시작시 강제 즉시 적용(원하면 제거)
    void ApplyDayNightImmediate(bool toDay)
    {
        if (directionalLight == null) return;
        if (toDay)
        {
            directionalLight.color = Color.white;
            directionalLight.intensity = 3f;
            targetColor = directionalLight.color;
            targetIntensity = directionalLight.intensity;

            //// 낮이 되는 로직: 3초 뒤에 UImanager.GameClear() 호출
            //if (dayClearRoutine != null) StopCoroutine(dayClearRoutine);
            //if (uimanager != null)
            //    dayClearRoutine = StartCoroutine(CallGameClearAfterDelay(3f));
        }
        else
        {
            targetColor = new Color32(8, 136, 254, 255);
            targetIntensity = 1f;
            directionalLight.color = targetColor;
            directionalLight.intensity = targetIntensity;

            // 밤으로 전환 시 예약된 낮 클리어 취소
            if (dayClearRoutine != null)
            {
                StopCoroutine(dayClearRoutine);
                dayClearRoutine = null;
            }
        }
    }

    void SetDayTargets()
    {
        targetColor = Color.white;
        targetIntensity = 3f;
    }

    void SetNightTargets()
    {
        targetColor = new Color32(8, 136, 254, 255);
        targetIntensity = 1f;
    }

    System.Collections.IEnumerator CallGameClearAfterDelay(float delay)
    {
        Debug.Log("Calling GameClear after delay");
        yield return new WaitForSeconds(delay);
        dayClearRoutine = null;
        if (uimanager != null)
        {
            uimanager.GameClear();
            Debug.Log("GameClear called");
        }
    }


    //gameManager.ShowTitleText(); 호출방식
    public void ShowTitleText()
    {
        if (titleText == null)
        {
            Debug.LogWarning("[GameManager] titleText가 연결되지 않았습니다.");
            return;
        }

        // 이미 실행 중인 페이드아웃 중지
        if (titleFadeRoutine != null)
        {
            StopCoroutine(titleFadeRoutine);
        }

        titleFadeRoutine = StartCoroutine(ShowAndFadeOutTitleText());
    }

    private System.Collections.IEnumerator ShowAndFadeOutTitleText()
    {
        GameObject titleObject = titleText.gameObject;

        // 텍스트 활성화
        titleObject.SetActive(true);

        Color titleColor = titleText.color;

        // 투명한 상태에서 시작
        titleColor.a = 0f;
        titleText.color = titleColor;

        // 페이드인
        float elapsedTime = 0f;

        while (elapsedTime < titleFadeInDuration)
        {
            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Lerp(
                0f,
                1f,
                elapsedTime / titleFadeInDuration
            );

            titleColor.a = alpha;
            titleText.color = titleColor;

            yield return null;
        }

        // 완전히 표시
        titleColor.a = 1f;
        titleText.color = titleColor;

        // 3초 동안 표시
        yield return new WaitForSeconds(titleDisplayDuration);

        // 페이드아웃
        elapsedTime = 0f;

        while (elapsedTime < titleFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;

            float alpha = Mathf.Lerp(
                1f,
                0f,
                elapsedTime / titleFadeOutDuration
            );

            titleColor.a = alpha;
            titleText.color = titleColor;

            yield return null;
        }

        // 완전히 투명하게 만든 후 비활성화
        titleColor.a = 0f;
        titleText.color = titleColor;
        titleObject.SetActive(false);

        titleFadeRoutine = null;
    }
}
