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
        if (_taskStartTime > 0f)
        {
             _taskStartTime = Time.time;
        }
    }

    public void PlayerMissed()
    {
        _currentAttemptsForTask++;
        Debug.Log($"Player Missed! Current Attempts: {_currentAttemptsForTask}");
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
        
        // Resets for next task
        _taskStartTime = 0f;
        _currentAttemptsForTask = 0; 
    }

    private void EvaluatePerformance(float precision, float timeTaken)
    {
        if (precision >= precisionThresholdToIncreaseDiff)
        {
            _excelCounter++;
            _struggleCounter = 0; 

            Debug.Log("CarnyWise: Excel Counter Incremented");
            
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
        Debug.Log("Mantaining Flow State");

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
        Debug.Log($"Checking to change difficulty: Excel {_excelCounter}, Struggle {_struggleCounter}");
        if (_excelCounter >= thresholdToChangeDiff)
        {
            _gameManager.IncreaseDifficulty();

            showFeedback.Invoke(true);

            _excelCounter = 0; 
            return;
        }

        if (_struggleCounter >= thresholdToChangeDiff)
        {
            _gameManager.DecreaseDifficulty();

            showFeedback.Invoke(false);

           _struggleCounter = 0; 
        }
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