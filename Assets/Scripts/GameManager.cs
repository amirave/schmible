using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private SchmibleLevel[] _schmibleLevels;
    [SerializeField] private GameObject _baseSchmiblePrefab;
    [SerializeField] public TMP_Text _scoreText;
    [SerializeField] public DifficultySettings _difficultySettings;
    
    [SerializeField] private float _cameraBalanceSpringStrength;
    [SerializeField] private float _cameraBalanceSpringDamper;
    [SerializeField] private float _summonSpeed = 5f;
    [SerializeField] public float _infectionDistance = 2;


    private float _lastSpawned = 0f;
    private List<Schmible> _schmibles;
    private Camera _mainCam;
    private Rigidbody2D _mainCamRb;
    public float startTime;
    public float CurTime => Time.time - startTime;

    [HideInInspector] public float score;
    private float _originalOrthSize;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (AppManager.Instance == null)
        {
            Debug.Log("h");
            new GameObject("app_manager").AddComponent<AppManager>().BeginGame();
        }

        Time.timeScale = 1;
        startTime = Time.time;

        _mainCam = Camera.main;
        _mainCamRb = _mainCam.GetComponent<Rigidbody2D>();

        _originalOrthSize = _mainCam.orthographicSize;
        
        _schmibles = new List<Schmible>();

        _scoreText.text = "000000";
    }

    void Update()
    {
        _mainCam.orthographicSize = _originalOrthSize * Screen.height / Screen.width;
        
        if (AppManager.Instance.interactable == false)
            return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
            AppManager.Instance.PauseGame();

        _mainCamRb.AddForce(-1 * _cameraBalanceSpringStrength * _mainCam.transform.position.xy() - _cameraBalanceSpringDamper * _mainCamRb.velocity);
        
        if (CurTime - _lastSpawned >= 1 / _summonSpeed)
        {
            var bounds = _mainCam.orthographicSize * new Vector2(Screen.width / Screen.height, 1);
            var spawnPos = new Vector2(Random.Range(-1f, 1f) * bounds.x, Random.Range(-1f, 1f) * bounds.y) * 0.5f;
            var close = true;
            var iter = 0;

            while (close && iter < 10)
            {
                spawnPos = new Vector2(Random.Range(-1f, 1f) * bounds.x, Random.Range(-1f, 1f) * bounds.y) * 0.5f;
                close = GodManager.Instance.laserPositions.Any(t => Utils.DistFromLine(spawnPos, t.Item1, t.Item2) > 0.3f);
                iter++;
            }

            var newSchmible = Instantiate(_baseSchmiblePrefab, spawnPos, Quaternion.identity).GetComponent<Schmible>();
            _schmibles.Add(newSchmible);
            newSchmible.onDestroy.AddListener(() => _schmibles.Remove(newSchmible));
            
            _lastSpawned = CurTime;
        }
    }

    public void GameOver()
    {
        AppManager.Instance.GameOver();
    }

    public void OnDestroy()
    {
        Instance = null;
    }

    public void AddScore(float delta, Vector3 pos)
    {
        // TODO add indicator above pos
        score += delta;
        _scoreText.text = ((int)score).ToString().PadLeft(6, '0');
    }

    public SchmibleLevel GetSchmibleLevel(int index)
    {
        return _schmibleLevels[index];
    }

    public List<Schmible> GetSchmibles()
    {
        return _schmibles;
    }

    public void ShakeScreen(float amount)
    {
        _mainCam?.GetComponent<Rigidbody2D>().AddForce(Vector2.up.Rotate(Random.value * 360) * amount);
    }
}
