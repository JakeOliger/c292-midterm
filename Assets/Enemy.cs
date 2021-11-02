using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected Player _player;
    [SerializeField] protected float _movementSpeed = 1f;
    [SerializeField] protected float _rotationSpeed = 20f;
    protected int bounty = 0;
    protected Rigidbody2D rb;

    protected virtual void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetPlayer(Player player) {
        _player = player;
    }

    public int GetBounty() { return bounty; }

    // Sets the transform.rotation to a rotation looking within a 45deg cone of the player
    public void LookNearPlayer() {
        if (!_player) {
            Debug.LogError("Tried to look at a nonexistant player!");
            return;
        }

        Vector3 playerPos = _player.transform.position;
        Vector3 myPos = transform.position;

        // Hypotenuse and adjacent length of (non-entity) triangle between Triangle enemy and Player
        float h = Mathf.Sqrt(Mathf.Pow(playerPos.x - myPos.x, 2) + Mathf.Pow(playerPos.y - myPos.y, 2));
        float a = playerPos.y - myPos.y;
        // Make the new rotation directly towards the player, +/- 45 degrees
        float targetRotation = Mathf.Rad2Deg * Mathf.Acos(a / h) + Random.Range(-22.5f, 22.5f);

        // Add signs back in that were removed by doing trig
        if (myPos.x < playerPos.x)
            targetRotation *= -1;

        transform.rotation = Quaternion.Euler(0, 0, targetRotation);
    }

    // Sets the enemy rotation to a random direction
    public void LookRandomly() {
        if (!_player) {
            Debug.LogError("Tried to look at a nonexistant player!");
            return;
        }

        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
    }

    protected void Move() {
        if (!_player) {
            Debug.LogError("Player does not exist while trying to move enemy");
            return;
        }

        Vector3 playerPos = _player.transform.position;
        Vector3 myPos = transform.position;

        // Hypotenuse and adjacent between enemy and Player
        float h = Mathf.Sqrt(Mathf.Pow(playerPos.x - myPos.x, 2) + Mathf.Pow(playerPos.y - myPos.y, 2));
        float a = playerPos.y - myPos.y;
        float targetRotation = Mathf.Rad2Deg * Mathf.Acos(a / h);

        // Add signs back in that were removed by doing trig
        if (myPos.x < playerPos.x)
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

        // Finally apply our rotation and ensure velocity is kept up
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
        rb.velocity = transform.up * _movementSpeed;
    }
}
