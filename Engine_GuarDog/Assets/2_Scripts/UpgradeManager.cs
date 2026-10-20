using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class UpgradeData
{
    public GameObject prefabs;
    public UnityEvent onSelect; // 선택 시 실행할 이벤트
}

public class UpgradeManager : MonoBehaviour
{
    [Header("생성 시 부모로 사용할 캔버스(또는 다른 Transform)를 할당")]
    public GameObject parentCanvas;
    public GameObject UpgradePanel; // UpgradePanel을 인스펙터에서 할당
    public Transform spawnPoint1; // 생성 위치를 지정할 Transform
    public Transform spawnPoint2; // 생성 위치를 지정할 Transform
    public Transform spawnPoint3; // 생성 위치를 지정할 Transform
    [Header("프리팹 리스트 (인스펙터 할당)")]
    public UpgradeData[] upgradeData;

    // Upgrade UI 참조 (인스펙터 할당 가능, 없으면 Awake에서 Find)
    public UpgradeUI upgradeUI;

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
        playerShotRef = FindObjectOfType<PlayerShot>();
        healthItemRef = FindObjectOfType<HealthItem>();

        if (upgradeUI == null)
            upgradeUI = FindObjectOfType<UpgradeUI>();
    }

    // 인스펙터 우클릭(또는 컴포넌트 메뉴)에서 실행 가능
    [ContextMenu("Pick Three Unique Numbers And Spawn")]
    public void UpgradeNumbers()
    {
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
                    selectedData.onSelect?.Invoke();

                    // UpgradeUI로 전달: 선택한 카드의 인덱스 번호(1-based)
                    upgradeUI?.SetPickedNumbers(new List<int> { index + 1 });

                    ExcludeValue(index + 1);

                    CleanupAfterSelection(
                        selectedData.prefabs.name,
                        instance
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
    }
}