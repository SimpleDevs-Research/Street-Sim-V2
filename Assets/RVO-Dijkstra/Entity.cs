using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [System.Flags]
    public enum Type { Pedestrian=1, Vehicle=2, TrafficLight=4, PedestrianLight=8, Crosswalk=16, Obstacle=32, Hand=64 }

    [System.Serializable]
    public class RandomFloat {
        public float value;
        public bool randomize = false;
        public Vector2 limits = Vector2.zero;
        public RandomFloat(float value) {
            this.value = value;
            this.randomize = false;
            this.limits = Vector2.zero;
        }
        public RandomFloat(float value, bool randomize) {
            this.value = value;
            this.randomize = randomize;
            this.limits = Vector2.zero;
        }
        public RandomFloat(float value, bool randomize, Vector2 limits) {
            this.value = value;
            this.randomize = randomize;
            this.limits = limits;
        }
        public void Randomize() {
            if (!randomize) return;
            this.value = UnityEngine.Random.Range(limits.x, limits.y);
        }
        public static implicit operator float(RandomFloat myFloat) {
            return myFloat.value;
        }
        public static implicit operator RandomFloat(float value) {
            return new RandomFloat(value);
        }

        // Override ToString for easy debugging
        public override string ToString() {
            return this.value.ToString();
        }
    }

    [Header("=== Entity Stats ===")]
    public Type type;
    private Type m_type;
    [SerializeField] protected Color m_color = Color.clear;
    public Color color { get => m_color; set {} }
    [SerializeField] protected float m_avoidanceRadius = 0f;
    public float avoidanceRadius => m_avoidanceRadius;
    public Vector3 position => transform.position;

    [Header("=== READ ONLY - FROM LATE UPDATE ===")]
    public Vector3 velocity;
    public Vector3 displacement;
    public float speed => velocity.magnitude;
    private Vector3 prevPosition;

    protected virtual void OnValidate() {
        if (m_type == type) return;
        switch(type) {
            case Type.Pedestrian:
                m_color = Color.red;
                break;
            case Type.Vehicle:
                m_color = Color.blue;
                break;
            case Type.TrafficLight:
                m_color = Color.yellow;
                break;
            case Type.PedestrianLight:
                m_color = Color.green;
                break;
            case Type.Crosswalk:
                m_color = Color.white;
                break;
            default:
                m_color = Color.clear;
                break;
        }
        m_type = type;
    }

    protected virtual void Awake() {}

    protected virtual void Start() {
        displacement = Vector3.zero;
        velocity = Vector3.zero;
        prevPosition = transform.position;        
    }

    protected virtual void FixedUpdate() {
        displacement = transform.position - prevPosition;
        velocity = displacement / Time.fixedDeltaTime;
        prevPosition = transform.position;
    }
    protected virtual void OnDestroy()
    {
    }
}
