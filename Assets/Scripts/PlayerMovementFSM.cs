using System;
using UnityEngine;

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

    private AbstractEntityController m_Controller = null;
    private FSM m_FSM = null;
    private float m_StartMoveBeginTime = 0f;
    private int m_LastDirection = 0;

    public void Initialize(AbstractEntityController a_Controller)
    {
        m_Controller = a_Controller;
        m_FSM = new FSM();
        m_FSM.TransitionTo(IdleState);
    }

    public void Update()
    {
        m_FSM.Update();
    }

    private FSM.FSMDelegate IdleState(FSM.Step a_Step)
    {
        if (m_Controller.CurrentDirectionPressed != 0)
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
                    m_LastDirection = m_Controller.CurrentDirectionPressed;
                    m_SpeedMultiplier = 0f;
                    return null;
                }
            case FSM.Step.Update:
                {
                    if (m_LastDirection != m_Controller.CurrentDirectionPressed)
                    {
                        if (m_Controller.CurrentDirectionPressed == 0)
                        {
                            return StopMoveState;
                        }
                        else
                        {
                            return StartMoveState;
                        }
                    }

                    float elapsedTime = Time.time - m_StartMoveBeginTime;
                    float ratio = Mathf.Clamp01(elapsedTime / m_StartMoveDuration);
                    m_SpeedMultiplier = Mathf.Clamp01(m_StartMoveCurve.Evaluate(ratio));

                    if (ratio == 1f)
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
                        if (m_Controller.CurrentDirectionPressed == 0)
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
                    return null;
                }
            case FSM.Step.Update:
                {
                    if (m_Controller.CurrentDirectionPressed != 0)
                    {
                        return StartMoveState;
                    }

                    float elapsedTime = Time.time - m_StopMoveBeginTime;
                    float ratio = Mathf.Clamp01(elapsedTime / m_StopMoveDuration);
                    m_SpeedMultiplier = Mathf.Clamp01(m_StopMoveCurve.Evaluate(ratio));

                    if (ratio == 1f)
                    {
                        return IdleState;
                    }

                    return null;
                }
        }

        return null;
    }
}
