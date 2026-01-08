using UnityEngine;

public class GameManager : MonoBehaviour
{   
    [SerializeField]
    private int sessionScoreGoal = 3;

    public void SessionScoreGoal(int currentScore)
    {
        if (currentScore == sessionScoreGoal)
        {   
            // Pauses the game
            Time.timeScale = 0f;
        }
    }
}
