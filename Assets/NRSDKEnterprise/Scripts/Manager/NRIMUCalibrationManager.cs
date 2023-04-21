using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using NRKernal.Enterprise;

public class NRIMUCalibrationManager : SingleTon<NRIMUCalibrationManager>
{
    const float PHASE_FINISH_LAST_TIME = 0.5f;

    public enum State
    {
        None,
        // Calibration has been started, phase is preparing
        Running,
        // Phase is running. Get running phase by RunningPhase
        PhaseRunning,
        // Phase is finishing, lasted for a specular time.  Get running phase by RunningPhase
        PhaseFinishing,
        // Calibration has been finished.
        Finish,
    }

    State m_State = State.None;
    public State CurState { get { return m_State; }}
    bool[] m_PhaseFinish = new bool[(int)NRIMUCalibrationPhase.Num];
    public bool Success
    {
        get { 
            for (int i = 0; i < (int)NRIMUCalibrationPhase.Num; i++)
            {
                if (!m_PhaseFinish[i])
                    return false;
            }

            return true;
        }
    }

    NRIMUCalibrationPhase m_RunningPhase = NRIMUCalibrationPhase.None;
    public NRIMUCalibrationPhase RunningPhase
    {
        get { return m_RunningPhase; }
    }

    int m_PhaseRate = 0;
    public int PhaseRate
    {
        get { return m_PhaseRate; }
    }
    Action<NRIMUCalibrationPhase, bool> m_OnPhaseFinish;
    float m_TimePhaseFinishBeg;

    NRIMUCalibration m_IMUCalibration;
    public void Start(float delta_degress, float last_time)
    {
        NRDebugger.Info("[NRIMUCalibrationManager] Start: deltaDeg={0}, lastTime={1}", delta_degress, last_time);
        Debug.Assert(m_State == State.None);
        if (m_IMUCalibration == null)
            m_IMUCalibration = new NRIMUCalibration();

        m_IMUCalibration.Start(delta_degress, last_time);
        m_State = State.Running;

        NRKernalUpdater.OnUpdate += Update;
    }

    public void Stop()
    {
        NRDebugger.Info("[NRIMUCalibrationManager] Stop");
        if (m_State != State.None || m_State != State.Finish)
            m_IMUCalibration?.Stop(NRCalibrationResult.NR_CALIBRATION_RESULT_FAILED);

        m_OnPhaseFinish = null;
        m_IMUCalibration = null;
        for (int i = 0; i < (int)NRIMUCalibrationPhase.Num; i++)
            m_PhaseFinish[i] = false;
        m_RunningPhase = NRIMUCalibrationPhase.None;
        m_PhaseRate = 0;
        m_State = State.None;
        NRKernalUpdater.OnUpdate -= Update;
    }

    public bool StartPhase(NRIMUCalibrationPhase phase, float pitch_degree, Action<NRIMUCalibrationPhase, bool> OnPhaseFinish)
    {
        NRDebugger.Info("[NRIMUCalibrationManager] StartPhase: phase={0}, pitch_degree={1}", phase, pitch_degree);
        Debug.Assert(phase != NRIMUCalibrationPhase.None && m_State == State.Running && m_RunningPhase == NRIMUCalibrationPhase.None);
        if (IsPhaseFinish(phase))
        {
            OnPhaseFinish?.Invoke(phase, true);
            return true;
        }

        m_State = State.PhaseRunning;
        m_RunningPhase = phase;
        m_PhaseRate = 0;
        m_OnPhaseFinish = OnPhaseFinish;

        return m_IMUCalibration.StartPhase(pitch_degree);
    }

    void EnterFinishPhase(NRIMUCalibrationPhase phase)
    {
        NRDebugger.Info("[NRIMUCalibrationManager] EnterFinishPhase: phase={0}", phase);
        m_State = State.PhaseFinishing;
        m_TimePhaseFinishBeg = Time.realtimeSinceStartup;
        m_OnPhaseFinish?.Invoke(phase, false);
        if (phase == NRIMUCalibrationPhase.PitchDown)
        {
            LeaveFinishPhase(phase);
        }
    }

    void LeaveFinishPhase(NRIMUCalibrationPhase phase)
    {
        NRDebugger.Info("[NRIMUCalibrationManager] LeaveFinishPhase: phase={0}", phase);
        Debug.Assert(m_RunningPhase == phase);
        m_RunningPhase = NRIMUCalibrationPhase.None;
        m_PhaseRate = 0;
        m_PhaseFinish[(int)phase] = true;

        if (Success)
        {
            m_IMUCalibration.Stop(NRCalibrationResult.NR_CALIBRATION_RESULT_SUCCESS);
            m_State = State.Finish;
        }
        else
        {
            m_State = State.Running;
        }

        m_OnPhaseFinish?.Invoke(phase, true);
    }

    public bool IsPhaseFinish(NRIMUCalibrationPhase phase)
    {
        Debug.Assert(phase != NRIMUCalibrationPhase.None);
        return m_PhaseFinish[(int)phase];
    }

    public NRIMUCalibrationPhase NextPhase()
    {
        for (NRIMUCalibrationPhase phase = NRIMUCalibrationPhase.PitchUp; phase < NRIMUCalibrationPhase.Num; phase++)
        {
            if (!IsPhaseFinish(phase))
                return phase;
        }

        return NRIMUCalibrationPhase.None;
    }

    void Update()
    {
        if (m_RunningPhase == NRIMUCalibrationPhase.None)
            return;
            
        if (m_State == State.PhaseRunning)
        {
            m_PhaseRate = m_IMUCalibration.PhaseQueryRate();
            if (m_PhaseRate >= 100)
            {
                EnterFinishPhase(m_RunningPhase);
            }
        }
        else if (m_State == State.PhaseFinishing)
        {
            if (Time.realtimeSinceStartup - m_TimePhaseFinishBeg > PHASE_FINISH_LAST_TIME)
            {
                LeaveFinishPhase(m_RunningPhase);
            }
        }
    }

    public float QueryPitch()
    {
        return m_IMUCalibration.QueryPitch();
    }
}
