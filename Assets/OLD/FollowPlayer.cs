using System.Collections;
using System.Collections.Generic;
using LlamAcademy.Spring;
using LlamAcademy.Spring.Runtime;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform _player;

    private SpringToTarget2D _spring;
    
    void Start()
    {
        _spring = GetComponent<SpringToTarget2D>();
    }

    void Update()
    {
        transform.position = _player.position + Vector3.back * 10;
    }
}
