using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private GameObject background;
    [SerializeField] private EnemySpawner _enemySpawner;
    Vector3 _gameBounds;
    Vector3 _gameCenter;
    private float _cameraHeight;
    private float _cameraWidth;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    [SerializeField] TextMeshProUGUI _scoreLabel;
    [SerializeField] GameObject _gameOverCanvas;
    private bool _isGameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = background.gameObject.GetComponent<Renderer>();
        
        _gameBounds = renderer.bounds.size;
        _gameCenter = renderer.bounds.center;

        _cameraHeight = Camera.main.orthographicSize;
        _cameraWidth = _cameraHeight * Screen.width / Screen.height;

        minX = _gameCenter.x - _gameBounds.x / 2;
        maxX = _gameCenter.x + _gameBounds.x / 2;
        minY = _gameCenter.y - _gameBounds.y / 2;
        maxY = _gameCenter.y + _gameBounds.y / 2;
    }

    void Update() {
        if (_isGameOver) {
            if (Input.GetKeyDown("space")) {
                _isGameOver = false;
                _gameOverCanvas.SetActive(false);
                SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            }
        }
    }

    public void UpdateScoreLabel(int score) {
        _scoreLabel.text = "Score: " + score;
    }

    public void UpdateScoreLabel() {
        _scoreLabel.text = "Score: " + _player.GetScore();
    }

    public void GameOver() {
        UpdateScoreLabel();
        _isGameOver = true;
        _enemySpawner.KillAllEnemies();
        _enemySpawner.PauseSpawning();
        _gameOverCanvas.SetActive(true);
    }

    public Vector3 ConstrainPosition(Vector3 position) {
        float x = position.x;
        float y = position.y;
        
        if (x < minX + _cameraWidth) {
            x = minX + _cameraWidth;
        } else if (x > maxX - _cameraWidth) {
            x = maxX - _cameraWidth;
        }

        if (y < minY + _cameraHeight) {
            y = minY + _cameraHeight;
        } else if (y > maxY - _cameraHeight) {
            y = maxY - _cameraHeight;
        }

        return new Vector3(x, y, position.z);
    }

    public Player GetPlayer() {
        return _player;
    }
}
