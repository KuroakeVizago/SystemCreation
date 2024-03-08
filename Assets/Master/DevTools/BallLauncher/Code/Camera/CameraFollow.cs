using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Config:")] 
    [SerializeField] private Transform m_targetObject;
    [SerializeField] private float m_offsetLength;
    
    private Vector3 _initialOffsetDirection;
    
    void Start()
    {
        _initialOffsetDirection = transform.position - m_targetObject.position;
        _initialOffsetDirection.Normalize();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = m_targetObject.position + (_initialOffsetDirection * m_offsetLength);
    }
}
