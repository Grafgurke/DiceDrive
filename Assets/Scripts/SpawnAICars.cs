using UnityEngine;

public class SpawnAICars : MonoBehaviour
{
    public GameObject aiCar1;
    public GameObject aiCar2;
    public GameObject aiCar3;
    void Start()
    {
       switch (PlayerPrefs.GetInt("AICars"))
        {
            case 1:
                aiCar1.SetActive(true);
                aiCar2.SetActive(false);
                aiCar3.SetActive(false);
                break;
            case 2:
                aiCar1.SetActive(true);
                aiCar2.SetActive(true);
                aiCar3.SetActive(false);
                break;
            case 3:
                aiCar1.SetActive(true);
                aiCar2.SetActive(true);
                aiCar3.SetActive(true);
                break;
            default:
                aiCar1.SetActive(false);
                aiCar2.SetActive(false);
                aiCar3.SetActive(false);
                break;
        } 
    }
}
