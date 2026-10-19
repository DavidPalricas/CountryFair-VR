using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Dynamic Difficulty Adjustment (DDA) system that monitors player performance and
/// fires difficulty change events based on precision thresholds and consecutive-attempt counters.
/// </summary>
/// <remarks>
/// Precision per task = 1 / attempts_until_score. High-precision tasks fill <see cref="_excelCounter"/> and
/// low-precision ones fill <see cref="_struggleCounter"/>; every other task drains them, so a change needs a net run of
/// <see cref="_thresholdToChangeDiff"/> tasks rather than strictly consecutive ones. If the player misses
/// <see cref="_failingConsecutiveThreshold"/> times on the same task, difficulty drops immediately.
/// At session end, <see cref="SessionGoalReached"/> marks the session complete in <see cref="GameManager"/>
/// and returns to the hub scene.
/// </remarks>
[RequireComponent(typeof(CarnyWiseDiffFeedback))]
public class CarnyWise : MonoBehaviour
{
    /// <summary>Wired in Inspector to <see cref="MiniGameManager.ChangeDifficulty"/>.</summary>
    [SerializeField]
    private UnityEvent <bool> _changeDifficulty;

    /// <summary>Raised by <see cref="ResetDifficulty"/>; wired in Inspector to <see cref="MiniGameManager.ResetDifficulty"/>.</summary>
    [SerializeField]
    private UnityEvent _resetDifficulty;

    /// <summary>Precision ratio at or above which a task counts as excelling (triggers potential increase).</summary>
    [Header("Adaptation Configuration")]
    [SerializeField, Range(0f, 1f)]
    private float _precisionThresholdToIncreaseDiff = 0.7f;

    /// <summary>Precision ratio at or below which a task counts as struggling (triggers potential decrease).</summary>
    [SerializeField, Range(0f, 1f)]
    private float _precisionThresholdToDecreaseDiff = 0.3f;

    /// <summary>Number of consecutive excelling or struggling tasks required to change difficulty.</summary>
    [SerializeField]
    private int _thresholdToChangeDiff = 3;

    /// <summary>Number of consecutive misses that immediately trigger a difficulty decrease.</summary>
    [SerializeField]
    private int _failingConsecutiveThreshold = 4;

    /// <summary><see cref="Time.time"/> when the current task started; 0 means no task is running.</summary>
    private float _taskStartTime = 0f;
    /// <summary>Attempts (misses + the final score) spent on the current task; precision is its inverse.</summary>
    private int _currentAttemptsForTask = 0; 
    
    /// <summary>
    /// Struggle buffer: +1 for each scored task with precision at or below <see cref="_precisionThresholdToDecreaseDiff"/>,
    /// −1 (never below 0) for an excelling or in-flow task. Reaching <see cref="_thresholdToChangeDiff"/> lowers the
    /// difficulty and resets it to 0, so a single unlucky task cannot lower the difficulty on its own.
    /// </summary>
    private int _struggleCounter = 0;

    /// <summary>
    /// Excel buffer: +1 for each scored task with precision at or above <see cref="_precisionThresholdToIncreaseDiff"/>,
    /// −1 (never below 0) for a struggling or in-flow task. Reaching <see cref="_thresholdToChangeDiff"/> raises the
    /// difficulty and resets it to 0, so a single lucky task cannot raise the difficulty on its own.
    /// </summary>
    private int _excelCounter = 0;

    /// <summary>Session key for the save file, taken when the mini-game scene loads; used by <see cref="SaveSessionData"/>.</summary>
    private readonly string _sessionID = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

    /// <summary>
    /// Seconds each scored task took, from <see cref="StartTaskTimer"/> to <see cref="PlayerScored"/>. Misses add nothing;
    /// they only lower the precision of the task. Averaged for the session summary saved by <see cref="SaveSessionData"/>.
    /// </summary>
    private readonly List<float> _tasksTimes = new ();
    
