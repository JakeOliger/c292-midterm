using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameManager _manager;
    [SerializeField] Triangle trianglePrefab;
    private Player _player;
    [SerializeField] private int _cullDistance = 7;
    [SerializeField] private int _cullCooldown = 3;
    [SerializeField] private int _maxTriangles = 10;
    [SerializeField] private int _triangleSpawnCooldown = 5;
    private int _numTriangles = 0;
    private List<Triangle> _triangles = new List<Triangle>();
    private float _freeTriangleSpawns = 0;
    private float _lastSpawn;
    private float _lastCull;
    private float _cameraHeight;
    private float _cameraWidth;

    // Start is called before the first frame update
    void Start()
    {
        _player = _manager.GetPlayer();
        _lastSpawn = _lastCull = Time.time;
        _cameraHeight = Camera.main.orthographicSize;
        _cameraWidth = _cameraHeight * Screen.width / Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (_freeTriangleSpawns > 0 || (_numTriangles < _maxTriangles && Time.time - _lastSpawn > _triangleSpawnCooldown)) {
            SpawnTriangle();
        }

        if (Time.time - _lastCull > _cullCooldown) {
            CullDistantEnemies();
        }
    }

    // Spawns a triangle in a random location around the viewport and aims it roughly at the player
    void SpawnTriangle() {
        Triangle tri = Instantiate(trianglePrefab, GetValidSpawnLocation(), Quaternion.identity);
        tri.SetPlayer(_player);
        tri.LookNearPlayer();
        _triangles.Add(tri);
        _lastSpawn = Time.time;
        _numTriangles++;
        if (_freeTriangleSpawns > 0)
            _freeTriangleSpawns--;
    }

    // Kills all enemies that are outside of a radius of the viewport.
    //
    // Uses a radius instead of a box because I like that better and this only runs every few seconds,
    // so using the more costly sqrt function won't matter much at all
    void CullDistantEnemies() {
        float cornerX = Camera.main.transform.position.x + _cameraWidth;
        float cornerY = Camera.main.transform.position.y + _cameraHeight;
        float pX = _player.transform.position.x;
        float pY = _player.transform.position.y;
        float maxSurvivalDistance = _cullDistance + Mathf.Sqrt(Mathf.Pow(cornerX - pX, 2) + Mathf.Pow(cornerY - pY, 2));

        List<Triangle> toCull = new List<Triangle>();

        // Make a list of who's naughty and who's nice
        foreach (Triangle tri in _triangles) {
            float tX = tri.transform.position.x;
            float tY = tri.transform.position.y;
            float d = Mathf.Sqrt(Mathf.Pow(tX - pX, 2) + Mathf.Pow(tY - pY, 2));
            if (d > maxSurvivalDistance) {
                toCull.Add(tri);
            }
        }

        // Check it twice
        foreach (Triangle tri in toCull) {
            if (_triangles.Remove(tri)) {
                Destroy(tri.gameObject);
                _numTriangles--;
                _freeTriangleSpawns++;
            } else {
                Debug.LogError("Failed to despawn a Triangle!");
            }
        }

        _lastCull = Time.time;
    }

    // Returns a Vector3 containing a random off-but-near-screen location
    Vector3 GetValidSpawnLocation() {
        float minX, maxX;
        float minY, maxY;
        float x, y;
        float camX = Camera.main.transform.position.x;
        float camY = Camera.main.transform.position.y;

        // Put it on the left or right
        if (Random.value < 0.5) {
            maxY = camY + _cameraHeight + 2;
            minY = camY - _cameraHeight - 2;
            // Left
            if (Random.value < 0.5) {
                minX = camX - _cameraWidth - 3;
                maxX = camX - _cameraWidth - 2;
            // Right
            } else {
                minX = camX + _cameraWidth + 2;
                maxX = camX + _cameraWidth + 3;
            }
        // Put it on the top or bottom
        } else {
            maxX = camX + _cameraWidth + 2;
            minX = camX - _cameraWidth - 2;
            // Top
            if (Random.value < 0.5) {
                minY = camY - _cameraHeight - 3;
                maxY = camY - _cameraHeight - 2;
            // Bottom
            } else {
                minY = camY + _cameraHeight + 2;
                maxY = camY + _cameraHeight + 3;
            }
        }

        x = Random.Range(minX, maxX);
        y = Random.Range(minY, maxY);

        return new Vector3(x, y, 0);
    }
}
