using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //public static GameManager Instance; // 싱글톤 인스턴스
    [Header("낮밤 설정")]
    public Light directionalLight;
    public float transitionSpeed = 1f;
    [Header("게임 적 관리 설정")]
    public float EnemyCount = 0f; // 현재 씬에 존재하는 적의 수
    public int KillCount = 0; // 플레이어가 처치한 적의 수
    public UImanager uimanager;

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
        // 초기 적 수 계산
        EnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // 라이트 초기 타깃 값 설정
        if (directionalLight != null)
        {
            targetColor = directionalLight.color;
            targetIntensity = directionalLight.intensity;
        }

        // 초기 낮/밤 결정 및 적용
        isDay = (EnemyCount == 0);
        lastIsDay = !isDay;
        ApplyDayNightImmediate(isDay);
    }

    void Update()
    {
        // 적 수 갱신
        EnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (uimanager != null)
            uimanager.GetComponent<UImanager>();


        // 적 유무로 낮/밤 결정 (적 있음 -> 밤(false), 적 없음 -> 낮(true))
        bool newIsDay = (EnemyCount == 0);
        if (newIsDay != lastIsDay)
        {
            lastIsDay = newIsDay;
            isDay = newIsDay;
            ApplyDayNight(isDay);
        }

        // directional light가 있으면 서서히 보간
        if (directionalLight != null)
        {
            directionalLight.color = Color.Lerp(directionalLight.color, targetColor, Time.deltaTime * transitionSpeed);
            directionalLight.intensity = Mathf.MoveTowards(directionalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
        }
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

            // 낮이 되는 로직: 3초 뒤에 UImanager.GameClear() 호출
            if (dayClearRoutine != null) StopCoroutine(dayClearRoutine);
            if (uimanager != null)
                dayClearRoutine = StartCoroutine(CallGameClearAfterDelay(3f));
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

        // 낮으로 전환 시에도 GameClear 예약 (서서히 보간 중일 때 적용)
        if (dayClearRoutine != null) StopCoroutine(dayClearRoutine);
        if (uimanager != null)
            dayClearRoutine = StartCoroutine(CallGameClearAfterDelay(3f));
    }

    void SetNightTargets()
    {
        targetColor = new Color32(8, 136, 254, 255);
        targetIntensity = 1f;

        if (dayClearRoutine != null)
        {
            StopCoroutine(dayClearRoutine);
            dayClearRoutine = null;
        }
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
}
