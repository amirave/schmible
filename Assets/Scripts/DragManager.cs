using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance { get; private set; }

    private GameManager _gameManager => GameManager.Instance;

    [SerializeField] private float _balanceSpringStrength;
    [SerializeField] private float _balanceSpringDamper;
    
    private bool _dragging = false;
    private Schmible _schmible = null;

    private Vector3 _velocity;
    private int _vSteps = 10;
    private Vector3 _prevMousePos;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Update()
    {
        if (AppManager.Instance.interactable == false)
            return;
        
        _velocity = (_vSteps - 1) * _velocity / _vSteps + (Input.mousePosition - _prevMousePos) / _vSteps;
        
        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                _schmible = hit.transform.GetComponent<Schmible>();
                if (_schmible != null)
                {
                    _schmible.BeginDrag();
                    _dragging = true;
                    _schmible.onDestroy.AddListener(EndDrag);
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && _schmible)
        {
            if (_velocity.sqrMagnitude > 25 * 25 && _schmible.IsInfected() == false && _schmible.Level >= 2)
            {
                // Disable collider so schmible can shoot past the screen
                _schmible.GetComponent<Collider2D>().enabled = false;
                _schmible.GetComponent<Rigidbody2D>().drag = 0;
                GodManager.Instance.Sacrifice(_schmible);
            }
            else
            {
                var hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                foreach (var hit in hits)
                {
                    var otherSchmible = hit.transform.GetComponent<Schmible>();
                    if (otherSchmible != null && otherSchmible != _schmible)
                    {
                        _schmible.Merge(otherSchmible);
                        break;
                    }
                }
            }

            _schmible.onDestroy.RemoveListener(EndDrag);
            EndDrag();
        }

        if (_dragging)
        {
            // TODO add springs
            var rb = _schmible.GetComponent<Rigidbody2D>();
            var dir = Camera.main.ScreenToWorldPoint(Input.mousePosition).SetZ(0) - _schmible.transform.position;
            
            rb.AddForce(dir.xy() * _balanceSpringStrength - rb.velocity * _balanceSpringDamper);
        }

        _prevMousePos = Input.mousePosition;
    }

    private void EndDrag()
    {
        _schmible?.EndDrag();
        _schmible = null;
        _dragging = false;
    }
}
