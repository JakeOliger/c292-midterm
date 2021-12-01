using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool _isMoving = false;
    private bool _isSlinging = false;
    private bool _killedEnemyInShot = false;
    private float _timeStartedMoving = -1;
    private int _score = 0;
    private bool gameHasStarted = false;
    [SerializeField] float _speedToAutoStop = 0.45f;
    [SerializeField] float _damageRate = 25f;
    [SerializeField] float _regenRate = 10f;
    [SerializeField] float _regenCooldown = 5f;
    private Vector3 _slingStart;
    private Vector3 _slingEnd;
    [SerializeField] GameManager _manager;
    [SerializeField] EnemySpawner _enemySpawner;
    [SerializeField] GameObject _playerDeathEffect;
    [SerializeField] GameObject _playerDeathSound;
    [SerializeField] HealthBar _healthBar;
    [SerializeField] bool _playerImmune = false;
    private AudioSource _pewSound;
    private Rigidbody2D _rb;
    private float _health = 100;
    bool _isTakingDamage = false;
    private float _lastDamageTaken;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _pewSound = GetComponent<AudioSource>();
        _lastDamageTaken = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameHasStarted)
            return;

        Vector3 oldPos = transform.position;
        transform.position = _manager.ConstrainPosition(transform.position);
        // If we hit the edge of the world, don't kill the player, but do stop them
        if (_isMoving && oldPos != transform.position) {
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _isMoving = false;
        }
        
        Vector3 newCameraPosition = new Vector3(transform.position.x, transform.position.y, 0);
        Camera.main.transform.position = newCameraPosition;

        if (!_isMoving) {
            // If not slinging and the mouse button is released
            if (!_isSlinging && Input.GetMouseButtonDown(0)) {
                StartSlinging();
            } else if (_isSlinging) {
                // If the mouse button is released
                if (Input.GetMouseButtonUp(0)) {
                    StopSlinging();
                // If we're in the middle of slinging, keep the rotation updated
                } else {
                    UpdateSlingingRotation();
                }
            }
        // If we're moving
        } else {
            if (Input.GetMouseButtonDown(0)) {
                StartSlinging();
                StopPlayer();
            // Time.time - _timeStartedMoving accounts for the fact that it takes a few frames
            // for the velocity values to update, so we don't think we immediately stopped moving by mistake
            // Otherwise, we're just waiting until we slow down below _speedToAutoStop before fully stopping and
            // checking to see if we've killed anything in that shot.
            } else if (Time.time - _timeStartedMoving > 0.1f && _rb.velocity.magnitude < _speedToAutoStop) {
                StopPlayer();
            }
        }

        // Inflict damage or regeneration; update the health bar if needed
        float dHealth = 0;
        if (_isTakingDamage) {
            dHealth = -_damageRate * Time.deltaTime;
            _lastDamageTaken = Time.time;
        } else if (Time.time - _lastDamageTaken > _regenCooldown) {
            dHealth = _regenRate * Time.deltaTime;
        }

        if (dHealth != 0) {
            _health += dHealth;
            if (_health < 0f) {
                _health = 0f;
                KillPlayer();
            }
            if (_health > 100f) _health = 100f;
            _healthBar.SetHealth(_health);
        }
    }

    // Stops the player and kills the player if they haven't hit anything in that shot
    void StopPlayer() {
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _isMoving = false;
        if (!_killedEnemyInShot)
            KillPlayer();
        _killedEnemyInShot = false;
    }

    // Begins "Slinging mode," the click-and-drag process of choosing a direction for the next shot
    void StartSlinging() {
        _isSlinging = true;
        _slingStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void AlertGameHasStarted() {
        gameHasStarted = true;
    }

    // Ends slinging, begins shot
    void StopSlinging() {
        _slingEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 diff = _slingStart - _slingEnd;
        Vector2 f = new Vector2(diff.x, diff.y);
        _rb.AddForce(f * 100);
        _isSlinging = false;
        _isMoving = true;
        _timeStartedMoving = Time.time;
        if (_pewSound)
            _pewSound.Play();
        else
            Debug.LogError("Couldn't play pew sound");
    }

    // Points player in the direction of the sling
    void UpdateSlingingRotation() {
        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Rad2Deg * Mathf.Atan2(_slingStart.y - currentMousePos.y, _slingStart.x - currentMousePos.x);
        transform.eulerAngles = Vector3.forward * angle + Vector3.forward * 90;
    }

    // Kills the player, ends the game
    void KillPlayer() {
        if (_playerImmune)
            return;
        Vector3 pdePos = new Vector3(transform.position.x, transform.position.y, 0);
        Instantiate(_playerDeathEffect, pdePos, Quaternion.identity);
        Instantiate(_playerDeathSound, pdePos, Quaternion.identity);
        Destroy(gameObject);
        _manager.GameOver();
    }

    // Called when the player collides with an enemy (or vice versa, I suppose)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isMoving) {
            Triangle tri = other.GetComponent<Triangle>();
            if (tri) {
                _score += tri.GetBounty();
                _manager.UpdateScoreLabel(_score);
                _enemySpawner.KillTriangle(tri);
                _killedEnemyInShot = true;
            } else {
                Square sq = other.GetComponent<Square>();
                if (sq) {
                    _score += sq.GetBounty();
                    _manager.UpdateScoreLabel(_score);
                    _enemySpawner.KillSquare(sq);
                    _killedEnemyInShot = true;
                } else {
                    Circle cir = other.GetComponent<Circle>();
                    if (cir) {
                        _score += cir.GetBounty();
                        _manager.UpdateScoreLabel(_score);
                        _enemySpawner.KillCircle(cir);
                        _killedEnemyInShot = true;
                    }
                }
            }
        } else {
            KillPlayer();
        }
    }

    public int GetScore() { return _score; }
    public void SetIsTakingDamage(bool isTakingDamage) { _isTakingDamage = isTakingDamage; }
}
