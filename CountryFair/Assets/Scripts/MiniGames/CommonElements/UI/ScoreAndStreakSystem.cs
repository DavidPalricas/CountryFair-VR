using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

/// <summary>
/// Manages score and streak tracking with animated visual feedback for mini-games.
/// Provides dynamic UI updates with DOTween animations including punch effects, color flashes, and shake animations.
/// </summary>
/// <remarks>
/// <para>
/// This system tracks two metrics:
/// <list type="bullet">
/// <item><description><b>Score:</b> Total successful actions, increments indefinitely</description></item>
/// <item><description><b>Streak:</b> Consecutive successful actions, resets on failure</description></item>
/// </list>
/// </para>
/// <para>
/// Visual feedback features:
/// <list type="bullet">
/// <item><description>Score animations: Punch scale effect with color flash</description></item>
/// <item><description>Streak animations: Dynamic color changes based on threshold levels</description></item>
/// <item><description>High streak bonus: Additional rotation animation</description></item>
/// <item><description>Miss animations: Shake effect with red color flash and scale down</description></item>
/// </list>
/// </para>
/// <para>
/// Requires DOTween (DG.Tweening) for animations.
/// </para>
/// </remarks>
public class ScoreAndStreakSystem : MonoBehaviour
{   
    /// <summary>
    /// TextMeshProUGUI component displaying the current score value.
    /// Shows text in format: "Pontos Atuais: {value}".
    /// </summary>
    [Header("UI Elements")]
    [SerializeField]
    private TextMeshProUGUI _scoreText;

    /// <summary>
    /// TextMeshProUGUI component displaying the current streak value.
    /// Shows text in format: "Sequencia: {value}".
    /// Color changes dynamically based on streak thresholds.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI _streakText;

    /// <summary>TextMeshProUGUI component displaying the session's score goal, read once from <see cref="PlayerPrefs"/> in <see cref="Start"/>.</summary>
    [SerializeField]
    private TextMeshProUGUI _sessionGoalText;

    /// <summary>
    /// GameObject representing the streak indicator symbol.
    /// Activated when a streak begins and deactivated when the streak is broken.
    /// </summary>
    [SerializeField]
    private GameObject _streakSymbol;
    
    /// <summary>
    /// Scale multiplier for the score text punch animation.
    /// Values greater than 1.0 make the text briefly grow larger.
    /// </summary>
    [Header("Animation Settings")]
    [SerializeField]
    private float _scorePunchScale = 1.2f;
    
    /// <summary>
    /// Duration in seconds of the score punch animation.
    /// </summary>
    [SerializeField]
    private float _scorePunchDuration = 0.3f;
    
    /// <summary>
    /// Scale multiplier for the streak text punch animation.
    /// Typically larger than score punch to emphasize streak importance.
    /// </summary>
    [SerializeField]
    private float _streakPunchScale = 1.3f;
    
    /// <summary>
    /// Duration in seconds of the streak punch animation.
    /// </summary>
    [SerializeField]
    private float _streakPunchDuration = 0.4f;
    
    /// <summary>
    /// Duration in seconds of the streak loss shake animation.
    /// </summary>
    [SerializeField]
    private float _streakLoseShakeDuration = 0.5f;
    
    /// <summary>
    /// Strength/intensity of the shake effect when losing a streak.
    /// Higher values create more pronounced shaking.
    /// </summary>
    [SerializeField]
    private float _streakLoseShakeStrength = 20f;
    
    /// <summary>
    /// Color used for the score text flash animation when the player scores.
    /// Flashes briefly before returning to the original color.
    /// </summary>
    [Header("Color Settings")]
    [SerializeField]
    private Color _scoreFlashColor = Color.yellow;
    
    /// <summary>
    /// Color for low streak values (below <see cref="_streakMidThreshold"/>).
    /// Default is yellow, indicating a starting streak.
    /// </summary>
    [SerializeField]
    private Color _streakLowColor = Color.yellow;
    
