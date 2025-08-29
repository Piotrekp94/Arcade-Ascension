using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPaddle : MonoBehaviour
{
    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private float minX = -7.5f; // Example boundary, adjust as needed
    [SerializeField]
    private float maxX = 7.5f;  // Example boundary, adjust as needed

    void Update()
    {
        // Read input from the new Input System
        float horizontalInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput = -1f;
        }
        else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput = 1f;
        }

        Vector2 position = transform.position;
        position.x += horizontalInput * _speed * Time.deltaTime; // Use _speed here
        position.x = Mathf.Clamp(position.x, minX, maxX); // Clamp position within boundaries
        transform.position = position;
    }

    public float speed { get { return _speed; } } // Public getter for speed

    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }

    public void SetSize(float newSize)
    {
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(newSize, currentScale.y, currentScale.z);
    }
}