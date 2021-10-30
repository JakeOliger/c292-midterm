using UnityEngine;

public class Square : Enemy
{
    float _lastSpawn;
    [SerializeField] float _spawnCooldown = 3f;
    EnemySpawner _enemySpawner;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        _lastSpawn = Time.time;
        bounty = 35;
    }

    public void SetEnemySpawner(EnemySpawner es) {
        _enemySpawner = es;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - _lastSpawn > _spawnCooldown) {
            Enemy e = _enemySpawner.SpawnEnemy("triangle", transform.position);
            if (e) {
                e.LookRandomly();
                _lastSpawn = Time.time;
            } else {
                Debug.LogError("Square failed to spawn Triangle");
            }
        }
        Move();
    }
}
