using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    public int currentHP;
    public Slider hpSlider; //ui 표시 슬라이더
    public TextMeshProUGUI HPText;
    public bool isMaxHP = true;
    public GameManager gameManager;
    public HealthItem healthItem; // HealthItem 참조

    void Start()
    {
        // 인스펙터에 할당되지 않았을 때 자동으로 찾기
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (healthItem == null)
            healthItem = FindObjectOfType<HealthItem>();

        currentHP = maxHP;
        UpdateBar();
        UpdateHPText();
    }

    void Update()
    {
        currentHP = Mathf.Min(currentHP, maxHP);
        if (currentHP == maxHP)
        {
            isMaxHP = true;
        }
        else
        {
            isMaxHP = false;
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        Debug.Log(name + " HP: " + currentHP + " / " + maxHP + " 받은피해: " + damage);
        UpdateBar();
        UpdateHPText();



        if (currentHP <= 0) Die();
    }

    // 회복을 위한 공개 메서드 추가
    public void Heal(int amount)
    {
        if (currentHP <= 0) return; // 이미 사망한 경우 회복 적용하지 않음
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        UpdateBar();
        UpdateHPText();
    }

    void UpdateBar()
    {
        if (hpSlider != null)
            hpSlider.value = (float)currentHP / maxHP;
    }
    void UpdateHPText()
    {
        if (HPText != null)
            HPText.text = currentHP.ToString() + " / " + maxHP.ToString();
    }

    void Die()
    {
        if (CompareTag("Player"))
        {
            if (healthItem.canRevive == false)
            {
                Debug.Log("Game Over");
                gameManager.CallGameOverText();

            }
            else if (healthItem.canRevive == true)
            {
                Debug.Log("Revived");
                Revieve();
                healthItem.canRevive = false;
            }
        }
        else if (CompareTag("Home"))
        {
            Debug.Log("Game Over");
            gameManager.CallGameOverText();
        }
        else
        {
            Debug.Log(name + " Died");
            Destroy(gameObject);
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            gameManager.KillCount++;
        }
    }

    public void Revieve()
    {
        //효과 추가하면 조음
        currentHP = 50;
        UpdateBar();
        UpdateHPText();
        Debug.Log(name + " Revived");
    }
}
