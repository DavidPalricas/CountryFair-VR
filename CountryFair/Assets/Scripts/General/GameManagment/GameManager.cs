using UnityEngine;

public class GameManager : MonoBehaviour
{   
    [SerializeField]
    protected int sessionScoreGoal = 3;


    protected virtual void Awake()
    {
       PlayerPrefs.SetInt("SessionGoal", sessionScoreGoal);
    }

    public void SessionGoalReached()
    {  
        Time.timeScale = 0f;
    }
}
