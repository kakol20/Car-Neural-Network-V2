using UnityEngine;

public abstract class CarControls : MonoBehaviour
{
    /// <summary>
    /// Set steering angle of front wheel
    /// </summary>
    public abstract float SteeringAngle { get; }

    /// <summary>
    /// Set car's throttle
    /// </summary>
    public abstract float Throttle { get; }

    /// <summary>
    /// Set car's brake force
    /// </summary>
    public abstract float Brake { get; }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    protected Rigidbody rigidbody;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

    [SerializeField] protected float maxSteeringAngle = 60;
    [SerializeField] protected float brakeForce = 5000;
    [SerializeField] protected float throttleForce = 10000;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }
}