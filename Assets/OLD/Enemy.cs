using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyMode _mode = EnemyMode.Attack;
    private Transform _player => Manager.Instance.player.transform;

    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _drag = 0.95f;

    private Vector3 _velocity;

    private void Start()
    {
        
    }

    private void Update()
    {
        // TODO if player is stronger, escape
        
        var dir = _player.position - transform.position;
        transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg);
        
        _velocity += Vector3.ClampMagnitude(_player.position - transform.position, _maxSpeed * Time.deltaTime);

        transform.position += _velocity * Time.deltaTime;
        _velocity *= 1 - (1 - _drag) * Time.deltaTime;
    }
}

public enum EnemyMode
{
    Roam,
    Attack,
    Escaping
}