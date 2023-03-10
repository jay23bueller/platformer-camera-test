using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;
using System;
using TMPro;
using System.Text;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _moveSpeed;
    private CharacterController _controller;

    private CinemachineVirtualCamera _currentCam;


    private bool _justTransitioned = false;
    private Vector3 _previousInput;
    private Vector3 _currentInput;
    private Quaternion _previousRotation;

    private bool _startingTransition;

    private bool _isStrafing = false;

    [SerializeField]
    private TMP_Text _cameraText;
    [SerializeField]
    private TMP_Text _inputText;
    [SerializeField]
    private TMP_Text _keyText;


    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

    }

    private void Start()
    {
        _currentCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineVirtualCamera;
        _cameraText.text = _currentCam.name;
    }

    private void UpdateKeyText()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (Input.GetKey(KeyCode.W))
        {
            stringBuilder.Append(" W ");
        }
        if (Input.GetKey(KeyCode.S))
        {
            stringBuilder.Append(" S ");
        }
        if (Input.GetKey(KeyCode.A))
        {
            stringBuilder.Append(" A ");
        }
        if (Input.GetKey(KeyCode.D))
        {
            stringBuilder.Append(" D ");
        }

        //_keyText.text = stringBuilder.ToString();

        _keyText.text = "" + new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }

    private void Update()
    {

        UpdateKeyText();
        Vector3 inputVec = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (inputVec.sqrMagnitude != 0 && !_startingTransition)
        {
            inputVec.Normalize();
            _currentInput = inputVec;


            if (_justTransitioned)
            {
                if (Vector3.Dot(_previousInput, _currentInput) < .88f)
                {
                    Debug.Log("Previous: " + _previousInput + " New Transformed: " + transform.rotation * _currentInput);
                    inputVec = Vector3.zero;
                    _justTransitioned = false;
                }
                else
                {

                    if (_currentCam.GetCinemachineComponent<CinemachinePOV>() == null)
                    {

                        inputVec = transform.forward;
                        Debug.Log("Applying Vec");

                    }
                    else
                    {

                        CinemachinePOV cinemachinePOV = _currentCam.GetCinemachineComponent<CinemachinePOV>();
                        inputVec = Quaternion.Euler(0f, cinemachinePOV.m_HorizontalAxis.Value, 0f) * Vector3.forward;
                        transform.rotation = Quaternion.Euler(0f, cinemachinePOV.m_HorizontalAxis.Value, 0f);
                        Debug.Log("Applying POV");

                    }
                }
            }








            if (!_justTransitioned && inputVec.sqrMagnitude != 0f)
            {

                Quaternion cameraWorldUpRotation = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);

                inputVec = cameraWorldUpRotation * inputVec;
                if (!_isStrafing)
                {
                    transform.rotation = Quaternion.LookRotation(inputVec, Vector3.up);
                }
                else
                {
                    transform.rotation = cameraWorldUpRotation;
                }
            }

            _inputText.text = "" + inputVec;

        }

        if ((_currentCam.GetCinemachineComponent<CinemachinePOV>() != null) && !_startingTransition)
        {
            CinemachinePOV cinemachinePOV = _currentCam.GetCinemachineComponent<CinemachinePOV>();

            transform.rotation = Quaternion.Euler(0f, cinemachinePOV.m_HorizontalAxis.Value, 0f);
        }

        _controller.SimpleMove(inputVec * _moveSpeed);

    }


    private void OnTriggerEnter(Collider other)
    {
        _startingTransition = true;

        if (other.TryGetComponent<TriggerInfo>(out TriggerInfo info) && info.CameraOne != _currentCam.name)
        {
            CinemachineVirtualCamera newCam = null;




            _previousInput = _currentInput;


            newCam = GameObject.Find(other.GetComponent<TriggerInfo>().CameraOne).GetComponent<CinemachineVirtualCamera>();


            if (newCam.GetCinemachineComponent<CinemachinePOV>() != null)
            {
                CinemachinePOV currentPOV = newCam.GetCinemachineComponent<CinemachinePOV>();
                currentPOV.m_VerticalAxis.Value = transform.rotation.eulerAngles.x;
                currentPOV.m_HorizontalAxis.Value = transform.rotation.eulerAngles.y;

                _isStrafing = true;
            }
            else
            {
                if(!_justTransitioned)
               
                    transform.forward = transform.rotation * _previousInput;
                
               
      
                _isStrafing = false;
            }


            _justTransitioned = true;


            Debug.DrawLine(transform.position, transform.position + (transform.forward * 5f), Color.blue, 5f);

            newCam.Priority = 10;
            _currentCam.Priority = 0;

            _currentCam = newCam;
            _cameraText.text = _currentCam.name;
        }

        _startingTransition = false;

    }
}
