using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;

[System.Serializable]
public class UpgradeData
{
    public GameObject prefabs;
    public UnityEvent onSelect; // 선택 시 실행할 이벤트
}

public class UpgradeManager : MonoBehaviour
{
    public TextMeshProUGUI upgText;
    public GameObject parentCanvas;
    public GameObject UpgradePanel; // UpgradePanel을 인스펙터에서 할당
    public Transform spawnPoint1; // 생성 위치를 지정할 Transform
    public Transform spawnPoint2; // 생성 위치를 지정할 Transform
    public Transform spawnPoint3; // 생성 위치를 지정할 Transform
    public Health playerhealth;
    public Button rerollButton;
    public UpgradeUI upgradeUI;
    public GameManager gameManager;
    public TextMeshProUGUI BonusUpg;
    [Header("프리팹 리스트 (인스펙터 할당)")]
    public UpgradeData[] upgradeData;
    [Header("카드 등장 애니메이션")]
    public float scaleUpDuration = 0.3f; // 커지는 데 걸리는 시간


    private PlayerShot playerShotRef;
    private HealthItem healthItemRef;

    // 이번 업그레이드 호출에서 생성한 카드 인스턴스들만 추적
    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    // 생성된 카드 인스턴스 -> 원래 prefabs 리스트의 인덱스 매핑
    private readonly Dictionary<GameObject, int> spawnedCardToPrefabIndex = new Dictionary<GameObject, int>();

    // 한 번 선택된 번호(1-based)를 다시 뽑지 않도록 저장
    public readonly HashSet<int> excludedValues = new HashSet<int>();

    void Awake()
    {
        upgText.text = "(원하는 능력을 하나 선택하세요!)\n강해지기 위한 고민중...";

        playerShotRef = FindObjectOfType<PlayerShot>();
        healthItemRef = FindObjectOfType<HealthItem>();

        if (upgradeUI == null)
            upgradeUI = FindObjectOfType<UpgradeUI>();
        playerhealth = FindObjectOfType<Health>();
        gameManager = FindObjectOfType<GameManager>();
    }
    void Update()
    {
        rerollButton.interactable = playerhealth.currentHP > 50f;
        BonusUpg.text = $"{gameManager.BonusUpg}";
    }

    public void RerollRisk()
    {
        playerhealth.TakeDamage(50);
        UpgradeNumbers();
    }

