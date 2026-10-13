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

    void Start()
    {
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
        Debug.Log(name + " HP: " + currentHP + " / " + maxHP + "Damage Taken: " + damage);
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
        if (CompareTag("Player") || CompareTag("Home"))
        {
            Debug.Log("Game Over");
            var uiManager = FindObjectOfType<UImanager>();
            uiManager.GameOver();
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
}
