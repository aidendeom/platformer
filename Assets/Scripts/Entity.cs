using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private float m_MaxSpeedWalk = 5f;
    [SerializeField] private float m_MaxSpeedRun = 30;
    [SerializeField] private float m_JumpStrength = 10f;
    [SerializeField] private float m_GravityStrength = 70f;

    [SerializeField] private AnimationCurve m_StartMoveCurve = null;
    [SerializeField] private float m_StartMoveDuration = 0.5f;
    [SerializeField] private AnimationCurve m_StopMoveCurve = null;
    [SerializeField] private float m_StopMoveDuration = 0.5f;

    private Vector3 m_Velocity = Vector3.zero;

    private new Transform transform = null;
    private Collider2D m_Collider = null;
    private AbstractEntityController m_Controller = null;
    private FSM m_FSM = null;

    private List<Platform> m_Touching = new List<Platform>();

    public bool IsFalling
    {
        get
        {
            return m_Touching.Count == 0;
        }
    }

    public int CurrentControllerDirection
    {
        get
        {
            if (m_Controller.IsMovingLeft) { return -1; }
            if (m_Controller.IsMovingRight) { return 1; }
            return 0;
        }
    }

    private void Awake()
    {
        transform = gameObject.transform;
        m_Collider = GetComponent<Collider2D>();
        m_Controller = GetComponentInChildren<AbstractEntityController>();

        m_FSM = new FSM();
        m_FSM.TransitionTo(IdleState);
    }

    private void Update()
    {
        m_FSM.Update();

        DoTouchingUpdate();
        DoVerticalUpdate();

        transform.position += m_Velocity * Time.deltaTime;

        // Debug reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3 screenMiddle = new Vector3(Screen.width, Screen.height) / 2f;
            Vector3 pos = Camera.main.ScreenToWorldPoint(screenMiddle);
            pos.z = 0f;
            transform.position = pos;
            m_Velocity = Vector3.zero;
        }
    }

    public void StopDownwardsMovement()
    {
        if (m_Velocity.y < 0f)
        {
            m_Velocity.y = 0f;
        }
    }

    public void AddTouching(Platform a_Platform)
    {
        if (!m_Touching.Contains(a_Platform))
        {
            m_Touching.Add(a_Platform);
        }
    }

    public void RemoveTouching(Platform a_Platform)
    {
        m_Touching.Remove(a_Platform);
    }

    public Vector3 GetFeetPosition()
    {
        return transform.position +
            m_Collider.bounds.center +
            Vector3.down * m_Collider.bounds.extents.y;
    }

    private void DoTouchingUpdate()
    {
        if (IsFalling) { return; }

        StopDownwardsMovement();

        float highest = float.MinValue;
        foreach (Platform plat in m_Touching)
        {
            float top = plat.GetTopPosition();
            if (top > highest)
            {
                highest = top;
            }
        }

        Vector3 pos = transform.position;
        pos.y = highest + m_Collider.bounds.extents.y;
        transform.position = pos;
    }

    private void DoHorizontalUpdate()
    {
        int direction = CurrentControllerDirection;
        if (direction == 0)
        {
            direction = (int)Mathf.Sign(m_Velocity.x);
        }

        float maxSpeed = m_MaxSpeedWalk;
        if (m_Controller.IsRunning)
        {
            maxSpeed = m_MaxSpeedRun;
        }

        float vel = direction * maxSpeed * m_SpeedMultiplier;

        m_Velocity.x = vel;
    }

    private void DoVerticalUpdate()
    {
        if (m_Controller.IsJumping && !IsFalling)
        {
            m_Velocity += Vector3.up * m_JumpStrength;
            m_Touching.Clear();
        }
        else if (IsFalling)
        {
            m_Velocity += Vector3.down * m_GravityStrength * Time.deltaTime;
        }
    }

    #region FSM Delegates
    private FSM.FSMDelegate IdleState(FSM.Step a_Step)
    {
        if (CurrentControllerDirection != 0)
        {
            return StartMoveState;
        }

        return null;
    }

    private float m_StartMoveBeginTime = 0f;
    private int m_LastDirection = 0;
    private float m_SpeedMultiplier = 0f;
    private FSM.FSMDelegate StartMoveState(FSM.Step a_Step)
    {
        switch (a_Step)
        {
            case FSM.Step.Enter:
                {
                    m_StartMoveBeginTime = Time.time;
                    m_LastDirection = CurrentControllerDirection;
                    m_SpeedMultiplier = 0f;
                    return null;
                }
            case FSM.Step.Update:
                {
                    if (m_LastDirection != CurrentControllerDirection)
                    {
                        if (CurrentControllerDirection == 0)
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

                    DoHorizontalUpdate();

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
                    m_LastDirection = CurrentControllerDirection;
                    m_SpeedMultiplier = 1f;
                    return null;
                }
            case FSM.Step.Update:
                {
                    if (m_LastDirection != CurrentControllerDirection)
                    {
                        if (CurrentControllerDirection == 0)
                        {
                            return StopMoveState;
                        }
                        else
                        {
                            return StartMoveState;
                        }
                    }
                    DoHorizontalUpdate();
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
                    if (CurrentControllerDirection != 0)
                    {
                        return StartMoveState;
                    }

                    float elapsedTime = Time.time - m_StopMoveBeginTime;
                    float ratio = Mathf.Clamp01(elapsedTime / m_StopMoveDuration);
                    m_SpeedMultiplier = Mathf.Clamp01(m_StopMoveCurve.Evaluate(ratio));

                    DoHorizontalUpdate();

                    if (ratio == 1f)
                    {
                        return IdleState;
                    }

                    return null;
                }
        }

        return null;
    }
    #endregion
}
