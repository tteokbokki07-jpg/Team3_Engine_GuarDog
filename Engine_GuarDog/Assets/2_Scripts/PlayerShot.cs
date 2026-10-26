using System.Xml.Serialization;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerShot : MonoBehaviour
{
    public GameObject bulletPrefab; //총알 프리팹
    public Transform firePoint; //총알 발사 위치
    public bool isClick = false;
    public int damagepoint = 0; //총알 데미지 증가량(확인용)
    public float bulletSpeed = 20f; //총알 속도
    public float fireCooldown = 0.4f; //연사 속도
    public float minFireCooldown = 0.05f; //최소 연사 속도
    public Image CoolDownImage; //쿨다운 UI 이미지
    public bool isMultiShot = false; //멀티샷 여부
    public bool isDoubleShot = false; //더블샷 여부
    public bool isBigShot = false; //크기증가/관통샷 여부

    float lastFireTime = -999f; //마지막 발사 시간

    public void OnMultiShot()
    {
        isMultiShot = true;
    }
    public void OnDoubleShot()
    {
        isDoubleShot = true;
    }
    public void OnBigShot()
    {
        isBigShot = true;
        bulletSpeed *= 1.5f; //총알 속도 증가
    }

    public void StatATKUpg1()
    {
        var bullet = bulletPrefab.GetComponent<Bullet>();
        bullet.moredamage += 3; //총알 데미지 증가
    }
    public void StatATKUpgPlus2()
    {
        var bullet = bulletPrefab.GetComponent<Bullet>();
        bullet.moredamage += 5; //총알 데미지 증가
    }
    public void StatCoolUpg7()
    {
        fireCooldown = Mathf.Max(minFireCooldown, fireCooldown - 0.1f); //공격속도 0.1 감소, 최소값 보정
    }
    public void StatCoolUpgPlus8()
    {
        fireCooldown = Mathf.Max(minFireCooldown, fireCooldown - 0.15f); //공격속도 0.15 감소, 최소값 보정
    }


    void Start()
    {
        if (CoolDownImage != null)
            CoolDownImage.fillAmount = 1f;
        if (bulletPrefab != null)
        {
            var bullet = bulletPrefab.GetComponent<Bullet>();
            if (bullet != null)
                bullet.moredamage = 0; // 초기 추가 데미지 설정
        }
    }

    void Update()
    {
        // 마우스 왼쪽 클릭 상태 반영: 누르면 true, 떼면 false
        if (Input.GetMouseButtonDown(0))
            isClick = true;
        else if (Input.GetMouseButtonUp(0))
            isClick = false;

        // isClick 동안 쿨다운 검사 후 공격 시도
        if (isClick)
        {
            TryAttack();
        }

        if (bulletPrefab != null)
        {
            var bullet = bulletPrefab.GetComponent<Bullet>();
            if (bullet != null)
                damagepoint = bullet.moredamage; //총알 추가 데미지 확인
        }

        // 최소 연사속도 보정: fireCooldown이 minFireCooldown보다 작아지지 않도록 유지
        fireCooldown = Mathf.Max(minFireCooldown, fireCooldown);

        // 쿨다운 진행에 따라 이미지 채우기
        if (CoolDownImage != null)
        {
            if (fireCooldown <= 0f)
            {
                CoolDownImage.fillAmount = 1f;
            }
            else
            {
                float elapsed = Time.time - lastFireTime;
                if (elapsed < 0f) elapsed = 0f;
                float progress = Mathf.Clamp01(elapsed / fireCooldown);
                CoolDownImage.fillAmount = progress;
            }
        }
    }

    // Input System 이벤트에서도 동일 로직 사용
    public void OnAttack(InputValue value)
    {
        // 입력 이벤트로 즉시 한 발 발사(또는 입력 바인딩에 따라 호출)
        TryAttack();
    }

    // 쿨다운 검사 및 발사 처리를 담당 (공통)
    void TryAttack()
    {
        if (Time.timeScale == 0f) return; //게임 일시정지 시 공격 무시
        if (Time.time < lastFireTime + fireCooldown) return; //연사 속도 제한
        lastFireTime = Time.time; //마지막 발사 시간 업데이트

        if (CoolDownImage != null)
            CoolDownImage.fillAmount = 0f;

        FireBullets();

        // 더블샷이면 지연 발사
        if (isDoubleShot)
        {
            StartCoroutine(DoubleShotCoroutine(0.2f));
        }
    }

    // 실제 총알 생성 로직 (쿨다운 검사 없이 호출)
    void FireBullets()
    {
        float[] angles = isMultiShot ? new float[] { -10f, 0f, 10f } : new float[] { 0f };
        foreach (float angle in angles)
        {
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0f, angle, 0f); //firePoint 회전 기준으로 y축 회전
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot); //firePoint 위치 기반 총알 생성
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = rot * Vector3.forward;
                rb.linearVelocity = dir * bulletSpeed; //총알 발사
            }

            // 생성된 총알의 추가 데미지 값을 조정 (인스턴스 수준)
            var bulletComp = bullet.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                int reduction = 0;
                if (isMultiShot) reduction += 1;
                if (isDoubleShot) reduction += 1;
                bulletComp.moredamage = Mathf.Max(0, bulletComp.moredamage - reduction);

                // 관통 여부 전달
                bulletComp.isBigShot = isBigShot;
            }
        }

        SoundManager.Instance.PlaySFX(SoundManager.Instance.swingSound, 0.25f, 1f); //사운드 재생(음량, 높이)
        SoundManager.Instance.PlaySFX(SoundManager.Instance.popSound, 0.05f, 1f); //사운드 재생(음량, 높이)
    }

    IEnumerator DoubleShotCoroutine(float delay) // delay 후에 한 번 더 발사
    {
        yield return new WaitForSeconds(delay); //지연 시간 대기
        if (Time.timeScale == 0f) yield break; //일시정지 상태면 취소

        if (CoolDownImage != null)
            CoolDownImage.fillAmount = 0f;

        FireBullets();
    }

}
