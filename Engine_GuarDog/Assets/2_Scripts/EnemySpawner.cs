using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;

    [Header("적 생성 위치")]
    public Vector3[] spawnPositions;

    public float timer;
    public float spawnInterval = 3f; //소환주기
    public int totalSpawn = 10; //총 소환값
    public int currentSpawn = 0; //현재 소환값

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < spawnInterval)
            return;

        timer = 0f;

        // 더 이상 소환할 적이 없다면 스포너 제거
        if (currentSpawn >= totalSpawn)
        {
            Destroy(gameObject);
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] enemyPrefabs가 비어 있습니다.");
            return;
        }

        if (spawnPositions == null || spawnPositions.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] spawnPositions가 비어 있습니다.");
            return;
        }

        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        int positionIndex = Random.Range(0, spawnPositions.Length);

        Vector3 spawnPosition = spawnPositions[positionIndex];

        Instantiate(
            enemyPrefabs[enemyIndex],
            spawnPosition,
            Quaternion.identity
        );

        currentSpawn++;

        if (currentSpawn >= totalSpawn)
        {
            Destroy(gameObject);
        }
    }
}