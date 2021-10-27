using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] float _rotationSpeed = 20f;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void SetPlayer(Player player) {
        _player = player;
    }

    void Move() {
        Vector3 playerPos = _player.transform.position;
        Vector3 triPos = transform.position;

        // Hypotenuse and adjacent length of (non-entity) triangle between Triangle enemy and Player
        float h = Mathf.Sqrt(Mathf.Pow(playerPos.x - triPos.x, 2) + Mathf.Pow(playerPos.y - triPos.y, 2));
        float a = playerPos.y - triPos.y;
        float targetRotation = Mathf.Rad2Deg * Mathf.Acos(a / h);

        // Add signs back in that were removed by doing trig
        if (triPos.x < playerPos.x)
            targetRotation *= -1;
        
        float currentRotation = transform.rotation.eulerAngles.z;

        // If the rotation is "close enough," don't bother calculating further.
        if (Mathf.Abs(currentRotation - targetRotation) < 0.3f)
            return;

        // Ensures we're always turning in the most efficient direction
        if (Mathf.Abs(currentRotation - 360 - targetRotation) < Mathf.Abs(currentRotation - targetRotation))
            currentRotation -= 360;

        // Direction of rotation
        int direction = -1;
        if (currentRotation - targetRotation < 0)
            direction = 1;
            
        currentRotation += direction * _rotationSpeed * Time.deltaTime;

        // Finally apply our rotation
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        rb.velocity = transform.up;
    }

    // Sets the transform.rotation to a rotation looking within a 45deg cone of the player
    public void LookNearPlayer() {
        if (!_player) {
            Debug.LogError("Tried to look at a nonexistant player!");
            return;
        }

        Vector3 playerPos = _player.transform.position;
        Vector3 triPos = transform.position;

        // Hypotenuse and adjacent length of (non-entity) triangle between Triangle enemy and Player
        float h = Mathf.Sqrt(Mathf.Pow(playerPos.x - triPos.x, 2) + Mathf.Pow(playerPos.y - triPos.y, 2));
        float a = playerPos.y - triPos.y;
        // Make the new rotation directly towards the player, +/- 45 degrees
        float targetRotation = Mathf.Rad2Deg * Mathf.Acos(a / h) + Random.Range(-22.5f, 22.5f);

        // Add signs back in that were removed by doing trig
        if (triPos.x < playerPos.x)
            targetRotation *= -1;

        transform.rotation = Quaternion.Euler(0, 0, targetRotation);
    }
}
