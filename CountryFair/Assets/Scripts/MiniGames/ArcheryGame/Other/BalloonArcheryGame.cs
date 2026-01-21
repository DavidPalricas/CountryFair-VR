using UnityEngine;


[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class BalloonArcheryGame : MonoBehaviour
{ 
    public Colors color = Colors.RED;

    [Header("Movement Settings")]
    [SerializeField]
    private  bool canMove;
    [SerializeField] 
    private float moveSpeed = 1f;
    [SerializeField]
     private float moveAmount = 1f;
    private float startY;

    [Header("Explosion Effect")]
    public GameObject popEffect;

    [SerializeField] 
    private GameObject balloonPrefab;

    private Renderer _renderer;
    private Color _popEffectColor;
    
    [Header("Score Value")]
    [SerializeField]
    private  int scoreValue;


    private BoxCollider _spawnArea;
    private Vector3 _extents;


    public enum Colors
    {
        RED,
        BLUE,
        YELLOW,
    }


    private ArcheryAudioManager _archeryAudioManager;

    private readonly AudioManager.GameSoundEffects popSoundEffect = AudioManager.GameSoundEffects.BALLOON_POP;
    private void Awake()
    {  

        if (popEffect == null)
        {
            Debug.LogError("Pop");
        }

        _spawnArea = GameObject.FindGameObjectWithTag("BalloonSpawn")
            .GetComponent<BoxCollider>();


        _renderer = GetComponent<Renderer>();
        _popEffectColor =  _renderer.material.HasProperty("_Color")
            ? _renderer.material.color
            : Color.white;

        _extents = GetComponent<Collider>().bounds.extents;

        _archeryAudioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ArcheryAudioManager>();

        if (_archeryAudioManager == null)
        {
            Debug.LogError("ArcheryAudioManager not found in the scene or game manager game object");
        }

        startY = transform.position.y;
    }

    private void Update()
    {
        if (canMove)
        {
            Bounds area = _spawnArea.bounds;

            float newY = startY + Mathf.Sin(Time.time * moveSpeed) * moveAmount;

            newY = Mathf.Clamp(
                newY,
                area.min.y + _extents.y,
                area.max.y - _extents.y
            );

            float clampedX = Mathf.Clamp(
                transform.position.x,
                area.min.x + _extents.x,
                area.max.x - _extents.x
            );

            transform.position = new Vector3(
                clampedX,
                newY,
                transform.position.z
            );
            }
    }

    public void Pop()
    {    
        GameObject fx = Instantiate(popEffect, transform.position, Quaternion.identity);
        
        if (fx.TryGetComponent<ParticleSystem>(out var ps))
         {
            ParticleSystem.MainModule main = ps.main;
            main.startColor = _popEffectColor;
        }
        
        SpawnBalloon();

        _archeryAudioManager.PlaySpatialSoundEffect(popSoundEffect, gameObject);

        Destroy(transform.parent.gameObject);
    }

    public void SpawnBalloon()
    {
        Vector3 halfSize = _spawnArea.size * 0.5f;

        Vector3 localRandom = new(
            Random.Range(-halfSize.x + _extents.x, halfSize.x - _extents.x),
            Random.Range(-halfSize.y + _extents.y, halfSize.y - _extents.y),
            Random.Range(-halfSize.z + _extents.z, halfSize.z - _extents.z)
        );

        Vector3 worldPos = _spawnArea.transform.TransformPoint(
            _spawnArea.center + localRandom
        );

        Instantiate(balloonPrefab, worldPos, Quaternion.identity);
    }

    public int GetScoreValue()
    {   

        string colorName = color.ToString().ToLower();

        string colorToScore = PlayerPrefs.GetString("BalloonColorToScore", "RED").ToLower();
         
        return colorName != colorToScore ? 0 : scoreValue;
    }
}
