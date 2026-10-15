using UnityEngine;

public class StatManager : MonoBehaviour
{
    // 최소 쿨다운(너무 빨라지는 것을 방지)
    private const float MinFireCooldown = 0.05f;

    // NOTE:
    // "amount" 등 더할 값들은 이 메서드를 호출하는 쪽에서 전달합니다.
    // 예:
    //  - UpgradeManager에서 버튼 클릭 시: statManager.UpgradeBulletDamage(1);
    //  - UI 버튼의 OnClick()에 직접 연결(인스펙터)하여 인자 지정 가능(또는 래퍼 함수 사용).
    //  - 코드 예: FindObjectOfType<StatManager>().UpgradePlayerMaxHP(2);
    //
    // 필요한 경우 이 스크립트에 public int defaultXXXAmount 필드를 추가해 인스펙터에서 설정하도록 변경할 수 있습니다.

    // 공격력(총알 데미지)을 올립니다.
    // amount: 더할 정수값 (예: +1). 호출하는 쪽에서 값을 정하세요.
    public void UpgradeBulletDamage(int amount)
    {
        if (amount == 0) return;

        // 이미 생성된 총알들에 적용
        var bullets = FindObjectsOfType<Bullet>();
        foreach (var b in bullets)
        {
            b.damage += amount;
        }

        // PlayerShot의 prefab에 붙은 Bullet 컴포넌트에도 적용 (향후 생성될 총알)
        var playerShot = FindObjectOfType<PlayerShot>();
        if (playerShot != null && playerShot.bulletPrefab != null)
        {
            var prefabBullet = playerShot.bulletPrefab.GetComponent<Bullet>();
            if (prefabBullet != null)
            {
                prefabBullet.damage += amount;
            }
        }

        Debug.Log($"[StatManager] Bullet damage +{amount} 적용 (scene bullets: {bullets.Length}).");
    }

    // 플레이어 최대 체력을 올립니다.
    // amount: 더할 정수값 (예: +1). 호출하는 쪽에서 값을 정하세요.
    public void UpgradePlayerMaxHP(int amount)
    {
        if (amount == 0) return;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("[StatManager] Player 오브젝트를 찾을 수 없습니다 (태그 'Player' 확인).");
            return;
        }

        var hp = playerObj.GetComponent<Health>();
        if (hp == null)
        {
            Debug.LogWarning("[StatManager] Player에 Health 컴포넌트가 없습니다.");
            return;
        }

        hp.maxHP += amount;
        // 현재 체력을 함께 증가시켜 플레이어가 즉시 혜택을 받도록 함
        hp.currentHP = Mathf.Min(hp.currentHP + amount, hp.maxHP);
        Debug.Log($"[StatManager] Player maxHP +{amount} 적용. New maxHP: {hp.maxHP}, currentHP: {hp.currentHP}");
    }

    // 집(Home) 최대 체력을 올립니다.
    // amount: 더할 정수값 (예: +1). 호출하는 쪽에서 값을 정하세요.
    public void UpgradeHomeMaxHP(int amount)
    {
        if (amount == 0) return;

        var homeObj = GameObject.FindWithTag("Home");
        if (homeObj == null)
        {
            Debug.LogWarning("[StatManager] Home 오브젝트를 찾을 수 없습니다 (태그 'Home' 확인).");
            return;
        }

        var hp = homeObj.GetComponent<Health>();
        if (hp == null)
        {
            Debug.LogWarning("[StatManager] Home에 Health 컴포넌트가 없습니다.");
            return;
        }

        hp.maxHP += amount;
        // 집도 현재 체력을 함께 증가시킴
        hp.currentHP = Mathf.Min(hp.currentHP + amount, hp.maxHP);
        Debug.Log($"[StatManager] Home maxHP +{amount} 적용. New maxHP: {hp.maxHP}, currentHP: {hp.currentHP}");
    }

    // 공격속도(쿨타임)를 개선합니다.
    // delta: fireCooldown에서 뺄 값 (예: 0.05f를 주면 쿨타임이 짧아짐 => 공격속도 증가)
    // 호출하는 쪽에서 delta 값을 정하세요.
    public void UpgradeFireCooldown(float delta)
    {
        if (Mathf.Approximately(delta, 0f)) return;

        var playerShot = FindObjectOfType<PlayerShot>();
        if (playerShot == null)
        {
            Debug.LogWarning("[StatManager] PlayerShot을 찾을 수 없습니다.");
            return;
        }

        float old = playerShot.fireCooldown;
        playerShot.fireCooldown = Mathf.Max(MinFireCooldown, playerShot.fireCooldown - delta);
        Debug.Log($"[StatManager] fireCooldown {old} -> {playerShot.fireCooldown} (감소: {delta}).");
    }
}
