using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private LayerMask playerMask;

    private Vector3 currentEnd;

    private void Awake()
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        currentEnd = end.position;
        RaycastHit2D hit = Physics2D.Raycast(start.position, end.position - start.position, Vector3.Distance(start.position, end.position), collisionMask | playerMask);
        if (hit)
        {
            currentEnd = hit.point;
            if ((playerMask & (1 << hit.collider.gameObject.layer)) != 0)
                Debug.Log("Te cogi chico");
        }
        lineRenderer.SetPositions(new Vector3[] { start.position, currentEnd });
    }
}
