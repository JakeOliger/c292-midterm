using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool isMoving = false;
    private bool isSlinging = false;
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
        if (isMoving && oldPos != transform.position) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            isMoving = false;
        }
        
        Camera.main.transform.position = transform.position;
        if (!isMoving) {
            // If not slinging and the mouse button is released
            if (!isSlinging && Input.GetMouseButtonDown(0)) {
                isSlinging = true;
                slingStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            } else if (isSlinging) {
                // If the mouse button is pressed
                if (Input.GetMouseButtonUp(0)) {
                    slingEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 diff = slingStart - slingEnd;
                    Vector2 f = new Vector2(diff.x, diff.y);
                    rb.AddForce(f * 100);
                    isSlinging = false;
                    isMoving = true;
                    timeStartedMoving = Time.time;
                // If we're in the middle of slinging, keep the rotation updated
                } else {
                    Vector3 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    float angle = Mathf.Rad2Deg * Mathf.Atan2(slingStart.y - currentMousePos.y, slingStart.x - currentMousePos.x);
                    transform.eulerAngles = Vector3.forward * angle + Vector3.forward * 90;
                }
            }
        } else {
            if (Time.time - timeStartedMoving > 0.1f && rb.velocity.magnitude < speedToAutoStop) {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                isMoving = false;
            }
        }
    }

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
            }
        } else {
            KillPlayer();
        }
    }

    public int GetScore() { return score; }
}
