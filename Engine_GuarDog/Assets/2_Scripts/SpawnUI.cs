using UnityEngine;
using TMPro;

public class SpawnUI : MonoBehaviour
{
    public TMP_Text spawnUItext;
    public GameManager gameManager;

    private EnemySpawner enemySpawner;
    private int currentSpawn;
    private int totalSpawn;

    void Start()
    {
        FindEnemySpawner();
    }

    void Update()
    {
        if (spawnUItext == null || gameManager == null)
            return;

        // 스포너가 없으면 다시 검색합니다.
        FindEnemySpawner();

        // 스포너가 존재할 때만 스포너 정보를 갱신합니다.
        // 스포너가 제거되어도 마지막 값은 유지됩니다.
        if (enemySpawner != null)
        {
            currentSpawn = enemySpawner.currentSpawn;
            totalSpawn = enemySpawner.totalSpawn;
        }

        // EnemySpawner가 없어도 UI 전체는 계속 갱신합니다.
        spawnUItext.text =
            $"처치한 모든 적 : {gameManager.KillCount}" +
            $"\n오늘 찾아올 적 : {currentSpawn} / {totalSpawn}" +
            $"\nEXP : {gameManager.MissionKillCount} / {gameManager.MissionCount}";
    }

    private void FindEnemySpawner()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }
    }
}