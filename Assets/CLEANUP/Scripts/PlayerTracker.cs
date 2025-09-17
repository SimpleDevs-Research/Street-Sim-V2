using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    public static PlayerTracker Instance;
    public ObstacleRVO m_rvoData;
    void Start()
    {
        Instance = this;
        PedestrianKDTree.Instance.AddObstacle(GetComponent<ObstacleRVO>());
        m_rvoData = GetComponent<ObstacleRVO>();
    }

    // Update is called once per frame
    void Update()
    {
        m_rvoData.UpdateData(new Vector2(transform.position.x, transform.position.z), new Vector2(transform.forward.x, transform.forward.z), new Vector2(transform.forward.x, transform.forward.z));
    }
}
