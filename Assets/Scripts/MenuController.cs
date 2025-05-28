using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject splitScreenPanel;
    public GameObject splitScreenCanvas2;
    public Camera cameraP1;
    public Camera cameraP2;
    public GameObject player2;
    public RectTransform Money;
    public RectTransform Speed;
    public RectTransform TopStuff;

    void Start()
    {
        Cursor.visible = false;
        if (PlayerPrefs.GetInt("SplitScreen") == 1)
        {
            PlayInSplitScreen();
        }
        else
        {
            PlayInFullScreen();
        }
    }
    public void PlayInSplitScreen()
    {
        splitScreenCanvas2.SetActive(true);
        cameraP1.rect = new Rect(0, 0.5f, 1, 1);
        splitScreenPanel.SetActive(false);
        Money.transform.localPosition = new Vector3(0, 0, 0);
        Speed.transform.localPosition = new Vector3(0, 0, 0);
        TopStuff.transform.localPosition = new Vector3(0, 0, 0);

    }
    public void PlayInFullScreen()
    {
        splitScreenCanvas2.SetActive(false);
        cameraP1.rect = new Rect(0, 0, 1, 1);
        player2.SetActive(false);
        splitScreenPanel.SetActive(false);
        cameraP2.gameObject.SetActive(false);
        Money.transform.localPosition = new Vector3(0, 356, 0);
        Speed.transform.localPosition = new Vector3(0, -350, 0);
        TopStuff.transform.localPosition = new Vector3(0, 360, 0);
    }
}
