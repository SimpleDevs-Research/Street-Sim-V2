using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class AudioSensitiveAgentsTrialController : MonoBehaviour
{

    [Header("Parameters")]
    public List<string> trialScenes;
    public BlinkCalibration blinkCalibrationController;

    [Header("Outcomes -- READ ONLY")]
    public int index;


    public static AudioSensitiveAgentsTrialController Instance;

    public delegate void TrialChangeEvent();
    public TrialChangeEvent onTrialChanged;
    void Start()
    {
        trialScenes = trialScenes.OrderBy(x => Random.value).ToList();
        index = 0;
        Instance = this;
        blinkCalibrationController.onCalibrationFinished += onCalibrationEventFinished;
    }

    public void TrialCollision(int index)
    {
        //SceneManager.UnloadSceneAsync(trialScenes[index]);
        blinkCalibrationController.gameObject.SetActive(true);
        blinkCalibrationController.m_targetForward = new Vector3(index, 0, 0);
        onTrialChanged?.Invoke();
    }

    private void onCalibrationEventFinished()
    {
        SceneManager.LoadSceneAsync(trialScenes[index], LoadSceneMode.Additive);
        index += 1;
    }
}
