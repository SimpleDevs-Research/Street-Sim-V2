using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAttention : MonoBehaviour
{
    public List<ObjectOfAttention> observedTargets = new List<ObjectOfAttention>();
    public List<ObjectOfAttention> objectsInSight = new List<ObjectOfAttention>();
    public List<float> targetObservationTime = new List<float>();

    public AgentHeadTurn agentHeadTurn;

    public ObjectOfAttention currentAttention = null;
    public float currentAttentionPriority = 0;

    public ObjectOfAttention lastAttention = null;
    public bool fullAttention = false; //Look with eyes and head or just watch with eyes?

    public float transitiveAttentionPriority = 0;

    float attentionDecisionCounter = 0;
    void Start()
    {

    }

    void Update()
    {
        if (objectsInSight.Count > 0)
        {
            currentAttentionPriority = transitiveAttentionPriority;
            if (currentAttention != null)
                currentAttentionPriority = Mathf.Max(transitiveAttentionPriority, currentAttention.GetAttentionPriority(transform));
            foreach (ObjectOfAttention obj in objectsInSight)
            {
                float attPr = obj.GetAttentionPriority(transform);
                if (currentAttention == null || attPr > currentAttentionPriority)
                {
                    currentAttention = obj;
                    currentAttentionPriority = attPr;
                }
            }
            if(currentAttention != lastAttention)
            {
                attentionDecisionCounter = 0;
                fullAttention = false;
            }
            lastAttention = currentAttention;
            attentionDecisionCounter += Time.deltaTime;
            if (attentionDecisionCounter > Mathf.Max(2 - currentAttention.GetAttentionPriority(transform), 0.25f))
            {
                fullAttention = true;
            }
            if(fullAttention && attentionDecisionCounter > Mathf.Max(4 - currentAttention.GetAttentionPriority(transform), 0.25f))
            {
                Debug.Log("taking a suggestion");
                (BehaviorSuggestion, ObjectOfAttention) suggestion = currentAttention.GetBehaviorSuggestion(transform);
                Debug.Log(suggestion.Item1);
                switch(suggestion.Item1)
                {
                    case BehaviorSuggestion.LOOKAT:
                        transitiveAttentionPriority = currentAttention.GetAttentionPriority(transform)+0.1f;
                        currentAttention = suggestion.Item2;
                        Debug.Log("Looking at ", currentAttention);
                        break;
                }
            }
            if(attentionDecisionCounter > 5) { transitiveAttentionPriority = 0; }
        }
        else
        {
            currentAttention = null;
            lastAttention = currentAttention;
        }
        Debug.Log(transitiveAttentionPriority);
    }
    public void ObjectVisionUpdate()
    {
        ObjectOfAttention obj = null;
        float attentionPriority = -1;

        for(int i = 0; i < objectsInSight.Count; i++)
        {
            float thisAttPr = objectsInSight[i].GetAttentionPriority(transform);
            if (thisAttPr > attentionPriority)
            {
                attentionPriority = thisAttPr;
                obj = objectsInSight[i];
            }
        }
    }
}
