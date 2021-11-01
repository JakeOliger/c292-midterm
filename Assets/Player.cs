using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool isMoving = false;
    private bool isSlinging = false;
    private bool killedEnemyInShot = false;
    private float timeStartedMoving = -1;
    private int score = 0;
    [SerializeField] float speedToAutoStop = 0.45f;
    private Vector3 slingStart;
    private Vector3 slingEnd;
    [SerializeField] GameManager _manager;
    [SerializeField] EnemySpawner _enemySpawner;
    [SerializeField] GameObject _playerDeathEffect;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 oldPos = transform.position;
        transform.position = _manager.ConstrainPosition(transform.position);
        // If we hit the edge of the world, don't kill the player, but do stop them
        if (isMoving && oldPos != transform.position) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            isMoving = false;
        }
        
        Camera.main.transform.position = transform.position;
        if (!isMoving) {
            // If not slinging and the mouse button is released
            if (!isSlinging && Input.GetMouseButtonDown(0)) {
                StartSlinging();
            } else if (isSlinging) {
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
            // Time.time - timeStartedMoving accounts for the fact that it takes a few frames
            // for the velocity values to update, so we don't think we immediately stopped moving by mistake
            // Otherwise, we're just waiting until we slow down below speedToAutoStop before fully stopping and
            // checking to see if we've killed anything in that shot.
            } else if (Time.time - timeStartedMoving > 0.1f && rb.velocity.magnitude < speedToAutoStop) {
                StopPlayer();
            }
        }
    }

    // Stops the player and kills the player if they haven't hit anything in that shot
    void StopPlayer() {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isMoving = false;
        if (!killedEnemyInShot)
            KillPlayer();
        killedEnemyInShot = false;
    }

    // Begins "Slinging mode," the click-and-drag process of choosing a direction for the next shot
    void StartSlinging() {
        isSlinging = true;
        slingStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    // Ends slinging, begins shot
    void StopSlinging() {
        slingEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 diff = slingStart - slingEnd;
        Vector2 f = new Vector2(diff.x, diff.y);
        rb.AddForce(f * 100);
        isSlinging = false;
        isMoving = true;
        timeStartedMoving = Time.time;
    }

    // Points player in the direction of the sling
    void UpdateSlingingRotation() {
        Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Rad2Deg * Mathf.Atan2(slingStart.y - currentMousePos.y, slingStart.x - currentMousePos.x);
        transform.eulerAngles = Vector3.forward * angle + Vector3.forward * 90;
    }

    // Kills the player, ends the game
    void KillPlayer() {
        Vector3 pdePos = new Vector3(transform.position.x, transform.position.y, 0);
        Instantiate(_playerDeathEffect, pdePos, Quaternion.identity);
        Destroy(gameObject);
        _manager.GameOver();
    }

    // called when the cube hits the floor
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isMoving) {
            Triangle tri = other.GetComponent<Triangle>();
            if (tri) {
                score += tri.GetBounty();
                _manager.UpdateScoreLabel(score);
                _enemySpawner.KillTriangle(tri);
                killedEnemyInShot = true;
            } else {
                Square sq = other.GetComponent<Square>();
                if (sq) {
                    score += sq.GetBounty();
                    _manager.UpdateScoreLabel(score);
                    _enemySpawner.KillSquare(sq);
                    killedEnemyInShot = true;
                }
            }
        } else {
            KillPlayer();
        }
    }

    public int GetScore() { return score; }
}
