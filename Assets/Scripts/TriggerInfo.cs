using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerInfo : MonoBehaviour
{
    [SerializeField]
    private CameraPairScriptableObject _cameraPairInfo;


    public string CameraOne { get => _cameraPairInfo.cameraOne; }

}