    /// <summary>
    /// Precision of each scored task: <c>1 / attempts</c> (1 = first try, 0.5 = second, ...). Averaged for the session
    /// summary saved by <see cref="SaveSessionData"/>; its count is also the session's points.
    /// </summary>
    private readonly List<float> _tasksPrecisions = new ();

    /// <summary>Derived from the scene name; selects the PlayerPrefs difficulty key and the session-complete flag.</summary>
    private bool _isFrisbeeMiniGame = false;

    /// <summary>
    /// Carny Wise dialog on the same GameObject that tells the player the difficulty went up or down. Cached in
    /// <see cref="Awake"/> and not null-checked, so the component must exist next to this one.
    /// </summary>
    private CarnyWiseDiffFeedback _diffFeedback;

    /// <summary>Detects which mini-game is running and caches the difficulty feedback dialog.</summary>
    private void Awake()
    {
        SetMiniGameName();
        _diffFeedback = GetComponent<CarnyWiseDiffFeedback>();
    }


    /// <summary>
    /// Sets <see cref="_isFrisbeeMiniGame"/> from the active scene name (contains <c>frisbee</c> or <c>archery</c>,
    /// case-insensitive).
    /// </summary>
    /// <remarks>Any other scene logs an error and keeps the Archery behaviour (flag stays false).</remarks>
    private void SetMiniGameName()
    {
        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("frisbee"))
        {
            _isFrisbeeMiniGame = true;

             return;
        }

        if (sceneName.Contains("archery"))
        {
            _isFrisbeeMiniGame = false;

            return;
        }
      
