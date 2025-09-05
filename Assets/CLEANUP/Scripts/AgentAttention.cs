using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAttention : MonoBehaviour
{
    public static Vector3 nullLocation = new Vector3(-999f, -999f, -999f);
    public List<ObjectOfAttention> observedTargets = new List<ObjectOfAttention>();
    public List<ObjectOfAttention> objectsInSight = new List<ObjectOfAttention>();
    public List<float> targetObservationTime = new List<float>();

    private AgentHeadTurn agentHeadTurn;
    private Pedestrian pedestrian;

    public ObjectOfAttention currentAttention = null;
    public Vector3 currentAttentionLocation = nullLocation;
    public float currentAttentionPriority = 0;

    public ObjectOfAttention lastAttention = null;
    public bool fullAttention = false; //Look with eyes and head or just watch with eyes?

    //public float transitiveAttentionPriority = 0;

    float attentionDecisionCounter = 0;

    public float attentionLingerMax = 2.0f;
    public float attentionLingerTime = 0;
    void Start()
    {
        if(!GetComponent<AgentHeadTurn>())
        {
            gameObject.AddComponent(typeof(AgentHeadTurn));
        }
        agentHeadTurn = GetComponent<AgentHeadTurn>();
        pedestrian = GetComponent<Pedestrian>();
    }

    void Update()
    {
        //If there's objects in our vision, set our attention to whatever has the highest priority
        if (objectsInSight.Count > 0)
        {
            foreach (ObjectOfAttention obj in objectsInSight)
            {
                float attPr = obj.GetAttentionPriority(transform);
                if (attPr > currentAttentionPriority)
                {
                    currentAttention = obj;
                    currentAttentionPriority = attPr;
                    fullAttention = true;
                }
            }
        }

        //If we have something to focus on, put our focus in its direction
        if (currentAttention != null && objectsInSight.Contains(currentAttention))
        {
            currentAttentionLocation = currentAttention.transform.position;
            attentionLingerTime = 0;

            if (pedestrian != null)
            {
                switch (currentAttention.behaviorSuggestion)
                {
                    case BehaviorSuggestion.LOOKAT:
                        pedestrian.SetBehaviorMode(Pedestrian.BehaviorMode.Look);
                        break;
                }
            }
        }

        //If there's nothing to focus on for long enough, stop focusing at all
        if (attentionLingerTime < attentionLingerMax)
        {
            attentionLingerTime += Time.deltaTime;
            if (attentionLingerTime >= attentionLingerMax)
            {
                currentAttention = null;
                currentAttentionPriority = 0;
                currentAttentionLocation = nullLocation;
                lastAttention = currentAttention;
                fullAttention = false;
            }
        }


        /*if (objectsInSight.Count > 0)
        {
            attentionLingerTime = 0;
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
                (BehaviorSuggestion, ObjectOfAttention) suggestion = currentAttention.GetBehaviorSuggestion(transform);
                switch(suggestion.Item1)
                {
                    case BehaviorSuggestion.LOOKAT:
                        transitiveAttentionPriority = currentAttention.GetAttentionPriority(transform)+0.1f;
                        currentAttention = suggestion.Item2;
                        //Debug.Log("Looking at ", currentAttention);
                        break;
                }
            }
            if(attentionDecisionCounter > 5) { transitiveAttentionPriority = 0; }
        }
        else
        {
            if(attentionLingerTime < attentionLingerMax)
            {
                attentionLingerTime += Time.deltaTime;
                if (attentionLingerTime >= attentionLingerMax)
                {
                    currentAttention = null;
                    lastAttention = currentAttention;
                }
            }

        }*/
    }
    /*public void ObjectVisionUpdate()
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
    }*/
}
