using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helpers;
using SerializableTypes;

[System.Serializable]
public class SAnimatorState {
    public int fullPathHash;
    public float length;
    public bool loop;
    public float normalizedTime;
    public int shortNameHash;
    public float speed;
    public float speedMultiplier;
    public int tagHash;

    public SAnimatorState(
        int fullPathHash, 
        float length,
        bool loop,
        float normTime,
        int shortHash,
        float speed,
        float speedMultiplier,
        int tagHash
    ) {
        this.fullPathHash = fullPathHash;
        this.length = length;
        this.loop = loop;
        this.normalizedTime = normTime;
        this.shortNameHash = shortHash;
        this.speed = speed;
        this.speedMultiplier = speedMultiplier;
        this.tagHash = tagHash;
    }

    public SAnimatorState(AnimatorStateInfo state) {
        this.fullPathHash = state.fullPathHash;
        this.length = state.length;
        this.loop = state.loop;
        this.normalizedTime = state.normalizedTime;
        this.shortNameHash = state.shortNameHash;
        this.speed = state.speed;
        this.speedMultiplier = state.speedMultiplier;
        this.tagHash = state.tagHash;
    }
}

[System.Serializable]
public class SAnimatorTransition {
    public bool anyState;
    public float duration;
    
}

public class AnimationSaveState {
    public SVector3 position;
    public SQuaternion rotation;
    public SAnimatorState currentState;
    public SAnimatorState nextState;

}

public class TestSaveAnimation : MonoBehaviour
{   
    /*
    public struct State
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public AnimatorStateInfo AnimatorState;
        public AnimatorStateInfo AnimatorStateNext;
        public AnimatorTransitionInfo AnimatorTransition;
    }

    public State m_ReplayState;
    public Animator anim;

    private void Awake() {
        if (anim == null) {
            anim = GetComponent<Animator>();
        }
    }

    public void RestoreState()
    {
        transform.position = m_ReplayState.Position;
        transform.rotation = m_ReplayState.Rotation;
        anim.Play( m_ReplayState.AnimatorState.shortNameHash, 0, m_ReplayState.AnimatorState.normalizedTime );
        anim.Update( 0f );
        anim.CrossFadeInFixedTime( m_ReplayState.AnimatorStateNext.shortNameHash, m_ReplayState.AnimatorTransition.duration, 0, 0f, m_ReplayState.AnimatorTransition.normalizedTime );
    }
    public void RecordState()
    {
        m_ReplayState.Position = transform.position;
        m_ReplayState.Rotation = transform.rotation;
        m_ReplayState.AnimatorState = anim.GetCurrentAnimatorStateInfo( 0 );
        m_ReplayState.AnimatorStateNext = anim.GetNextAnimatorStateInfo( 0 );
        m_ReplayState.AnimatorTransition = anim.GetAnimatorTransitionInfo( 0 );

        AnimationSaveState saveState = new AnimationSaveState(
            transform.position,
            transform.rotation,
            m_ReplayState.AnimatorState,
            m_ReplayState.AnimatorStateNext
        );
    }
    */
 
}