using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class WanderAgent : MonoBehaviour
{
    [Header("Wander")]
    public float wanderRadius = 8f;
    public float minWait = 0.8f;
    public float maxWait = 2.0f;

    NavMeshAgent _agent;
    Animator _anim;
    SimpleGreetController greet;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        greet = GetComponent<SimpleGreetController>();
    }

    void OnEnable() { StartCoroutine(WanderLoop()); }

    bool Ready => _agent && _agent.enabled && _agent.isOnNavMesh;

    IEnumerator WanderLoop()
    {
        yield return new WaitUntil(() => Ready);

        while (true)
        {
            if (greet && greet.IsBusy)
            {
                if (_anim) _anim.SetFloat("Speed", 0f);
                yield return null;
                continue;
            }

            if (!Ready)
            {
                if (_anim) _anim.SetFloat("Speed", 0f);
                yield return null;
                continue;
            }

            bool hasPath = _agent.hasPath && !_agent.pathPending;
            float dist = hasPath ? _agent.remainingDistance : Mathf.Infinity;

            if (!hasPath || dist <= _agent.stoppingDistance)
            {
                Vector3 dest = RandomNavmeshLocation(wanderRadius);
                _agent.SetDestination(dest);

                float wait = Random.Range(minWait, maxWait);
                float t = 0f;
                while (t < wait)
                {
                    if (greet && greet.IsBusy) break;
                    if (!Ready) break;

                    if (_anim) _anim.SetFloat("Speed", _agent.velocity.magnitude, 0.12f, Time.deltaTime);
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                if (_anim) _anim.SetFloat("Speed", _agent.velocity.magnitude, 0.12f, Time.deltaTime);
                yield return null;
            }
        }
    }

    Vector3 RandomNavmeshLocation(float radius)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = Random.insideUnitSphere * radius + transform.position;
            random.y = transform.position.y;
            if (NavMesh.SamplePosition(random, out var hit, radius, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position;
    }
}
