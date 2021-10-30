using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameManager _manager;
    [SerializeField] Enemy trianglePrefab;
    [SerializeField] Enemy squarePrefab;
    private Player _player;
    [SerializeField] private int _cullDistance = 7;
    [SerializeField] private int _cullCooldown = 3;
    [SerializeField] private int _maxTriangles = 10;
    [SerializeField] private int _maxSquares = 10;
    [SerializeField] private int _triangleSpawnCooldown = 5;
    [SerializeField] private int _squareSpawnCooldown = 5;
    [SerializeField] GameObject enemyDeathEffect;
    private bool pauseSpawning = false;
    private int _numTriangles = 0;
    private int _numSquares = 0;
    private List<Enemy> _triangles = new List<Enemy>();
    private List<Enemy> _squares = new List<Enemy>();
    private float _freeTriangleSpawns = 0;
    private float _freeSquareSpawns = 0;
    private float _lastTriangleSpawn;
    private float _lastSquareSpawn;
    private float _lastCull;
    private float _cameraHeight;
    private float _cameraWidth;

    // Start is called before the first frame update
    void Start()
    {
        _player = _manager.GetPlayer();
        _lastTriangleSpawn = _lastCull = Time.time;
        _cameraHeight = Camera.main.orthographicSize;
        _cameraWidth = _cameraHeight * Screen.width / Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseSpawning) {
            bool underTriCount = _numTriangles < _maxTriangles;
            bool freeTris = _freeTriangleSpawns > 0;
            bool triCooldownFinished = Time.time - _lastTriangleSpawn > _triangleSpawnCooldown;
            if (freeTris || (underTriCount && triCooldownFinished)) {
                SpawnEnemy("triangle");
            }

            bool underSqCount = _numSquares < _maxSquares;
            bool freeSqs = _freeSquareSpawns > 0;
            bool sqCooldownFinished = Time.time - _lastSquareSpawn > _squareSpawnCooldown;
            if (freeSqs || (underSqCount && sqCooldownFinished)) {
                SpawnEnemy("square");
            }
        }

        if (Time.time - _lastCull > _cullCooldown) {
            CullDistantEnemies();
        }
    }

    // Returns spawned enemy if successful, null otherwise
    public Enemy SpawnEnemy(string eType) {
        return SpawnEnemy(eType, GetValidSpawnLocation());
    }

    // Returns spawned enemy if successful, null otherwise
    public Enemy SpawnEnemy(string eType, Vector3 location) {
        Enemy prefab = null, e = null;

        eType = eType.ToLowerInvariant();

        bool isTriangle = eType.Equals("triangle");
        bool isSquare = eType.Equals("square");

        if (isTriangle)
            prefab = trianglePrefab;
        else if (isSquare)
            prefab = squarePrefab;

        if (prefab == null) return null;

        e = Instantiate(prefab, location, Quaternion.identity);
        e.SetPlayer(_player);
        e.LookNearPlayer();

        if (isTriangle) {
            _triangles.Add(e);
            _numTriangles++;
            _lastTriangleSpawn = Time.time;
            if (_freeTriangleSpawns > 0)
                _freeTriangleSpawns--;
            return e;
        } else if (isSquare) {
            Square sq = e as Square;
            if (sq != null) {
                sq.SetEnemySpawner(this);
                _squares.Add(sq);
                _numSquares++;
                _lastSquareSpawn = Time.time;
                if (_freeSquareSpawns > 0)
                    _freeSquareSpawns--;
                return sq;
            } else {
                Debug.LogError("Cannot cast Enemy as Square");
                return null;
            }
        }

        return null;
    }

    // Kills all enemies that are outside of a radius of the viewport.
    //
    // Uses a radius instead of a box because I like that better and this only runs every few seconds,
    // so using the more costly sqrt function won't matter much at all
    void CullDistantEnemies() {
        if (_player == null) return;
        
        float cornerX = Camera.main.transform.position.x + _cameraWidth;
        float cornerY = Camera.main.transform.position.y + _cameraHeight;
        float pX = _player.transform.position.x;
        float pY = _player.transform.position.y;
        float maxSurvivalDistance = _cullDistance + Mathf.Sqrt(Mathf.Pow(cornerX - pX, 2) + Mathf.Pow(cornerY - pY, 2));

        List<Enemy> toCull = new List<Enemy>();

        // Make a list of who's naughty and who's nice
        foreach (Enemy tri in _triangles) {
            float tX = tri.transform.position.x;
            float tY = tri.transform.position.y;
            float d = Mathf.Sqrt(Mathf.Pow(tX - pX, 2) + Mathf.Pow(tY - pY, 2));
            if (d > maxSurvivalDistance) {
                toCull.Add(tri);
            }
        }

        // Make a list of who's naughty and who's nice
        foreach (Enemy sq in _squares) {
            float tX = sq.transform.position.x;
            float tY = sq.transform.position.y;
            float d = Mathf.Sqrt(Mathf.Pow(tX - pX, 2) + Mathf.Pow(tY - pY, 2));
            if (d > maxSurvivalDistance) {
                toCull.Add(sq);
            }
        }

        // Check it twice
        foreach (Enemy e in toCull) {
            Triangle t = e as Triangle;
            if (t != null) {
                KillTriangle(t);
                _freeTriangleSpawns++;
                continue;
            }

            Square s = e as Square;
            if (s != null) {
                KillSquare(s);
                _freeSquareSpawns++;
                continue;
            }
        }

        _lastCull = Time.time;
    }

    void KillEnemy(Enemy e) {
        Instantiate(enemyDeathEffect, e.gameObject.transform.position, Quaternion.identity);
        Destroy(e.gameObject);
    }

    public void KillTriangle(Triangle tri) {
        if (_triangles.Remove(tri)) {
            KillEnemy(tri);
            _numTriangles--;
        } else {
            Debug.LogError("Failed to despawn a Triangle!");
        }
    }

    public void KillSquare(Square sq) {
        if (_squares.Remove(sq)) {
            KillEnemy(sq);
            _numSquares--;
        } else {
            Debug.LogError("Failed to despawn a Square!");
        }
    }

    public void KillAllEnemies() {
        while (_triangles.Count > 0) {
            Triangle tri = _triangles[0] as Triangle;
            if (tri != null)
                KillTriangle(tri);
            else
                Debug.LogError("Non-Triangle in _triangles List");
        }

        while (_squares.Count > 0) {
            Square sq = _squares[0] as Square;
            if (sq != null)
                KillSquare(sq);
            else
                Debug.LogError("Non-Square in _squares List");
        }
    }

    public void KillTriangle(GameObject tri_go) {
        Triangle tri = null;
        foreach (Triangle t in _triangles) {
            if (t.gameObject == tri_go) {
                tri = t;
                break;
            }
        }
        KillTriangle(tri);
    }

    public void PauseSpawning() { pauseSpawning = true; }
    public void UnpauseSpawning() { pauseSpawning = false; }

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
