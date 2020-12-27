using System.Collections.Generic;
using UnityEngine;

public enum WheelSide
{
    Left, Right
}

public enum WheelType
{
    Front, Rear
}

[System.Serializable]
public struct Wheel
{
#pragma warning disable CA2235 // Mark all non-serializable fields
    public WheelCollider wheelCollider;
#pragma warning restore CA2235 // Mark all non-serializable fields
    public WheelSide wheelSide;
    public WheelType wheelType;
}

[RequireComponent(requiredComponent: typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Car Body")]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    [SerializeField] private Rigidbody rigidbody;

#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    [Header("Wheels")]
    [SerializeField] private float antiRoll = 5000;

    [SerializeField] private List<Wheel> wheels = new List<Wheel>(4);

    [Header("Controls")]
    [SerializeField] private CarControls carControls;

    [SerializeField] [Range(0.5f, 1f)] private float frontBrakeBias = 0.6f;
    [SerializeField] private WheelType drivingWheel = WheelType.Rear;

    /// <summary>
    /// Prevent Rolling
    /// </summary>
    /// <param name="wheelType"></param>
    private void AntiRoll(WheelType wheelType)
    {
        // ----- GET WHEELS -----
        float travelL = 1.0f, travelR = 1.0f;
        bool groundedL = false, groundedR = false;
        Vector3 wheelLUp = Vector3.zero, wheelRUp = Vector3.zero, wheelLPos = Vector3.zero, wheelRPos = Vector3.zero;

        WheelHit hit;

        foreach (Wheel item in wheels)
        {
            if (item.wheelType == wheelType)
            {
                switch (item.wheelSide)
                {
                    case WheelSide.Left:
                        wheelLUp = item.wheelCollider.transform.up;
                        wheelLPos = item.wheelCollider.transform.position;

                        groundedL = item.wheelCollider.GetGroundHit(out hit);

                        if (groundedL)
                        {
                            travelL = (-item.wheelCollider.transform.InverseTransformPoint(hit.point).y - item.wheelCollider.radius) / item.wheelCollider.suspensionDistance;
                        }

                        break;

                    case WheelSide.Right:
                        wheelRUp = item.wheelCollider.transform.up;
                        wheelRPos = item.wheelCollider.transform.position;

                        groundedR = item.wheelCollider.GetGroundHit(out hit);

                        if (groundedR)
                        {
                            travelR = (-item.wheelCollider.transform.InverseTransformPoint(hit.point).y - item.wheelCollider.radius) / item.wheelCollider.suspensionDistance;
                        }

                        break;
                }
            }
        }

        // ----- APPLY FORCE -----
        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
        {
            rigidbody.AddForceAtPosition(wheelLUp * -antiRollForce, wheelLPos);
        }

        if (groundedR)
        {
            rigidbody.AddForceAtPosition(wheelRUp * -antiRollForce, wheelRPos);
        }
    }

    private void ControlCar()
    {
        float throttle = carControls.Throttle;
        float brake = carControls.Brake;
        float steerAngle = carControls.SteeringAngle;

        //Debug.Log(throttle);

        foreach (Wheel item in wheels)
        {
            // ----- APPLY STEERING -----
            if (item.wheelType == WheelType.Front)
            {
                item.wheelCollider.steerAngle = steerAngle;
            }

            // ----- APPLY THROTTLE TO DRIVING WHEELS -----
            if (item.wheelType == drivingWheel)
            {
                item.wheelCollider.motorTorque = throttle;
            }

            // ----- APPLY BRAKE -----
            switch (item.wheelType)
            {
                case WheelType.Front:
                    item.wheelCollider.brakeTorque = (2 * brake) * frontBrakeBias;

                    break;

                case WheelType.Rear:
                    item.wheelCollider.brakeTorque = (2 * brake) * (1 - frontBrakeBias);

                    break;
            }
        }
    }

    /// <summary>
    /// For debugging only
    /// </summary>
    public void DebugControls()
    {
        float throttle = carControls.Throttle;
        float brake = carControls.Brake;
        float steerAngle = carControls.SteeringAngle;

        Debug.Log(throttle);
    }

    private void FixedUpdate()
    {
        // ----- ANTI-ROLL -----
        AntiRoll(WheelType.Rear);

        // ----- CONTROL CAR -----
        ControlCar();
    }

    private void Reset()
    {
        rigidbody = rigidbody.GetComponent<Rigidbody>();

        rigidbody.mass = 2000;
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