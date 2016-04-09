using UnityEngine;

public abstract class AbstractEntityController : MonoBehaviour
{
    public abstract bool IsMovingLeft { get; }
    public abstract bool IsMovingRight { get; }
    public abstract bool IsJumping { get; }
    public abstract bool IsRunning { get; }

    public int CurrentDirectionPressed
    {
        get
        {
            if (IsMovingLeft) { return -1; }
            if (IsMovingRight) { return 1; }
            return 0;
        }
    }
}
