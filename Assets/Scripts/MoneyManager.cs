using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public int startMoney = 0;
    public int currentMoney = 0;
    public int maxMoney = 10000;
    public int minMoney = 0;
    public TMPro.TextMeshProUGUI roundText;
    private int roundCount = 0;
    private int roundLimit = 2;
    
    public TMPro.TextMeshProUGUI moneyText;
    public List<GameObject> moneyVisuals;
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI positionText;
    
    private float timer = 0f;
    private bool roundActive = false;
    private int currentPosition = 0;

    private RoundTrigger roundTrigger;
    public GameObject winText;
    public GameObject loseText;



    public void Start()
    {
        roundTrigger = FindFirstObjectByType<RoundTrigger>();
        currentMoney = startMoney;
        roundText.text = roundCount.ToString() + "/" + roundLimit.ToString();
        UpdateMoneyText();
        timer = 0f;
        //roundActive = true;
        timerText.text = "00:00.00";
        StartCoroutine(waitForSeconds(3f));
        // Make sure we have a PlayerRacerController
        PlayerRacerController playerRacer = GetComponent<PlayerRacerController>();
        if (playerRacer == null)
        {
            playerRacer = gameObject.AddComponent<PlayerRacerController>();
            playerRacer.racerName = "Player"; // Default name
        }
    }
    IEnumerator waitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        roundActive = true;
    }
    
    public void Update()
    {
        if (roundActive)
        {
            timer += Time.deltaTime;
            int minutes = (int)(timer / 60);
            int seconds = (int)(timer % 60);
            int hundredths = (int)((timer * 100) % 100);
            timerText.text = $"{minutes:00}:{seconds:00}.{hundredths:00}";
        }
    }
    
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (currentMoney > maxMoney)
        {
            currentMoney = maxMoney;
        }
        UpdateMoneyText();
    }
    
    public void SubtractMoney(int amount)
    {
        currentMoney -= amount;
        if (currentMoney < minMoney)
        {
            currentMoney = minMoney;
        }
        UpdateMoneyText();
    }

    public void UpdateMoneyText()
    {
        moneyText.text = currentMoney.ToString();
        foreach (GameObject moneyVisual in moneyVisuals)
        {
            moneyVisual.SetActive(false);
        }
        if (currentMoney > 0)
        {
            moneyVisuals[0].SetActive(true);
        }
        if (currentMoney > 300)
        {
            moneyVisuals[1].SetActive(true);
        }
        if (currentMoney > 500)
        {
            moneyVisuals[2].SetActive(true);
        }
        if (currentMoney > 700)
        {
            moneyVisuals[3].SetActive(true);
        }
        if (currentMoney > 900)
        {
            moneyVisuals[4].SetActive(true);
        }
    }
    
    public void UpdateRoundText()
    {
        roundCount++;
        if (roundCount > roundLimit)
        {
            roundCount = roundLimit; // Ensure we don't exceed the limit
            Debug.Log("Round limit reached, no further rounds allowed." + " Current round: " + roundCount + "furthestLap: " + roundTrigger.furthestLap);
            if (roundTrigger.furthestLap >= roundCount)
            {
                winText.SetActive(true);
                CarController carController = GetComponent<CarController>();
                if (carController != null)
                {
                    carController.enabled = false; // Disable car control
                }
            }
            else
            {
                loseText.SetActive(true);
                CarController carController = GetComponent<CarController>();
                if (carController != null)
                {
                    carController.enabled = false; // Disable car control
                }
            }
        }
        roundText.text = roundCount.ToString() + "/" + roundLimit.ToString();
        
        // Only reset timer if we haven't reached the limit
 
        timer = 0f;
        timerText.text = "00:00.00";
        
        
        roundActive = roundCount <= roundLimit;
        if (roundCount > roundLimit)
        {
            Debug.Log("Round limit reached!");
        }
    }

    
    // Set player position and update the UI
    public void SetPosition(int position)
    {
        currentPosition = position;
        UpdatePositionText();
    }
    
    // Update position text with proper suffix (1st, 2nd, 3rd, etc.)
    public void UpdatePositionText()
    {
        string suffix;
        
        if (currentPosition % 100 >= 11 && currentPosition % 100 <= 13)
        {
            // Special case for 11th, 12th, 13th
            suffix = "th";
        }
        else
        {
            // Normal cases
            switch (currentPosition % 10)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }
        }
        
        positionText.text = currentPosition + suffix;
    }
}