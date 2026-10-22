using UnityEngine;
using TMPro;

public class SpawnUI : MonoBehaviour
{
    public TMP_Text currentSpawnText;
    public TMP_Text totalSpawnText;
    public TMP_Text spawnUItext;

    EnemySpawner enemySpawner;

    void Start()
    {
        enemySpawner = FindObjectOfType<EnemySpawner>();
    }

    void Update()
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
            if (enemySpawner == null) return;
        }

        if (currentSpawnText != null)
            currentSpawnText.text = enemySpawner.currentSpawn.ToString();

        if (totalSpawnText != null)
            totalSpawnText.text = enemySpawner.totalSpawn.ToString();
        
        spawnUItext.text = $"Current Spawn: {enemySpawner.currentSpawn} / Total Spawn: {enemySpawner.totalSpawn}";
    }
}
