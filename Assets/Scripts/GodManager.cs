using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GodManager : MonoBehaviour
{
    public static GodManager Instance { get; private set; }

    [SerializeField] private Slider _angerSlider;
    [SerializeField] private float _infectionInterval;
    [SerializeField] private float _laserInterval;

    [SerializeField] private int maxPopulationForInfection;

    [SerializeField] private float _laserWarningTime = 5;
    [SerializeField] private float _laserHangTime = 5;
    [SerializeField] private float _laserBlinkSpeed = 2;
    [SerializeField] private float _laserBlinkExp = 1.1f;
    [SerializeField] private float _laserScreenShake;
    [SerializeField] private float _laserMoveSpeed = 0.4f;
    [SerializeField] private LineRenderer _laserWarningLine;
    [SerializeField] private LineRenderer _laserLine;

    public float _anger = 0;
    private float _angerRate;

    private TimedEvent _infectionEvent;
    private TimedEvent _laserEvent;

    public List<Tuple<Vector2, Vector2>> laserPositions;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        _infectionEvent = new TimedEvent(_infectionInterval, _anger);
        _laserEvent = new TimedEvent(_laserInterval, _anger);

        laserPositions = new List<Tuple<Vector2, Vector2>>();
    }

    void Update()
    {
        if (AppManager.Instance.interactable == false)
            return;
        
        _angerRate = GameManager.Instance._difficultySettings.godMeterRate.GetCurrent(GameManager.Instance.CurTime);
        
        _anger += _angerRate * Time.deltaTime;
        _anger = Mathf.Clamp01(_anger);
        
        _angerSlider.value = _anger;
        
        _infectionEvent.SetChance(_anger - 0.5f);
        _laserEvent.SetChance(_anger - 0.25f);

        var schmibles = GameManager.Instance.GetSchmibles();

        if (_anger >= 1)
            GameManager.Instance.GameOver();
        
        if (_infectionEvent.Occured() &&
            schmibles.Count != 0 && 
            Random.value < 1 - schmibles.Count / maxPopulationForInfection)
        {
            Debug.Log("INFETION");
            schmibles.PickRandom().Infect();
        }

        if (_laserEvent.Occured() && !GameManager.Instance.GetSchmibles().Any(s => s.IsInfected()))
        {
            SummonLaser();
        }
    }

    private async UniTask SummonLaser()
    {
        Debug.Log("LASER START");

        var newLaserWarningLine = Instantiate(_laserWarningLine);

        var orthSize = Camera.main.orthographicSize;
        var bounds = new Vector2(orthSize * Screen.width / Screen.height, orthSize) * 1.1f;
        var points = new Vector2[2];

        if (Random.value > 0.5)
        {
            points[0] = new Vector2(bounds.x, Random.Range(-bounds.y, bounds.y) * 0.5f);
            points[1] = new Vector2(-1 * bounds.x, Random.Range(-bounds.y, bounds.y) * 0.5f);
        }
        else
        {
            points[0] = new Vector2(Random.Range(-bounds.x, bounds.x) * 0.5f, bounds.y);
            points[1] = new Vector2(Random.Range(-bounds.x, bounds.x) * 0.5f, -1 * bounds.y);
        }

        points = points.OrderBy(p => p.y).ToArray();

        var avg = (points[0] + points[1]) * 0.5f;
        points[0] = 5 * (points[0] - avg) + avg;
        points[1] = 5 * (points[1] - avg) + avg;

        var pointsTuple = new Tuple<Vector2, Vector2>(points[0], points[1]);
        laserPositions.Add(pointsTuple);

        var angle = Mathf.Atan2(points[1].y - points[0].y, points[1].x - points[0].x) * Mathf.Rad2Deg;
        angle += Random.value > 0.5 ? 90 : 270;
        var perpendicular =
            Vector2.right.Rotate(angle);

        newLaserWarningLine.positionCount = 2;
        newLaserWarningLine.SetPositions(points.Select(p => p.ToVector3(0)).ToArray());

        var blinks = 0;

        while (blinks < 20 * _laserBlinkSpeed)
        {
            newLaserWarningLine.enabled = blinks % 2 == 0;
            await UniTask.Delay((int) (1000 * (_laserBlinkExp - 1) * _laserWarningTime * Mathf.Pow(_laserBlinkExp, -1 * blinks) / _laserBlinkSpeed));
            
            blinks++;
        }
        
        Debug.Log("LASER BLINK END");

        Destroy(newLaserWarningLine);
        var newLaserLine = Instantiate(_laserLine);
        
        newLaserLine.positionCount = 2;
        newLaserLine.SetPositions(points.Select(p => p.ToVector3(0)).ToArray());

        await UniTask.Delay(300);
        
        newLaserLine.enabled = true;

        var laserStart = GameManager.Instance.CurTime;

        while (laserStart + _laserHangTime > GameManager.Instance.CurTime)
        {
            GameManager.Instance.ShakeScreen(_laserScreenShake);
            var hits = Physics2D.LinecastAll(points[0], points[1]);
            
            foreach (var hit in hits)
            {
                var schmible = hit.transform.GetComponent<Schmible>();
                if (schmible != null && schmible.IsInfected() == false)
                    schmible.Kill();
            }

            points[0] += perpendicular * (Time.deltaTime * _laserMoveSpeed);
            points[1] += perpendicular * (Time.deltaTime * _laserMoveSpeed);
            newLaserLine.SetPositions(points.Select(p => p.ToVector3(0)).ToArray());

            await UniTask.WhenAny(UniTask.Yield(PlayerLoopTiming.Update).ToUniTask(), UniTask.WaitUntil(() => AppManager.Instance.interactable));
        }

        Destroy(newLaserLine);

        laserPositions.Remove(pointsTuple);
    }

    public async UniTask Sacrifice(Schmible schmible)
    {
        await UniTask.Delay(500);
        
        Debug.Log("SACRIFICED SCHMIBLE");
        schmible.Kill();
        GameManager.Instance.ShakeScreen(300f);
        _anger -= Mathf.Pow(3f, schmible.Level) * 0.005f;
    }
}
