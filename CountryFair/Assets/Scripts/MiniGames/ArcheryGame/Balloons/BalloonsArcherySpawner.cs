using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BalloonsArcherySpawner : MonoBehaviour
{   
    [Header("Ballon Prefabs")]
    [SerializeField]
    private GameObject redBalloonPrefab;

    [SerializeField]
    private GameObject blueBalloonPrefab;

     [SerializeField]
    private GameObject yellowBalloonPrefab;

    private BoxCollider _spawnArea;

    private void Awake()
    {
        _spawnArea = GetComponent<BoxCollider>();
    }


    public void SpawnBalloon()
    {
        Vector3 halfSize = _spawnArea.size * 0.5f;
       
       /*
        Vector3 localRandom = new(
            Random.Range(-halfSize.x + _extents.x, halfSize.x - _extents.x),
            Random.Range(-halfSize.y + _extents.y, halfSize.y - _extents.y),
            Random.Range(-halfSize.z + _extents.z, halfSize.z - _extents.z)
        );

        Vector3 worldPos = _spawnArea.transform.TransformPoint(
            _spawnArea.center + localRandom
        );

        Instantiate(balloonPrefab, worldPos, Quaternion.identity);
        */
    }
}