using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PhysicsWanderer : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float turnInterval = 2f;
    public float turnJitter = 0.6f;
    public float raycastDistance = 1.3f;
    public LayerMask obstacleMask = ~0;

    Rigidbody _rb;
    Vector3 _dir;
    float _nextTurn;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        PickNewDir();
    }

    void PickNewDir()
    {
        Vector2 p = Random.insideUnitCircle.normalized;
        _dir = new Vector3(p.x, 0f, p.y);
        _nextTurn = Time.time + turnInterval + Random.Range(-turnJitter, turnJitter);
    }

    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, _dir);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 reflect = Vector3.Reflect(_dir, hit.normal);
            _dir = new Vector3(reflect.x, 0f, reflect.z).normalized;
            _nextTurn = Time.time + 0.25f;
        }
        else if (Time.time >= _nextTurn)
        {
            PickNewDir();
        }

        Vector3 next = _rb.position + _dir * speed * Time.fixedDeltaTime;
        _rb.MovePosition(next);
        if (_dir.sqrMagnitude > 0.0001f)
        {
            Quaternion face = Quaternion.LookRotation(_dir, Vector3.up);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, face, 10f * Time.fixedDeltaTime));
        }
    }

    void OnCollisionEnter(Collision c)
    {
        Vector3 n = c.contacts[0].normal;
        Vector3 reflect = Vector3.Reflect(_dir, n);
        _dir = new Vector3(reflect.x, 0f, reflect.z).normalized;
        _nextTurn = Time.time + 0.2f;
    }
}
