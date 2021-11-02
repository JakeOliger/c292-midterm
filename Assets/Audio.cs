using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour
{
    float _timeCreated;
    AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource) {
            Destroy(gameObject);
            return;
        }

        _audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_audioSource.isPlaying)
            Destroy(gameObject);
    }
}
