using UnityEngine;

public class KeyboardGreetDebug : MonoBehaviour
{
    [Tooltip("Assign the SimpleGreetController on the avatar you want to test. If left empty, will try to find one on this GameObject.")]
    public SimpleGreetController target;

    public KeyCode key = KeyCode.Space;

    void Awake()
    {
        if (!target)
            target = GetComponent<SimpleGreetController>();
    }

    void Update()
    {
        if (target && Input.GetKeyDown(key))
        {
            target.TriggerOnce();
        }
    }
}
