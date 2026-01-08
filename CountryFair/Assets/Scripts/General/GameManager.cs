using UnityEngine;

public class GameManager : MonoBehaviour
{   
    [SerializeField]
    protected int sessionScoreGoal = 3;

    public void SessionScoreGoal(int currentScore)
    {  
        Debug.Log("Current Score: " + currentScore + " / Session Score Goal: " + sessionScoreGoal);
        if (currentScore >= sessionScoreGoal)
        {   
            // Pauses the game
            Time.timeScale = 0f;
        }
    }
}
