using System.Collections;
using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public Health PlayerHealth; // 인스펙터에서 할당 가능. 할당하지 않으면 Start에서 자동으로 Find.
    public Health HomeHealth; // 인스펙터에서 할당 가능. 할당하지 않으면 Start에서 자동으로 Find.
    public bool isMaxHP = true; // Health.isMaxHP 복사본
    [Header("버프세팅")]
    public bool isBestCondition = false;
    private int bestBuffAppliedAmount = 0; // BestCondition 버프 추적, 현재 적용된 best 버프 양(0이면 없음)
    public bool isAdrenaline = false;
    public float AdCool = 20f;
    public float currentAdCool = 0f;
    public float AdHeal = 10f;
    Coroutine adrenalineRoutine;

    public bool canRevive = false; // 사망 후 부활 가능 여부

    // 총알 데미지 버프 관리
    [SerializeField] private bool bulletBuffApplied = false;
    private int bulletBuffAmount = 3;
    private PlayerShot playerShotRef;
    
    public void OnBestCondition()
    {
        isBestCondition = true;
    }
    public void OnAdrenaline()
    {
        isAdrenaline = true;
    }
    public void OnRevieve()
    {
        canRevive = true;
    }


    void Start()
    {
        if (PlayerHealth == null)
            PlayerHealth = FindObjectOfType<Health>();

        playerShotRef = FindObjectOfType<PlayerShot>();

        UpdateIsMaxHP();
    }

    void Update()
    {
        UpdateIsMaxHP();

        // Adrenaline 회복 코루틴 관리 (기존 동작)
        if (isAdrenaline && !isMaxHP)
        {
            if (adrenalineRoutine == null)
                adrenalineRoutine = StartCoroutine(AdrenalineCoroutine());
        }
        else if (isMaxHP)
        {
            if (adrenalineRoutine != null)
            {
                StopCoroutine(adrenalineRoutine);
                adrenalineRoutine = null;
                currentAdCool = 0f;
                AdBuffOff(bulletBuffAmount);
                bulletBuffApplied = false;
            }
        }

        // BestCondition 기반 총알 데미지 버프 관리
        UpdateBestBuff();
    }

    void UpdateIsMaxHP()
    {
        if (PlayerHealth == null) return;
        isMaxHP = PlayerHealth.isMaxHP;
    }

    public void StatHPMax0()
    {
        PlayerHealth.Heal(999); // 현재 체력을 최대 체력으로 회복
    }
    public void StatHPUpg3()
    {
        PlayerHealth.maxHP += 30; // 최대 체력 증가
        PlayerHealth.Heal(80); // 증가한 만큼 + 추가 회복
    }
    public void StatHPUpgPlus4()
    {
        PlayerHealth.maxHP += 50; // 최대 체력 증가
        PlayerHealth.Heal(140); // 증가한 만큼 + 추가 회복
    }
    public void StatHomeHPHeal5()
    {
        HomeHealth.Heal(50); // 회복
    }
    public void StatHomeHPUpg6()
    {
        HomeHealth.maxHP += 100; // 최대 체력 증가
        HomeHealth.Heal(100); // 증가한 만큼 회복
    }
    public void OnAgainUpgrade()
    {
        var prefabBullet = playerShotRef.bulletPrefab.GetComponent<Bullet>();
        prefabBullet.damage += 3; //총알 데미지 증가
        PlayerHealth.maxHP += 25; // 최대 체력 증가
        PlayerHealth.Heal(999); // 체력 회복
    }

    IEnumerator AdrenalineCoroutine()
    {
        AdBuffOn(bulletBuffAmount);
        bulletBuffApplied = true;
        // 1초 간격으로 currentAdCool을 1씩 증가시키고 AdCool(예:20)에 도달하면 회복 적용
        currentAdCool = 0f;
        while (isAdrenaline && !isMaxHP)
        {
            yield return new WaitForSeconds(1f);
            currentAdCool += 1f;

            if (currentAdCool >= AdCool)
            {
                currentAdCool = 0f;

                if (PlayerHealth != null)
                {
                    int healAmount = Mathf.RoundToInt(AdHeal);
                    PlayerHealth.Heal(healAmount);
                }
            }

            if (PlayerHealth == null) break;
        }

        adrenalineRoutine = null;
    }

    // 씬에 존재하는 Bullet들 및 prefab의 Bullet(있다면)에 데미지 증가 적용
    void AdBuffOn(int amount)
    {
        var bullets = FindObjectsOfType<Bullet>();
        foreach (var b in bullets)
        {
            b.damage += amount;
        }

        if (playerShotRef != null && playerShotRef.bulletPrefab != null)
        {
            var prefabBullet = playerShotRef.bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null)
            {
                prefabBullet.damage += amount;
            }
        }
    }

    // 적용된 버프를 제거(되돌리기)
    void AdBuffOff(int amount)
    {
        var bullets = FindObjectsOfType<Bullet>();
        foreach (var b in bullets)
        {
            b.damage = Mathf.Max(0, b.damage - amount);
        }

        if (playerShotRef != null && playerShotRef.bulletPrefab != null)
        {
            var prefabBullet = playerShotRef.bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null)
            {
                prefabBullet.damage = Mathf.Max(0, prefabBullet.damage - amount);
            }
        }
    }

    // BestCondition 버프 적용/갱신
    void UpdateBestBuff()
    {
        if (PlayerHealth == null) return;

        // 새로 적용해야 할 버프량 결정:
        // isBestCondition == false -> desired = 0 (버프 없음)
        // isBestCondition == true && isMaxHP == true -> desired = 5
        // isBestCondition == true && isMaxHP == false -> desired = 2
        int desired = 0;
        if (isBestCondition)
        {
            desired = PlayerHealth.isMaxHP ? 8 : 1;
        }

        if (bestBuffAppliedAmount != desired)
        {
            // 기존 적용량 되돌리기
            if (bestBuffAppliedAmount != 0)
            {
                BestBuffOff(bestBuffAppliedAmount);
                bestBuffAppliedAmount = 0;
            }

            // 새로 적용
            if (desired != 0)
            {
                BestBuffOn(desired);
                bestBuffAppliedAmount = desired;
            }
        }
    }

    void BestBuffOn(int amount)
    {
        var bullets = FindObjectsOfType<Bullet>();
        foreach (var b in bullets)
        {
            b.damage += amount;
        }

        if (playerShotRef != null && playerShotRef.bulletPrefab != null)
        {
            var prefabBullet = playerShotRef.bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null)
            {
                prefabBullet.damage += amount;
            }
        }
    }

    void BestBuffOff(int amount)
    {
        var bullets = FindObjectsOfType<Bullet>();
        foreach (var b in bullets)
        {
            b.damage = Mathf.Max(0, b.damage - amount);
        }

        if (playerShotRef != null && playerShotRef.bulletPrefab != null)
        {
            var prefabBullet = playerShotRef.bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null)
            {
                prefabBullet.damage = Mathf.Max(0, prefabBullet.damage - amount);
            }
        }
    }

    void OnDestroy()
    {
        // 오브젝트 제거 시 적용된 버프 복구
        if (bulletBuffApplied) AdBuffOff(bulletBuffAmount);
        if (bestBuffAppliedAmount != 0) BestBuffOff(bestBuffAppliedAmount);
    }
}
