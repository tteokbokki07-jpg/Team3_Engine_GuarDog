using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("씬의 DayNightCycle을 할당하거나 자동으로 찾습니다.")]
    public DayNightCycle dayNightCycle;

    // 내부 상태 추적
    private readonly HashSet<GameObject> enemies = new();
    private bool enemiesPresent = false;

    void Start()
    {
        if (dayNightCycle == null)
            dayNightCycle = FindObjectOfType<DayNightCycle>();

        // 씬에 이미 있는 적들 초기 등록 (옵션)
        var existing = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in existing) RegisterEnemy(e);
        ApplyDayNight();
    }

    void ApplyDayNight()
    {
        if (dayNightCycle == null) return;
        if (enemies.Count > 0) dayNightCycle.UpdateNight();
        else dayNightCycle.UpdateDay();
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        enemies.Add(enemy);
        if (!enemiesPresent)
        {
            enemiesPresent = true;
            ApplyDayNight();
        }
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        enemies.Remove(enemy);
        if (enemiesPresent && enemies.Count == 0)
        {
            enemiesPresent = false;
            ApplyDayNight();
        }
    }
}
