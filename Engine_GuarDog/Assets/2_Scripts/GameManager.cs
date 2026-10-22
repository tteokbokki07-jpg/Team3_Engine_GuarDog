using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("낮밤 설정")]
    public Light directionalLight;
    public float transitionSpeed = 1f;
    public bool isDefence = true;
    public float Wave = 1;
    public UpgradeManager upgradeManager;
    [Header("게임 설정")]
    public PlayerController playerController;
    public float EnemyCount = 0f; // 현재 씬에 존재하는 적의 수
    public float EnemySpawnerCount = 0f; // 현재 씬에 존재하는 적 스포너의 수
    public int KillCount = 0; // 플레이어가 처치한 적의 수
    public UImanager uimanager;
    [Header("웨이브별 스포너 설정")]
    public List<GameObject> spawnerList = new List<GameObject>();
    public Transform spawnerSpawnPoint;

    // 타이틀 텍스트 페이드아웃 코루틴 참조
    private Coroutine titleFadeRoutine;

    [Header("타이틀 텍스트 설정")]
    public TextMeshProUGUI titleText;
    float titleDisplayDuration = 3f;
    float titleFadeInDuration = 0.5f;
    float titleFadeOutDuration = 0.5f;


    // 내부 상태
    private bool isDay = true;
    private bool lastIsDay;
    private Color targetColor;
    private float targetIntensity;

    // 낮 전환 후 GameClear 호출 코루틴 참조
    private Coroutine dayClearRoutine;

    void Start()
    {
        TitleTextNormal();
        titleText.text = Wave.ToString() + "번째 밤";
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
        SpawnSpawnerByWave();
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
            TitleTextNormal();
            uimanager.isGaming = false;
            titleText.text = "무사히 밤을 보냈다!";
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
    public void TitleTextNormal()
    {
        titleDisplayDuration = 3f;
        titleFadeInDuration = 0.5f;
        titleFadeOutDuration = 0.5f;
    }
    public void TitleTextFast()
    {
        titleDisplayDuration = 1f;
        titleFadeInDuration = 0.2f;
        titleFadeOutDuration = 0.2f;
    }
    public void CallHomeText()
    {
        TitleTextFast();
        titleText.text = "집이 공격받고 있습니다!";
        titleText.color = new Color(1, 0, 0); // 빨간색
        ShowTitleText();
    }

    private void CallUpgrade() //강화 부르기
    {
        uimanager.GameStopNoPanel();
        upgradeManager.UpgradeNumbers();
    }
    private void CallNextWave()
    {
        TitleTextNormal();
        uimanager.isGaming = true;
        SpawnSpawnerByWave(); //스포너 생성
        isDefence = true;
        titleText.text = Wave.ToString() + "번째 밤";
        titleText.color = new Color(1, 1, 0); // 노란색
        ShowTitleText();
    }
    public void CallGameOverText()
    {
        TitleTextNormal();
        uimanager.isGaming = false;
        titleText.text = "집을 지켜내지 못했습니다...";
        titleText.color = new Color(1, 0, 0); // 빨간색
        ShowTitleText();

        // 조작 비활성화
        if (playerController != null)
        {
            playerController.SetInputEnabled(false);
        }
        Invoke(nameof(CallGameOver), 4f);
    }
    private void CallGameOver()
    {
        if (uimanager != null)
        {
            uimanager.GameOver();
        }
        else
        {
            Debug.LogWarning("[GameManager] UImanager가 연결되지 않았습니다.");
        }
    }

    public void SpawnSpawnerByWave()
    {
        if (spawnerList == null || spawnerList.Count == 0)
        {
            Debug.LogWarning("[GameManager] spawnerList가 비어 있습니다.");
            return;
        }

        // Wave 1 -> 리스트 0번
        int spawnerIndex = Mathf.RoundToInt(Wave) - 1;

        if (spawnerIndex < 0 || spawnerIndex >= spawnerList.Count)
        {
            Debug.LogWarning(
                $"[GameManager] Wave {Wave}에 해당하는 스포너가 없습니다."
            );
            return;
        }

        GameObject spawnerPrefab = spawnerList[spawnerIndex];

        if (spawnerPrefab == null)
        {
            Debug.LogWarning(
                $"[GameManager] spawnerList[{spawnerIndex}]가 비어 있습니다."
            );
            return;
        }

        Vector3 spawnPosition = spawnerSpawnPoint != null
            ? spawnerSpawnPoint.position
            : transform.position;

        Quaternion spawnRotation = spawnerSpawnPoint != null
            ? spawnerSpawnPoint.rotation
            : Quaternion.identity;

        Instantiate(
            spawnerPrefab,
            spawnPosition,
            spawnRotation
        );

        Debug.Log(
            $"[GameManager] {Wave}번째 웨이브 스포너 생성: {spawnerPrefab.name}"
        );
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
