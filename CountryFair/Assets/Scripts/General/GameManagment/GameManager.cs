using UnityEngine;
using UnityEngine.SceneManagement;
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
        Invoke(nameof(ReturnToFair), 5f);
    }

    public virtual void ChangeDifficulty(bool isToIncreaseDiff){
        Debug.LogError("ChangeDifficulty should be overridden in derived classes.");
    }


    public virtual void TutorialCompleted(){
        Debug.LogError("TutorialCompleted should be overridden in derived classes.");
    }

    private void ReturnToFair()
    {
        SceneManager.LoadScene("CountryFair");
    }
}
