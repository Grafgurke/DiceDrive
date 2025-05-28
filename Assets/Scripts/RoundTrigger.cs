using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class RoundTrigger : MonoBehaviour
{
    public int roundMoney;
    public List<Rigidbody> players;
    public List<Rigidbody> AIPlayers;
    public List<GameObject> countdownObjects;
    public int furthestLap = 0;
    [SerializeField] private AudioSource backgroundMusic;
    public AudioClip secondRoundMusic;
    public AudioClip thirdRoundMusic;
    private bool inLap1 = false;
    private bool inLap2 = false;
    public List<GameObject> powerUps = new List<GameObject>();
    void Start()
    {
        foreach (Rigidbody player in players)
        {
            player.constraints = RigidbodyConstraints.FreezePositionX;
            player.constraints = RigidbodyConstraints.FreezePositionZ; // Freeze position
            foreach (GameObject countdownObject in countdownObjects)
            {
                countdownObject.SetActive(true); // Activate countdown objects
            }
            StartCoroutine(WaitForCountdown(3f)); // Start the countdown
        }
        foreach (Rigidbody AIPlayer in AIPlayers)
        {
            AIPlayer.constraints = RigidbodyConstraints.FreezeAll; // Freeze position for AI players
            StartCoroutine(WaitForCountdown(3f)); // Start the countdown
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("P1") || other.CompareTag("P2"))
        {
            MoneyManager colliderMoneyManager = other.GetComponent<MoneyManager>();
            colliderMoneyManager.AddMoney(roundMoney);
            colliderMoneyManager.UpdateRoundText();
        }
        foreach (GameObject powerUp in powerUps)
        {
            powerUp.SetActive(true); // Activate power-ups
        }
        
    }
    IEnumerator WaitForCountdown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        foreach (Rigidbody player in players)
        {
            player.constraints = RigidbodyConstraints.None; // Unfreeze position
        }
        foreach (Rigidbody AIPlayer in AIPlayers)
        {
            AIPlayer.gameObject.GetComponent<AICarController>().gameStarted = true; // Start AI car
            AIPlayer.constraints = RigidbodyConstraints.None; // Unfreeze position
        }
        foreach (GameObject countdownObject in countdownObjects)
        {
            countdownObject.SetActive(false); // Deactivate countdown objects
        }
    }
    public void UpdateFurthestLap(int lap)
    {
      //  Debug.Log("Lap: " + lap);
        if (lap > furthestLap)
        {
            furthestLap = lap;
        }
        if (lap == 1 && !inLap1)
        {
            inLap1 = true;
            backgroundMusic.clip = secondRoundMusic;
            backgroundMusic.Play();
        }
        else if (lap == 2 && !inLap2)
        {
            inLap2 = true;
            backgroundMusic.clip = thirdRoundMusic;
            backgroundMusic.Play();
        }
    }
}
