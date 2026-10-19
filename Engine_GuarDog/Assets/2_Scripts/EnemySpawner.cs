using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;   // 근접, 원거리
    public Transform[] spawnPoints;
    public float timer; //타이머
    public float spawnInterval = 3f; //스폰 간격
    public int totalSpawn = 10; //소환할 적 수
    public int currentSpawn = 0; //현재 소환한 적 수

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        // 더 이상 소환할 적이 없다면 스포너 제거
        if (currentSpawn >= totalSpawn)
        {
            Destroy(gameObject);
            return;
        }

        int e = Random.Range(0, enemyPrefabs.Length);
        int s = Random.Range(0, spawnPoints.Length);
        Vector3 pos = spawnPoints[s].position;
        Instantiate(enemyPrefabs[e], pos, Quaternion.identity);

        currentSpawn++;
        if (currentSpawn >= totalSpawn)
        {
            Destroy(gameObject);
        }
    }

}