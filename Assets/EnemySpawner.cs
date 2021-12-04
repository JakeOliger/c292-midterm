using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * EnemySpawner
 *
 * So the EnemySpawner is really more of an EnemyManager. It handles
 * the spawning of new enemies and also controls despawning, checking
 * if they're too far from the player to stay alive, as well as
 * some of the enemy logic such as the circle lattice damage mechanic.
 * It could use some refactoring, but for now... it works!
 */
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameManager _manager;
    [SerializeField] Enemy trianglePrefab;
    [SerializeField] Enemy squarePrefab;
    [SerializeField] Enemy circlePrefab;
    [SerializeField] LineRenderer linePrefab;
    private Player _player;
    // Distance off-screen that enemies begin de-spawning and re-spawning closer to the player
    [SerializeField] private int _cullDistance = 10;
    [SerializeField] private int _cullCooldown = 3;
    [SerializeField] private int _maxTriangles = 10;
    [SerializeField] private int _maxSquares = 10;
    [SerializeField] private int _maxCircles = 10;
    [SerializeField] private int _triangleSpawnCooldown = 5;
    [SerializeField] private int _squareSpawnCooldown = 5;
    [SerializeField] private int _circleSpawnCooldown = 5;
    [SerializeField] GameObject _enemyDeathEffect;
    [SerializeField] GameObject _enemyDeathSound;
    private bool pauseSpawning = true;
    private int _numTriangles = 0;
    private int _numSquares = 0;
    private int _numCircles = 0;
    private List<Enemy> _triangles = new List<Enemy>();
    private List<Enemy> _squares = new List<Enemy>();
    private List<Enemy> _circles = new List<Enemy>();
    private float _freeTriangleSpawns = 0;
    private float _freeSquareSpawns = 0;
    private float _freeCircleSpawns = 0;
    private float _lastTriangleSpawn;
    private float _lastSquareSpawn;
    private float _lastCircleSpawn;
    private float _lastCull;
    private float _cameraHeight;
    private float _cameraWidth;

    // Start is called before the first frame update
    void Start()
    {
        _player = _manager.GetPlayer();
        ResetSpawnTimers();
        _cameraHeight = Camera.main.orthographicSize;
        _cameraWidth = _cameraHeight * Screen.width / Screen.height;
    }

    public void ResetSpawnTimers() {
        _lastTriangleSpawn = _lastSquareSpawn = _lastCircleSpawn = _lastCull = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseSpawning) {
            // Could maybe improve this by having there be a certain percent chance for a spawn,
            // or having a range of times that the spawns could randomly happen in.
            // As it is, the spawns feel very robotic.
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

            bool underCirCount = _numCircles < _maxCircles;
            bool freeCirs = _freeCircleSpawns > 0;
            bool cirCooldownFinished = Time.time - _lastCircleSpawn > _circleSpawnCooldown;
            if (freeCirs || (underCirCount && cirCooldownFinished)) {
                SpawnEnemy("circle");
            }
        }

        if (Time.time - _lastCull > _cullCooldown) {
            CullDistantEnemies();
        }

        // This could be made more efficient by treating each cluster of circle connections
        // as an undirected graph and doing a breadth-first-search style iteration through
        // each graph, but this works for now with the way connections are implemented
        bool hasHitPlayer = false;
        List<int> visited = new List<int>();
        foreach (Circle c in _circles) {
            if (visited.Contains(c.GetInstanceID())) continue;
            visited.Add(c.GetInstanceID());

            List<Circle> connections = c.GetConnections();
            foreach (Circle conn in connections) {
                Vector2 direction = (conn.transform.position - c.transform.position).normalized;
                float distance = Mathf.Sqrt(Mathf.Pow(conn.transform.position.x - c.transform.position.x, 2) + Mathf.Pow(conn.transform.position.y - c.transform.position.y, 2));
                RaycastHit2D hit = Physics2D.Raycast(c.transform.position, direction, distance, LayerMask.GetMask("Player"));
                Debug.DrawRay(c.transform.position, direction, Color.red, 0.5f);
                if (hit.collider != null) {
                    hasHitPlayer = true;
                    break;
                }
            }
            if (hasHitPlayer) break;
        }

        _player.SetIsTakingDamage(hasHitPlayer);
    }

    // Returns spawned enemy if successful, null otherwise
    public Enemy SpawnEnemy(string eType) {
        Enemy e = SpawnEnemy(eType, GetValidSpawnLocation());
        if (e == null) {
            Debug.LogError("Error spawning enemy of type '" + eType + "'");
        }
        return e;
    }

    // Returns spawned enemy if successful, null otherwise
    public Enemy SpawnEnemy(string eType, Vector3 location) {
        Enemy prefab = null, e = null;

        eType = eType.ToLowerInvariant();

        bool isTriangle = eType.Equals("triangle");
        bool isSquare = eType.Equals("square");
        bool isCircle = eType.Equals("circle");

        if (isTriangle)
            prefab = trianglePrefab;
        else if (isSquare)
            prefab = squarePrefab;
        else if (isCircle)
            prefab = circlePrefab;

        if (prefab == null) return null;

        e = Instantiate(prefab, location, Quaternion.identity);
        e.SetPlayer(_player);
        e.SetEnemySpawner(this);
        e.LookNearPlayer();

        if (isTriangle) {
            return SpawnTriangle(e);
        } else if (isSquare) {
            return SpawnSquare(e);
        } else if (isCircle) {
            return SpawnCircle(e);
        } else {
            return null;
        }
    }

    Enemy SpawnTriangle(Enemy e) {
        _triangles.Add(e);
        _numTriangles++;
        _lastTriangleSpawn = Time.time;
        if (_freeTriangleSpawns > 0)
            _freeTriangleSpawns--;
        return e;
    }

    Enemy SpawnSquare(Enemy e) {
        Square sq = e as Square;
        if (sq != null) {
            _squares.Add(sq);
            _numSquares++;
            _lastSquareSpawn = Time.time;
            if (_freeSquareSpawns > 0)
                _freeSquareSpawns--;
        }
        return sq;
    }

    Enemy SpawnCircle(Enemy e) {
        Circle cir = e as Circle;
        if (cir != null) {
            cir.SetLineRendererPrefab(linePrefab);
            foreach (Circle c in _circles) {
                cir.AddConnection(c);
            }
            _circles.Add(cir);
            _numCircles++;
            _lastCircleSpawn = Time.time;
            if (_freeCircleSpawns > 0)
                _freeCircleSpawns--;
        }
        return cir;
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

        // Make a list of who's naughty and who's nice
        foreach (Enemy cir in _circles) {
            float tX = cir.transform.position.x;
            float tY = cir.transform.position.y;
            float d = Mathf.Sqrt(Mathf.Pow(tX - pX, 2) + Mathf.Pow(tY - pY, 2));
            if (d > maxSurvivalDistance) {
                toCull.Add(cir);
            }
        }

        // Check it twice
        foreach (Enemy e in toCull) {
            Triangle t = e as Triangle;
            if (t != null) {
                KillTriangle(t, true);
                _freeTriangleSpawns++;
                continue;
            }

            Square s = e as Square;
            if (s != null) {
                KillSquare(s, true);
                _freeSquareSpawns++;
                continue;
            }

            Circle c = e as Circle;
            if (c != null) {
                KillCircle(c, true);
                _freeCircleSpawns++;
                continue;
            }
        }

        _lastCull = Time.time;
    }

    // Should only be called by KillTriangle and KillSquare
    void KillEnemy(Enemy e, bool silently=false) {
        Instantiate(_enemyDeathEffect, e.gameObject.transform.position, Quaternion.identity);
        if (!silently) {
            Instantiate(_enemyDeathSound, e.gameObject.transform.position, Quaternion.identity);
        }
        Destroy(e.gameObject);
        Destroy(e);
    }

    public void KillTriangle(Triangle tri, bool silently=false) {
        if (_triangles.Remove(tri)) {
            KillEnemy(tri, silently);
            _numTriangles--;
        } else {
            Debug.LogError("Failed to despawn a Triangle!");
        }
    }

    public void KillSquare(Square sq, bool silently=false) {
        if (_squares.Remove(sq)) {
            KillEnemy(sq, silently);
            _numSquares--;
        } else {
            Debug.LogError("Failed to despawn a Square!");
        }
    }

    public void KillCircle(Circle cir, bool silently=false) {
        if (_circles.Remove(cir)) {
            cir.EmptyConnections();
            KillEnemy(cir, silently);
            _numCircles--;
        } else {
            Debug.LogError("Failed to despawn a Circle!");
        }
    }

    public void KillAllEnemies() {
        Instantiate(_enemyDeathSound, gameObject.transform.position, Quaternion.identity);

        while (_triangles.Count > 0) {
            Triangle tri = _triangles[0] as Triangle;
            if (tri != null)
                KillTriangle(tri, true);
            else
                Debug.LogError("Non-Triangle in _triangles List");
        }

        while (_squares.Count > 0) {
            Square sq = _squares[0] as Square;
            if (sq != null)
                KillSquare(sq, true);
            else
                Debug.LogError("Non-Square in _squares List");
        }

        while (_circles.Count > 0) {
            Circle cir = _circles[0] as Circle;
            if (cir != null)
                KillCircle(cir, true);
            else
                Debug.LogError("Non-Circle in _circles List");
        }
    }

    public void PauseSpawning() { pauseSpawning = true; }
    public void UnpauseSpawning() {
        pauseSpawning = false;
        ResetSpawnTimers();
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

        return new Vector3(x, y, transform.position.z);
    }
}
