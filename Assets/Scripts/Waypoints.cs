using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [SerializeField] private float offset = 1.5f;
    [SerializeField] private GameObject nextWaypoint;
    [SerializeField] private GameObject previousWaypoint;
    [SerializeField] private int loops = 10;
    public GameObject GetNextWaypoint => nextWaypoint;
    public GameObject GetPreviousWaypoint => previousWaypoint;

    private void DrawSpline(Color color, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Gizmos.color = color;

        //Vector3 p0 = previousWaypoint.transform.position;
        //Vector3 p1 = transform.position;
        //Vector3 p2 = nextWaypoint.transform.position;
        //Vector3 p3 = nextWaypoint.GetComponent<Waypoints>().GetNextWaypoint.transform.position;

        Vector3 lastPos = p1;

        float resolution = 1f / (float)loops;

        for (int i = 0; i <= loops; i++)
        {
            float t = i * resolution;

            Vector3 newPos = SplinePosition(t, p0, p1, p2, p3);

            Gizmos.DrawLine(lastPos, newPos);

            lastPos = newPos;
        }
    }

    private void OnDrawGizmos()
    {

        DrawSpline(Color.blue, previousWaypoint.transform.position, transform.position, nextWaypoint.transform.position, nextWaypoint.GetComponent<Waypoints>().GetNextWaypoint.transform.position);

        //DrawSpline(Color.red, previousWaypoint.transform.position - (previousWaypoint.transform.right * offset), transform.position - (transform.right * offset), nextWaypoint.transform.position - (nextWaypoint.transform.right * offset), nextWaypoint.GetComponent<Waypoints>().GetNextWaypoint.transform.position - (nextWaypoint.GetComponent<Waypoints>().GetNextWaypoint.transform.right * offset));

    }

    private Vector3 SplinePosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }

    // Start is called before the first frame update
    private void Start()
    {
    }
    // Update is called once per frame
    private void Update()
    {
    }
}