using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class BallController : MonoBehaviour
{
    [Header("Basic Config:")] 
    [SerializeField] private float m_launchForce = 10.0f;
    [SerializeField] private DragMode m_mode = DragMode.XZ;
    
    [Header("Visualization:")] 
    [SerializeField] private LineRenderer m_dragLineRenderer;
    [SerializeField] private LineRenderer m_predictionLineRenderer;
    
    public Vector3 DragDirection { get; set; }
    public float DragLength { get; set; }
    
    
    private Vector3 _mouseWorldPosition = Vector3.zero;
    private Vector3 _previousPosition = Vector3.zero;

    private bool _canDrag = false;
    private Rigidbody _physicBody;
    private Camera _mainCam;
    private Plane _rayCastPlane;
    
    private void Start()
    {
        
        _mainCam = Camera.main;
        if (!_physicBody)
            _physicBody = gameObject.GetComponent<Rigidbody>();
        
    }

    private void Update()
    {

        if (Input.GetMouseButton(0))
        {
            var mouseMovement = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );
            
            if (mouseMovement != Vector2.zero)
            {
                DragDirection = CalculateDragDirection();
                DragLength = (_mouseWorldPosition - transform.position).magnitude;
            }
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            LaunchBall();

            EraseLines(m_dragLineRenderer);
            EraseLines(m_predictionLineRenderer);
        }
    }

    private Vector3 CalculateDragDirection()
    {
        var currentPosition = transform.position;

        if (_previousPosition != currentPosition)
        {
            //Ray cast plane based on the Drag Mode
            _rayCastPlane = m_mode switch
            {
                DragMode.XY => new Plane(Vector3.back, currentPosition.z),
                DragMode.XZ => new Plane(Vector3.down, currentPosition.y),
                DragMode.ZY => new Plane(Vector3.left, currentPosition.x),
                _ => new Plane(Vector3.down, currentPosition.y)
            };

            _previousPosition = currentPosition;
        }

        if (!_mainCam) return Vector3.zero;
        
        var mousePos = Input.mousePosition;
        mousePos.z = -_mainCam.transform.position.z;
            
        var mouseRayData = _mainCam.ScreenPointToRay(mousePos);
        
        if (_rayCastPlane.Raycast(mouseRayData, out var distance))
        {

            var hitPosition = mouseRayData.GetPoint(distance);
            
            _mouseWorldPosition = new Vector3(hitPosition.x, hitPosition.y, hitPosition.z);
            var inverseMouseWorldPosition = -_mouseWorldPosition + currentPosition * 2;

            //Adjusting with the current position depth in world
            switch (m_mode)
            {
                case DragMode.XY:
                    _mouseWorldPosition.z = currentPosition.z;
                    inverseMouseWorldPosition.z = currentPosition.z;
                    break;
                case DragMode.XZ:
                    _mouseWorldPosition.y = currentPosition.y;
                    inverseMouseWorldPosition.y = currentPosition.y;
                    break;
                case DragMode.ZY:
                    _mouseWorldPosition.x = currentPosition.x;
                    inverseMouseWorldPosition.x = currentPosition.x;
                    break;
                default:
                    _mouseWorldPosition.y = currentPosition.y;
                    inverseMouseWorldPosition.y = _mouseWorldPosition.y;
                    break;
            }
            
            //Drawing visual Line
            DrawStraightLine(m_dragLineRenderer, currentPosition, _mouseWorldPosition);
            DrawStraightLine(m_predictionLineRenderer, currentPosition, inverseMouseWorldPosition);

            return currentPosition - _mouseWorldPosition;
        }

        return Vector3.zero;
        //if (hit.transform.gameObject != gameObject) return Vector3.zero;

    }

    private void LaunchBall()
    {
        _physicBody.velocity = Vector3.zero;
        _physicBody.AddForce(DragDirection * m_launchForce, ForceMode.Impulse);
    }
    
    private void DrawStraightLine(LineRenderer target, Vector3 pointA, Vector3 pointB)
    {
        
        target.enabled = true;

        target.positionCount = 2;
        target.SetPosition(0, pointA);
        target.SetPosition(1, pointB);
    }

    private void EraseLines(LineRenderer target)
    {
        target.enabled = false;
        target.positionCount = 0;
    }
    
    private void OnDrawGizmos()
    {
        var ballPosition = transform.position;
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_mouseWorldPosition, 0.4f);
    }
}
