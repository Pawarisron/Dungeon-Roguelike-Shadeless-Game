using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{

    public Image healthBar;
    public float maxHealth = 100f;
    public float healthAmount = 100f;
    public bool isDeath;
    public Canvas DeadScene;
    
    public UnityEvent OnHit;

    public void TakeDamage(int amoutOfDamage)
    {
        //Debug.Log("Hit");
        healthAmount -= amoutOfDamage;
        healthBar.fillAmount = healthAmount / maxHealth;
        OnHit?.Invoke();
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
