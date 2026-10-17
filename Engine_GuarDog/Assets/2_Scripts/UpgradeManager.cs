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

    // 한 번 선택된 번호(1-based)를 다시 뽑지 않도록 저장
    private readonly HashSet<int> excludedValues = new HashSet<int>();

    void Awake()
    {
        playerShotRef = FindObjectOfType<PlayerShot>();
        healthItemRef = FindObjectOfType<HealthItem>();
    }

    // 인스펙터 우클릭(또는 컴포넌트 메뉴)에서 실행 가능
    [ContextMenu("Pick Three Unique Numbers And Spawn")]
    public void UpgradeNumbers() // UpgradePanel을 활성화하고, prefabs 리스트에서 3개의 고유한 숫자를 뽑아 SpawnPickedPrefabs 호출
    {
        if (UpgradePanel != null) UpgradePanel.SetActive(true);

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
            pool.Remove(1); // value 1 제거 -> prefabs[0] 제외 (기존 규칙 유지)
        }

        // 이미 제외된 값들은 풀에서 제거 (0 및 7 예외는 ExcludeValue에서 처리되므로 excludedValues에는 추가되지 않음)
        pool.RemoveAll(v => excludedValues.Contains(v));

        if (pool.Count == 0)
        {
            Debug.Log("[UpgradeManager] 선택 가능한 업그레이드가 없습니다(모두 제외됨).");
            return;
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

    void SpawnPickedPrefabs(List<int> picked) // 뽑힌 숫자에 해당하는 프리팹을 spawnPoint1/2/3에 소환 (지정된 캔버스의 자식으로 생성)
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

    // randvalue: 0..4 (0:StatAgainUpgrade, 1:OnRevive, 2:OnAdrenaline, 3:OnBestCondition, 4:OnMultiShot, 5:OnDoubleShot ...)
    // instance: 클릭된 카드 인스턴스, prefabIndex: 원래 prefabs 리스트 인덱스
    public void SwitchCase(int randvalue, Button button, GameObject instance, int prefabIndex)
    {
        // 버튼 클릭 시 능력 적용과 창 닫기를 수행할 래퍼를 연결
        button.onClick.RemoveAllListeners();
        switch (randvalue)
        {
            case 0:
                button.onClick.AddListener(() => { StatAgainUpgrade(); ExcludeValue(prefabIndex + 1); CleanupAfterSelection("StatAgainUpgrade", instance); });
                break;
            case 1:
                button.onClick.AddListener(() => { OnRevive(); ExcludeValue(prefabIndex + 1); CleanupAfterSelection("OnRevive", instance); });
                break;
            case 2:
                button.onClick.AddListener(() => { OnAdrenaline(); ExcludeValue(prefabIndex + 1); CleanupAfterSelection("OnAdrenaline", instance); });
                break;
            case 3:
                button.onClick.AddListener(() => { OnBestCondition(); ExcludeValue(prefabIndex + 1); CleanupAfterSelection("OnBestCondition", instance); });
                break;
            case 4:
                button.onClick.AddListener(() => { OnMultiShot(); ExcludeValue(prefabIndex + 1); CleanupAfterSelection("OnMultiShot", instance); });
                break;
            case 5:
                button.onClick.AddListener(() => { OnDoubleShot(); ExcludeValue(prefabIndex + 1); CleanupAfterSelection("OnDoubleShot", instance); });
                break;
            default:
                Debug.LogWarning($"[UpgradeManager] SwitchCase: 알 수 없는 randvalue {randvalue}");
                break;
        }
    }

    // 선택된 1-based 값을 제외 목록에 추가 (다음 뽑기에서 제외)
    // 예외: prefabs[0] (value 1) 및 prefabs[7] (value 8)은 제외되지 않음
    void ExcludeValue(int value)
    {
        // value는 1-based 번호
        if (value == 1 || value == 8)
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

    // 각 능력 구현 (playerShot/healthItem 안전 참조)
    public void StatAgainUpgrade()
    {
        if (healthItemRef == null) healthItemRef = FindObjectOfType<HealthItem>();
        if (playerShotRef == null) playerShotRef = FindObjectOfType<PlayerShot>();

        // PlayerShot prefab 및 이미 생성된 총알들에 데미지 +3 적용
        if (playerShotRef != null && playerShotRef.bulletPrefab != null)
        {
            var prefabBullet = playerShotRef.bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null)
            {
                prefabBullet.damage += 3;
                Debug.Log("[UpgradeManager] Prefab bullet damage increased by 3.");
            }
        }

        var bullets = FindObjectsOfType<Bullet>();
        foreach (var b in bullets) b.damage += 3;
        Debug.Log($"[UpgradeManager] Scene bullets damage increased by 3 (count: {bullets.Length}).");

        if (healthItemRef != null && healthItemRef.PlayerHealth != null)
        {
            healthItemRef.PlayerHealth.maxHP += 25;
            healthItemRef.PlayerHealth.Heal(999);
        }
    }

    public void OnRevive()
    {
        if (healthItemRef == null) healthItemRef = FindObjectOfType<HealthItem>();
        if (healthItemRef != null)
        {
            healthItemRef.OnRevieve();
            Debug.Log("[UpgradeManager] OnRevive applied.");
        }
    }

    public void OnAdrenaline()
    {
        if (healthItemRef == null) healthItemRef = FindObjectOfType<HealthItem>();
        if (healthItemRef != null)
        {
            healthItemRef.OnAdrenaline();
            Debug.Log("[UpgradeManager] OnAdrenaline applied.");
        }
    }

    public void OnBestCondition()
    {
        if (healthItemRef == null) healthItemRef = FindObjectOfType<HealthItem>();
        if (healthItemRef != null)
        {
            healthItemRef.OnBestCondition();
            Debug.Log("[UpgradeManager] OnBestCondition applied.");
        }
    }

    public void OnMultiShot()
    {
        if (playerShotRef == null) playerShotRef = FindObjectOfType<PlayerShot>();
        if (playerShotRef != null)
        {
            playerShotRef.OnMultiShot();
            Debug.Log("[UpgradeManager] OnMultiShot applied.");
        }
    }

    public void OnDoubleShot()
    {
        if (playerShotRef == null) playerShotRef = FindObjectOfType<PlayerShot>();
        if (playerShotRef != null)
        {
            playerShotRef.OnDoubleShot();
            Debug.Log("[UpgradeManager] OnDoubleShot applied.");
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