using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Cinemachine;

public class WheelController : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] ParticleSystem leftSmokePrefab;
    [SerializeField] ParticleSystem rightSmokePrefab;
    [SerializeField] ParticleSystem screenEffect;

    [SerializeField] Slider leftSlider;
    [SerializeField] Slider rightSlider;

    [SerializeField] CinemachineVirtualCamera camera;
    [SerializeField] CinemachineVirtualCamera ZoomCamera;


    [SerializeField] Transform cameraTransform;
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

    bool isShaking = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        if (!view.IsMine)
        {
            Destroy(camera);
            leftSlider.gameObject.SetActive(false);
            rightSlider.gameObject.SetActive(false);
            Destroy(screenEffect);
        }
        leftSmokePrefab.Stop();
        rightSmokePrefab.Stop();

    }
    private void Update()
    {

        if (view.IsMine)
        {
            if(leftNActivated||rightNActivated)
            {
                if (CameraSwitcher.IsActiveCamera(camera))
                {
                    CameraSwitcher.SwitchCamera(ZoomCamera);
                }
                //if (!isShaking)
                //{
                //    isShaking = true;
                //    StartCoroutine(ShakeCamera());
                //}
                CinemachineShake.Instance.ShakeCamera(5f, .1f);
                screenEffect.gameObject.SetActive(true);

            }
            else
            {
                if (!CameraSwitcher.IsActiveCamera(camera))
                    CameraSwitcher.SwitchCamera(camera);
                screenEffect.gameObject.SetActive(false);
            }
            leftSlider.value = leftNitroVal;
            rightSlider.value = rightNitroVal;

            fixedRotation = transform.rotation;
            fixedRotation.x = 0;
            fixedRotation.z = 0;
            transform.rotation = fixedRotation;

            if (Input.GetKey(KeyCode.LeftControl) && leftNitroVal > 0)
            {
                if (!leftNActivated)
                {
                    leftSmokePrefab.Play();
                    leftNActivated = true;
                    leftForce = 1;
                    leftSpeed *= nitroSpeedMul;
                    StartCoroutine(ActivateLeftNitro());                                    
                }               
            }
            else
            {                
                leftSmokePrefab.Stop();
                leftForce = 0;
                leftNActivated = false;
            }
            if (Input.GetKey(KeyCode.RightControl) && rightNitroVal > 0)
            {
                if (!rightNActivated)
                {
                    rightSmokePrefab.Play();
                    rightNActivated = true;
                    rightForce = 1;
                    rightSpeed *= nitroSpeedMul;
                    StartCoroutine(ActivateRightNitro());                                        
                }              
            }
            else
            {
                rightSmokePrefab.Stop();
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
            if (leftNitroVal < 0) leftNitroVal = 0;
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
        while (leftNitroVal < 4)
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
            rightNitroVal += 0.5f;
            yield return new WaitForSeconds(1.0f);
        }
        StopCoroutine(ChargeRightNitro());
        rightNCharging = false;
    }

    IEnumerator ShakeCamera()
    {
        float allX = 0;
        float allY = 0;
        while (leftNActivated || rightNActivated)
        {
            Debug.Log("test");
            float x = Random.Range(-1f, 1f) * 0.2f;
            float y = Random.Range(-1f, 1f) * 0.2f;
            allX += x;
            allY += y;
            ZoomCamera.transform.position = new Vector3(ZoomCamera.transform.position.x + x, ZoomCamera.transform.position.y + y, ZoomCamera.transform.position.z);
            yield return null;
        }
        ZoomCamera.transform.position = new Vector3(ZoomCamera.transform.position.x - allX, ZoomCamera.transform.position.y - allY, ZoomCamera.transform.position.z);
        isShaking = false;
        StopCoroutine(ShakeCamera());
    }

    private void OnEnable()
    {
        CameraSwitcher.Register(camera);
        CameraSwitcher.Register(ZoomCamera);
        CameraSwitcher.SwitchCamera(camera);
    }

    private void OnDisable()
    {
        CameraSwitcher.Unregister(camera);
        CameraSwitcher.Unregister(ZoomCamera);
    }
}
