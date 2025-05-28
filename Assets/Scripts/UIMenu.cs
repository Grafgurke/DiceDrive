
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject mainMenuPanel;
    public AudioMixer audioMixer;
    public Slider sfxSlider;
    public Slider musicSlider;
    public Slider aiCarsSlider;
    public bool gameScene = false;
    public void PlayInSplitScreen()
    {
        PlayerPrefs.SetInt("SplitScreen", 1);
        SceneManager.LoadScene("MainGame");
    }
    public void PlayInFullScreen()
    {
        PlayerPrefs.SetInt("SplitScreen", 0);
        SceneManager.LoadScene("MainGame");
    }
    public void QuitGame()
    {
        PlayerPrefs.SetInt("SplitScreen", 0);
        Application.Quit();
        Debug.Log("Quit Game");
    }
    public void OnSettingsButtonClicked()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        mainMenuPanel.SetActive(!settingsPanel.activeSelf);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameScene)
            {

                if (settingsPanel.activeSelf)
                {
                    settingsPanel.SetActive(false);
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    settingsPanel.SetActive(true);
                    Time.timeScale = 0f;
                }
                return;
            }
            else
            {
                if (settingsPanel.activeSelf)
                {
                    settingsPanel.SetActive(false);
                    mainMenuPanel.SetActive(true);
                }
                else
                {
                    settingsPanel.SetActive(true);
                    mainMenuPanel.SetActive(false);
                }
            }
        }
    }
    public void SFXSliderChanged()
    {
        float value = sfxSlider.value;
        audioMixer.SetFloat("SFX", value);
        PlayerPrefs.SetFloat("SFX", value);
    }
    public void MusicSliderChanged()
    {
        float value = musicSlider.value;
        audioMixer.SetFloat("Music", value);
        PlayerPrefs.SetFloat("Music", value);
    }
    public void AICarsSliderChanged()
    {
        float value = aiCarsSlider.value;
        int intValue = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt("AICars", intValue);
    }
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

}
