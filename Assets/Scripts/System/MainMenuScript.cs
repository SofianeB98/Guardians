using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private Slider camSpeedslider;
    [SerializeField] private Slider camXSpeedslider;
    [SerializeField] private Slider camYSpeedslider;
    [SerializeField] private Dropdown selectInput;
    [SerializeField] private InputField inputFieldPlayerName;

    private void Awake()
    {
        LoadValue();
    }

    public void LoadLevel(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void ActiveOptionPanel(GameObject optionPanel)
    {
        optionPanel.SetActive(!optionPanel.activeSelf);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OnValueChanged()
    {
        PlayerPrefs.SetString("PlayerName", this.inputFieldPlayerName.text);
        PlayerPrefs.SetFloat("SpeedCam", this.camSpeedslider.value);
        PlayerPrefs.SetFloat("XCam", this.camXSpeedslider.value);
        PlayerPrefs.SetFloat("YCam", this.camYSpeedslider.value);
        PlayerPrefs.SetInt("Input", this.selectInput.value);
        PlayerPrefs.Save();
    }

    private void LoadValue()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            this.inputFieldPlayerName.text = PlayerPrefs.GetString("PlayerName");
        }

        if (PlayerPrefs.HasKey("SpeedCam"))
        {
            this.camSpeedslider.value = PlayerPrefs.GetFloat("SpeedCam");
        }
        if (PlayerPrefs.HasKey("XCam"))
        {
            this.camXSpeedslider.value = PlayerPrefs.GetFloat("XCam");
        }
        if (PlayerPrefs.HasKey("YCam"))
        {
            this.camYSpeedslider.value = PlayerPrefs.GetFloat("YCam");
        }

        if (PlayerPrefs.HasKey("Input"))
        {
            this.selectInput.value = PlayerPrefs.GetInt("Input");
        }
    }
}
