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

        for (int i = 1; i <= n; i++)
            pool.Add(i);

        // 기존 규칙 유지
        if (n > 3)
            pool.Remove(1);

        pool.RemoveAll(v => excludedValues.Contains(v));

        if (pool.Count == 0)
        {
            Debug.Log("선택 가능한 업그레이드가 없습니다.");
            return;
        }

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int take = Mathf.Min(3, pool.Count);
        var picked = pool.GetRange(0, take);

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
                int index = prefabIndex;
                UpgradeData selectedData = data;

                // 기존 방식: onSelect 실행 + 제외 처리 + 창 닫기
                btn.onClick.AddListener(() =>
                {
                    selectedData.onSelect?.Invoke();

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