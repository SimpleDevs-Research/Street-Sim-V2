using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EVRA.Inputs;

public class GetHeightFromFloor : MonoBehaviour
{
    [SerializeField] private Transform extractHeightFrom = null;
    [SerializeField] private TextMeshProUGUI textbox;
    [SerializeField] private float m_height = 0f;
    public float height {
        get => m_height;
        set {}
    }

    // Update is called once per frame
    void Update() {
        if (extractHeightFrom == null) return;
        m_height = extractHeightFrom.position.y;
        textbox.text = "Height: " + m_height.ToString();
    }
}
