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
    private PlayerInput playerInput;

    private void Awake()
    {
        originEnergyPerSec = energyPerSec;
        playerInput = GetComponent<PlayerInput>();
    }

    public void TakeEnergy(int amountOfDamage)
    {
        energyAmount -= amountOfDamage;
        UpdateHealthBar();

        // Getting hit always resets the growth-rate ramp.
        energyPerSec = originEnergyPerSec;

        //Drained here
        if (energyAmount <= 0 && !isDrained)
        {
            energyAmount = 0;
            UpdateHealthBar();
            StartCoroutine(ResetEnergyAfterDelay());
        }
    }

    public void Energy(float healingAmount)
    {
        if (isDrained) return;
        energyAmount += healingAmount;
        energyAmount = Mathf.Clamp(energyAmount, 0, maxEnergy);
        UpdateHealthBar();
    }

    private void Update()
    {
        if (isDrained) return;

        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            Energy(energyPerSec);
            energyPerSec *= energyGrowthRate;
            timer = 0f;
        }

        // Stop the growth ramp once energy is full again.
        if (energyAmount >= maxEnergy)
        {
            energyPerSec = originEnergyPerSec;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = energyAmount / maxEnergy;
        }
    }

    private IEnumerator ResetEnergyAfterDelay()
    {
        isDrained = true;
        if (playerInput != null) playerInput.DraindedEnergyEffect();
        Debug.Log("drained");

        yield return new WaitForSeconds(delayOfResetEnergy);

        if (playerInput != null) playerInput.UnDraindedEnergyEffect();
        Debug.Log("undrainded");
        isDrained = false;
    }
}