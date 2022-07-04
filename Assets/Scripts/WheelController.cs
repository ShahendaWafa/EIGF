using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    [SerializeField] WheelCollider leftWheel;
    [SerializeField] WheelCollider rightWheel;

    [SerializeField] Transform leftWheelTransform;
    [SerializeField] Transform rightWheelTransform;

    public float acceleration = 100f;
    public float breakingForce = 500f;
    public float maxTurnAngle = 90f;

    //Left Wheel
    public float currentLeftTurnAngle = 0f;
    private float currentLeftAcceleration = 0f;
    private float currentLeftBreakForce = 0f;

    //RightWheel
    public float currentRightTurnAngle = 0f;
    private float currentRightAcceleration = 0f;
    private float currentRightBreakForce = 0f;


    private void FixedUpdate()
    {
        LeftWheelMovement();
        RightWheelMovement();
    }


    void UpdateWheel(WheelCollider col, Transform trans)
    {
        Vector3 poisition;
        Quaternion rotation;

        col.GetWorldPose(out poisition, out rotation);
        trans.position = poisition;
        trans.rotation = rotation;
    }

    void LeftWheelMovement()
    {
        currentLeftAcceleration = acceleration * Input.GetAxis("LeftVertical");

        leftWheel.motorTorque = currentLeftAcceleration;
        leftWheel.brakeTorque = currentLeftBreakForce;

        currentLeftTurnAngle = maxTurnAngle * Input.GetAxis("LeftHorizontal");
        leftWheel.steerAngle = currentLeftTurnAngle;

        UpdateWheel(leftWheel, leftWheelTransform);
    }

    void RightWheelMovement()
    {
        currentRightAcceleration = acceleration * Input.GetAxis("RightVertical");

        rightWheel.motorTorque = currentRightAcceleration;
        rightWheel.brakeTorque = currentRightBreakForce;

        currentRightTurnAngle = maxTurnAngle * Input.GetAxis("RightHorizontal");
        rightWheel.steerAngle = currentRightTurnAngle;

        UpdateWheel(rightWheel, rightWheelTransform);
    }
}
