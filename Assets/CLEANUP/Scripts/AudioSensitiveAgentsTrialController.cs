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
    void Start()
    {
        trialScenes = trialScenes.OrderBy(x => Random.value).ToList();
        index = 0;
        SceneManager.LoadScene(trialScenes[0], LoadSceneMode.Additive);
        Instance = this;
    }

    public void TrialCollision()
    {
        SceneManager.UnloadSceneAsync(trialScenes[index]);
        index += 1;
        SceneManager.LoadSceneAsync(trialScenes[index], LoadSceneMode.Additive);
    }
}
