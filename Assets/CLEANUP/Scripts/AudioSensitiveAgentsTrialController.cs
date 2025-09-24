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
    public EyeGazeTracker gazeTracker;

    [Header("Outcomes -- READ ONLY")]
    public int index;
    public GameObject m_toActivate;

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

    public void TrialCollision(int index, GameObject toActivate)
    {
        //SceneManager.UnloadSceneAsync(trialScenes[index]);
        gazeTracker.RecordEvent("Calibration");
        blinkCalibrationController.gameObject.SetActive(true);
        blinkCalibrationController.GetComponent<AudioSource>().Play();
        blinkCalibrationController.m_targetForward = new Vector3(index, 0, 0);
        onTrialChanged?.Invoke();
        m_toActivate = toActivate;
    }

    private void onCalibrationEventFinished()
    {
        gazeTracker.RecordEvent($"{trialScenes[index]} Start");
        SceneManager.LoadSceneAsync(trialScenes[index], LoadSceneMode.Additive);
        index += 1;
        m_toActivate.SetActive(true);
    }
}
