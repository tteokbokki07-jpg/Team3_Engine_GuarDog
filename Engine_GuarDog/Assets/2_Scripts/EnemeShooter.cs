using System.Collections;
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
    [Header("변종관리")]
    public bool isBigType = false; // 큰 적 여부
    public bool EnemyisMultiShot = false; // 멀티샷 여부
    public bool EnemyisDoubleShot = false; // 더블샷 여부

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
                // 첫 발사에서는 현재 시점의 플레이어 위치를 전달
                Vector3 currentPlayerTarget = player.position + Vector3.up * 1f;
                FireBurst(currentPlayerTarget, "Player", false);
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
                Vector3 homeTarget;
                if (homeCollider != null)
                {
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
                FireBurst(homeTarget, "Home", false);
            }
        }
    }

    // 여러 탄환을 한 번에 발사 (멀티샷 / 더블샷 지원)
    void FireBurst(Vector3 target, string targetTag, bool isSecondary)
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.swingSound, 0.35f, 1f); //사운드 재생(음량, 높이)
        SoundManager.Instance.PlaySFX(SoundManager.Instance.enemyshootSound, 0.35f, 1f); //사운드 재생(음량, 높이)

        Vector3 baseDir = (target - firePoint.position).normalized;
        // 멀티샷이면 -15,0,15도 각도로 발사, 아니면 정면 0도만
        float[] angles = EnemyisMultiShot ? new float[] { -15f, 0f, 15f } : new float[] { 0f };

        // Determine base damage from the bullet prefab's Bullet component
        int baseDamage = 1;
        if (bulletPrefab != null)
        {
            Bullet prefabBullet = bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null) baseDamage = prefabBullet.damage;
        }

        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.LookRotation(baseDir) * Quaternion.Euler(0f, angle, 0f);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = rot * Vector3.forward * bulletSpeed;
            }

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.targetTag = targetTag;
                b.damage = (targetTag == "Home") ? baseDamage /2 : baseDamage;
                b.ownerTag = string.IsNullOrEmpty(gameObject.tag) ? "Enemy" : gameObject.tag;
                // 멀티샷/더블샷 변종에 따라 추가 설정(예: 큰 총알) 처리 가능
                if (isBigType)
                {
                    b.isBigShot = true;
                }
            }
        }

        // 더블샷이면 0.2초 뒤에 다시 발사하되, 두번째 발사는 '현재 시점'의 타겟 위치를 사용
        if (EnemyisDoubleShot && !isSecondary)
        {
            StartCoroutine(DoubleBurstCoroutine(0.5f, targetTag));
        }
    }

    IEnumerator DoubleBurstCoroutine(float delay, string targetTag)
    {
        yield return new WaitForSeconds(delay);
        if (Time.timeScale == 0f) yield break;

        // 두번째 발사는 발사 시점(지금)의 타겟 위치를 재계산해서 사용
        Vector3 secondTarget;
        if (targetTag == "Player")
        {
            if (player != null)
                secondTarget = player.position + Vector3.up * 1f;
            else
                secondTarget = firePoint.position + transform.forward; // fallback
        }
        else // "Home"
        {
            if (homeCollider != null)
                secondTarget = homeCollider.ClosestPoint(firePoint.position);
            else if (home != null)
                secondTarget = home.position + Vector3.up * 1f;
            else
                secondTarget = firePoint.position + transform.forward;
        }

        // isSecondary=true로 호출하여 추가 재스케줄 방지
        FireBurst(secondTarget, targetTag, true);
    }
}
