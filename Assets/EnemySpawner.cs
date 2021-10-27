using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameManager _manager;
    [SerializeField] Triangle trianglePrefab;
    private Player _player;
    [SerializeField] private int _maxTriangles = 10;
    private int _numTriangles = 0;
    private List<Triangle> _triangles = new List<Triangle>();
    private float _lastSpawn;

    // Start is called before the first frame update
    void Start()
    {
        _player = _manager.GetPlayer();
        _lastSpawn = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (_numTriangles < _maxTriangles && Time.time - _lastSpawn > 5) {
            Triangle tri = Instantiate(trianglePrefab, Vector3.zero, Quaternion.identity);
            tri.SetPlayer(_player);
            _triangles.Add(tri);
            _lastSpawn = Time.time;
            _numTriangles++;
        }
    }
}
