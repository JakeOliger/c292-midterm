using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private GameObject background;
    Vector3 _gameBounds;
    Vector3 _gameCenter;
    private float _cameraHeight;
    private float _cameraWidth;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

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
