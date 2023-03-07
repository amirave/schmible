using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Instance;
    
    [HideInInspector] public Camera mainCam;
    
    [SerializeField] public Player player;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _summonSpeed = 5f;

    private float _lastSpawned = 0f;

    private List<Enemy> _enemies;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        _enemies = new List<Enemy>();
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - _lastSpawned >= 1 / _summonSpeed)
        {
            SummonEnemy();
            _lastSpawned = Time.time;
        }
    }

    private void SummonEnemy()
    {
        var bounds = new Vector2(mainCam.orthographicSize * Screen.width / Screen.height, mainCam.orthographicSize) * 1.3f;
        Vector2 spawnPos;

        var sign = Random.Range(0, 2) * 2 - 1;

        if (Random.value > 0.5)
            spawnPos = new Vector2(sign * bounds.x, Random.Range(-bounds.y, bounds.y));
        else
            spawnPos = new Vector2(Random.Range(-bounds.x, bounds.x), sign * bounds.y);

        spawnPos += mainCam.transform.position.xy();
        
        var enemy = Instantiate(_enemyPrefab, spawnPos, Quaternion.identity).GetComponent<Enemy>();
        _enemies.Add(enemy);
    }

    public void UnloadEnemy(Enemy scrap)
    {
        _enemies.Remove(scrap);
        Destroy(scrap.gameObject);
    }
}