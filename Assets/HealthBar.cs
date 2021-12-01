using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private float _startX;
    private float _startWidth;
    private float _health = 100f;
    RectTransform _rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startWidth = _rectTransform.sizeDelta.x;
        _startX = _rectTransform.position.x;

        Debug.Log(_startX);
        Debug.Log(_startWidth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHealth(float health) {
        _health = health;

        if (_health > 100f) health = 100f;
        if (_health < 0f)   health = 0f;

        float newWidth = _startWidth * (_health / 100f);
        float dWidth = _startWidth - newWidth;

        _rectTransform.position = new Vector3(_startX + dWidth / 2, transform.position.y, transform.position.z);
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }
}
