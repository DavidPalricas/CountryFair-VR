using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class CannyWise : MonoBehaviour
{
    [Header("Adaptation Configuration")] 
    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToIncreaseDiff = 0.7f; 

    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToDecreaseDiff = 0.3f; 

    [SerializeField]
    private int thresholdToChangeDiff = 3; 

    // Variáveis de Estado
    private float _taskStartTime = 0f;
    private int _currentAttemptsForTask = 0; 
    
    // Contadores de buffer
    private int _struggleCounter = 0;
    private int _excelCounter = 0;

    private readonly string _sessionID = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

    private readonly List<float> _tasksTimes = new ();
    private readonly List<float> _tasksPrecisions = new ();

    private string _miniGameName;


    private void Awake()
    {
        GameManager gameManager = GetComponent<GameManager>();

        _miniGameName = gameManager is FrisbeeGameManager ? "frisbee" : "archery";
    }

    public void StartTaskTimer()
    {   // If the timer already started, do not reset
        if (_taskStartTime != 0f)
        {
             _taskStartTime = Time.time;
            _currentAttemptsForTask = 0; 
        }
    }

    public void PlayerMissed()
    {
        _currentAttemptsForTask++;
    }

    public void PlayerScored()
    {
        _currentAttemptsForTask++;

        float timeTaken = Time.time - _taskStartTime;

        const int SCORE_ATTEMPT = 1; 

        float taskPrecision = SCORE_ATTEMPT / (float)_currentAttemptsForTask; 

        _tasksPrecisions.Add(taskPrecision);
        _tasksTimes.Add(timeTaken);

        EvaluatePerformance(taskPrecision, timeTaken);
    }

    private void EvaluatePerformance(float precision, float timeTaken)
    {
        if (precision >= precisionThresholdToIncreaseDiff)
        {
            _excelCounter++;
            _struggleCounter = 0; 
            
            CheckToChangeDifficulty();
            return;
        }

        if (precision <= precisionThresholdToDecreaseDiff)
        {
            _struggleCounter++;
            _excelCounter = 0; 

            CheckToChangeDifficulty();

            return;
        }

        MantainFlowState();
    }


    private void MantainFlowState()
    {
        if (_struggleCounter > 0)
        {
            _struggleCounter--;
        }

        if (_excelCounter > 0)
        {
            _excelCounter--;
        }
    }

    private void CheckToChangeDifficulty()
    {
        if (_excelCounter >= thresholdToChangeDiff)
        {
            Debug.Log(">>> AUMENTAR DIFICULDADE DO JOGO <<<");

            _excelCounter = 0; // Reset após mudança

            return;
        }

        if (_struggleCounter >= thresholdToChangeDiff)
        {
            Debug.Log(">>> DIMINUIR DIFICULDADE DO JOGO <<<");

            _struggleCounter = 0; // Reset após mudança
        }
    }

    public void SessionGoalReached()
    {
        SaveSessionData();
    }

    private void SaveSessionData()
    {
       float averageTaskPrecision = _tasksPrecisions.Average();
       float averageTaskTime = _tasksTimes.Average();

       SessionData sessionData = new()
       {
           SessionGoal = _tasksPrecisions.Count,
           AverageTaskPrecision = averageTaskPrecision,
           AverageTaskCompletionTime = averageTaskTime
       };

       DataFileManager.GetInstance().SaveSessionData(sessionData, _sessionID, _miniGameName);
    }
}