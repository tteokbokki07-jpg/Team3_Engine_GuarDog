using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UpgradeManager : MonoBehaviour
{
    [Header("생성 시 부모로 사용할 캔버스(또는 다른 Transform)를 할당)")]
    public GameObject parentCanvas;
    public Transform spawnPoint1; // 생성 위치를 지정할 Transform
    public Transform spawnPoint2; // 생성 위치를 지정할 Transform
    public Transform spawnPoint3; // 생성 위치를 지정할 Transform
    [Header("프리팹 리스트 (인스펙터 할당)")]
    public List<GameObject> prefabs = new List<GameObject>();

    private PlayerShot playerShotRef;
    private HealthItem healthItemRef;

    // 인스펙터 우클릭(또는 컴포넌트 메뉴)에서 실행 가능
    [ContextMenu("Pick Three Unique Numbers And Spawn")]
    public void UpgradeNumbers()
    {
        int n = prefabs.Count;
        if (n == 0)
        {
            Debug.Log("[UpgradeManager] prefabs 리스트가 비어있습니다.");
            return;
        }

        // 1..n 숫자 풀 생성
        List<int> pool = new List<int>(n);
        for (int i = 1; i <= n; i++) pool.Add(i);

        // Fisher-Yates 셔플
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = pool[i];
            pool[i] = pool[j];
            pool[j] = tmp;
        }

        int take = Mathf.Min(3, n);
        var picked = pool.GetRange(0, take);

        Debug.Log($"[UpgradeManager] 리스트 길이: {n}. 뽑힌 숫자({take}개): {string.Join(", ", picked)}");

        SpawnPickedPrefabs(picked);
    }

    void Awake()
    {
        playerShotRef = FindObjectOfType<PlayerShot>();
        healthItemRef = FindObjectOfType<HealthItem>();
    }

    // 뽑힌 숫자에 해당하는 프리팹을 spawnPoint1/2/3에 소환 (지정된 캔버스의 자식으로 생성)
    void SpawnPickedPrefabs(List<int> picked)
    {
        Transform[] spawnPoints = new Transform[] { spawnPoint1, spawnPoint2, spawnPoint3 };
        Transform parent = parentCanvas != null ? parentCanvas.transform : null;

        for (int i = 0; i < picked.Count && i < spawnPoints.Length; i++)
        {
            int prefabIndex = picked[i] - 1; // pool은 1-based
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

            Debug.Log($"[UpgradeManager] Spawned prefab index {prefabIndex} at spawnPoint {i + 1} as child of {(parent != null ? parent.name : "scene root")}.");

            // prefabIndex를 0..4 범위의 randvalue로 변환 (리스트 항목이 업그레이드 타입에 대응한다고 가정)
            int randvalue = prefabIndex % 5; // 0..4 매핑
            // 버튼이 있으면 리스너 할당
            var btn = instance.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                SwitchCase(randvalue, btn);
            }
            else
            {
                Debug.LogWarning("[UpgradeManager] 생성된 인스턴스에 Button 컴포넌트가 없습니다.");
            }
        }
    }

    // randvalue: 0..4 (0:OnAdrenaline, 1:OnBestCondition, 2:OnMultiShot, 3:OnDoubleShot, 4:OnBigShot)
    public void SwitchCase(int randvalue, Button button)
    {
        switch (randvalue)
        {
            case 0:
                button.onClick.AddListener(OnAdrenaline);
                break;
            case 1:
                button.onClick.AddListener(OnBestCondition);
                break;
            case 2:
                button.onClick.AddListener(OnMultiShot);
                break;
            case 3:
                button.onClick.AddListener(OnDoubleShot);
                break;
            case 4:
                button.onClick.AddListener(OnBigShot);
                break;
            default:
                Debug.LogWarning($"[UpgradeManager] SwitchCase: 알 수 없는 randvalue {randvalue}");
                break;
        }
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
}

