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

    void LaunchBall()
    {
        float x = Random.Range(0, 2) == 0 ? -1 : 1;
        float y = Random.Range(0, 2) == 0 ? -1 : 1;
        rb.linearVelocity = new Vector2(x, y).normalized * initialSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Basic collision handling (e.g., for paddle and walls)
        // More complex logic can be added here later for blocks
    }

    public float initialSpeed { get { return _initialSpeed; } } // Public getter for initialSpeed

    public void SetSpeed(float newSpeed)
    {
        _initialSpeed = newSpeed;
        rb.linearVelocity = rb.linearVelocity.normalized * _initialSpeed; // Update current velocity
    }
}