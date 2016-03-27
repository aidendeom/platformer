using System.Collections.Generic;
using UnityEngine;

public class PlatformCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject m_BlockPrefab = null;
    [SerializeField]
    private GameObject m_PlatformPrefab = null;

    private GameObject m_Target = null;
    private List<GameObject> m_Blocks = new List<GameObject>();
    private Camera m_Cam = null;

    private void Start()
    {
        m_Target = Instantiate(m_BlockPrefab);
        SpriteRenderer sr = m_Target.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0.5f;
        sr.color = c;

        m_Cam = Camera.main;
    }

    private void Update()
    {
        m_Target.transform.position = ScreenToWorldSnap(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            CreateBlock(Input.mousePosition);
        }
        if (Input.GetMouseButtonDown(2))
        {
            CreatePlatform(Input.mousePosition);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = m_Cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue);
            foreach (RaycastHit2D hit in hits)
            {
                GameObject go = hit.collider.gameObject;

                if (go == m_Target)
                {
                    continue;
                }

                m_Blocks.Remove(go);
                Destroy(go);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            m_Blocks.ForEach(b => Destroy(b));
            m_Blocks.Clear();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void CreateBlock(Vector3 a_ScreenCoord)
    {
        CreateAndSnap(m_BlockPrefab, a_ScreenCoord);
    }

    private void CreatePlatform(Vector3 a_ScreenCoord)
    {
        CreateAndSnap(m_PlatformPrefab, a_ScreenCoord);
    }

    private void CreateAndSnap(GameObject a_Prefab, Vector3 a_ScreenCoord)
    {
        GameObject instance = Instantiate(a_Prefab);
        instance.transform.position = ScreenToWorldSnap(a_ScreenCoord);
        m_Blocks.Add(instance);
    }

    private Vector3 ScreenToWorldSnap(Vector3 a_ScreenCoord)
    {
        Vector3 worldPos = m_Cam.ScreenToWorldPoint(a_ScreenCoord);
        worldPos.x = Mathf.Round(worldPos.x);
        worldPos.y = Mathf.Round(worldPos.y);
        worldPos.z = 0;

        return worldPos;
    }
}
