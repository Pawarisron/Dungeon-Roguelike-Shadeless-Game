using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public TMP_Text coinText;
    [SerializeField]
    private int coinAmount;

    public void Start()
    {
        UpdataCoin();
    }
    public void AddCoin(int amount)
    {
        coinAmount += amount;
        UpdataCoin();
    }

    public void RemoveCoin(int amount) {
        coinAmount -= amount;
        UpdataCoin();
    }

    private void UpdataCoin()
    {
        coinText.text = coinAmount.ToString();
    }
}
