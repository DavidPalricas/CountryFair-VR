using UnityEngine;

[RequireComponent(typeof(CheatCodes))]
public class MiniGameManager : MonoBehaviour
{   
    [SerializeField]
    protected int sessionScoreGoal = 3;
   
   [SerializeField]
    protected int difficultyLevel = 0;


    private CheatCodes _cheatCodes;

    protected virtual void Awake()
    {
       PlayerPrefs.SetInt("SessionGoal", sessionScoreGoal);

       _cheatCodes = GetComponent<CheatCodes>();

       _cheatCodes.enabled = false;
    }
    public virtual void ChangeDifficulty(bool isToIncreaseDiff){
        Debug.LogError("ChangeDifficulty should be overridden in derived classes.");
    }

    protected virtual void ApplyDifficultySettings(){
        Debug.LogError("ApplyDifficultySettings should be overridden in derived classes.");
    }


    public virtual void TutorialCompleted(){
        _cheatCodes.enabled = true;
    }
}
