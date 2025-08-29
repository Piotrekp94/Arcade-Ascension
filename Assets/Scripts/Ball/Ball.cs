using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private float _initialSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        LaunchBall();
    }

    public void LaunchBall()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (rb != null)
        {
            float x = Random.Range(0, 2) == 0 ? -1 : 1;
            float y = Random.Range(0, 2) == 0 ? -1 : 1;
            rb.linearVelocity = new Vector2(x, y).normalized * _initialSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Basic collision handling (e.g., for paddle and walls)
        // More complex logic can be added here later for blocks
    }

}