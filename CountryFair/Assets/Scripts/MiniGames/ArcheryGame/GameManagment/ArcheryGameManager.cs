using System;
using TMPro;
using UnityEngine;

public class ArcheryGameManager : GameManager
{   

    [SerializeField]
    private TextMeshProUGUI balloonColorToScoreText;

    [SerializeField]
    private GameObject balloonArcherySpawnerObject;

    protected override void Awake()
    {   
        base.Awake();

        if (balloonColorToScoreText == null)
        {
            Debug.LogError("Balloon Color To Score Text is not assigned in ArcheryGameManager.");

            return;
        }

        SetBalloonColorToScore();
    }


    private void SetBalloonColorToScore()
    {   
        int randomColorIndex = Utils.RandomValueInRange(0, Enum.GetValues(typeof(BalloonArcheryGame.Colors)).Length);

        string colorToScore = ((BalloonArcheryGame.Colors)randomColorIndex).ToString().ToLower();

        DisplayBalloonColorToScore(colorToScore);
        
        PlayerPrefs.SetString("BalloonColorToScore", colorToScore); 
    }

    private void DisplayBalloonColorToScore(string colorToScore)
    {
        switch (colorToScore)
        {
            case "red":
                balloonColorToScoreText.text = "Cor para marcar pontos: Vermelho";
                return;
            case "blue":
                balloonColorToScoreText.text = "Cor para marcar pontos: Azul";
                return;
            case "yellow":
                balloonColorToScoreText.text = "Cor para marcar pontos: Amarelo";
                return;
            default:
                Debug.LogError("Invalid balloon color to score: " + colorToScore);
                return;
        }
        
    }
}
