using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ObstacleRVO : MonoBehaviour
{
    [System.Serializable]
    public struct RVOData
    {
        public int guid;
        public float2 position;
        public float2 velocity;
        public float2 desiredVelocity;
        public float radius;
        public RVOData(int guid, Vector2 position, Vector2 velocity, Vector2 desiredVelocity, float radius)
        {
            this.guid = guid;
            this.position = (float2)position;
            this.velocity = (float2)velocity;
            this.desiredVelocity = (float2)desiredVelocity;
            this.radius = radius;
        }
        public void UpdateData(Vector2 position, Vector2 velocity, Vector2 desiredVelocity)
        {
            this.position = (float2)position;
            this.velocity = (float2)velocity;
            this.desiredVelocity = (float2)desiredVelocity;
        }
    }
    public RVOData m_rvoData;

    public void UpdateData(Vector2 position, Vector2 velocity, Vector2 desiredVelocity)
    {
        m_rvoData.UpdateData(position, velocity, desiredVelocity);
    }
}
