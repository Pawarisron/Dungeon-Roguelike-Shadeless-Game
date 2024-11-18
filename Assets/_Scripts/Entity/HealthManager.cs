using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{

    public Image healthBar;
    public float maxHealth = 100f;
    public float healthAmount = 100f;
    public bool isDeath;
    public Canvas DeadScene;

    public void TakeDamage(int amoutOfDamage)
    {
        healthAmount -= amoutOfDamage;
        healthBar.fillAmount = healthAmount / maxHealth;

        if (healthAmount <= 0)
        {
            isDeath = true;
        }
    }

    public void Heal(float healingAmount)
    {
        healthAmount += healingAmount;
        healthAmount = Mathf.Clamp(healthAmount, 0, maxHealth);

        healthBar.fillAmount = healthAmount / maxHealth;
    }
}
