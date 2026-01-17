using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class ScoreAreaExtraSettings : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] 
    private float moveSpeed = 2f;
    
    [SerializeField] 
    private float radius = 0.5f;

    [Header("Visibility Settings")]
    [SerializeField] 
    private float timeVisible = 3f;  
    [SerializeField] 
    private float timeInvisible = 2f; 
    public bool IsMovable { get; set; } = false;
    public bool CanChangeVisibility { get; set; } = false;

    private Renderer _renderer;

    private Collider _collider;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (IsMovable)
        {
             StartMoving();
        }

        if (CanChangeVisibility)
        {
             StartBlinking();
        }
    }

    private void StartMoving()
    {
        Vector3[] circularPath = GetCircularWaypoints(radius, 8);

        transform.DOPath(circularPath, moveSpeed, PathType.CatmullRom)
            .SetOptions(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

   private void StartBlinking()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.AppendInterval(timeVisible);

        sequence.AppendCallback(() => { 
                                    _renderer.enabled = false;
                                    _collider.enabled = false; 
                                });
        sequence.AppendInterval(timeInvisible);

        sequence.AppendCallback(() => { 
                                    _renderer.enabled = true; 
                                    _collider.enabled = true; 
                                });
       
        const int INFINITE_LOOPS = -1;

        sequence.SetLoops(INFINITE_LOOPS);
    }

    private Vector3[] GetCircularWaypoints(float r, int steps)
    {
        Vector3[] points = new Vector3[steps];

        // Converting degrees to radians for calculation 360 degrees is 2*PI radians
        float angleStep = 2 * Mathf.PI / steps;
        
        Vector3 center = transform.position + (transform.right * r); 
        
        for (int i = 0; i < steps; i++)
        {
            float angle = i * angleStep + Mathf.PI; 
            float x = center.x + r * Mathf.Cos(angle);
            float z = center.z + r * Mathf.Sin(angle);
            
            points[i] = new Vector3(x, transform.position.y, z);
        }

        return points;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}