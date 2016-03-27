﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public float m_MaxSpeedWalk = 5f;
    public float m_MaxSpeedRun = 30;
    public float m_MinSpeed = 0.1f;
    public float m_TickAccel = 5f;
    public float m_FrictionConstant = 10f;
    public float m_JumpStrength = 10f;
    public float m_GravityStrength = 70f;

    private Vector3 m_Velocity = Vector3.zero;

    private new Transform transform = null;
    private Collider2D m_Collider = null;
    private AbstractEntityController m_Controller = null;

    private List<Platform> m_Touching = new List<Platform>();

    public bool IsFalling
    {
        get
        {
            return m_Touching.Count == 0;
        }
    }

    private void Awake()
    {
        transform = gameObject.transform;
        m_Collider = GetComponent<Collider2D>();
        m_Controller = GetComponentInChildren<AbstractEntityController>();
    }

    private void Update()
    {
        DoTouchingUpdate();
        DoHorizontalUpdate();
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
        int direction = 0;
        if (m_Controller.IsMovingLeft)
        {
            direction += -1;
        }
        if (m_Controller.IsMovingRight)
        {
            direction += 1;
        }

        float hVel = m_Velocity.x;

        hVel += direction * m_TickAccel * Time.deltaTime;

        if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(hVel))
        {
            if (Mathf.Abs(hVel) < m_MinSpeed)
            {
                hVel = 0f;
            }
            else
            {
                hVel += -Mathf.Sign(hVel) * m_FrictionConstant * Time.deltaTime;
            }
        }

        float maxSpeed = m_MaxSpeedWalk;
        if (m_Controller.IsRunning)
        {
            maxSpeed = m_MaxSpeedRun;
        }

        hVel = Mathf.Clamp(hVel, -maxSpeed, maxSpeed);

        m_Velocity.x = hVel;
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
}
