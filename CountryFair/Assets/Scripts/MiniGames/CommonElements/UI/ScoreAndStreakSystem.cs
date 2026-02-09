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
    /// Shows text in format: "Pontos: {value}".
    /// </summary>
    [Header("UI Elements")]
    [SerializeField]
    private TextMeshProUGUI scoreText;
    
    /// <summary>
    /// TextMeshProUGUI component displaying the current streak value.
    /// Shows text in format: "Sequencia: {value}".
    /// Color changes dynamically based on streak thresholds.
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI streakText;

    [SerializeField]
    private TextMeshProUGUI sessionGoalText;

    /// <summary>
    /// GameObject representing the streak indicator symbol.
    /// Activated when a streak begins and deactivated when the streak is broken.
    /// </summary>
    [SerializeField]
    private GameObject streakSymbol;
    
    /// <summary>
    /// Scale multiplier for the score text punch animation.
    /// Values greater than 1.0 make the text briefly grow larger.
    /// </summary>
    [Header("Animation Settings")]
    [SerializeField]
    private float scorePunchScale = 1.2f;
    
    /// <summary>
    /// Duration in seconds of the score punch animation.
    /// </summary>
    [SerializeField]
    private float scorePunchDuration = 0.3f;
    
    /// <summary>
    /// Scale multiplier for the streak text punch animation.
    /// Typically larger than score punch to emphasize streak importance.
    /// </summary>
    [SerializeField]
    private float streakPunchScale = 1.3f;
    
    /// <summary>
    /// Duration in seconds of the streak punch animation.
    /// </summary>
    [SerializeField]
    private float streakPunchDuration = 0.4f;
    
    /// <summary>
    /// Duration in seconds of the streak loss shake animation.
    /// </summary>
    [SerializeField]
    private float streakLoseShakeDuration = 0.5f;
    
    /// <summary>
    /// Strength/intensity of the shake effect when losing a streak.
    /// Higher values create more pronounced shaking.
    /// </summary>
    [SerializeField]
    private float streakLoseShakeStrength = 20f;
    
    /// <summary>
    /// Color used for the score text flash animation when the player scores.
    /// Flashes briefly before returning to the original color.
    /// </summary>
    [Header("Color Settings")]
    [SerializeField]
    private Color scoreFlashColor = Color.yellow;
    
    /// <summary>
    /// Color for low streak values (below <see cref="streakMidThreshold"/>).
    /// Default is yellow, indicating a starting streak.
    /// </summary>
    [SerializeField]
    private Color streakLowColor = Color.yellow;
    
    /// <summary>
    /// Color for medium streak values (at or above <see cref="streakMidThreshold"/> but below <see cref="streakHighThreshold"/>).
    /// Default is orange, indicating a growing streak.
    /// </summary>
    [SerializeField]
    private Color streakMidColor = new (1f, 0.5f, 0f); // Orange
    
    /// <summary>
    /// Color for high streak values (at or above <see cref="streakHighThreshold"/>).
    /// Default is bright orange/red, indicating an impressive streak.
    /// </summary>
    [SerializeField]
    private Color streakHighColor = new (1f, 0.3f, 0f); // Bright orange
    
    /// <summary>
    /// Color flashed when the player misses and loses their streak.
    /// Default is red, providing strong negative feedback.
    /// </summary>
    [SerializeField]
    private Color streakMissColor = Color.red;
    
    /// <summary>
    /// Default color to reset the streak text to after animations complete.
    /// Used when the streak is reset to zero.
    /// </summary>
    [SerializeField]
    private Color streakResetColor = Color.white;
    
    /// <summary>
    /// Streak value threshold for transitioning to medium streak color.
    /// When streak reaches this value, the color changes from <see cref="streakLowColor"/> to <see cref="streakMidColor"/>.
    /// </summary>
    [Header("Other Settings")]
    [SerializeField]
    private int streakMidThreshold = 5;
    
    /// <summary>
    /// Streak value threshold for transitioning to high streak color.
    /// When streak reaches this value, the color changes to <see cref="streakHighColor"/>.
    /// </summary>
    [SerializeField]
    private int streakHighThreshold = 10;
    
    /// <summary>
    /// Minimum streak value required to trigger bonus animations (rotation effect).
    /// High streaks receive additional visual emphasis.
    /// </summary>
    [SerializeField]
    private int highStreaksNumber = 5;
    
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

    private int _sessionGoal = 0;
    
    /// <summary>
    /// Event invoked to check if the session score goal has been reached.
    /// </summary>
    public UnityEvent sessionGoalReached;

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
        if (scoreText == null)
        {
            Debug.LogError("Score TextMeshProUGUI reference is not assigned.");
            return;
        }

        if (streakText == null)
        {
            Debug.LogError("Streak TextMeshProUGUI reference is not assigned.");
            return;
        }

        if (streakSymbol == null)
        {
            Debug.LogError("Streak Symbol GameObject reference is not assigned.");

            return;
        }

        streakSymbol.SetActive(false);

        UpdateScoreText();
        UpdateStreakText();
    }

    private void Start()
    {   
        _sessionGoal = PlayerPrefs.GetInt("SessionGoal", 0);
        sessionGoalText.text = $"Objetivo da Sessao: {_sessionGoal}";
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
    /// <item><description>If streak >= <see cref="highStreaksNumber"/>, adds bonus rotation animation</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// All animations use DOTween and automatically kill any existing animations on the same elements
    /// to prevent conflicts.
    /// </para>
    /// </remarks>
    public void PlayerScored(int points = 1)
    {
        _scoreValue += points;
        _streakValue += 1;
        streakSymbol.SetActive(true);
        
        // Update text
        UpdateScoreText();
        UpdateStreakText();
        
        // Animate score with punch effect
        scoreText.transform.DOKill();
        scoreText.transform.DOPunchScale(Vector3.one * scorePunchScale, scorePunchDuration, 5, 0.5f);
        
        // Animate color flash for score
        scoreText.DOKill();
        scoreText.DOColor(scoreFlashColor, 0.1f).SetLoops(2, LoopType.Yoyo);
        
        // Animate streak with bigger punch effect
        streakText.transform.DOKill();
        streakText.transform.DOPunchScale(Vector3.one * streakPunchScale, streakPunchDuration, 6, 0.5f);
        
        // Animate color based on streak value
        Color streakColor = GetStreakColor(_streakValue);
        streakText.DOKill();
        streakText.DOColor(streakColor, 0.2f).SetLoops(2, LoopType.Yoyo);
        
        // Add rotation for high streaks
        if (_streakValue >= highStreaksNumber)
        {
            streakText.transform.DOPunchRotation(new Vector3(0, 0, 15), streakPunchDuration, 8, 0.5f);
        }

        if (_scoreValue >= _sessionGoal)
        {
            sessionGoalReached.Invoke();
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
    /// <item><description>Flashes red color 4 times, then resets to <see cref="streakResetColor"/></description></item>
    /// <item><description>Scales down to 0.7x then back to normal (emphasizes loss)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: The score value is NOT affected by missing - only the streak resets.
    /// All animations use DOTween and automatically kill any existing animations to prevent conflicts.
    /// </para>
    /// </remarks>
    public void PlayerMissed()
    {    
        if (_streakValue > 0 && !DOTween.IsTweening(streakText.transform)){
            streakSymbol.SetActive(false);
            _streakValue = 0;
            UpdateStreakText();
            
            // Shake animation for losing streak
            streakText.transform.DOKill();
            streakText.transform.DOShakePosition(streakLoseShakeDuration, streakLoseShakeStrength, 20, 90, false, true);
            
            // Flash red color
            streakText.DOKill();
            streakText.DOColor(streakMissColor, 0.15f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
            {
                streakText.color = streakResetColor;
            });
            
            // Scale down effect
            streakText.transform.DOScale(0.7f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }
    
    /// <summary>
    /// Updates the score text UI to display the current score value.
    /// </summary>
    /// <remarks>
    /// Formats the text as "Pontos: {value}" in Portuguese.
    /// Called automatically by <see cref="PlayerScored"/> and during initialization.
    /// </remarks>
    private void UpdateScoreText()
    {
        scoreText.text = $"Pontos Atuais: {_scoreValue}";
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
        streakText.text = $"Sequencia: {_streakValue}";
    }
    
    /// <summary>
    /// Determines the appropriate color for the streak text based on the current streak value.
    /// </summary>
    /// <param name="streak">The current streak value to evaluate.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="streakHighColor"/> if streak >= <see cref="streakHighThreshold"/></description></item>
    /// <item><description><see cref="streakMidColor"/> if streak >= <see cref="streakMidThreshold"/></description></item>
    /// <item><description><see cref="streakLowColor"/> for all other values</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This creates a progressive visual feedback system where colors intensify as the streak grows,
    /// providing immediate visual indication of performance level.
    /// </remarks>
    private Color GetStreakColor(int streak)
    {
        if (streak >= streakHighThreshold)
            return streakHighColor;
        
        if (streak >= streakMidThreshold)
            return streakMidColor;
        
        return streakLowColor;
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
        scoreText.transform.DOKill();
        scoreText.DOKill();
        streakText.transform.DOKill();
        streakText.DOKill();
    }
}