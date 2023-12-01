using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class StreetSimRandomSound : MonoBehaviour
{
    
    private AudioSource _as;
    public Vector2 timeRange = new Vector2(1f, 3f);
    [SerializeField] private float waitTime = 1f;
    private IEnumerator waitCoroutine;

    private void Awake() {
        _as = GetComponent<AudioSource>();
        waitCoroutine = WaitCoroutine();
        timeRange.x = Mathf.Max(0f, timeRange.x);
        timeRange.y = Mathf.Max(timeRange.x, timeRange.y);
        waitTime = CalculateWaitTime();
    }

    private void Start() {
        StartCoroutine(waitCoroutine);
    }

    private float CalculateWaitTime() {
        return Random.Range(timeRange.x, timeRange.y);
    }

    private IEnumerator WaitCoroutine() {
        while(true) {
            yield return new WaitForSeconds(waitTime);
            _as.Play();
            waitTime = CalculateWaitTime();
        }
    }

    void OnDisable() {
        StopCoroutine(waitCoroutine);
    }
}
