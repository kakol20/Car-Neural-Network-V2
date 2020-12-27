using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] private WheelCollider wheelCollider;

    // Start is called before the first frame update
    private void Start()
    {
        //wheelCollider = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, wheelCollider.steerAngle - transform.localEulerAngles.z, transform.localEulerAngles.z);

        transform.Rotate(wheelCollider.rpm / 60.0f * 360.0f * Time.deltaTime, 0, 0);
    }
}