using System;
using UnityEngine;

public enum Direction
{
    Left = -1,
    None = 0,
    Right = 1
}

[Serializable]
public class PlayerMovementFSM
{
    [HideInInspector]
    public float m_SpeedMultiplier = 0f;

    [SerializeField]
    private AnimationCurve m_StartMoveCurve = null;
    [SerializeField]
    private float m_StartMoveDuration = 0.5f;
    [SerializeField]
    private AnimationCurve m_StopMoveCurve = null;
    [SerializeField]
    private float m_StopMoveDuration = 0.5f;
    [SerializeField]
    private bool m_EnableFSMDebug = false;

    private AbstractEntityController m_Controller = null;
    private FSM m_FSM = null;
    private float m_StartMoveBeginTime = 0f;
    private Direction m_LastDirection = Direction.None;
    private float m_ElapsedTimeExtra = 0f;

    private float MoveRampRatio { get; set; }
    private float StopMoveRampRatio
    {
        get { return 1 - MoveRampRatio; }
        set { MoveRampRatio = 1 - value; }
    }

    public void Initialize(AbstractEntityController a_Controller)
    {
        m_Controller = a_Controller;
        m_FSM = new FSM(m_EnableFSMDebug);
        m_FSM.TransitionTo(IdleState);
    }

    public void Update()
    {
        m_FSM.Update();
    }

    private FSM.FSMDelegate IdleState(FSM.Step a_Step)
    {
        if (m_Controller.CurrentDirectionPressed != Direction.None)
        {
            return StartMoveState;
        }

        return null;
    }

    private FSM.FSMDelegate StartMoveState(FSM.Step a_Step)
    {
        switch (a_Step)
        {
            case FSM.Step.Enter:
                {
                    m_StartMoveBeginTime = Time.time;
                    m_ElapsedTimeExtra = MoveRampRatio * m_StartMoveDuration;
                    m_LastDirection = m_Controller.CurrentDirectionPressed;
                    m_SpeedMultiplier = 0f;
                    return null;
                }
            case FSM.Step.Update:
                {
                    if (m_LastDirection != m_Controller.CurrentDirectionPressed)
                    {
                        if (m_Controller.CurrentDirectionPressed == Direction.None)
                        {
                            return StopMoveState;
                        }
                        else
                        {
                            return StartMoveState;
                        }
                    }

                    float elapsedTime = Time.time - m_StartMoveBeginTime + m_ElapsedTimeExtra;
                    MoveRampRatio = Mathf.Clamp01(elapsedTime / m_StartMoveDuration);
                    m_SpeedMultiplier = Mathf.Clamp01(m_StartMoveCurve.Evaluate(MoveRampRatio));

                    if (MoveRampRatio == 1f)
                    {
                        return MoveState;
                    }
                    return null;
                }
        }

        return null;
    }

    private FSM.FSMDelegate MoveState(FSM.Step a_Step)
    {
        switch (a_Step)
        {
            case FSM.Step.Enter:
                {
                    m_LastDirection = m_Controller.CurrentDirectionPressed;
                    m_SpeedMultiplier = 1f;
                    return null;
                }
            case FSM.Step.Update:
                {
                    if (m_LastDirection != m_Controller.CurrentDirectionPressed)
                    {
                        if (m_Controller.CurrentDirectionPressed == Direction.None)
                        {
                            return StopMoveState;
                        }
                        else
                        {
                            return StartMoveState;
                        }
                    }
                    return null;
                }
        }

        return null;
    }

    private float m_StopMoveBeginTime = 0f;

    private FSM.FSMDelegate StopMoveState(FSM.Step a_Step)
    {
        switch (a_Step)
        {
            case FSM.Step.Enter:
                {
                    m_StopMoveBeginTime = Time.time;
                    m_ElapsedTimeExtra = StopMoveRampRatio * m_StopMoveDuration;
                    return null;
                }
            case FSM.Step.Update:
                {
                    if (m_Controller.CurrentDirectionPressed != Direction.None)
                    {
                        return StartMoveState;
                    }

                    float elapsedTime = Time.time - m_StopMoveBeginTime + m_ElapsedTimeExtra;
                    StopMoveRampRatio = Mathf.Clamp01(elapsedTime / m_StopMoveDuration);
                    m_SpeedMultiplier = Mathf.Clamp01(m_StopMoveCurve.Evaluate(StopMoveRampRatio));

                    if (StopMoveRampRatio == 1f)
                    {
                        return IdleState;
                    }

                    return null;
                }
        }

        return null;
    }
}