    /// <summary>
    /// Color for medium streak values (at or above <see cref="_streakMidThreshold"/> but below <see cref="_streakHighThreshold"/>).
    /// Default is orange, indicating a growing streak.
    /// </summary>
    [SerializeField]
    private Color _streakMidColor = new (1f, 0.5f, 0f); // Orange
    
    /// <summary>
    /// Color for high streak values (at or above <see cref="_streakHighThreshold"/>).
    /// Default is bright orange/red, indicating an impressive streak.
    /// </summary>
    [SerializeField]
    private Color _streakHighColor = new (1f, 0.3f, 0f); // Bright orange
    
    /// <summary>
    /// Color flashed when the player misses and loses their streak.
    /// Default is red, providing strong negative feedback.
    /// </summary>
    [SerializeField]
    private Color _streakMissColor = Color.red;
    
    /// <summary>
    /// Default color to reset the streak text to after animations complete.
    /// Used when the streak is reset to zero.
    /// </summary>
    [SerializeField]
    private Color _streakResetColor = Color.white;
    
    /// <summary>
    /// Streak value threshold for transitioning to medium streak color.
    /// When streak reaches this value, the color changes from <see cref="_streakLowColor"/> to <see cref="_streakMidColor"/>.
    /// </summary>
    [Header("Other Settings")]
    [SerializeField]
    private int _streakMidThreshold = 5;
    
    /// <summary>
    /// Streak value threshold for transitioning to high streak color.
    /// When streak reaches this value, the color changes to <see cref="_streakHighColor"/>.
    /// </summary>
    [SerializeField]
    private int _streakHighThreshold = 10;
    
    /// <summary>
    /// Minimum streak value required to trigger bonus animations (rotation effect).
    /// High streaks receive additional visual emphasis.
    /// </summary>
    [SerializeField]
    private int _highStreaksNumber = 5;

    /// <summary>
    /// Event invoked to check if the session score goal has been reached.
    /// </summary>
    [SerializeField]
    private UnityEvent _sessionGoalReached;
    
    /// <summary>
    /// Current score value tracking total successful actions.
    /// Increments with each <see cref="PlayerScored"/> call and never decreases.
    /// </summary>
    private int _scoreValue = 0;
    
    /// <summary>
    /// Current streak value tracking consecutive successful actions.
    /// Increments with each <see cref="PlayerScored"/> call and resets to 0 on <see cref="PlayerMissed"/>.
    /// </summary>
    private int _streakValue = 0;

    /// <summary>Score goal for the current session, read from the <c>SessionGoal</c> <see cref="PlayerPrefs"/> key in <see cref="Start"/>.</summary>
    private int _sessionGoal = 0;

    /// <summary>
    /// Initializes the score and streak system by validating references and setting initial UI state.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the script instance is being loaded.
    /// <para>
    /// Performs the following initialization:
    /// <list type="number">
    /// <item><description>Validates all required UI component references (scoreText, streakText, streakSymbol)</description></item>
    /// <item><description>Deactivates the streak symbol (only shown when a streak is active)</description></item>
    /// <item><description>Updates UI text to display initial values (0 for both score and streak)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Logs errors and returns early if any required references are missing.
    /// </para>
    /// </remarks>
    private void Awake()
    {
        if (_scoreText == null)
        {
            Debug.LogError("Score TextMeshProUGUI reference is not assigned.");
            return;
        }

        if (_streakText == null)
        {
            Debug.LogError("Streak TextMeshProUGUI reference is not assigned.");
            return;
        }

        if (_streakSymbol == null)
        {
            Debug.LogError("Streak Symbol GameObject reference is not assigned.");

            return;
        }

        _streakSymbol.SetActive(false);

        UpdateScoreText();
        UpdateStreakText();
    }

    /// <summary>Reads the session score goal from <see cref="PlayerPrefs"/>, displays it, and reports the initial (zeroed) progress to <see cref="ConnectToWebApp"/>.</summary>
    private void Start()
    {
        _sessionGoal = PlayerPrefs.GetInt("SessionGoal", 0);

        UpdateSessionGoalText();
       
        ConnectToWebApp.Instance.UpdatePlayerProgessOnMiniGame(_scoreValue, _sessionGoal, _streakValue);
    }

