using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class InstructionsUI : MonoBehaviour
{

    [SerializeField]
    private Transform positionTarget = null;
    [SerializeField]
    private Transform lookAtTarget = null;
    private CanvasGroup canvasGroup;
    [SerializeField]
    private TextMeshProUGUI textbox = null;
    
    [SerializeField]
    private float movementSpeed = 1f;
    [SerializeField]
    private AnimationCurve movementMultiplier;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (positionTarget != null) UpdatePosition();
        if (lookAtTarget != null) UpdateRotation();
    }

    private void UpdatePosition() {
        float distance = Vector3.Distance(positionTarget.position, transform.position);
        if (distance <  0.05f) {
            transform.position = positionTarget.position;
            canvasGroup.alpha = 1f;
            return;
        }
        float gradientValue = Mathf.Clamp(distance/2f, 0f, 1f);
        float step = movementSpeed * Time.deltaTime * movementMultiplier.Evaluate(gradientValue);
        transform.position = Vector3.MoveTowards(transform.position, positionTarget.position, step);
        canvasGroup.alpha = 1f - gradientValue;
    }

    private void UpdateRotation() {
        transform.rotation = Quaternion.LookRotation(transform.position - lookAtTarget.position);
    }

    public void SetText(string newText) {
        if (textbox == null) {
            Debug.LogError("Cannot render text onto a nonexisting textbox");
            return;
        }
        textbox.text = newText;
    }
}
