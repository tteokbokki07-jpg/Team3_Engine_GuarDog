using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bulletPrefab;    // EnemyBullet
    public Transform firePoint;
    public float moveSpeed = 2f;
    public float detectRange = 15f; //플레이어 추적 범위
    public float keepDistance = 8f;    // 여기까지만 다가옴
    public float fireRange = 12f;      // 이 안이면 발사
    public float fireInterval = 2f; // 발사 간격
    public float bulletSpeed = 10f;
    public bool goHome = false; //플레이어가 추적 범위에 없을때 집 추격

    Transform player;
    Transform home;
    Transform currentTarget;
    Collider playerCollider;
    Collider homeCollider;
    float lastFireTime;
    float lastAttackTime = -999f;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
        if (p != null) playerCollider = p.GetComponent<Collider>();
        GameObject h = GameObject.FindWithTag("Home");
        if (h != null)
        {
            home = h.transform;
            homeCollider = h.GetComponent<Collider>();
        }
        currentTarget = player != null ? player : home;
        lastFireTime = Time.time;

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

        // 플레이어가 존재하고 거리 계산 (콜라이더 표면 기준)
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

        if (currentTarget == null) return;

        // 타겟까지의 거리 계산: 콜라이더가 있으면 콜라이더의 가장 가까운 지점을 사용
        Collider targetCollider = currentTarget == home ? homeCollider : (currentTarget == player ? playerCollider : null);

        Vector3 closestPoint = targetCollider != null
            ? targetCollider.ClosestPoint(transform.position)
            : currentTarget.position;

        Vector3 toTarget = closestPoint - transform.position;
        toTarget.y = 0;
        float distance = toTarget.magnitude;

        // 타겟 방향으로 회전
        if (toTarget.sqrMagnitude > 0.001f)
            transform.forward = toTarget.normalized;

        // 플레이어를 추적할 때: 일정 거리까지 접근 후 정지, 사거리 내에서 발사
        if (!goHome && currentTarget == player)
        {
            if (distance > keepDistance)
            {
                float move = moveSpeed * Time.deltaTime;
                transform.position += transform.forward * move;
            }

            bool inRange = distance < fireRange;
            bool ready = Time.time >= lastFireTime + fireInterval;
            if (inRange && ready)
            {
                lastFireTime = Time.time;
                Shoot(player.position + Vector3.up * 1f, "Player");
            }

            return;
        }

        // 집을 추적할 때: 원거리 공격_일정 거리(keepDistance)까지 접근하고 사거리(fireRange) 내이면 발사
        if (currentTarget == home)
        {
            if (distance > keepDistance)
            {
                float move = moveSpeed * Time.deltaTime;
                transform.position += transform.forward * move;
            }

            bool inRangeHome = distance < fireRange;
            bool readyHome = Time.time >= lastFireTime + fireInterval;
            if (inRangeHome && readyHome)
            {
                lastFireTime = Time.time;
                // 홈의 콜라이더 표면을 목표로 발사 (Start에서 가져온 homeCollider 사용)
                Vector3 homeTarget;
                if (homeCollider != null)
                {
                    // 발사 지점(firePoint) 기준으로 콜라이더에서 가장 가까운 지점을 목표로 함
                    homeTarget = homeCollider.ClosestPoint(firePoint.position);
                }
                else if (home != null)
                {
                    homeTarget = home.position + Vector3.up * 1f;
                }
                else
                {
                    homeTarget = closestPoint + Vector3.up * 1f;
                }
                Shoot(homeTarget, "Home");
            }
        }
    }

    void Shoot(Vector3 target, string targetTag)
    {
        Vector3 dir = (target - firePoint.position).normalized;

        // Determine base damage from the bullet prefab's Bullet component (do not use local damage field)
        int baseDamage = 1;
        if (bulletPrefab != null)
        {
            Bullet prefabBullet = bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null) baseDamage = prefabBullet.damage;
        }

        GameObject bullet = Instantiate(bulletPrefab,
            firePoint.position, Quaternion.LookRotation(dir));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * bulletSpeed;
        }

        // Configure instantiated bullet: use prefab baseDamage and double for Home
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.targetTag = targetTag;
            b.damage = (targetTag == "Home") ? baseDamage * 2 : baseDamage;
            b.ownerTag = string.IsNullOrEmpty(gameObject.tag) ? "Enemy" : gameObject.tag;
        }
    }


}
