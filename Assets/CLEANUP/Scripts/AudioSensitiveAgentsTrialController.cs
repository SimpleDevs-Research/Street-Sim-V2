using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class AudioSensitiveAgentsTrialController : MonoBehaviour
{
    public List<string> trialScenes;
    public static AudioSensitiveAgentsTrialController Instance;
    public int index;

    public delegate void TrialChangeEvent();
    public TrialChangeEvent onTrialChanged;
    void Start()
    {
        trialScenes = trialScenes.OrderBy(x => Random.value).ToList();
        index = 0;
        Instance = this;
    }

    public void TrialCollision()
    {
        //SceneManager.UnloadSceneAsync(trialScenes[index]);
        onTrialChanged?.Invoke();
        SceneManager.LoadSceneAsync(trialScenes[index], LoadSceneMode.Additive);
        index += 1;

    }
}
