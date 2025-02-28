using System;
using Interfaces;
using UnityEngine;
using VContainer;

public class CameraLookAt : MonoBehaviour
{
    private Transform _target;
    private bool _isLookingAt = false;
    
    private void SetTarget(Transform target)
    {
        _target = target;
        _isLookingAt = true;
    }
    
    private void Update()
    {
        if (_isLookingAt)
        {
            transform.LookAt(_target);
        }
    }
}
