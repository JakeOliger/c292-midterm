using UnityEngine;

public class Triangle : Enemy
{
    private bool _flyingLeft = false;
    private float _lastDirectionChange;
    [SerializeField] private float _minDirectionChangeCooldown = 2f;
    [SerializeField] private float _maxDirectionChangeCooldown = 4f;
    private float _directionChangeCooldown;
    [SerializeField] private int _minTotalDirectionChanges = 1;
    [SerializeField] private int _maxTotalDirectionChanges = 5;
    private int _totalDirectionChanges;
    private int _numDirectionChanges = 0;
    private float _playerDistanceCheckCooldown = 1f;
    private float _lastPlayerDistanceCheck;
    private bool _nearPlayer = true;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        bounty = 10;
        _flyingLeft = Random.value < 0.5;
        _lastDirectionChange = Time.time;
        _directionChangeCooldown = Random.Range(_minDirectionChangeCooldown, _maxDirectionChangeCooldown);
        _totalDirectionChanges = Random.Range(_minTotalDirectionChanges, _maxTotalDirectionChanges + 1);
        _lastPlayerDistanceCheck = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // Every few seconds, check how far we are from the player and if we're far, just
        // try to get closer before doing weird behavioral movement
        if (Time.time - _lastPlayerDistanceCheck > _playerDistanceCheckCooldown) {
            Vector3 playerPos = _player.transform.position;
            float distance = Mathf.Sqrt(Mathf.Pow(playerPos.x - transform.position.x, 2)
                + Mathf.Pow(playerPos.y - transform.position.y, 2));
            _nearPlayer = distance < 7f;
            _lastPlayerDistanceCheck = Time.time;
        }

        if (!_nearPlayer || _numDirectionChanges >= _totalDirectionChanges) {
            Move();
        } else {
            MoveRandomly();
        }
    }

    void MoveRandomly() {
        if (Time.time - _lastDirectionChange > _directionChangeCooldown) {
            _flyingLeft = !_flyingLeft;
            _lastDirectionChange = Time.time;
            _numDirectionChanges++;
        }

        // Direction of rotation
        int direction = _flyingLeft ? -1 : 1;
        float rotation = transform.rotation.eulerAngles.z + direction * _rotationSpeed * Time.deltaTime;

        // Finally apply our rotation and ensure velocity is kept up
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        rb.velocity = transform.up * _movementSpeed;
    }
}
