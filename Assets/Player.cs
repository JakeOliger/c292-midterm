using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool isMoving = false;
    private bool isSlinging = false;
    private float timeStartedMoving = -1;
    [SerializeField] float speedToAutoStop = 0.45f;
    private Vector3 slingStart;
    private Vector3 slingEnd;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.position = transform.position;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (!isMoving) {
            // If not slinging and the mouse button is released
            if (!isSlinging && Input.GetMouseButtonDown(0)) {
                isSlinging = true;
                Debug.Log("Starting sling");
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
            Debug.Log(rb.velocity.magnitude);
            if (Time.time - timeStartedMoving > 0.1f && rb.velocity.magnitude < speedToAutoStop) {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                isMoving = false;
            }
        }
    }
}