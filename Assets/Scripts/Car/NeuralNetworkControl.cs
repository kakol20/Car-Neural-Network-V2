using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkControl : CarControls
{
    [Header("Raycast")]
    [SerializeField] private float maxRayLength = 10;

    [SerializeField] private int rayCount = 5;
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private Material lineMat;

    private List<RayInfo> rays = new List<RayInfo>();
    private float memoryValue = 0;

    [Header("Neural Network")]
    [SerializeField] private float secondsPerGeneration = 60;

    [SerializeField] private float speedTimeLimit = 5;
    [SerializeField] private float velocityThreshold = 0.1f;
    [SerializeField] private float waypointTimeLimit = 15;
    [SerializeField] private int[] layers;
    [SerializeField] private LayerMask waypointLayerMask;

    private bool initial = false;

    private GameObject nextWaypoint;

    // Other Variables
    private Color color = new Color(0, 0, 0);

    // Fitness
    private bool hitWall = false;
    private bool hitPreviousWP = false;

    private float distanceTravelled = 0;
    private float lastThrottle = 0, lastSteering = 0;
    private float throttleDiff = 0, steeringDiff = 0;
    private float timeElapsed = 0;
    private float totalThrottle = 0;
    private float waypointTimeElapsed = 0;
    private int waypointsHit = 0;
    private float count = 0;

    public override float Brake
    {
        get
        {
            float brake = 0;

            if (NeuralNetwork.Output[2] < 0f)
            {
                brake = NeuralNetwork.Output[2] * -1f;
            }

            return IsFinished ? brakeForce : brake * brakeForce;

            //float brake = (NeuralNetwork.Output[2] + 1f) / 2f;

            //return IsFinished ? brakeForce : brake * brakeForce;
            //return 0f;
        }
    }

    //public void Mutate(float mutationRate, float min, float max)
    //{
    //    NeuralNetwork.Mutate(mutationRate, min, max);
    //}

    public float Fitness
    {
        get
        {
            float fitness = 0;
            // ----- OLD WAY -----

            fitness = distanceTravelled;
            fitness += distanceTravelled / timeElapsed;

            // Calculate relative distance to next waypoint
            float relativeDistance = 0;
            if (waypointsHit >= 1)
            {
                float maxDistance = (nextWaypoint.transform.position - nextWaypoint.GetComponent<Waypoints>().GetPreviousWaypoint.transform.position).magnitude;
                float distanceToPrevious = (nextWaypoint.GetComponent<Waypoints>().GetPreviousWaypoint.transform.position - transform.position).magnitude;

                relativeDistance = distanceToPrevious / maxDistance;
            }

            fitness *= waypointsHit + 1 + relativeDistance;

            fitness += totalThrottle * (waypointsHit + 1);

            //fitness -= (throttleDiff /*/ count*/) + (steeringDiff /*/ count*/);

            //return hitWall ? fitness / 2.0f : fitness;

            fitness = hitPreviousWP ? 0 : fitness;

            return fitness;
        }
    }

    public float GetHue
    {
        get
        {
            Color.RGBToHSV(color, out float H, out _, out _);

            return H;
        }
    }

    public bool IsFinished { private set; get; }

    //public void CopyNetwork(NeuralNetwork network)
    //{
    //    NeuralNetwork.CopyNetwork(network);
    //}
    public NeuralNetwork NeuralNetwork { get; private set; }

    public override float SteeringAngle
    {
        get
        {
            return IsFinished ? 0 : NeuralNetwork.Output[1] * maxSteeringAngle;
        }
    }

    public override float Throttle
    {
        get
        {
            //float throttle = (NeuralNetwork.Output[2] + 1) / 2f;
            ////Debug.Log(throttle);

            //return IsFinished ? 0 : throttle * throttleForce;
            float throttle = 0;

            if (NeuralNetwork.Output[2] > 0)
            {
                throttle = NeuralNetwork.Output[2];
            }

            return IsFinished ? 0 : throttleForce * throttle;
        }
    }

    public void DrawRays()
    {
        foreach (RayInfo item in rays)
        {
            GL.PushMatrix();

            GL.Begin(GL.LINES);
            lineMat.SetPass(0);

            lineMat.color = Color.white;

            //GL.Color(Color.green);
            GL.Vertex3(transform.position.x, transform.position.y, transform.position.z);
            GL.Vertex3(item.destination.x, item.destination.y, item.destination.z);
            GL.End();

            GL.PopMatrix();
        }
    }

    public void Graph(object throttle, object brake, object steering)
    {
        DebugGUI.Graph(throttle, Throttle / throttleForce);
        DebugGUI.Graph(brake, Brake / brakeForce);
        DebugGUI.Graph(steering, SteeringAngle / maxSteeringAngle);
    }

    public void ResetCar()
    {
        color.a = 1;
        GetComponent<Renderer>().material.color = color;

        timeElapsed = 0;
        distanceTravelled = 0;
        waypointsHit = 0;
        waypointTimeElapsed = 0;
        memoryValue = 0;
        totalThrottle = 0;
        throttleDiff = 0;
        steeringDiff = 0;
        count = 0;

        // Reset to starting position
        transform.position = transform.parent.position;
        transform.rotation = transform.parent.rotation;

        IsFinished = false;
        hitWall = false;
        hitPreviousWP = false;

        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        initial = true;
    }

    /// <summary>
    /// Set Color of Body
    /// </summary>
    /// <param name="hue"></param>
    public void SetHue(float hue)
    {
        float s = Own.Random.Range(0.75f, 1f);
        float v = Own.Random.Range(0.75f, 1f);
        color = Color.HSVToRGB(hue, 1, v);

        GetComponent<Renderer>().material.color = color;
    }

    private void Awake()
    {
        CreateRays();

        CreateNetwork();

        rigidbody = GetComponent<Rigidbody>();

        ResetCar();
    }

    /// <summary>
    /// Create Neural Network
    /// </summary>
    private void CreateNetwork()
    {
        // Raycount + other input
        //layers[0] = rayCount + 0;

        NeuralNetwork = new NeuralNetwork(layers);

        NeuralNetwork.Randomise(-1, 1);
    }

    private void CreateRays()
    {
        if (rays.Count > 0) rays.Clear();

        for (int i = 0; i < rayCount; i++)
        {
            RayInfo rayInfo = new RayInfo();

            float angle = (180.0f / (float)(rayCount - 1f)) * (float)i * -1.0f;

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            rayInfo.direction = Quaternion.Euler(0.0f, angle, 0.0f) * right;
            rayInfo.direction.Normalize();

            Ray ray = new Ray(transform.position, rayInfo.direction);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayLength, wallLayerMask.value))
            {
                rayInfo.distance = hit.distance;
            }
            else
            {
                rayInfo.distance = maxRayLength;
            }

            Debug.DrawRay(transform.position, rayInfo.direction * rayInfo.distance);

            //Debug.Log(distance);

            rayInfo.destination = transform.position + (rayInfo.direction * rayInfo.distance);

            rays.Add(rayInfo);
        }
    }

    /// <summary>
    /// Input value to the neural network
    /// </summary>
    private void FeedForward()
    {
        List<float> input = new List<float>();

        foreach (RayInfo item in rays)
        {
            input.Add(item.distance / maxRayLength);
        }

        if (initial)
        {
            input.Add(0.0f);

            input.Add(0.0f);

            initial = false;
        }
        else
        {
            input.Add(rigidbody.velocity.magnitude);

            input.Add(memoryValue); // add memory value
        }

        NeuralNetwork.FeedForward(input.ToArray(), ActivationFunction.Tanh);

        // Output order: Throttle, Steering, Brake, Memory

        memoryValue = NeuralNetwork.Output[0];

        //Debug.Log(neuralNetwork.Output[0]);
    }

    private void FixedUpdate()
    {
        if (!IsFinished)
        {
            timeElapsed += Time.fixedDeltaTime;
            waypointTimeElapsed += Time.fixedDeltaTime;

            totalThrottle += (NeuralNetwork.Output[2] > 0 ? NeuralNetwork.Output[2] : 0) * Time.fixedDeltaTime;

            if (!initial)
            {
                throttleDiff += Mathf.Abs(lastThrottle - NeuralNetwork.Output[2]);
                steeringDiff += Mathf.Abs(lastSteering - NeuralNetwork.Output[1]);

                count++;
            }

            lastSteering = NeuralNetwork.Output[1];
            lastThrottle = NeuralNetwork.Output[2];

            UpdateRays();
            FeedForward();

            // ----- ACCUMULATE DISTANCE TRAVELLED -----
            Vector3 velocity = rigidbody.velocity;
            velocity.y = 0;
            distanceTravelled += velocity.magnitude * Time.fixedDeltaTime;

            // ----- CHECK IF STATIONARY OR BARELY MOVING -----
            if (timeElapsed >= speedTimeLimit && velocity.sqrMagnitude <= (velocityThreshold * velocityThreshold))
            {
                IsFinished = true;
            }

            // ----- CHECK IF TAKING TOO LONG TO REACH WAYPOINT -----
            if (waypointTimeElapsed >= waypointTimeLimit)
            {
                IsFinished = true;
            }

            // ----- STOP GENERATION AFTER CERTAIN TIME -----
            if (timeElapsed >= secondsPerGeneration)
            {
                IsFinished = true;
            }
        }
        else
        {
            //color.a = 0.5f;
            //color = Color.black;

            Color.RGBToHSV(color, out float H, out float S, out _);
            Color temp = Color.HSVToRGB(H, S, 0.1f);

            GetComponent<Renderer>().material.color = temp;

            rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & wallLayerMask.value) != 0)
        {
            IsFinished = true;
            hitWall = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & waypointLayerMask.value) != 0)
        {
            //waypointsHit++;

            if (waypointsHit < 1)
            {
                nextWaypoint = other.gameObject.GetComponent<Waypoints>().GetNextWaypoint;

                waypointsHit++;

                waypointTimeElapsed = 0;
            }
            else
            {
                if (Equals(other.gameObject, nextWaypoint))
                {
                    nextWaypoint = other.gameObject.GetComponent<Waypoints>().GetNextWaypoint;

                    waypointsHit++;

                    waypointTimeElapsed = 0;
                }

                if (Equals(other.gameObject, nextWaypoint.GetComponent<Waypoints>().GetPreviousWaypoint)) hitPreviousWP = true;
            }
        }
    }

    //private void Start()
    //{
    //}

    private void UpdateRays()
    {
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (180.0f / (float)(rayCount - 1f)) * (float)i * -1.0f;

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            rays[i].direction = Quaternion.Euler(0.0f, angle, 0.0f) * right;
            rays[i].direction.y = 0;
            rays[i].direction.Normalize();

            Ray ray = new Ray(transform.position, rays[i].direction);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayLength, wallLayerMask.value))
            {
                rays[i].distance = hit.distance;
            }
            else
            {
                rays[i].distance = maxRayLength;
            }

            //Debug.DrawRay(transform.position, rays[i].direction * rays[i].distance);

            //Debug.Log(distance);

            rays[i].destination = transform.position + (rays[i].direction * rays[i].distance);
        }
    }
}

public class RayInfo
{
    public Vector3 destination;
    public Vector3 direction;
    public float distance;
}