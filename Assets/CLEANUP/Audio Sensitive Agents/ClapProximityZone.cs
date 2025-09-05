using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ClapProximityZone : MonoBehaviour
{
    [Tooltip("Sound source (if empty, use this object's position)")]
    public Transform soundSource;

    [Tooltip("Avatar (root) to detect. If empty, this zone becomes 'multi-user'.")]
    public Transform avatar;

    [HideInInspector] public bool avatarInside = false;
    Transform _current;

    public float WorldRadius
    {
        get
        {
            var col = GetComponent<SphereCollider>();
            if (!col) return 0f;
            float maxAxis = Mathf.Max(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.y),
                Mathf.Abs(transform.lossyScale.z)
            );
            return col.radius * maxAxis;
        }
    }

    public Vector3 SoundPosition => soundSource ? soundSource.position : transform.position;

    public bool ContainsPosition(Vector3 worldPos)
    {
        float R = WorldRadius;
        Vector3 p = SoundPosition;
        return (worldPos - p).sqrMagnitude <= R * R;
    }
    public bool Contains(Transform t) => t && ContainsPosition(t.position);

    void Reset()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var tr = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        if (avatar)
        {
            if (tr == avatar || tr.IsChildOf(avatar))
            { avatarInside = true; _current = tr; }
        }
        else
        {
            avatarInside = true; _current = tr;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var tr = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        if (_current != null && (tr == _current || tr.IsChildOf(_current)))
        { avatarInside = false; _current = null; }
    }

    void OnDisable() { avatarInside = false; _current = null; }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<SphereCollider>();
        if (!col) return;

        float R = WorldRadius;
        var prev = Gizmos.color;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.15f);
        Gizmos.DrawSphere(transform.position, R);

        UnityEditor.Handles.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, R);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.1f, $"World R ≈ {R:F2} m");
        Gizmos.color = prev;
    }
#endif
}
