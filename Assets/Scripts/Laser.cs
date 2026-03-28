using UnityEngine;

public class Laser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private LayerMask playerMask;

    [Header("Parameters")]
    [SerializeField] private float laserSpeed;

    private Vector3 currentEnd;

    private void Awake()
    {
        currentEnd = start.position;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        Vector3 targetEnd = end.position;
        RaycastHit2D hit = Physics2D.Raycast(start.position,
            end.position - start.position,
            Vector3.Distance(start.position, end.position),
            collisionMask | playerMask);

        if (hit)
        {
            targetEnd = hit.point;
            if ((playerMask & (1 << hit.collider.gameObject.layer)) != 0)
                hit.collider.GetComponent<Player>().KillPlayer();
        }
        currentEnd = Vector3.MoveTowards(currentEnd, targetEnd, laserSpeed * Time.deltaTime);
        if (Vector3.Distance(start.position, targetEnd) < Vector3.Distance(start.position, currentEnd))
            currentEnd = targetEnd;
        lineRenderer.SetPositions(new Vector3[] { start.position, currentEnd });
    }
}
