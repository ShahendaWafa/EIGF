using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class WheelController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] GameObject HB;
    [SerializeField] Slider slider;
    [SerializeField] Camera camera;
    [SerializeField] WheelCollider leftWheel;
    [SerializeField] WheelCollider rightWheel;

    [SerializeField] Transform leftWheelTransform;
    [SerializeField] Transform rightWheelTransform;

    [SerializeField] Transform leftWheelCylinderTransform;
    [SerializeField] Transform rightWheelCylinderTransform;

    [SerializeField] float acceleration = 500f;
    [SerializeField] float maxTurnAngle = 90f;

    [SerializeField] float leftSpeed = 100f;
    [SerializeField] float rightSpeed = 100f;

    [SerializeField] float nitroSpeedMul = 10;

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

    int leftNitro = 20;
    int rightNitro = 10;

    int leftForce = 0;
    int rightForce = 0;
    

    bool leftNActivated = false;
    bool rightNActivated = false;

    bool leftNCharging = false;
    bool rightNCharging = false;

    PhotonView view;
    Quaternion fixedRotation;

    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(camera);
            HB.SetActive(false);
        }
    }
    private void Update()
    {
        if (view.IsMine)
        {
            slider.value = leftNitro;
            fixedRotation = transform.rotation;
            fixedRotation.x = 0;
            fixedRotation.z = 0;
            transform.rotation = fixedRotation;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (!leftNActivated && leftNitro > 0)
                {
                    leftNActivated = true;
                    leftForce = 1;
                    leftSpeed *= nitroSpeedMul;
                    rightSpeed *= nitroSpeedMul;
                    StartCoroutine(ActivateLeftNitro());
                }
            }
            else
            {
                leftForce = 0;
                leftNActivated = false;
            }
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                if (!rightNActivated)
                {
                    rightNActivated = true;
                }
            }
        } 
    }
    private void FixedUpdate()
    {     
        leftVInput = Input.GetAxis("LeftVertical") + leftForce;
        rightVInput = Input.GetAxis("RightVertical") + rightForce;

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
            leftWheel.motorTorque = currentLeftAcceleration * leftSpeed;
        }
        Debug.Log("Left");
        Debug.Log(leftWheel.motorTorque);
        currentLeftTurnAngle = maxTurnAngle * Input.GetAxis("LeftHorizontal");
        leftWheel.steerAngle = currentLeftTurnAngle;

        UpdateWheel(leftWheel, leftWheelTransform, leftWheelCylinderTransform);
    }

    void RightWheelMovement()
    {
        currentRightAcceleration = acceleration * rightVInput;

        if (currentRightAcceleration == 0)
        {
            rightWheel.brakeTorque = Mathf.Infinity;      
        }
        else
        {
            rightWheel.brakeTorque = 0;
            rightWheel.motorTorque = currentRightAcceleration * rightSpeed;
        }

        Debug.Log("Right");
        Debug.Log(rightWheel.motorTorque);
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

    IEnumerator ActivateLeftNitro()
    {
        while (leftNActivated)
        {
            leftNitro -= 2;
            Debug.Log(leftNitro);
            if(leftNitro < 0) leftNitro = 0;
            yield return new WaitForSeconds(1.0f);

        }
        leftNActivated = false;
        StopCoroutine(ActivateLeftNitro());
        leftNCharging = true;
        StartCoroutine(ChargeLeftNitro());
        leftSpeed /= nitroSpeedMul;
        rightSpeed /= nitroSpeedMul;
    }

    IEnumerator ChargeLeftNitro()
    {
        while(leftNitro < 10)
        {
            if (leftNActivated)
                break;
            leftNitro++;
            yield return new WaitForSeconds(1.0f);
        }
        StopCoroutine(ChargeLeftNitro());
        leftNCharging = false;
    }

}
