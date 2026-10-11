using System.Xml.Serialization;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShot : MonoBehaviour
{
    public GameObject bulletPrefab; //총알 프리팹
    public Transform firePoint; //총알 발사 위치
    public float bulletSpeed = 20f; //총알 속도
    public float fireCooldown = 0.4f; //연사 속도
    public bool isMultiShot = false; //멀티샷 여부
    public bool isDoubleShot = false; //더블샷 여부
    public bool isBigShot = false; //크기증가/관통샷 여부

    float lastFireTime = -999f; //마지막 발사 시간


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnAttack(InputValue value) //유니티 인풋 시스템의 공격 입력 이벤트 처리
    {
        if (Time.timeScale == 0f) return; //게임 일시정지 시 공격 무시
        if (Time.time < lastFireTime + fireCooldown) return; //연사 속도 제한
        lastFireTime = Time.time; //마지막 발사 시간 업데이트
        // 실제 발사 처리
        FireBullets();

        // isDoubleShot이면 0.2초 뒤에 한 번 더 자동 발사
        if (isDoubleShot)
        {
            StartCoroutine(DoubleShotCoroutine(0.2f));
        }
    }

    // 실제 총알 생성 로직 (쿨다운 검사 없이 호출)
    void FireBullets()
    {
        float[] angles = isMultiShot ? new float[] { -15f, 0f, 15f } : new float[] { 0f };
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

            // 생성된 총알의 damage 값을 조정 (인스턴스 수준)
            var bulletComp = bullet.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                int reduction = 0;
                if (isMultiShot) reduction += 1;
                if (isDoubleShot) reduction += 1;
                bulletComp.damage = Mathf.Max(0, bulletComp.damage - reduction);

                // 관통 여부 전달
                bulletComp.isBigShot = isBigShot;
            }
        }
    }

    IEnumerator DoubleShotCoroutine(float delay) // delay 후에 한 번 더 발사
    {
        yield return new WaitForSeconds(delay); //지연 시간 대기
        if (Time.timeScale == 0f) yield break; //일시정지 상태면 취소
        // 두 번째 샷은 쿨타임 검사 없이 즉시 발사
        lastFireTime = Time.time; //마지막 발사 시간 업데이트
        FireBullets();
    }

}
