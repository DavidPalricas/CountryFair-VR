using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the Frisbee mini-game, including adaptive parameters and game state tracking.
/// </summary>
/// <remarks>
/// This manager handles dynamic difficulty adjustment through adaptive parameters
/// and maintains shared state information (like target positions) that dog states can access.
/// The adaptive parameters are recalculated at game start based on the initial positions of game entities.
/// </remarks>
public class FrisbeeGameManager : GameManager
{
    /// <summary>
    /// The current target position where the dog is positioned or moving toward.
    /// Used by states like <see cref="GoToPreviousTarget"/> to return the dog to a previous location.
    /// </summary>
    /// <remarks>
    /// Hidden in the Inspector but accessible to other scripts.
    /// Updated by dog states when new positions are established.
    /// </remarks>
   [HideInInspector]
    public Vector3 currentTargetPos = Vector3.zero;

    [SerializeField]
    private GameObject scoreAreaPrefab;

    [SerializeField]
    private Collider dogAreaCollider;

    private readonly List<GameObject> _scoreAreas = new();

    private Transform _playerTransform;

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Initializes the game manager at the start of the game.
    /// </summary>
    /// <remarks>
    /// Unity lifecycle callback invoked before the first frame update.
    /// Calls <see cref="SetAdaptiveParameters"/> to configure initial adaptive parameters
    /// based on the current game state.
    /// </remarks>
    private void Start()
    {   
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (_playerTransform == null){
            Debug.LogError("Player GameObject not found in the scene.");

            return;
        }

        SetAdaptiveParameters();
    }
    
    /// <summary>
    /// Configures all adaptive parameters used throughout the game.
    /// </summary>
    /// <remarks>
    /// Currently sets the "DogDistance" parameter by calculating the initial distance between
    /// the player and dog. This value is used by dog states to determine positioning relative to the player.
    /// Additional adaptive parameters can be added here as the game evolves.
    /// </remarks>
    private void SetAdaptiveParameters()
    {    

        PlayerPrefs.SetFloat("DogDistance", GetPlayerDistanceToDog());
    }

    /// <summary>
    /// Calculates the distance between the player and the dog at game start.
    /// </summary>
    /// <remarks>
    /// Finds GameObjects tagged with "Player" and "Dog" in the scene and calculates
    /// the 3D distance between their positions.
    /// This distance is used as a baseline for adaptive difficulty, determining how far
    /// the dog should position itself during gameplay.
    /// </remarks>
    /// <returns>
    /// The distance in world units between the player and dog, or 0f if either GameObject cannot be found.
    /// </returns>
    private float GetPlayerDistanceToDog()
    {
        GameObject dog = GameObject.FindGameObjectWithTag("Dog");

        if ( dog == null)
        {
            Debug.LogError("Dog GameObject not found in the scene.");
            return 0f;  
        }

        return Vector3.Distance(_playerTransform.position, dog.transform.position);
    }

    public override void IncreaseDifficulty()
    {
        GameObject nearstScoreArea =  _scoreAreas.OrderBy(distanceToPlayer => 
            Vector3.Distance(_playerTransform.position, distanceToPlayer.transform.position))
            .First();

        _scoreAreas.Remove(nearstScoreArea);
    }


    public override void DecreaseDifficulty()
    {
        GameObject newScoreArea = Instantiate(scoreAreaPrefab, GetScoreAreaPosition(), Quaternion.identity);
        _scoreAreas.Add(newScoreArea);
    }
    

   private Vector3 GetScoreAreaPosition()
    {
        float dogDistance = PlayerPrefs.GetFloat("DogDistance", 5f);

        const float MIN_OFFSET = 0.2f;
        const float MAX_OFFSET = 0.5f;
        const float MIN_DISTANCE_BETWEEN_AREAS = 1.0f; 

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        float scoreAreaDistance = dogDistance * Utils.RandomValueInRange(MIN_OFFSET, MAX_OFFSET);

        Vector3 scoreAreaPosition = _playerTransform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * scoreAreaDistance;
        
        scoreAreaPosition.y = dogAreaCollider.bounds.center.y;

        bool isTooClose = _scoreAreas.Any(scoreArea => Vector3.Distance(scoreArea.transform.position, scoreAreaPosition) < MIN_DISTANCE_BETWEEN_AREAS);

        if (!dogAreaCollider.bounds.Contains(scoreAreaPosition) || isTooClose)
        {
            return GetScoreAreaPosition(); 
        }

        _scoreAreas.Add(Instantiate(scoreAreaPrefab, scoreAreaPosition, Quaternion.identity));

        return scoreAreaPosition;
    }
}
