using UnityEngine;

public class MoneyTrigger : MonoBehaviour
{
    public int payMoney;
    public bool isBought = false;
    public MoneyManager ownerMoneyManager;
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("AICar") || other.CompareTag("Untagged"))
        {
            return;
        }
        MoneyManager colliderMoneyManager = other.GetComponent<MoneyManager>();
        if (isBought && colliderMoneyManager != ownerMoneyManager)
        {
            ownerMoneyManager.AddMoney(payMoney);
            colliderMoneyManager.SubtractMoney(payMoney);
        }
    }
}