    // 인스펙터 우클릭(또는 컴포넌트 메뉴)에서 실행 가능
    [ContextMenu("Pick Three Unique Numbers And Spawn")]
    public void UpgradeNumbers()
    {
        foreach (var go in spawnedCards)
        {
            if (go != null) Destroy(go);
        }
        spawnedCards.Clear();
        spawnedCardToPrefabIndex.Clear();

        if (UpgradePanel != null)
            UpgradePanel.SetActive(true);

        int n = upgradeData.Length;

        if (n == 0)
        {
            Debug.Log("업그레이드 리스트가 비어 있습니다.");
            return;
        }

        List<int> pool = new List<int>();
        // 1-based 번호 생성
        for (int i = 1; i <= n; i++)
            pool.Add(i);

        // excludedValues 먼저 제거한 뒤, 실제 후보 수가 3보다 크면 1번을 제외
        pool.RemoveAll(v => excludedValues.Contains(v));

        if (pool.Count > 3)
        {
            // 1번이 후보에 있으면 제거
            if (pool.Remove(1))
                Debug.Log("1번제외됨");
        }

        if (pool.Count == 0)
        {
            Debug.Log("선택 가능한 업그레이드가 없습니다.");
            return;
        }

        // 고유한 숫자 선택 (중복 발생 시 재추첨 방지)
        int take = Mathf.Min(3, pool.Count);
        List<int> picked = new List<int>();
        int maxAttempts = 100;
        int attempts = 0;
        while (picked.Count < take && pool.Count > 0 && attempts < maxAttempts)
        {
            attempts++;
            int idx = UnityEngine.Random.Range(0, pool.Count);
            int val = pool[idx];

            if (picked.Contains(val))
            {
                // 중복이면 건너뛰고 다시 시도
                continue;
            }

            picked.Add(val);
            // 이미 뽑은 값은 pool에서 제거하여 재등장 방지
            pool.RemoveAt(idx);
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning("[UpgradeManager] Unique selection attempts exceeded limit.");

        // 선택 결과 로그
        var pickedStrings = picked.ConvertAll(p => p.ToString()).ToArray();
        Debug.Log($"[UpgradeManager] Picked numbers: {string.Join(", ", pickedStrings)}");

        // UI로는 지금 '랜덤 뽑힌 숫자'를 전달하지 않음.
        SpawnPickedPrefabs(picked);
    }

    void SpawnPickedPrefabs(List<int> picked)
    {
        Transform[] spawnPoints =
        {
            spawnPoint1,
            spawnPoint2,
            spawnPoint3
        };

        Transform parent = parentCanvas != null
            ? parentCanvas.transform
            : null;

        spawnedCards.Clear();
        spawnedCardToPrefabIndex.Clear();

        for (int i = 0; i < picked.Count && i < spawnPoints.Length; i++)
        {
            int prefabIndex = picked[i] - 1;

            if (prefabIndex < 0 || prefabIndex >= upgradeData.Length)
                continue;

            UpgradeData data = upgradeData[prefabIndex];
            GameObject prefab = data.prefabs;
            Transform spawnPoint = spawnPoints[i];

            if (prefab == null || spawnPoint == null)
            {
                Debug.LogWarning($"업그레이드 {prefabIndex}의 프리팹 또는 생성 위치가 없습니다.");
                continue;
            }

            GameObject instance = parent != null
                ? Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, parent)
                : Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            // Scale을 0으로 초기화
            instance.transform.localScale = Vector3.zero;

            // Scale이 1까지 커지는 애니메이션 실행
            StartCoroutine(ScaleUp(instance.transform));

            spawnedCards.Add(instance);
            spawnedCardToPrefabIndex[instance] = prefabIndex;

            Button btn = instance.GetComponentInChildren<Button>();

            if (btn != null)
            {
                int index = prefabIndex; // 0-based
                UpgradeData selectedData = data;

                // 클릭 시: 선택 이벤트 실행, UI에 '선택된 카드 인덱스(1-based)' 전달, 제외 처리, 창 닫기
                btn.onClick.AddListener(() =>
                {
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.cardSelectSound, 0.5f, 1f); //사운드 재생(음량, 높이)
                    // 중복 클릭 방지
                    foreach (GameObject card in spawnedCards)
                    {
                        if (card == null)
                            continue;

                        Button cardButton = card.GetComponentInChildren<Button>();

                        if (cardButton != null)
                            cardButton.interactable = false;
                    }

                    // 선택한 카드의 효과 적용
                    selectedData.onSelect?.Invoke();

                    // 선택한 카드 번호 전달
                    upgradeUI?.SetPickedNumbers(new List<int> { index + 1 });

                    // 선택한 카드 제외 처리
                    ExcludeValue(index + 1);

                    // 선택하지 않은 카드 2개 사라지는 애니메이션 실행
                    StartCoroutine(
                        DisappearUnselectedCards(
                            instance,
                            selectedData.prefabs.name
                        )
                    );
                });
            }
            else
            {
                Debug.LogWarning("생성된 프리팹에 Button 컴포넌트가 없습니다.");
            }
        }
    }

    // SwitchCase는 이제 클릭 시 '창 닫기' 로직만 수행하도록 변경
    public void SwitchCase(int randvalue, Button button, GameObject instance, int prefabIndex)
    {
        button.onClick.RemoveAllListeners();
        // 클릭하면 선택 이벤트 적용은 다른 경로(UpgradeData.onSelect)에서 처리되고,
        // 여기서는 오직 창 닫기(및 카드 정리)만 수행합니다.
        button.onClick.AddListener(() =>
        {
            CleanupAfterSelection("Selected", instance);
        });
    }

    // 선택된 1-based 값을 제외 목록에 추가 (다음 뽑기에서 제외)
    // 예외: prefabs[0] (value 1) 및 prefabs[7] (value 8) 및 prefabs[12] (value 13)은 제외되지 않음
    void ExcludeValue(int value)
    {
        // value는 1-based 번호
        if (value == 1 || value == 8 || value == 13) //예외처리
        {
            Debug.Log($"[UpgradeManager] Value {value} is exception and will not be excluded.");
            return;
        }

        if (!excludedValues.Contains(value))
        {
            excludedValues.Add(value);
            Debug.Log($"[UpgradeManager] Value {value} added to excludedValues.");
        }
    }

    
    // 선택 후 정리: 이번에 생성한 카드 인스턴스들만 파괴하고 UpgradePanel 비활성화
    void CleanupAfterSelection(string abilityName, GameObject selectedInstance)
    {
        Debug.Log($"[UpgradeManager] Ability selected: {abilityName}");

        // 1) 이번에 생성한 카드 인스턴스들만 파괴
        foreach (var go in spawnedCards)
        {
            if (go != null) Destroy(go);
        }
        spawnedCards.Clear();
        spawnedCardToPrefabIndex.Clear();

        // 2) UpgradePanel 객체 비활성화 (우선 UpgradePanel, 없으면 parentCanvas)
        if (UpgradePanel != null) UpgradePanel.SetActive(false);
        else if (parentCanvas != null) parentCanvas.SetActive(false);

        if (gameManager.BonusUpg > 0)
        {
            UpgradeNumbers();
            Debug.Log("보너스뽑기");
            gameManager.BonusUpg--;
        }
        else
        {
            // uimanager의 GamePause() 호출하여 게임 재개
            var uiManager = FindObjectOfType<UImanager>();
            uiManager.GamePauseNoPanel();
        }

        
    }

    IEnumerator ScaleUp(Transform cardTransform) // 카드가 커지면서 등장하는 애니메이션
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.cardSound, 1.5f, 1f); //사운드 재생(음량, 높이)

        float elapsed = 0f;

        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;

        cardTransform.localScale = startScale;

        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / scaleUpDuration);

            // 부드럽게 커지는 효과
            progress = Mathf.SmoothStep(0f, 1f, progress);

            cardTransform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                progress
            );

            yield return null;
        }

        // 마지막 Scale을 정확하게 1로 설정
        cardTransform.localScale = targetScale;
    }
    
    IEnumerator ScaleDown(Transform cardTransform) // 선택하지 않은 카드가 작아지면서 사라지는 애니메이션
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.cardSound, 1.5f, 1f); //사운드 재생(음량, 높이)

        float elapsed = 0f;
        float duration = 0.2f;

        Vector3 startScale = cardTransform.localScale;
        Vector3 targetScale = Vector3.zero;

        while (elapsed < duration)
        {
            if (cardTransform == null)
                yield break;

            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            cardTransform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                progress
            );

            yield return null;
        }

        if (cardTransform != null)
            cardTransform.localScale = Vector3.zero;
    }

    IEnumerator DisappearUnselectedCards(
        GameObject selectedInstance,
        string abilityName) // 선택한 카드를 제외한 나머지 카드 제거 애니메이션
    {
        List<Coroutine> animations = new List<Coroutine>();

        foreach (GameObject card in spawnedCards)
        {
            // 선택한 카드는 제외
            if (card == null || card == selectedInstance)
                continue;

            animations.Add(
                StartCoroutine(ScaleDown(card.transform))
            );
        }

        // 모든 카드의 축소 애니메이션이 끝날 때까지 대기
        yield return new WaitForSecondsRealtime(0.2f);

        // 기존 카드 선택 후 정리 로직 실행
        CleanupAfterSelection(abilityName, selectedInstance);
    }

}