    /// <summary>
    /// Called when the player successfully completes an action (e.g., catches a frisbee, hits a target).
    /// Increments both score and streak, then triggers animated visual feedback.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs the following sequence:
    /// <list type="number">
    /// <item><description>Increments <see cref="_scoreValue"/> by 1</description></item>
    /// <item><description>Increments <see cref="_streakValue"/> by 1</description></item>
    /// <item><description>Activates the streak symbol GameObject</description></item>
    /// <item><description>Updates the score and streak text displays</description></item>
    /// <item><description>Animates score text with punch scale effect and yellow color flash</description></item>
    /// <item><description>Animates streak text with punch scale effect and dynamic color based on streak value</description></item>
    /// <item><description>If streak >= <see cref="_highStreaksNumber"/>, adds bonus rotation animation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// All animations use DOTween and automatically kill any existing animations on the same elements
    /// to prevent conflicts.
    /// </para>
    /// </remarks>
    /// <remarks>Invoked via Inspector by the scoring UnityEvents of the mini-games' score areas/targets (e.g. Frisbee's ScoreArea, Archery's balloon targets).</remarks>
    public void PlayerScored(int points = 1)
    {
        _scoreValue += points;
        _streakValue += 1;
        _streakSymbol.SetActive(true);

        ConnectToWebApp.Instance.UpdatePlayerProgessOnMiniGame(_scoreValue, _sessionGoal, _streakValue);

        // Update text
        UpdateScoreText();
        UpdateStreakText();
        
        // Animate score with punch effect
        _scoreText.transform.DOKill();
        _scoreText.transform.DOPunchScale(Vector3.one * _scorePunchScale, _scorePunchDuration, 5, 0.5f);
        
        // Animate color flash for score
        _scoreText.DOKill();
        _scoreText.DOColor(_scoreFlashColor, 0.1f).SetLoops(2, LoopType.Yoyo);
        
        // Animate streak with bigger punch effect
        _streakText.transform.DOKill();
        _streakText.transform.DOPunchScale(Vector3.one * _streakPunchScale, _streakPunchDuration, 6, 0.5f);
        
        // Animate color based on streak value
        Color streakColor = GetStreakColor(_streakValue);
        _streakText.DOKill();
        _streakText.DOColor(streakColor, 0.2f).SetLoops(2, LoopType.Yoyo);
        
        // Add rotation for high streaks
        if (_streakValue >= _highStreaksNumber)
        {
            _streakText.transform.DOPunchRotation(new Vector3(0, 0, 15), _streakPunchDuration, 8, 0.5f);
        }

        if (_scoreValue >= _sessionGoal)
        {
            _sessionGoalReached.Invoke();
        }
    }
    
