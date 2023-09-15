using System;
using System.Collections;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public event Action OnMatchReady;
    
    [SerializeField] private GroundUnit _redUnit;
    [SerializeField] private GroundUnit _blueUnit;
    public int MaxUnits => MAX_UNITS;

    private const int MAX_UNITS = 20;

    private int _redAmount;
    private int _blueAmount;

    private void Awake()
    {
        if (IsAwake) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) EndMatch();
    }

    public void StartMatch(int redTeamAmount, int blueTeamAmount)
    {
        _redAmount = redTeamAmount;
        _blueAmount = blueTeamAmount;
        SceneManager.LoadSceneAsync(1);
    }

    public void EndMatch()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BattleScene") StartCoroutine(HandleMatch());
    }

    private IEnumerator HandleMatch()
    {
        var bosses = FindObjectsOfType<BossUnit>();
        var red = GetBoss(bosses, 0);
        var blue = GetBoss(bosses, 1);

        yield return StartCoroutine(SpawnMinions(red, _redUnit, _redAmount));
        yield return StartCoroutine(SpawnMinions(blue, _blueUnit, _blueAmount));

        OnMatchReady?.Invoke();

        yield return new WaitForSeconds(1.5f);

        red.Init();
        blue.Init();
    }

    private static BossUnit GetBoss(BossUnit[] bosses, int teamID)
    {
        return bosses.FirstOrDefault(n => n.TeamID == teamID);
    }

    private IEnumerator SpawnMinions(BossUnit boss, GroundUnit unit, int unitAmount)
    {
        var healthSpots = FindObjectsOfType<HealthSpot>()
            .Where(n => n.TeamID == boss.TeamID)
            .ToArray();

        for (var i = 0; i < unitAmount; i++)
        {
            var spawnPos = healthSpots[i % healthSpots.Length].transform.position;

            var newUnit = Instantiate(unit, spawnPos, Quaternion.identity);
            newUnit
                .GetComponent<LeaderBehavior>()
                .target = boss.transform;
            
            boss.AddMinion(newUnit);

            yield return new WaitForSeconds(.25f);
        }
    }
}
