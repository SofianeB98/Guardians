using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI camSpeedText;
    [SerializeField] private Dropdown selectInput;
    [SerializeField] private InputField inputFieldPlayerName;

    [SerializeField] [Range(2.0f,20.0f)] private float currentSensi = 12.0f;

    private void Start()
    {
        LoadValue();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

    public void AddSensiValue()
    {
        if (this.currentSensi < 20)
        {
            currentSensi++;
        }
        else
        {
            this.currentSensi = 20;
        }

        camSpeedText.text = this.currentSensi.ToString();
        PlayerPrefs.SetFloat("SpeedCam", this.currentSensi);
        PlayerPrefs.Save();
    }

    public void RemoveSensiValue()
    {
        if (this.currentSensi > 2)
        {
            currentSensi--;
        }
        else
        {
            this.currentSensi = 2;
        }

        camSpeedText.text = this.currentSensi.ToString();
        PlayerPrefs.SetFloat("SpeedCam", this.currentSensi);
        PlayerPrefs.Save();
    }

    public void OnValueChanged()
    {
        PlayerPrefs.SetString("PlayerName", this.inputFieldPlayerName.text);

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
            float a = PlayerPrefs.GetFloat("SpeedCam");
            currentSensi = a;
        }
        if (PlayerPrefs.HasKey("Input"))
        {
            selectInput.value = PlayerPrefs.GetInt("Input");
        }

        camSpeedText.text = currentSensi.ToString();
    }
}
