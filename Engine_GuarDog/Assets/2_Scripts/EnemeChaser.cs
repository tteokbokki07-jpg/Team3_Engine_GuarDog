using UnityEngine;

public class EnemeChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectRange = 15f; //플레이어 추적 범위
    public float attackRange = 2f; //공격 범위
    public int damage = 1;
    public float attackCooldown = 1f; //공격 쿨타임
    public bool goHome = false; //플레이어가 추적 범위에 없을때 집 추격

    Transform player;
    Transform home;
    Transform currentTarget;
    Collider playerCollider;
    Collider homeCollider;
    float lastAttackTime = -999f;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform; //플레이어 Transform 저장
            playerCollider = p.GetComponent<Collider>();
        }

        GameObject h = GameObject.FindWithTag("Home");
        if (h != null)
        {
            home = h.transform; //집 Transform 저장
            homeCollider = h.GetComponent<Collider>();
        }

        currentTarget = player != null ? player : home;
    }

    void Update()
    {
        // 플레이어가 씬에서 사라졌다면 다시 찾기 시도
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerCollider = p.GetComponent<Collider>();
            }
        }

        // 플레이어가 존재하고 거리 계산 (플레이어 중심이 아닌 콜라이더 표면 기준)
        float distanceToPlayer = float.MaxValue;
        if (player != null)
        {
            if (playerCollider != null)
                distanceToPlayer = Vector3.Distance(transform.position, playerCollider.ClosestPoint(transform.position));
            else
            {
                Vector3 toPlayerVec = player.position - transform.position;
                toPlayerVec.y = 0;
                distanceToPlayer = toPlayerVec.magnitude;
            }
        }

        // 플레이어가 detectRange 안에 있으면 플레이어 추적, 아니면 집 추적
        if (player != null && distanceToPlayer <= detectRange)
        {
            goHome = false;
            currentTarget = player;
        }
        else
        {
            goHome = true;
            currentTarget = home;
        }

        if (currentTarget == null) return; //추적할 대상이 없으면 아무것도 안함

        // 타겟까지의 거리 계산: 콜라이더가 있으면 콜라이더의 가장 가까운 지점을 사용
        Collider targetCollider = currentTarget == home ? homeCollider : (currentTarget == player ? playerCollider : null);

        Vector3 closestPoint = targetCollider != null
            ? targetCollider.ClosestPoint(transform.position)
            : currentTarget.position;

        Vector3 toTarget = closestPoint - transform.position;
        toTarget.y = 0; // 높이 무시
        float distance = toTarget.magnitude;

        // 타겟 방향으로 회전
        if (toTarget.sqrMagnitude > 0.001f)
            transform.forward = toTarget.normalized;

        // 플레이어 타겟 행동
        if (!goHome && currentTarget == player)
        {
            if (distance > attackRange)
            {
                float move = moveSpeed * Time.deltaTime;
                transform.position += transform.forward * move;
            }
            else if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                var hp = player.GetComponent<Health>();
                if (hp != null) hp.TakeDamage(damage);
            }
            return;
        }

        // 집 타겟 행동: 집 콜라이더 표면 기준으로 공격 가능하면 공격, 아니면 이동
        if (currentTarget == home)
        {
            if (distance > attackRange)
            {
                // 집으로 이동
                float move = moveSpeed * Time.deltaTime;
                transform.position += transform.forward * move;
            }
            else if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                var hp = home.GetComponent<Health>();
                if (hp != null) hp.TakeDamage(damage);
                Debug.Log("집공격");
            }
        }
    }
}
