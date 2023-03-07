using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteRenderer))]
public class Schmible : MonoBehaviour
{
    [HideInInspector]
    public int Level = 0;
    [HideInInspector] 
    public UnityEvent onDestroy;

    [SerializeField] private SpriteRenderer _infectionIndicator;
    private float _maxSpeed;
    private float _erraticness = 0.5f;
    
    private SpriteRenderer _renderer;
    private Collider2D _collider2D;
    
    private SchmibleMode _mode = SchmibleMode.Roam;
    private Vector3 _velocity;
    private Vector2 _direction;
    private Vector3 _prevPosition;

    private float _perlinOffset;
    
    private bool _infected = false;
    private float _infectionEndTime = 0;

    private TimedEvent _infectOther;
    private Rigidbody2D _rigidbody2D;

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
        SetLevel(0);

        _perlinOffset = Random.Range(0f, 100f);
        
        _infectionIndicator.enabled = false;
        _infectionIndicator.transform.localScale = 2 * GameManager.Instance._infectionDistance * Vector3.one;
    }

    void Update()
    {
        _renderer.flipX = _rigidbody2D.velocity.x > 0;
        
        if (_mode == SchmibleMode.Dragged)
            return;
        
        if (_infected)
        {
            if (_infectionEndTime < GameManager.Instance.CurTime)
            {
                var survived = Random.value < GameManager.Instance._difficultySettings.infectionSurvivalChance.GetCurrent(GameManager.Instance.CurTime);

                if (survived)
                {
                    // TODO play heal animation
                    _infected = false;
                    _renderer.color = Color.white;
                    _infectionIndicator.enabled = false;
                }
                else
                {
                    // TODO play death animation
                    Kill();
                }
            }

            if (_infectOther.Occured())
            {
                var maxDist = GameManager.Instance._infectionDistance;

                foreach (var other in GameManager.Instance.GetSchmibles())
                {
                    if (other == this) continue;
                    
                    var dist = Vector3.Distance(transform.position , other.transform.position);
                    if (dist > maxDist) continue;
                    
                    if (Random.value < 1 - Mathf.Pow(dist / maxDist, 2))
                        other.Infect();
                }
            }
        }

        _renderer.sortingOrder = 1000 - Mathf.RoundToInt((5 + transform.position.y) * 100f);

        if (_mode == SchmibleMode.Roam)
        {
            var offset = new Vector2(Mathf.PerlinNoise(_perlinOffset, GameManager.Instance.CurTime * _erraticness), 
                Mathf.PerlinNoise(GameManager.Instance.CurTime * _erraticness, _perlinOffset));
            offset = offset * 2 - Vector2.one;

            // Pull towards middle
            offset += -0.1f * transform.position.xy();

            // foreach (var other in GameManager.Instance.GetSchmibles())
            // {
            //     if (other == this) continue;
            //
            //     if (other.Level >= Level + 3)
            //         offset -= Vector2.ClampMagnitude((other.transform.position - transform.position).Inverse().xy(), 0.3f);
            //     else if (other.Level <= Level - 3)
            //         offset += Vector2.ClampMagnitude((other.transform.position - transform.position).Inverse().xy(), 0.1f);
            // }

            transform.position += _maxSpeed * Time.deltaTime * offset.ToVector3(0);
        }

        if (Level >= 3)
        {
            foreach (var other in GameManager.Instance.GetSchmibles())
            {
                if (other == this) continue;

                if (Level - 3 >= other.Level &&
                    other.GetComponent<Collider2D>().IsTouching(_collider2D))
                {
                    other.Kill();
                }
            }
        }
    }

    public void SetLevel(int level)
    {
        Level = level;
        var data = GameManager.Instance.GetSchmibleLevel(Level);
        _renderer.sprite = data.sprite;
        _maxSpeed = data.maxSpeed;
        
        ((BoxCollider2D)_collider2D).size = _renderer.sprite.bounds.extents;
    }

    public void BeginDrag()
    {
        _mode = SchmibleMode.Dragged;
    }

    public void EndDrag()
    {
        _mode = SchmibleMode.Roam;
    }

    public void Merge(Schmible other)
    {
        if (other.Level != Level)
            return;
        
        if (other._infected)
            Infect();
        
        other.Kill(false);
        GameManager.Instance.AddScore(Mathf.Pow(2.5f, Level) * 20, transform.position);
        GameManager.Instance.ShakeScreen(60f);
        
        SetLevel(Level + 1);
    }

    public void Infect()
    {
        // TODO set icon above head
        // TODO popup explaining

        _infected = true;
        _infectionEndTime = GameManager.Instance.CurTime + Random.Range(Constants.minInfectionSurvivalTime, Constants.maxInfectionSurvivalTime);
        
        _renderer.color = Color.green;
        _infectionIndicator.enabled = true;
        
        _infectOther = new TimedEvent(2f, 0.5f);
    }

    public bool IsInfected()
    {
        return _infected;
    }

    public void Kill(bool shake = true)
    {
        if (_mode == SchmibleMode.Dragged)
            return;

        if (shake)
            GameManager.Instance.ShakeScreen(70f);
        onDestroy.Invoke();
        Destroy(gameObject);
    }
}

public enum SchmibleMode
{
    Roam,
    Attack,
    Escape,
    Dragged
}