        Debug.LogError("Invalid scene"); 
    }

    /// <summary>
    /// Starts the per-task completion timer. Safe to call multiple times; only the first call sets the start time.
    /// </summary>
    /// <remarks>Invoked via Inspector when the player starts a task (e.g. when a score area is activated or the bow string is grabbed).</remarks>
    public void StartTaskTimer()
    {
        if (_taskStartTime <= 0f)
        {
             _taskStartTime = Time.time;
        }
    }

    /// <summary>
    /// Records a miss attempt on the current task. When the attempts on it reach <see cref="_failingConsecutiveThreshold"/>,
    /// immediately decreases difficulty (safety net) and starts a fresh task, without waiting for the struggle buffer.
    /// </summary>
    /// <remarks>
    /// The fresh task also clears the timer, so the next task's time only starts at the next <see cref="StartTaskTimer"/>
    /// call. The abandoned task is not added to <see cref="_tasksTimes"/>/<see cref="_tasksPrecisions"/>.
    /// Invoked via Inspector by the mini-game's <c>_playerMissed</c> event.
    /// </remarks>
    public void PlayerMissed()
    {
        _currentAttemptsForTask++;

        if (_currentAttemptsForTask >= _failingConsecutiveThreshold)
        {
            DecreaseDifficulty();

            ResetTask();
        }
    }

    /// <summary>
    /// Records a successful score, calculates task precision, and evaluates whether to change difficulty.
    /// </summary>
    /// <remarks>Invoked via Inspector by the mini-game's <c>_playerScored</c> event.</remarks>
    public void PlayerScored()
    {
        _currentAttemptsForTask++;

        // Debug.Log($"Player Scored! Current Attempts: {_currentAttemptsForTask}");
        float timeTaken = Time.time - _taskStartTime;

        const int ScoreAttempt = 1; 

        float taskPrecision = ScoreAttempt / (float)_currentAttemptsForTask; 

        // Debug.Log("Task Precision: " + taskPrecision);

        _tasksPrecisions.Add(taskPrecision);
        _tasksTimes.Add(timeTaken);

        ResetTask();

        EvaluatePerformance(taskPrecision, timeTaken);
    }


    /// <summary>Ends the current task: clears the start time (so <see cref="StartTaskTimer"/> can start a new one) and the attempt count.</summary>
    private void ResetTask()
    {
        _taskStartTime = 0f;
        _currentAttemptsForTask = 0; 
    }

    /// <summary>
    /// Classifies the task as excelling, struggling or in flow and updates the buffers. <paramref name="timeTaken"/>
    /// is not used in the decision yet.
    /// </summary>
    /// <param name="precision">Precision of the finished task (<c>1 / attempts</c>, 0–1].</param>
    /// <param name="timeTaken">Seconds the task took; currently ignored.</param>
    /// <remarks>
    /// Excelling (≥ <see cref="_precisionThresholdToIncreaseDiff"/>): excel +1, struggle −1, then checks for a change.
    /// Struggling (≤ <see cref="_precisionThresholdToDecreaseDiff"/>): struggle +1, excel −1, then checks for a change.
    /// Otherwise the task is in flow and <see cref="MantainFlowState"/> drains both.
    /// </remarks>
    private void EvaluatePerformance(float precision, float timeTaken)
    {
        if (precision >= _precisionThresholdToIncreaseDiff)
        {
            _excelCounter++;

            _struggleCounter = Mathf.Max(0, _struggleCounter - 1);

            CheckToChangeDifficulty();
            return;
        }

        if (precision <= _precisionThresholdToDecreaseDiff)
        {
            _struggleCounter++;

            _excelCounter = Mathf.Max(0, _excelCounter - 1);

            CheckToChangeDifficulty();

            return;
        }

        MantainFlowState();
    }


    /// <summary>
    /// The task was in the flow band (between the two precision thresholds): decays both buffers by 1, never below 0, so
    /// the difficulty stays and earlier streaks lose weight.
    /// </summary>
    private void MantainFlowState()
    {   
       // Debug.Log("Mantaining Flow State");

        _struggleCounter = Mathf.Max(0, _struggleCounter - 1);
        _excelCounter = Mathf.Max(0, _excelCounter - 1);
    }

    /// <summary>
    /// Raises the difficulty when <see cref="_excelCounter"/> reaches <see cref="_thresholdToChangeDiff"/>, otherwise lowers
    /// it when <see cref="_struggleCounter"/> does. The excel check runs first.
    /// </summary>
    private void CheckToChangeDifficulty()
    {  
        if (_excelCounter >= _thresholdToChangeDiff)
        {
            IncreaseDifficulty();
            return;
        }

        if (_struggleCounter >= _thresholdToChangeDiff)
        {
            DecreaseDifficulty();
        }
    }

    /// <summary>
    /// Forces a difficulty change outside the precision buffers, going through the same path (feedback dialog, buffer
    /// reset) as an automatic one.
    /// </summary>
    /// <param name="isToIncreaseDiff">True raises the difficulty, false lowers it.</param>
    /// <remarks>Invoked via Inspector by <c>MiniGameCheatCodes._changeDifficulty</c> (<c>increase</c>/<c>decrease</c> cheats).</remarks>
    public void ChangeDifficulty(bool isToIncreaseDiff)
    {
        if (isToIncreaseDiff)
        {
            IncreaseDifficulty();

            return;
        }
       
         DecreaseDifficulty();
    }


    /// <summary>
    /// Drops the mini-game back to difficulty 0 and clears the current task and both buffers, so the DDA starts over.
    /// Does nothing if the saved level is already 0.
    /// </summary>
    /// <remarks>Invoked via Inspector by <c>MiniGameCheatCodes._resetDifficulty</c> (<c>reset</c> cheat).</remarks>
    public void ResetDifficulty()
    {   string playerPrefsDiffKey = _isFrisbeeMiniGame ? "FrisbeeDifficultyLevel" : "ArcheryDifficultyLevel";
    
        int currentDifficulty = PlayerPrefs.GetInt(playerPrefsDiffKey, 0);

        if (currentDifficulty > 0)
        {
            _diffFeedback.ShowNewDiffFeedback(false);
            _resetDifficulty.Invoke();

            ResetTask();

            _struggleCounter = 0;
            _excelCounter = 0;
        }
    }



    /// <summary>Raises the difficulty, shows the Carny Wise feedback and clears the excel buffer.</summary>
    /// <remarks>Also reached by the <c>increase</c> cheat through <see cref="ChangeDifficulty"/>.</remarks>
    private void IncreaseDifficulty()
    {
        const bool IsToIncreaseDiff = true;

        _changeDifficulty.Invoke(IsToIncreaseDiff);
       

        _diffFeedback.ShowNewDiffFeedback(IsToIncreaseDiff);

        _excelCounter = 0; 
    }

    /// <summary>
    /// Lowers the difficulty and clears the struggle buffer. The feedback dialog is only shown when the saved level is
    /// above 0, so the player is not told the game got easier when it is already at the minimum.
    /// </summary>
    /// <remarks>
    /// Also called by the safety net in <see cref="PlayerMissed"/> and reached by the <c>decrease</c> cheat through
    /// <see cref="ChangeDifficulty"/>.
    /// </remarks>
    private void DecreaseDifficulty()
    {   
        string playerPrefsDiffKey = _isFrisbeeMiniGame ? "FrisbeeDifficultyLevel" : "ArcheryDifficultyLevel";
        
        int currentDifficulty = PlayerPrefs.GetInt(playerPrefsDiffKey, 0);

        const bool IsToIncreaseDiff = false;
        
        if (currentDifficulty > 0)
        {
            _diffFeedback.ShowNewDiffFeedback(IsToIncreaseDiff);
        }

        _changeDifficulty.Invoke(IsToIncreaseDiff);

        _struggleCounter = 0; 
    }

    /// <summary>
    /// Called when the player reaches the session score goal.
    /// Saves the session summary, then flags the session as complete in <see cref="GameManager"/> and loads the hub
    /// scene after a short delay.
    /// </summary>
    /// <remarks>Invoked via Inspector by the <c>_sessionGoalReached</c> event of <see cref="ScoreAndStreakSystem"/>.</remarks>
    public void SessionGoalReached()
    {
        SaveSessionData();

        Invoke(nameof(ReturnToFair), 1f);
    }

    /// <summary>Sets the mini-game's session-completed flag (shown by the hub dialogue) and loads <c>CountryFair</c>.</summary>
    private void ReturnToFair()
    {   
        GameManager gameManager = GameManager.GetInstance();

        if (_isFrisbeeMiniGame)
        {
            gameManager.FrisbeeSessionCompleted = true;
        }
        else
        {
            gameManager.ArcherySessionCompleted = true;
        }

        string sceneName = "CountryFair";

        SceneManager.LoadScene(sceneName);

        ConnectToWebApp.Instance.UpdateScene(sceneName.ToLower());
    }
     

    /// <summary>
    /// Writes this session's summary (points, average precision, average task time) to <c>survivorData.json</c> under
    /// <see cref="_sessionID"/>, in the current mini-game's section.
    /// </summary>
    /// <remarks>
    /// The averages are 0 when no task was scored (e.g. the session was ended with the <c>complete</c> cheat), because
    /// <c>Average()</c> throws on an empty list.
    /// </remarks>
    private void SaveSessionData()
    {
        int averageTaskPrecision = _tasksPrecisions.Count > 0 ? (int)System.Math.Round(_tasksPrecisions.Average() * 100) : 0;

        int averageTaskTime = _tasksTimes.Count > 0 ? (int)System.Math.Round(_tasksTimes.Average()) : 0;

        SessionData sessionData = new()
        {
            SessionGoal = $"{_tasksPrecisions.Count} pontos",
            AverageTaskPrecision = $"{averageTaskPrecision} %",
            AverageTaskCompletionTime = $"{averageTaskTime} segundos"
        };

        string miniGameName = _isFrisbeeMiniGame ? "frisbee" : "archery";

        DataFileManager.GetInstance().SaveSessionData(sessionData, _sessionID, miniGameName);
    }
}