using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVRA.Inputs;

public class MoveWithJoystick : MonoBehaviour
{

    [SerializeField]
    private float movementSpeed = 0.1f;

    private Vector2 joystickOrientation = Vector2.zero;

    public void SetPosition(Vector3 startPos) {
        transform.position = startPos;
    }

    private void Update() {
        Vector2 movementDisplacement = joystickOrientation * movementSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x + movementDisplacement.x, 
            transform.position.y + movementDisplacement.y,
            transform.position.z
        );
    }

    public void MoveObject(InputEventDataPackage dataPackage) {
        joystickOrientation = dataPackage.inputs[InputType.Thumbstick].response.direction;
    }
}
