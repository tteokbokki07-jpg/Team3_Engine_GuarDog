using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [Header("생성 시 부모로 사용할 캔버스(또는 다른 Transform)를 할당")]
    public GameObject parentCanvas;
    public GameObject UpgradePanel; // UpgradePanel을 인스펙터에서 할당
    public Transform spawnPoint1; // 생성 위치를 지정할 Transform
    public Transform spawnPoint2; // 생성 위치를 지정할 Transform
    public Transform spawnPoint3; // 생성 위치를 지정할 Transform
    [Header("프리팹 리스트 (인스펙터 할당)")]
    public List<GameObject> prefabs = new List<GameObject>();

    private PlayerShot playerShotRef;
    private HealthItem healthItemRef;

    // 이번 업그레이드 호출에서 생성한 카드 인스턴스들만 추적
    private readonly List<GameObject> spawnedCards = new List<GameObject>();
    // 생성된 카드 인스턴스 -> 원래 prefabs 리스트의 인덱스 매핑
    private readonly Dictionary<GameObject, int> spawnedCardToPrefabIndex = new Dictionary<GameObject, int>();

    void Awake()
    {
        playerShotRef = FindObjectOfType<PlayerShot>();
        healthItemRef = FindObjectOfType<HealthItem>();
    }

    // 인스펙터 우클릭(또는 컴포넌트 메뉴)에서 실행 가능
    [ContextMenu("Pick Three Unique Numbers And Spawn")]
    public void UpgradeNumbers()
    {
        UpgradePanel.SetActive(true);

        int n = prefabs.Count;
        if (n == 0)
        {
            Debug.Log("[UpgradeManager] prefabs 리스트가 비어있습니다.");
            return;
        }

        // 1..n 숫자 풀 생성 (1은 prefabs[0]에 대응)
        List<int> pool = new List<int>(n);
        for (int i = 1; i <= n; i++) pool.Add(i);

        // prefabs.Count > 3 이면 0번(prefabs[0] -> value 1)을 제외
        if (n > 3)
        {
            pool.Remove(1); // value 1 제거 -> prefabs[0] 제외
        }

        // Fisher-Yates 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = pool[i];
            pool[i] = pool[j];
            pool[j] = tmp;
        }

        int take = Mathf.Min(3, pool.Count);
        var picked = pool.GetRange(0, take);

        Debug.Log($"[UpgradeManager] 리스트 길이: {n}. 풀 크기:{pool.Count}. 뽑힌 숫자({take}개): {string.Join(", ", picked)}");

        SpawnPickedPrefabs(picked);
    }

    // 뽑힌 숫자에 해당하는 프리팹을 spawnPoint1/2/3에 소환 (지정된 캔버스의 자식으로 생성)
    void SpawnPickedPrefabs(List<int> picked)
    {
        Transform[] spawnPoints = new Transform[] { spawnPoint1, spawnPoint2, spawnPoint3 };
        Transform parent = parentCanvas != null ? parentCanvas.transform : null;

        // 이전에 생성한 카드 목록 초기화 (새 창 열 때 이전 카드가 있으면 제거되지 않도록 관리)
        spawnedCards.Clear();
        spawnedCardToPrefabIndex.Clear();

        for (int i = 0; i < picked.Count && i < spawnPoints.Length; i++)
        {
            int prefabIndex = picked[i] - 1; // pool은 1-based -> prefabIndex는 0-based
            if (prefabIndex < 0 || prefabIndex >= prefabs.Count)
            {
                Debug.LogWarning($"[UpgradeManager] 잘못된 인덱스 {prefabIndex} (picked value {picked[i]}).");
                continue;
            }

            var prefab = prefabs[prefabIndex]; // 0-based 인덱스
            var spawnPoint = spawnPoints[i];

            if (prefab == null)
            {
                Debug.LogWarning($"[UpgradeManager] prefab at index {prefabIndex} is null.");
                continue;
            }

            if (spawnPoint == null)
            {
                Debug.LogWarning($"[UpgradeManager] spawnPoint {i + 1} 가 할당되지 않았습니다. 위치에 소환하지 않습니다.");
                continue;
            }

            GameObject instance;
            if (parent != null)
            {
                instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, parent);
            }
            else
            {
                instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            }

            // 생성된 카드 인스턴스만 추적
            spawnedCards.Add(instance);
            spawnedCardToPrefabIndex[instance] = prefabIndex;

            Debug.Log($"[UpgradeManager] Spawned prefab index {prefabIndex} at spawnPoint {i + 1} as child of {(parent != null ? parent.name : "scene root")}.");

            // prefabIndex를 0..4 범위의 randvalue로 변환 (0:OnAdrenaline,1:OnBestCondition,2:OnMultiShot,3:OnDoubleShot,4:OnBigShot)
            int randvalue = prefabIndex % 5;

            // 버튼이 있으면 리스너 할당 (선택 시 능력 적용 후 창 닫음)
            var btn = instance.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                // 인스턴스과 prefabs 원래 인덱스를 전달
                SwitchCase(randvalue, btn, instance, prefabIndex);
            }
            else
            {
                Debug.LogWarning("[UpgradeManager] 생성된 인스턴스에 Button 컴포넌트가 없습니다.");
            }
        }
    }

    // randvalue: 0..4 (0:OnAdrenaline, 1:OnBestCondition, 2:OnMultiShot, 3:OnDoubleShot, 4:OnBigShot)
    // instance: 클릭된 카드 인스턴스, prefabIndex: 원래 prefabs 리스트 인덱스
    public void SwitchCase(int randvalue, Button button, GameObject instance, int prefabIndex)
    {
        // 버튼 클릭 시 능력 적용과 창 닫기를 수행할 래퍼를 연결
        button.onClick.RemoveAllListeners();
        switch (randvalue)
        {
            case 0:
                //button.onClick.AddListener(() => { OnAdrenaline(); RemovePrefabAtIndex(prefabIndex); CleanupAfterSelection("OnAdrenaline", instance); });
                break;
            case 1:
                //button.onClick.AddListener(() => { OnBestCondition(); RemovePrefabAtIndex(prefabIndex); CleanupAfterSelection("OnBestCondition", instance); });
                break;
            case 2:
                button.onClick.AddListener(() => { OnAdrenaline(); RemovePrefabAtIndex(prefabIndex); CleanupAfterSelection("OnAdrenaline", instance); });
                break;
            case 3:
                button.onClick.AddListener(() => { OnBestCondition(); RemovePrefabAtIndex(prefabIndex); CleanupAfterSelection("OnBestCondition", instance); });
                break;
            case 4:
                button.onClick.AddListener(() => { OnMultiShot(); RemovePrefabAtIndex(prefabIndex); CleanupAfterSelection("OnMultiShot", instance); });
                break;
            case 5:
                button.onClick.AddListener(() => { OnDoubleShot(); RemovePrefabAtIndex(prefabIndex); CleanupAfterSelection("OnDoubleShot", instance); });
                break;
            case 6:
                button.onClick.AddListener(() => { OnBigShot(); RemovePrefabAtIndex(prefabIndex); CleanupAfterSelection("OnBigShot", instance); });
                break;
            default:
                Debug.LogWarning($"[UpgradeManager] SwitchCase: 알 수 없는 randvalue {randvalue}");
                break;
        }
    }

    // prefabs 리스트에서 선택한 인덱스의 프리팹을 제거
    // 요구사항: 0번 엘리멘트(prefabs[0])는 예외로 처리하여 제거되지 않음
    void RemovePrefabAtIndex(int index)
    {
        if (index < 0 || index >= prefabs.Count)
        {
            Debug.LogWarning($"[UpgradeManager] RemovePrefabAtIndex: 잘못된 인덱스 {index}");
            return;
        }

        if (index == 0)
        {
            Debug.Log($"[UpgradeManager] RemovePrefabAtIndex: 인덱스 0은 예외로 처리되어 제거되지 않습니다.");
            return;
        }

        Debug.Log($"[UpgradeManager] Removing prefab at index {index} ({prefabs[index]?.name ?? "null"}) from prefabs list.");
        prefabs.RemoveAt(index);
    }

    // 각 버튼이 호출할 동작들 (간단히 PlayerShot / HealthItem에 플래그를 설정하거나 동작 호출)
    public void OnAdrenaline()
    {
        if (healthItemRef != null)
        {
            healthItemRef.isAdrenaline = true;
            Debug.Log("[UpgradeManager] OnAdrenaline applied.");
        }
        else Debug.LogWarning("[UpgradeManager] HealthItem을 찾을 수 없음.");
    }

    public void OnBestCondition()
    {
        if (healthItemRef != null)
        {
            healthItemRef.isBestCondition = true;
            Debug.Log("[UpgradeManager] OnBestCondition applied.");
        }
        else Debug.LogWarning("[UpgradeManager] HealthItem을 찾을 수 없음.");
    }

    public void OnMultiShot()
    {
        if (playerShotRef != null)
        {
            playerShotRef.OnMultiShot();
            Debug.Log("[UpgradeManager] OnMultiShot applied.");
        }
        else Debug.LogWarning("[UpgradeManager] PlayerShot을 찾을 수 없음.");
    }

    public void OnDoubleShot()
    {
        if (playerShotRef != null)
        {
            playerShotRef.OnDoubleShot();
            Debug.Log("[UpgradeManager] OnDoubleShot applied.");
        }
        else Debug.LogWarning("[UpgradeManager] PlayerShot을 찾을 수 없음.");
    }

    public void OnBigShot()
    {
        if (playerShotRef != null)
        {
            playerShotRef.OnBigShot();
            Debug.Log("[UpgradeManager] OnBigShot applied.");
        }
        else Debug.LogWarning("[UpgradeManager] PlayerShot을 찾을 수 없음.");
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

        // 2) UpgradePanel 객체 비활성화 (우선 parentCanvas, 없으면 이름으로 검색)
        if (parentCanvas != null)
        {
            parentCanvas.SetActive(false);
        }
        else
        {
            var panel = GameObject.Find("UpgradePanel");
            if (panel != null) panel.SetActive(false);
        }
    }
}

