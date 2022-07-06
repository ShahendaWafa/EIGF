using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
public class WheelController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] Camera camera;
    [SerializeField] WheelCollider leftWheel;
    [SerializeField] WheelCollider rightWheel;

    [SerializeField] Transform leftWheelTransform;
    [SerializeField] Transform rightWheelTransform;

    [SerializeField] Transform leftWheelCylinderTransform;
    [SerializeField] Transform rightWheelCylinderTransform;

    [SerializeField] float acceleration = 500f;
    [SerializeField] float maxTurnAngle = 90f;

    [SerializeField] float speed = 100f;

    float leftVInput;
    float rightVInput;
    //Left Wheel
    private float currentLeftTurnAngle = 0f;
    private float currentLeftAcceleration = 0f;
    private float currentLeftBreakForce = 0f;

    //Right Wheel
    private float currentRightTurnAngle = 0f;
    private float currentRightAcceleration = 0f;
    private float currentRightBreakForce = 0f;

    PhotonView view;
    Quaternion fixedRotation;

    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(camera);
        }
    }
    private void Update()
    {
        fixedRotation = transform.rotation;
        fixedRotation.x = 0;
        fixedRotation.z = 0;
        transform.rotation = fixedRotation;
    }
    private void FixedUpdate()
    {     
        leftVInput = Input.GetAxis("LeftVertical");
        rightVInput = Input.GetAxis("RightVertical");

        if (view.IsMine)
        {
            LeftWheelMovement();
            RightWheelMovement();
        }
    }

    void UpdateWheel(WheelCollider col, Transform trans, Transform Cyl)
    {
        Vector3 position;
        Quaternion rotation;

        col.GetWorldPose(out position, out rotation);
        trans.position = position;
        trans.rotation = rotation;

        rotation.x = 0;
        rotation.z = 0;
        Cyl.rotation = rotation;
    }

    void LeftWheelMovement()
    {
        currentLeftAcceleration = acceleration * leftVInput;

        if (currentLeftAcceleration == 0)
            leftWheel.brakeTorque = Mathf.Infinity;
        else
        {
            leftWheel.brakeTorque = 0;
            leftWheel.motorTorque = currentLeftAcceleration * speed;
        }

        currentLeftTurnAngle = maxTurnAngle * Input.GetAxis("LeftHorizontal");
        leftWheel.steerAngle = currentLeftTurnAngle;

        UpdateWheel(leftWheel, leftWheelTransform, leftWheelCylinderTransform);
    }

    void RightWheelMovement()
    {
        currentRightAcceleration = acceleration * rightVInput;
        Debug.Log(currentRightAcceleration);

        if (currentRightAcceleration == 0)
        {
            rightWheel.brakeTorque = Mathf.Infinity;      
        }
        else
        {
            rightWheel.brakeTorque = 0;
            rightWheel.motorTorque = currentRightAcceleration * speed;
        }

        currentRightTurnAngle = maxTurnAngle * Input.GetAxis("RightHorizontal");
        rightWheel.steerAngle = currentRightTurnAngle;

        UpdateWheel(rightWheel, rightWheelTransform, rightWheelCylinderTransform);
        rightWheelTransform.position = rightWheelCylinderTransform.position;
    }

    
    private void OnTriggerEnter(Collider other)
    {
        view.RPC(nameof(GameOver), RpcTarget.All, view.Owner.NickName);
    }

    [PunRPC]
    void GameOver(string winner)
    {
        Winner.winnerName = winner;
        PhotonNetwork.LoadLevel("GameOver");
    }

}
