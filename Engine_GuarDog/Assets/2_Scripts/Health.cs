using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHP = 3;
    public int currentHP;
    public Slider hpSlider; //ui 표시 슬라이더

    void Start()
    {
        currentHP = maxHP;
        UpdateBar();
    }

    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        Debug.Log("name" + name + " HP: " + currentHP);
        UpdateBar();

        if (currentHP <= 0) Die();
    }

    void UpdateBar()
    {
        if (hpSlider != null)
            hpSlider.value = (float)currentHP / maxHP;
    }

    void Die()
    {
        if (CompareTag("player"))
        {
            Debug.Log("Player Died");
            Time.timeScale = 0f;
        }
        else
        {
            Debug.Log(name + " Died");
            Destroy(gameObject);
        }
    }
}
