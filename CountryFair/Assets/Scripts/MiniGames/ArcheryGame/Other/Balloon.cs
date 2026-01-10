using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] bool canMove;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float moveAmount = 1f;
    private float startY;

    [Header("Explosion Effect")]
    public GameObject popEffect;

    private Renderer balloonRenderer;
    private Color balloonColor;

    [SerializeField] int scoreToAdd;
    [SerializeField] GameObject balloonPrefab;

    private BoxCollider spawnArea;
    private Vector3 balloonExtents;


    private ArcheryAudioManager _archeryAudioManager;

    private AudioManager.GameSoundEffects popSoundEffect = AudioManager.GameSoundEffects.BALLOON_POP;
    private void Awake()
    {
        spawnArea = GameObject.FindGameObjectWithTag("BalloonSpawn")
            .GetComponent<BoxCollider>();

        // Renderer / cor
        balloonRenderer = GetComponent<Renderer>();
        balloonColor = balloonRenderer != null && balloonRenderer.material.HasProperty("_Color")
            ? balloonRenderer.material.color
            : Color.white;

        // Collider do balão (tamanho real)
        Collider balloonCollider = GetComponent<Collider>();
        balloonExtents = balloonCollider.bounds.extents;

        _archeryAudioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ArcheryAudioManager>();

        if (_archeryAudioManager == null)
        {
            Debug.LogError("ArcheryAudioManager not found in the scene or game manager game object");
        }
    }

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        if (!canMove) return;

        Bounds area = spawnArea.bounds;

        float newY = startY + Mathf.Sin(Time.time * moveSpeed) * moveAmount;

        // Clamp em Y
        newY = Mathf.Clamp(
            newY,
            area.min.y + balloonExtents.y,
            area.max.y - balloonExtents.y
        );

        // Clamp em X (caso haja drift / escala / animação)
        float clampedX = Mathf.Clamp(
            transform.position.x,
            area.min.x + balloonExtents.x,
            area.max.x - balloonExtents.x
        );

        transform.position = new Vector3(
            clampedX,
            newY,
            transform.position.z
        );
    }

    public void Pop()
    {
        if (popEffect != null)
        {
            GameObject fx = Instantiate(popEffect, transform.position, Quaternion.identity);
            ParticleSystem ps = fx.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                var main = ps.main;
                main.startColor = balloonColor;
            }
        }

        ArcheryGameManager.Instance.SetScore(scoreToAdd);
        SpawnBalloon();
        _archeryAudioManager.PlaySpatialSoundEffect(popSoundEffect, gameObject);
        Destroy(transform.parent.gameObject);
    }

    public void SpawnBalloon()
    {
        Vector3 halfSize = spawnArea.size * 0.5f;

        Vector3 localRandom = new Vector3(
            Random.Range(-halfSize.x + balloonExtents.x, halfSize.x - balloonExtents.x),
            Random.Range(-halfSize.y + balloonExtents.y, halfSize.y - balloonExtents.y),
            Random.Range(-halfSize.z + balloonExtents.z, halfSize.z - balloonExtents.z)
        );

        Vector3 worldPos = spawnArea.transform.TransformPoint(
            spawnArea.center + localRandom
        );

        Instantiate(balloonPrefab, worldPos, Quaternion.identity);
    }

    [ContextMenu("Test Pop")]
    public void TestPop()
    {
        Pop();
    }
}
