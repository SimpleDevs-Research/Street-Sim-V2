using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EntityMath;

public class PedestrianMover : MonoBehaviour
{
    public Vector3 m_lastPosOnNavMesh;
    public Vector3 m_optimalVelocity;
    public Vector3 m_currentVelocity;
    public Vector3 localDestination => GetComponent<PedestrianRVO>().m_localDestination;

    
    [SerializeField] private RandomFloat m_maxAngularSpeed = new RandomFloat(90f);
    [SerializeField] private RandomFloat m_maxAngularSpeedStanding = new RandomFloat(90f);
    [SerializeField] private RandomFloat m_translateAcceleration = new RandomFloat(2f);

    private void Awake()
    {
        m_lastPosOnNavMesh = transform.position;

        m_maxAngularSpeed.Randomize();      // How fast do we turn?
        m_translateAcceleration.Randomize();    // How fast do we increase the agent's velocity?
    }
    private void LateUpdate()
    {

        // Rotate the agent to face the direction of the optimal velocity,. but only if the optimal velocity isn't Vector3.zero
        Quaternion targetRotation = (m_optimalVelocity != Vector3.zero)
            ? Quaternion.LookRotation(m_optimalVelocity)
            : Quaternion.LookRotation(localDestination - transform.position);
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        float angularStep = m_maxAngularSpeed * Time.deltaTime;
        //m_animTurn = angleDifference;
        // Rotate towards the target rotation but do not overshoot
        if (angularStep > angleDifference) transform.rotation = targetRotation;
        else transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularStep);

        // Calcualte the difference between our current velocity and the optimal velocity
        Vector3 diff = m_optimalVelocity - m_currentVelocity;

        // As long as there is a different in the two velocities, we HAVE to translate.
        if (diff.sqrMagnitude > 0f)
        {
            // Calculate the step needed to add to the current velocity
            Vector3 velStep = diff.normalized * m_translateAcceleration * Time.deltaTime;
            // Increment current velocity based on velStep, except in the case that the velocity step overshoots the optimal velocity
            if (velStep.sqrMagnitude > diff.sqrMagnitude) m_currentVelocity = m_optimalVelocity;
            else m_currentVelocity += velStep;
        }

        // Update the position
        transform.position += transform.forward * m_currentVelocity.magnitude * Time.deltaTime;

        // Update the animator based on the magnitude of the current velocity
        KeepInMesh();

    }

    private void KeepInMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas)) m_lastPosOnNavMesh = hit.position;
        transform.position = m_lastPosOnNavMesh;
    }

    private void RotateLookAt()
    {
        /*if (m_agentAttention.currentAttentionLocation == AgentAttention.nullLocation)
        {
            m_animTurn = 0;
            return;
        }
        Vector3 targetPos = new Vector3(m_agentAttention.currentAttentionLocation.x, transform.position.y, m_agentAttention.currentAttentionLocation.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        float angularStep = m_maxAngularSpeedStanding * Time.deltaTime;
        m_animTurn = angleDifference;
        // Rotate towards the target rotation but do not overshoot
        if (angularStep > angleDifference) transform.rotation = targetRotation;
        else transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularStep);*/
    }
}
