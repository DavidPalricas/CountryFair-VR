using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(GameManager))]
public class CarnyWise : MonoBehaviour
{
    [Header("Adaptation Configuration")] 
    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToIncreaseDiff = 0.7f; 

    [SerializeField, Range(0f, 1f)]
    private float precisionThresholdToDecreaseDiff = 0.3f; 

    [SerializeField]
    private int thresholdToChangeDiff = 3; 

    [SerializeField]
    private int failingConsecutiveThreshold = 4;

    [SerializeField]
    private UnityEvent<bool> showFeedback;

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


    private GameManager _gameManager;


    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();

        _miniGameName = _gameManager is FrisbeeGameManager ? "frisbee" : "archery";
    }

    public void StartTaskTimer()
    {   // If the timer already started, do not reset
        if (_taskStartTime <= 0f)
        {
             _taskStartTime = Time.time;
        }
    }

    public void PlayerMissed()
    {
        _currentAttemptsForTask++;

        if (_currentAttemptsForTask >= failingConsecutiveThreshold)
        {
            DecreaseDifficulty();

            ResetTask();
        }
    }

    public void PlayerScored()
    {   
        _currentAttemptsForTask++;

        Debug.Log($"Player Scored! Current Attempts: {_currentAttemptsForTask}");
        float timeTaken = Time.time - _taskStartTime;

        const int SCORE_ATTEMPT = 1; 

        float taskPrecision = SCORE_ATTEMPT / (float)_currentAttemptsForTask; 

        Debug.Log("Task Precision: " + taskPrecision);

        _tasksPrecisions.Add(taskPrecision);
        _tasksTimes.Add(timeTaken);

        EvaluatePerformance(taskPrecision, timeTaken);
        
        ResetTask();
    }


    private void ResetTask()
    {
        _taskStartTime = 0f;
        _currentAttemptsForTask = 0; 
    }

    private void EvaluatePerformance(float precision, float timeTaken)
    {
        if (precision >= precisionThresholdToIncreaseDiff)
        {
            _excelCounter++;

            _struggleCounter = Mathf.Max(0, _struggleCounter--);

            CheckToChangeDifficulty();
            return;
        }

        if (precision <= precisionThresholdToDecreaseDiff)
        {
            _struggleCounter++;

            _excelCounter = Mathf.Max(0, _excelCounter--);

            CheckToChangeDifficulty();

            return;
        }

        MantainFlowState();
    }


    private void MantainFlowState()
    {   
        Debug.Log("Mantaining Flow State");

        _struggleCounter = Mathf.Max(0, _struggleCounter--);
        _excelCounter = Mathf.Max(0, _excelCounter--);
    }

    private void CheckToChangeDifficulty()
    {  
        if (_excelCounter >= thresholdToChangeDiff)
        {
            IncreaseDifficulty();
            return;
        }

        if (_struggleCounter >= thresholdToChangeDiff)
        {
            DecreaseDifficulty();
        }
    }

    private void IncreaseDifficulty()
    {
        _gameManager.IncreaseDifficulty();

        showFeedback.Invoke(true);

        _excelCounter = 0; 
    }

    private void DecreaseDifficulty()
    {
        _gameManager.DecreaseDifficulty();

        showFeedback.Invoke(false);

        _struggleCounter = 0; 
    }


    public void SessionGoalReached()
    {
        SaveSessionData();
    }

    private void SaveSessionData()
    {
       int averageTaskPrecision = (int)System.Math.Round(_tasksPrecisions.Average() * 100);

       int averageTaskTime = (int)System.Math.Round(_tasksTimes.Average());

       SessionData sessionData = new()
       {
           SessionGoal = $"{_tasksPrecisions.Count} pontos",
           AverageTaskPrecision = $"{averageTaskPrecision} %",
           AverageTaskCompletionTime = $"{averageTaskTime} segundos"
       };

       DataFileManager.GetInstance().SaveSessionData(sessionData, _sessionID, _miniGameName);
    }
}