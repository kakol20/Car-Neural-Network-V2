using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualControl : CarControls
{
    public override float SteeringAngle
    {
        get
        {
            float steerAngle = Input.GetAxis("Horizontal") * maxSteeringAngle;

            //Debug.Log(steerAngle);

            return steerAngle;
        }
    }

    public override float Throttle
    {
        get
        {
            // ----- CHECK IF CONTROLLER PLUGGED IN -----
            string[] joystickNames = Input.GetJoystickNames();

            float throttle;
            if (joystickNames.Length > 0)
            {
                throttle = Input.GetAxis("Right Trigger") * throttleForce;
            }
            else
            {
                throttle = Input.GetAxis("Vertical") * throttleForce;

                throttle = throttle < 0f ? 0f : throttle;
            }

            return throttle;
        }
    }

    public override float Brake
    {
        get
        {
            // ----- CHECK IF CONTROLLER PLUGGED IN -----
            string[] joystickNames = Input.GetJoystickNames();

            float brake;
            if (joystickNames.Length > 0)
            {
                brake = Input.GetAxis("Left Trigger") * brakeForce;
            }
            else
            {
                brake = Input.GetAxis("Vertical");

                brake = brake > 0f ? 0 : brake * -1;

                brake *= brakeForce;
            }

            return brake;
        }
    }
}
