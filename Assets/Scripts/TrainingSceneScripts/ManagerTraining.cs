using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManagerTraining : MonoBehaviour
{
    [SerializeField] private GameObject PausePanel;

    [SerializeField] private TextMeshProUGUI camSpeedText;
    [SerializeField] private Dropdown selectInput;
    [SerializeField] private InputField inputFieldPlayerName;

    [SerializeField] [Range(2.0f, 20.0f)] private float currentSensi = 12.0f;

    private bool isPaused = false;

    private void Start()
    {
        LoadValue();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            this.isPaused = !this.isPaused;
        }

        if (this.isPaused)
        {
            Time.timeScale = 0.0f;
            this.PausePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1.0f;
            this.PausePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Resume()
    {
        this.isPaused = !this.isPaused;
    }

    public void ActiveOptionPanel(GameObject optionPanel)
    {
        optionPanel.SetActive(!optionPanel.activeSelf);
    }

    public void LoadLevel(int index)
    {
        SceneManager.LoadScene(index);
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
