using UnityEngine;

public class EnemeChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectRange = 15f; //플레이어 추적 범위
    public float attackRange = 2f; //공격 범위
    public int damage = 1;
    public float attackCooldown = 1f; //공격 쿨타임

    Transform player;
    float lastAttackTime = -999f;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform; //플레이어 Transform 저장
    }

    void Update()
    {
        if (player == null) return; //플레이어가 없으면 추적하지 않음

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0; //높이 무시
        float distance = toPlayer.magnitude; //플레이어와의 거리 계산

        if (distance > detectRange) return; //추적 범위 밖이면 추적X

        transform.forward = toPlayer.normalized; //플레이어 방향으로 회전

        if (distance > attackRange)
        { //공격 범위 밖이면 이동
            float move = moveSpeed * Time.deltaTime;
            transform.position += transform.forward * move; //플레이어 방향으로 이동
        }
        else if (Time.time >= lastAttackTime + attackCooldown)
        { //공격 범위 안이고 쿨타임이 지났으면 공격
            lastAttackTime = Time.time; //마지막 공격 시간 업데이트
            player.GetComponent<Health>().TakeDamage(damage); //데미지 적용
        }
    }


}
