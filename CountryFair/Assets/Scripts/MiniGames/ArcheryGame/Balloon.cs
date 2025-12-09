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

    private bool popped = false;

    private Renderer balloonRenderer;
    private Color balloonColor;
    [SerializeField] int scoreToAdd;


    private void Awake()
    {
        startY = transform.position.y;

        // Apanha o renderer do balão
        balloonRenderer = GetComponent<Renderer>();

        // Guarda a cor principal do material
        if (balloonRenderer != null && balloonRenderer.material.HasProperty("_Color"))
        {
            balloonColor = balloonRenderer.material.color;
        }
        else
        {
            balloonColor = Color.white; // fallback
        }
    }

    void Update()
    {
        if (canMove)
        {
            float newY = startY + Mathf.Sin(Time.time * moveSpeed) * moveAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    public void Pop()
    {
        popped = true;

        if (popEffect != null)
        {
            // Instancia o efeito
            GameObject fx = Instantiate(popEffect, transform.position, Quaternion.identity);

            // Apanha o ParticleSystem do efeito
            ParticleSystem ps = fx.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                var main = ps.main;
                main.startColor = balloonColor;    // define a cor no particle system
            }
        }

        ArcheryGameManager.Instance.SetScore(scoreToAdd);

        Destroy(gameObject);
    }

    [ContextMenu("Test Pop")]
    public void TestPop()
    {
        Pop();
    }
}
