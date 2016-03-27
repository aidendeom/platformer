using System;
using UnityEngine;

public class PlayerController : AbstractEntityController
{
    public override bool IsJumping
    {
        get
        {
            return Input.GetKeyDown(KeyCode.UpArrow);
        }
    }

    public override bool IsMovingLeft
    {
        get
        {
            return Input.GetKey(KeyCode.LeftArrow);
        }
    }

    public override bool IsMovingRight
    {
        get
        {
            return Input.GetKey(KeyCode.RightArrow);
        }
    }

    public override bool IsRunning
    {
        get
        {
            return Input.GetKey(KeyCode.LeftShift);
        }
    }
}
