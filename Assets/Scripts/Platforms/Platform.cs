using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Platform : MonoBehaviour
{
    private Collider2D m_Collider = null;

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D a_Collider)
    {
        Entity entity = a_Collider.gameObject.GetComponent<Entity>();
        if (entity != null)
        {
            entity.AddTouching(this);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D a_Collider)
    {
        Entity entity = a_Collider.gameObject.GetComponent<Entity>();
        if (entity != null)
        {
            entity.RemoveTouching(this);
        }
    }

    public float GetTopPosition()
    {
        return transform.position.y + m_Collider.bounds.extents.y;
    }
}
