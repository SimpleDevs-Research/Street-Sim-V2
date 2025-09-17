using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BlinkCalibration : MonoBehaviour
{
    enum State { START, COUNTDOWN, METRONOME, END }
    [Header("Parameters")]
    [SerializeField] private Image m_movingDot;
    [SerializeField] private TextMeshProUGUI m_tmp;
    [SerializeField] private float m_movingDotTime;
    [SerializeField] private float m_movingDotExtent;
    [SerializeField] private float m_movingDotCenterLeniency;
    [SerializeField] private Color m_movingDotOffColor;
    [SerializeField] private Color m_movingDotOnColor;
    [SerializeField] private int m_totalOverlaps;
    [SerializeField] private string m_nextScene;

    [Header("Outcomes -- READ ONLY")]
    [SerializeField] private State m_state;
    [SerializeField] private int m_moveDir;
    [SerializeField] private int m_overlaps;
    [SerializeField] private float m_movingDotSpeed;
    [SerializeField] private int m_lastOverlapDir;

    void Start()
    {
        m_moveDir = 1;
        m_movingDotSpeed = m_movingDotExtent / m_movingDotTime;
        m_state = State.COUNTDOWN;
    }

    void Update()
    {
        switch (m_state) {
            case State.METRONOME:
                m_movingDot.rectTransform.Translate(new Vector3(m_moveDir * m_movingDotSpeed * Time.deltaTime, 0, 0));
                if(Mathf.Abs(m_movingDot.transform.localPosition.x) <= m_movingDotCenterLeniency)
                {
                    m_movingDot.color = m_movingDotOnColor;
                    if(m_lastOverlapDir != m_moveDir)
                    {
                        m_overlaps += 1;
                        m_lastOverlapDir = m_moveDir;
                    }
                } else
                {
                    m_movingDot.color = m_movingDotOffColor;
                }
                if(m_movingDot.rectTransform.localPosition.x > m_movingDotExtent && m_moveDir == 1)
                {
                    m_moveDir = -1;
                }
                if (m_movingDot.rectTransform.localPosition.x < -m_movingDotExtent && m_moveDir == -1)
                {
                    m_moveDir = 1;
                }
                if (m_overlaps >= m_totalOverlaps)
                {
                    m_state = State.END;
                    m_tmp.text = "Calibration complete";
                    m_movingDot.gameObject.SetActive(false);
                    StartCoroutine(DelayThenNext());
                }
                break;
        }
    }
    public IEnumerator DelayThenNext()
    {
        yield return new WaitForSeconds(4.0f);
        SceneManager.LoadScene(m_nextScene, LoadSceneMode.Single);

    }
    public void StartAnimFinished()
    {
        m_state = State.METRONOME;
    }
}
