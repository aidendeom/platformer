using UnityEngine;

public class FSM
{
    public enum Step
    {
        Enter,
        Update,
        Exit
    }

    public delegate FSMDelegate FSMDelegate(Step a_Step);

    private FSMDelegate m_CurrentDelegate = null;

    public bool DebugEnabled { get; set; }

    public FSM(bool a_DebugEnabled = false)
    {
        DebugEnabled = a_DebugEnabled;
    }

    public void Update()
    {
        FSMDelegate next = null;

        if (m_CurrentDelegate != null)
        {
            next = m_CurrentDelegate(Step.Update);
        }

        if (next != null)
        {
            TransitionTo(next);
        }
    }

    public void TransitionTo(FSMDelegate a_NextState)
    {
        if (m_CurrentDelegate != null)
        {
            if (DebugEnabled)
            {
                Debug.LogFormat("Exiting state {0}", m_CurrentDelegate.Method.Name);
            }
            m_CurrentDelegate(Step.Exit);
        }

        m_CurrentDelegate = a_NextState;
        if (m_CurrentDelegate != null)
        {
            if (DebugEnabled)
            {
                Debug.LogFormat("Entering state {0}", m_CurrentDelegate.Method.Name);
            }
            m_CurrentDelegate(Step.Enter);
        }
    }
}
