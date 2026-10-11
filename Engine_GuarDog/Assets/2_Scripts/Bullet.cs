using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 2f; //총알이 사라질 시간
    public string targetTag = "Enemy"; //맞을 대상 태그
    public string ownerTag = "Player"; //발사자 태그(본인 통과)
    public bool isBigShot = false; // 관통/큰 총알 여부

    void Start()
    {
        Destroy(gameObject, lifeTime); //lifeTime 후에 총알 삭제
    }

    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return; //트리거 충돌 시 무시
        if (other.CompareTag(ownerTag)) return; //발사자와 충돌 시 무시

        if (other.CompareTag(targetTag)) //맞은 대상이 targetTag일 때
        {
            Health hp = other.GetComponent<Health>(); //맞은 대상의 Health 스크립트 가져오기
            if (hp != null) hp.TakeDamage(damage); //맞은 대상의 체력 감소
        }
        
        // isBigShot이면 관통(파괴하지 않음), 아니면 충돌 시 파괴
        if (!isBigShot)
        {
            Destroy(gameObject); //총알 삭제
        }
    }

}
