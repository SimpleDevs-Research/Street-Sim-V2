using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autorotate : MonoBehaviour
{
    public float xSpeed = 0.25f;
    public float ySpeed = 0.25f;
    public float zSpeed = 0.25f;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(
           xSpeed * Time.deltaTime,
           ySpeed * Time.deltaTime,
           zSpeed * Time.deltaTime
      );
    }
}
