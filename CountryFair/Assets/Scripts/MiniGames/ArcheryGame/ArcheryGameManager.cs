using UnityEngine;

public class ArcheryGameManager : GameManager
{
    public static ArcheryGameManager Instance { get; private set; }
    [SerializeField] private int score;
    [SerializeField] private ScoreAndStreakSystem scoreSystem;

    protected override void Awake()
    {   
        base.Awake();
        
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetScore() => score;
    public void SetScore(int score){
        this.score += score;
        scoreSystem.PlayerScored(score);
    }


}
