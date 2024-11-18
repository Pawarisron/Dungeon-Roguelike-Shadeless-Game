using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image healthBar;
    public float maxEnergy = 100f;
    public float energyAmount = 100f;
    public float energyPerSec = 6f;
    public float energyGrowthRate = 1.1f;
    public int delayOfResetEnergy = 3;

    private float timer = 0f;
    private bool isDrained = false;
    private float originEnergyPerSec = 6f;
    private bool triggerTakeEnergy = false;

    private void Awake()
    {
        originEnergyPerSec = energyPerSec;
    }
    public void TakeEnergy(int amountOfDamage)
    {
        triggerTakeEnergy = true;
        energyAmount -= amountOfDamage;
        healthBar.fillAmount = energyAmount / maxEnergy;

        //Drained here
        if (energyAmount <= 0 && !isDrained)
        {
            StartCoroutine(ResetEnergyAfterDelay());
            energyAmount = 0;
            
        }
        triggerTakeEnergy=false;
    }

    public void Energy(float healingAmount)
    {
        if (isDrained) return;  

        energyAmount += healingAmount;
        energyAmount = Mathf.Clamp(energyAmount, 0, maxEnergy);
        healthBar.fillAmount = energyAmount / maxEnergy;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1 && !isDrained)
        {
            Energy(energyPerSec);
            energyPerSec *= energyGrowthRate;
            timer = 0;
        }
        //stop growthRate
        if(energyAmount == maxEnergy || triggerTakeEnergy || isDrained)
        {
            energyPerSec = originEnergyPerSec;
        }
    }

    private IEnumerator ResetEnergyAfterDelay()
    {
        isDrained = true;
        GetComponent<PlayerInput>().DraindedEnergyEffect();
        Debug.Log("drained");
        yield return new WaitForSeconds(delayOfResetEnergy);
        GetComponent<PlayerInput>().UnDraindedEnergyEffect();
        Debug.Log("undrainded");
        isDrained = false;                  
    }
}

