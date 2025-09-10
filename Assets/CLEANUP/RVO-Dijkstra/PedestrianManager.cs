using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using AdvancedPeopleSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PedestrianManager : MonoBehaviour
{

    [Header("=== Settings ===")]
    [SerializeField] private Pedestrian[] m_pedestrianPrefabs;
    [SerializeField] public int m_numPedestrians = 10;
    [SerializeField] private RouteNode[] m_startNodes;
    [SerializeField] private RouteNode[] m_endNodes;
    [SerializeField] private Vector2 m_spawnDelayMinMax;
    [SerializeField] private Transform m_pedestrianParent;
    [SerializeField] private DemographicsPreset m_currentDemographics;
    [SerializeField] private float m_runTime;

    [Header("=== Outcomes - Read Only ===")]
    [SerializeField] private List<Pedestrian> m_activePedestrians;
    [SerializeField] private List<Pedestrian> m_inactivePedestrians;
    [SerializeField] public List<Pedestrian> m_TotalPedestrians;
    [SerializeField] private int[] currentDemographicCount;
    public List<Pedestrian> activePedestrians => m_activePedestrians;
    [SerializeField] private List<GameObject> m_toDestroy;
    public int totalCreatedPedestrians;

    public static PedestrianManager Instance;
    Vector3 m_inactivePos = new Vector3(100, 100, 100);

    public UnityAction onAwakeFinished;
    private float timeElapsed;

    public static int iterations = 0;

    private void Awake() {
        Instance = this;
        if (m_pedestrianParent == null) m_pedestrianParent = this.transform;
        m_toDestroy = new List<GameObject>();
        m_activePedestrians = new List<Pedestrian>();

        currentDemographicCount = new int[m_currentDemographics.groups.Length];
        for (int i = 0; i < currentDemographicCount.Length; i++) currentDemographicCount[i] = 0;

         
        //Pre-pool all pedestrians
        for(int i = 0; i < m_numPedestrians; i++)
        {
            Pedestrian newPed = Instantiate(m_currentDemographics.groups[0].pedestrians[i % m_currentDemographics.groups[0].pedestrians.Length],
                        m_inactivePos,
                        Quaternion.identity, m_pedestrianParent) as Pedestrian;

            m_TotalPedestrians.Add(newPed);
            newPed.gameObject.SetActive(false);
            newPed.gameObject.name = i.ToString();
        }

        m_inactivePedestrians = new List<Pedestrian>(m_TotalPedestrians);

        //onAwakeFinished.Invoke();
        if (GetComponent<PedestrianKDTree>())
        {
            GetComponent<PedestrianKDTree>().Init();
        }

        StartCoroutine(GeneratePedestrians());
    }

    private IEnumerator GeneratePedestrians()
    {
        while (true)
        {
            if (m_inactivePedestrians.Count == 0)
            {
                yield return null;
                continue;
            }

            int startIndex = Random.Range(0, (int)m_startNodes.Length);

            int endIndex = Random.Range(0, (int)m_endNodes.Length);
            while (m_startNodes[startIndex] == m_endNodes[endIndex])
            {
                endIndex = Random.Range(0, (int)m_endNodes.Length);
            }

            RouteNode startNode = m_startNodes[startIndex];
            RouteNode endNode = m_endNodes[endIndex]; //Forces parity between start and end nodes
            Vector3 startPos = startNode.transform.position;
            Quaternion startRot = startNode.transform.rotation;

            Pedestrian newPed = null;

            //Temporary fix for pooling pedestrians w/o regard for variation
            newPed = m_inactivePedestrians[0];
            if (newPed.gameObject.activeInHierarchy) newPed = null;
            else
            {
                m_inactivePedestrians.RemoveAt(0);

                newPed.gameObject.SetActive(true);
                newPed.transform.position = startPos;
                newPed.transform.rotation = startRot;
                
                newPed.SetRouteStart(startNode);
                newPed.SetRouteDestination(endNode);
                List<RouteNode> route = RouteManager.instance.getRoute(startNode, endNode, newPed.GetComponent<PedestrianController>().m_personality);
                newPed.SetRoute(route);
                newPed.SetDestination(route[1].transform.position);
                newPed.transform.position += new Vector3(UnityEngine.Random.Range(-startNode.acceptableRadius, startNode.acceptableRadius), 0, UnityEngine.Random.Range(-startNode.acceptableRadius, startNode.acceptableRadius));
                newPed.BeginCalculatingBestPath();
                m_activePedestrians.Add(newPed);
                totalCreatedPedestrians++;
            }


            yield return new WaitForSeconds(Random.Range(m_spawnDelayMinMax.x, m_spawnDelayMinMax.y));
            
        }
    }

    public void PedestrianAtEnd(Entity e) {
        Debug.Log("At End");
        if (e.type != Entity.Type.Pedestrian) return;
        Pedestrian p = (Pedestrian)e;
        m_activePedestrians.Remove(p);
        m_inactivePedestrians.Add(p);
        m_toDestroy.Add(p.gameObject);
    }

    private void LateUpdate() {
        while(m_toDestroy.Count > 0) {
            GameObject go = m_toDestroy[0];
            m_toDestroy.RemoveAt(0);
            go.transform.position = m_inactivePos;
            go.gameObject.SetActive(false);
        }


    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= m_runTime)
        {
            iterations += 1;
            Debug.Log(iterations);
#if UNITY_EDITOR
            if(iterations == 20)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#elif UNITY_WEBPLAYER
                        Application.OpenURL(webplayerQuitURL);
#else
                        Application.Quit();
#endif
        }
    }

    /*
     *     public void PedestrianAtEnd(EntityDetector d, Entity e) {
        if (e.type != Entity.Type.Pedestrian) return;
        Pedestrian p = (Pedestrian)e;
        m_activePedestrians.Remove(p);
        if(d != null)
            d.RemoveEntity(e);
        m_toDestroy.Add(p.gameObject);
    }

    private void LateUpdate() {
        while(m_toDestroy.Count > 0) {
            GameObject go = m_toDestroy[0];
            m_toDestroy.RemoveAt(0);
            Destroy(go);
        }
    }
     */
}
