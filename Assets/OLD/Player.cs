using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _maxSpeed = 0.5f;
    [SerializeField] private float _drag = 0.9f;

    private Vector3 _velocity;

    private Camera _mainCam;
    
    void Start()
    {
        _mainCam = Camera.main;
    }

    void Update()
    {
        var target = _mainCam.ScreenToWorldPoint(Input.mousePosition).SetZ(0);
        
        if (Input.GetMouseButton(1) && MouseInBounds())
        {
            if (Vector3.SqrMagnitude(transform.position - target) >= 0.05f)
            {
                var dir = target - transform.position;
                transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg);

                _velocity += Vector3.ClampMagnitude(target - transform.position, _maxSpeed * Time.deltaTime);
            }
        }
        
        var scaleX = Mathf.Clamp01(1 - _velocity.magnitude/_maxSpeed);
        var scaleY = 1 / scaleX;
        transform.localScale = new Vector3(scaleX, scaleY, 0);

        transform.position += _velocity * Time.deltaTime;

        _velocity *= _drag;
    }

    bool MouseInBounds()
    {
        #if UNITY_EDITOR
            return !(Input.mousePosition.x == 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= Handles.GetMainGameViewSize().y - 1);
        #else
            return !(Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1);
        #endif
    }
}
