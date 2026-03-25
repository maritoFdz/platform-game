using UnityEngine;

public class SwitchParticle : MonoBehaviour
{
    private Transform target;

    [Header("References")]
    [SerializeField] private ParticleSystem particles;

    [Header("Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float distanceEpsilon;

    public void ThrowRay(Transform target)
    {
        this.target = target;
        particles.Play();
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float dist = dir.magnitude;

        if (dist <= distanceEpsilon)
        {
            particles.Stop();
            Destroy(gameObject, 0.5f); // gives a little extra time to let the effect disspaear for itself first
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.SetPositionAndRotation(Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        ), Quaternion.Euler(0f, 0f, angle - 90f));
    }
}