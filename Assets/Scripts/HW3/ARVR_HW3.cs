using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ARVR_HW3 : MonoBehaviour
{
    public InstructionsUI textbox;
    private Vector3 deskStartPos, deskEndPos;

    // Start is called before the first frame update
    void Start() {
        if (textbox == null) return;
        textbox.SetText("First things first: drag your controller on a flat surface to indicate where your desk is.");
    }
}