    /// <summary>
    /// Called when the player fails to score (e.g, the firsbbe thrown not landed in the score zone,
    /// the arrow missed the target).
    /// Resets the streak to zero and triggers negative visual feedback if a streak was active.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method only performs actions if the player had an active streak (<see cref="_streakValue"/> > 0).
    /// </para>
    /// <para>
    /// Reset sequence:
    /// <list type="number">
    /// <item><description>Deactivates the streak symbol GameObject</description></item>
    /// <item><description>Resets <see cref="_streakValue"/> to 0</description></item>
    /// <item><description>Updates the streak text display</description></item>
    /// <item><description>Animates streak text with shake effect (position shake)</description></item>
    /// <item><description>Flashes red color 4 times, then resets to <see cref="_streakResetColor"/></description></item>
    /// <item><description>Scales down to 0.7x then back to normal (emphasizes loss)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: The score value is NOT affected by missing - only the streak resets.
    /// All animations use DOTween and automatically kill any existing animations to prevent conflicts.
    /// </para>
    /// </remarks>
    /// <remarks>Invoked via Inspector by the missing UnityEvents of the mini-games' miss triggers (e.g. Archery's <c>Arrow</c> hitting the ground/out-of-bounds).</remarks>
    public void PlayerMissed()
    {    
        if (_streakValue > 0 && !DOTween.IsTweening(_streakText.transform)){
            _streakSymbol.SetActive(false);
            _streakValue = 0;

            ConnectToWebApp.Instance.UpdatePlayerProgessOnMiniGame(_scoreValue, _sessionGoal, _streakValue);

            UpdateStreakText();
            
            // Shake animation for losing streak
            _streakText.transform.DOKill();
            _streakText.transform.DOShakePosition(_streakLoseShakeDuration, _streakLoseShakeStrength, 20, 90, false, true);
            
            // Flash red color
            _streakText.DOKill();
            _streakText.DOColor(_streakMissColor, 0.15f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
            {
                _streakText.color = _streakResetColor;
            });
            
            // Scale down effect
            _streakText.transform.DOScale(0.7f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }
    
    /// <summary>
    /// Updates the score text UI to display the current score value.
    /// </summary>
    /// <remarks>
    /// Formats the text as "Pontos Atuais: {value}" in Portuguese.
    /// Called automatically by <see cref="PlayerScored"/> and during initialization.
    /// </remarks>
    private void UpdateScoreText()
    {
        _scoreText.text = $"Pontos Atuais: {_scoreValue}";
    }
    
    /// <summary>
    /// Updates the streak text UI to display the current streak value.
    /// </summary>
    /// <remarks>
    /// Formats the text as "Sequencia: {value}" in Portuguese.
    /// Called automatically by <see cref="PlayerScored"/>, <see cref="PlayerMissed"/>, and during initialization.
    /// </remarks>
    private void UpdateStreakText()
    {   
        _streakText.text = $"Sequencia: {_streakValue}";
    }
    
    /// <summary>
    /// Determines the appropriate color for the streak text based on the current streak value.
    /// </summary>
    /// <param name="streak">The current streak value to evaluate.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="_streakHighColor"/> if streak >= <see cref="_streakHighThreshold"/></description></item>
    /// <item><description><see cref="_streakMidColor"/> if streak >= <see cref="_streakMidThreshold"/></description></item>
    /// <item><description><see cref="_streakLowColor"/> for all other values</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This creates a progressive visual feedback system where colors intensify as the streak grows,
    /// providing immediate visual indication of performance level.
    /// </remarks>
    private Color GetStreakColor(int streak)
    {
        if (streak >= _streakHighThreshold)
        {
            return _streakHighColor;
        }
                   
        if (streak >= _streakMidThreshold)
        {
             return _streakMidColor;
        }
           
        return _streakLowColor;
    }
    
    /// <summary>
    /// Cleans up all active DOTween animations when the GameObject is destroyed.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked when the MonoBehaviour will be destroyed.
    /// <para>
    /// Kills all tweens on:
    /// <list type="bullet">
    /// <item><description>Score text transform (scale/rotation animations)</description></item>
    /// <item><description>Score text component (color animations)</description></item>
    /// <item><description>Streak text transform (scale/rotation/position animations)</description></item>
    /// <item><description>Streak text component (color animations)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This prevents memory leaks and ensures animations don't continue running on destroyed objects.
    /// Important for proper DOTween cleanup when changing scenes or destroying the game object.
    /// </para>
    /// </remarks>
    private void OnDestroy()
    {
        // Clean up tweens when object is destroyed
        _scoreText.transform.DOKill();
        _scoreText.DOKill();
        _streakText.transform.DOKill();
        _streakText.DOKill();
    }


    public void UpdateSessionGoalText(int newGoal = -1)
    {   
        if (newGoal >= 0)
        {
            _sessionGoal = newGoal;
        }

        _sessionGoalText.text = $"Objetivo da Sessao: {_sessionGoal}";

        ConnectToWebApp.Instance.UpdatePlayerProgessOnMiniGame(_scoreValue, _sessionGoal, _streakValue);
    }
}