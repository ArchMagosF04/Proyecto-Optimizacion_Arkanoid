using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Toggle godModeToggle;

    [Header("UI Text")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI brickAmountText;
    [SerializeField] private TextMeshProUGUI paddleHitsText;

    [Header("UI Screens")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject levelSelectScreen;

    [Header("End Game Buttons")]
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject mainMenuButton;

    [Header("Sound Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;

    [SerializeField] private AudioMixer audioMixer;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if(levelSelectScreen != null) levelSelectScreen.SetActive(false);
        ToggleSettingsScreen(false);
        InitializeGameScreen();
        SetMasterVolume(PlayerPrefs.GetFloat("SavedMasterVolume", 100));
        SetMusicVolume(PlayerPrefs.GetFloat("SavedMusicVolume", 100));
        SetSoundVolume(PlayerPrefs.GetFloat("SavedSoundVolume", 100));

        if (godModeToggle != null)
        {
            if (PlayerPrefs.GetInt("GodMode", 0) == 1) godModeToggle.isOn = true;
            else godModeToggle.isOn = false;
        }

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        soundSlider.onValueChanged.AddListener(SetSoundVolume);
    }

    #region SceneManagement
    public void GoToNextScene()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextScene);
    }

    public void GoToScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void ReloadScene()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(nextScene);
    }
    public void QuitGame()
    {
        Debug.Log("EXIT GAME");
        Application.Quit();
    }

    #endregion

    #region Game HUD
    public void UpdateLives(int amount)
    {
        if (livesText == null) return;
        livesText.text = $"Lives: {amount}";
    }

    public void UpdateBrickAmount(int amount)
    {
        if (brickAmountText == null) return;
        brickAmountText.text = $"Bricks Left: {amount}";
    }

    public void UpdatePaddleHits(int amount)
    {
        if (paddleHitsText == null) return;
        paddleHitsText.text = $"Paddle Hits: {amount}";
    }

    #endregion
    public void WinScreen() 
    {
        if (winScreen == null) return;
        winScreen.SetActive(true);
        continueButton.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }

    public void LoseScreen()
    {
        if (loseScreen == null) return;
        loseScreen.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);
        mainMenuButton.SetActive(true);
    }

    private void InitializeGameScreen() 
    {
        if (loadingScreen != null)
        {
            ToggleLoadingScreen(true);
        }

        if (winScreen == null || loseScreen == null) return;
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        continueButton.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        mainMenuButton.SetActive(false);
    }

    public void SetGodModeToggle(bool toggle)
    {
        if(toggle)
        {
            PlayerPrefs.SetInt("GodMode", 1);
        }
        else PlayerPrefs.SetInt("GodMode", 0);

        if (UpdateManager.Instance != null) UpdateManager.Instance.SetGodModeStatus();
    }

    public void ToggleLoadingScreen(bool toggle)
    {
        loadingScreen.SetActive(toggle);
    }

    public void ToggleSettingsScreen(bool toggle)
    {
        settingsScreen.SetActive(toggle);
    }

    public void ToggleLevelSelectScreen(bool toggle)
    {
        levelSelectScreen.SetActive(toggle);
    }

    #region SoundSliders 
    public void SetMasterVolume(float volume)
    {
        if (volume < 1)
        {
            volume = 0.001f;
        }

        RefreshSlider(volume, masterSlider);

        PlayerPrefs.SetFloat("SavedMasterVolume", volume);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume / 100) * 20);
    }

    public void SetVolumeFromMasterSlider()
    {
        SetMasterVolume(masterSlider.value);
    }

    public void SetMusicVolume(float volume)
    {
        if (volume < 1)
        {
            volume = 0.001f;
        }

        RefreshSlider(volume, musicSlider);

        PlayerPrefs.SetFloat("SavedMusicVolume", volume);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume / 100) * 20);
    }

    public void SetVolumeFromMusicSlider()
    {
        SetMusicVolume(musicSlider.value);
    }

    public void SetSoundVolume(float volume)
    {
        if (volume < 1)
        {
            volume = 0.001f;
        }

        RefreshSlider(volume, soundSlider);

        PlayerPrefs.SetFloat("SavedSoundVolume", volume);
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(volume / 100) * 20);
    }

    public void SetVolumeFromSoundSlider()
    {
        SetSoundVolume(soundSlider.value);
    }

    public void RefreshSlider(float volume, Slider slider)
    {
        slider.value = volume;
    }
    #endregion
}
