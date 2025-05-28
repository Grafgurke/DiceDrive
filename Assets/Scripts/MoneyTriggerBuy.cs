using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class MoneyTriggerBuy : MonoBehaviour
{
    public int buyMoney;
    private bool isBought = false;
    public MoneyTrigger moneyTrigger;
    public GameObject curveShortCutBlock;
    public bool trainShortcut = false;
    public GameObject moneyVisual;
    public GameObject roadBlock;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AICar"))
        {
            return;
        }
        MoneyManager colliderMoneyManager = other.GetComponent<MoneyManager>();
        if (!isBought && colliderMoneyManager.currentMoney >= buyMoney)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
            colliderMoneyManager.SubtractMoney(buyMoney);
            isBought = true;
            moneyTrigger.isBought = true;
            moneyTrigger.ownerMoneyManager = colliderMoneyManager;
            curveShortCutBlock.SetActive(true);
            bool isPlayer1 = false;
            if (other.gameObject.CompareTag("P1"))
            {
                isPlayer1 = true;
                curveShortCutBlock.layer = 10;
                foreach (Transform child in curveShortCutBlock.transform)
                {
                    child.gameObject.layer = 10;
                }
            }
            else if (other.gameObject.CompareTag("P2"))
            {
                isPlayer1 = false;
                curveShortCutBlock.layer = 9;
                foreach (Transform child in curveShortCutBlock.transform)
                {
                    child.gameObject.layer = 9;
                }
            }
            if (trainShortcut)
            {
                StartCoroutine(waitForDeactivate());
            }
            else
            {
                roadBlock.GetComponent<PlayableDirector>().Play();
                StartCoroutine(WaitForDeactivateRoadBlock(isPlayer1));
            }

            moneyVisual.SetActive(false);

        }
    }
    IEnumerator WaitForDeactivateRoadBlock(bool player1)
    {
        if (player1)
        {
            roadBlock.GetComponent<BoxCollider>().excludeLayers = LayerMask.GetMask("P1Layer");
            roadBlock.layer = 9;
            foreach (Transform child in roadBlock.transform)
            {
                child.gameObject.layer = 9;
            }

        }
        else
        {
            roadBlock.GetComponent<BoxCollider>().excludeLayers = LayerMask.GetMask("P2Layer");
            roadBlock.layer = 10;
            foreach (Transform child in roadBlock.transform)
            {
                child.gameObject.layer = 10;
            }
        }
        yield return new WaitForSeconds(2f);
        roadBlock.SetActive(false);
    }
    IEnumerator waitForDeactivate()
    {
        yield return new WaitForSeconds(16.5f);
        curveShortCutBlock.SetActive(false);
        isBought = false;
        moneyVisual.SetActive(true);
    }
}

