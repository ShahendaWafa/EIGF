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

    [SerializeField] Slider leftSlider;
    [SerializeField] Slider rightSlider;


    [SerializeField] Camera camera;
    [SerializeField] WheelCollider leftWheel;
    [SerializeField] WheelCollider rightWheel;

    [SerializeField] Transform leftWheelTransform;
    [SerializeField] Transform rightWheelTransform;

    [SerializeField] Transform leftWheelCylinderTransform;
    [SerializeField] Transform rightWheelCylinderTransform;

    [SerializeField] float acceleration = 50;
    [SerializeField] float maxTurnAngle = 90f;

    [SerializeField] float leftSpeed = 10;
    [SerializeField] float rightSpeed = 10;

    [SerializeField] float nitroSpeedMul = 100;

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

    public float leftNitroVal = 4;
    public float rightNitroVal = 4;

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
            leftSlider.gameObject.SetActive(false);
            rightSlider.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (view.IsMine)
        {
            leftSlider.value = leftNitroVal;
            rightSlider.value = rightNitroVal;

            fixedRotation = transform.rotation;
            fixedRotation.x = 0;
            fixedRotation.z = 0;
            transform.rotation = fixedRotation;

            if (Input.GetKey(KeyCode.LeftControl) && leftNitroVal > 0)
            {
                if(!leftNActivated)
                {
                    leftNActivated = true;
                    leftForce = 1;
                    leftSpeed *= nitroSpeedMul;
                    StartCoroutine(ActivateLeftNitro());
                }

            }
            else
            {
                leftForce = 0;
                leftNActivated = false;
            }
            if (Input.GetKey(KeyCode.RightControl) && rightNitroVal > 0)
            {
                if (!rightNActivated)
                {
                    rightNActivated = true;
                    rightForce = 1;
                    rightSpeed *= nitroSpeedMul;
                    StartCoroutine(ActivateRightNitro());
                }
            }
            else
            {
                rightForce = 0;
                rightNActivated = false;
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
        currentLeftTurnAngle = maxTurnAngle * Input.GetAxis("LeftHorizontal");
        leftWheel.steerAngle = currentLeftTurnAngle;

        if (currentLeftAcceleration == 0)
            leftWheel.brakeTorque = Mathf.Infinity;
        else
        {
            leftWheel.brakeTorque = 0;
            leftWheel.motorTorque = currentLeftAcceleration * leftSpeed;

            if (leftNActivated)
            {
                rb.AddForce(transform.forward * 10000);
            }
        }
        
        

        UpdateWheel(leftWheel, leftWheelTransform, leftWheelCylinderTransform);
    }

    void RightWheelMovement()
    {
        currentRightAcceleration = acceleration * rightVInput;
        currentRightTurnAngle = maxTurnAngle * Input.GetAxis("RightHorizontal");
        rightWheel.steerAngle = currentRightTurnAngle;

        if (currentRightAcceleration == 0)
        {
            rightWheel.brakeTorque = Mathf.Infinity;      
        }
        else
        {
           
            rightWheel.brakeTorque = 0;
            rightWheel.motorTorque = currentRightAcceleration * rightSpeed;
            if (rightNActivated)
            {
                rb.AddForce(transform.forward * 10000);
            }
        }

        

        UpdateWheel(rightWheel, rightWheelTransform, rightWheelCylinderTransform);
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
            leftNitroVal -= 2;
            if(leftNitroVal < 0) leftNitroVal = 0;
            yield return new WaitForSeconds(1.0f);

        }
        leftNActivated = false;
        StopCoroutine(ActivateLeftNitro());
        leftNCharging = true;
        StartCoroutine(ChargeLeftNitro());
        leftSpeed /= nitroSpeedMul;
    }

    IEnumerator ChargeLeftNitro()
    {
        while(leftNitroVal < 4)
        {
            if (leftNActivated)
                break;
            leftNitroVal += 0.5f;
            yield return new WaitForSeconds(1.0f);
        }
        StopCoroutine(ChargeLeftNitro());
        leftNCharging = false;
    }

    IEnumerator ActivateRightNitro()
    {
        while (rightNActivated)
        {
            rightNitroVal -= 2;
            if (rightNitroVal < 0) rightNitroVal = 0;
            yield return new WaitForSeconds(1.0f);

        }
        rightNActivated = false;
        StopCoroutine(ActivateRightNitro());
        rightNCharging = true;
        StartCoroutine(ChargeRightNitro());
        rightSpeed /= nitroSpeedMul;
    }

    IEnumerator ChargeRightNitro()
    {
        while (rightNitroVal < 4)
        {
            if (rightNActivated)
                break;
            rightNitroVal +=  0.5f;
            yield return new WaitForSeconds(1.0f);
        }
        StopCoroutine(ChargeRightNitro());
        rightNCharging = false;
    }

}
