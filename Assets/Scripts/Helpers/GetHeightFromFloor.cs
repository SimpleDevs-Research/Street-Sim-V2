using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GetHeightFromFloor : MonoBehaviour
{
    [SerializeField] private Transform extractHeightFrom = null;
    [SerializeField] private TextMeshProUGUI textbox;

    // Update is called once per frame
    void Update() {
        if (extractHeightFrom == null) return;
        float y = extractHeightFrom.position.y;
        textbox.text = "Height: " + y.ToString();
    }
}
