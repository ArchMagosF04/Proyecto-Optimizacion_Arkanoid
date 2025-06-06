using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI brickAmountText;
    [SerializeField] private TextMeshProUGUI paddleHitsText;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject endGameButtom;

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
        InitializeGameScreen();
    }

    public void GoToNextScene()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextScene);
    }

    public void GoToScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void ReLoadScene()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(nextScene);
    }


    public void QuitGame()
    {
        Debug.Log("EXIT GAME");
        Application.Quit();
    }

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

    public void WinScreen() 
    {
        if (winScreen == null || endGameButtom == null) return;
        winScreen.SetActive(true);
        endGameButtom.SetActive(true);
    }

    public void LoseScreen()
    {
        if (loseScreen == null || endGameButtom == null) return;
        loseScreen.SetActive(true);
        endGameButtom.SetActive(true);
    }

    private void InitializeGameScreen() 
    {
        if (winScreen == null || loseScreen == null || endGameButtom == null) return;
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        endGameButtom.SetActive(false);
    }
}
