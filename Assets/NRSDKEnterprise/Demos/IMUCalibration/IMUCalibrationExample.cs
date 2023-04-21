using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;
using NRKernal.Enterprise;

public class IMUCalibrationExample : MonoBehaviour
{
    const float PITCH_UP_DEGREE = 20.0f;
    const float PITCH_DOWN_DEGREE = -20.0f;
    const float DELTA_DEGREE = 5.0f;
    const int LAST_TIME = 3;

    public Text m_TxtStats;
    public Toggle m_TogPitchUp;
    public Toggle m_TogPitchDown;
    public Text m_TxtGuide;
    public Button m_BtnStart;
    public Button m_BtnStop;
    public Text m_TxtPitch;
    public Slider m_SlidPhaseRate;
    NRIMUCalibrationManager m_Manager;
    // Start is called before the first frame update
    void Start()
    {
        m_Manager = NRIMUCalibrationManager.Instance;

        m_BtnStart.onClick.AddListener(OnClickStart);
        m_BtnStop.onClick.AddListener(OnClickStop);

        RefreshUI();
    }

    void RefreshUI()
    {
        m_TxtStats.text = string.Format("success: {0}, State: {1}, Phase: {2}", m_Manager.Success, m_Manager.CurState, m_Manager.RunningPhase);
        m_TogPitchUp.isOn = m_Manager.IsPhaseFinish(NRIMUCalibrationPhase.PitchUp);
        m_TogPitchDown.isOn = m_Manager.IsPhaseFinish(NRIMUCalibrationPhase.PitchDown);

        string guide = string.Empty;
        var state = m_Manager.CurState;
        switch (state)
        {
            case NRIMUCalibrationManager.State.None:
                guide = "Click StartButton to run calibration.";
                break;
            case NRIMUCalibrationManager.State.Running:
                guide = "Pitch Up/Down to reach the target degress.";
                break;
            case NRIMUCalibrationManager.State.PhaseRunning:
            case NRIMUCalibrationManager.State.PhaseFinishing:
                guide = "Please hold still!!!";
                break;
            case NRIMUCalibrationManager.State.Finish:
                guide = "Calibration has finished. Click StopButton to retry.";
                break;
        }
        m_TxtGuide.text = guide;
        
        m_BtnStart.gameObject.SetActive(state == NRIMUCalibrationManager.State.None);
        m_BtnStop.gameObject.SetActive(state != NRIMUCalibrationManager.State.None);
        m_TxtPitch.gameObject.SetActive(state == NRIMUCalibrationManager.State.Running || state == NRIMUCalibrationManager.State.PhaseRunning);
        m_SlidPhaseRate.gameObject.SetActive(state == NRIMUCalibrationManager.State.PhaseRunning);
    }

    float GetTargetDegreeForPhase(NRIMUCalibrationPhase phase)
    {
        if (phase == NRIMUCalibrationPhase.PitchUp)
            return PITCH_UP_DEGREE;
        else if (phase == NRIMUCalibrationPhase.PitchDown)
            return PITCH_DOWN_DEGREE;

        return 0;
    }

    void OnClickStart()
    {
        if (m_Manager.CurState == NRIMUCalibrationManager.State.None)
        {
            m_Manager.Start(DELTA_DEGREE, LAST_TIME);
            RefreshUI();
        }
    }

    void OnClickStop()
    {
        m_Manager.Stop();
        RefreshUI();
    }

    void StartPhase(NRIMUCalibrationPhase phase, float targetDegree)
    {
        if (phase != NRIMUCalibrationPhase.None)
        {
            m_Manager.StartPhase(phase, targetDegree, OnPhaseFinish);
        }
        RefreshUI();
    }

    void OnPhaseFinish(NRIMUCalibrationPhase phase, bool afterFinishWait)
    {
        if (m_Manager.Success)
        {
            RefreshUI();
            return;
        }

        // var nextPhase = m_Manager.NextPhase();
        // if (nextPhase != NRIMUCalibrationPhase.None)
        // {
        //     int targetDegree = GetTargetDegreeForPhase(nextPhase);
        //     StartPhase(nextPhase, targetDegree);
        // }
        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        var curState = m_Manager.CurState;
        if (curState == NRIMUCalibrationManager.State.Running || curState == NRIMUCalibrationManager.State.PhaseRunning || curState == NRIMUCalibrationManager.State.PhaseFinishing)
        {
            float targetDegree = 0;
            // int curPitchDeg = GetCurHeadPitch();
            float curPitchDeg = m_Manager.QueryPitch();
            if (curState == NRIMUCalibrationManager.State.Running)
            {
                var nextPhase = m_Manager.NextPhase();
                targetDegree = GetTargetDegreeForPhase(nextPhase);
                if (Mathf.Abs(targetDegree - curPitchDeg) <= DELTA_DEGREE)
                {
                    StartPhase(nextPhase, targetDegree);
                }
            }
            else if (curState == NRIMUCalibrationManager.State.PhaseRunning || curState == NRIMUCalibrationManager.State.PhaseFinishing)
            {
                targetDegree = GetTargetDegreeForPhase(m_Manager.RunningPhase);
                m_SlidPhaseRate.value = m_Manager.PhaseRate;
            }
            
            m_TxtPitch.text = string.Format("TargetPitch: {0} [+-{1}], CurPitch: {2}", targetDegree, DELTA_DEGREE, curPitchDeg);
        }
    }

    int GetCurHeadPitch()
    {
        var headPos = NRFrame.HeadPose;
        Vector3 headUp = headPos.rotation * Vector3.up;
        headUp = headUp.normalized;
        float pitchDeg = Mathf.Acos(headUp.y) * 180 / Mathf.PI;
        if (headPos.forward.y < 0)
            pitchDeg *= -1;
        return (int)pitchDeg;
    }

    private void OnDestroy() {
        m_BtnStart.onClick.RemoveListener(OnClickStart);
        m_BtnStop.onClick.RemoveListener(OnClickStop);

        m_Manager.Stop(); 
    }
}
