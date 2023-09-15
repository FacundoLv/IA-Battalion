using System.Collections;
using TMPro;
using UnityEngine;

public class MatchScreen : MonoBehaviour
{
    [SerializeField] private BossUnit _red;
    [SerializeField] private BossUnit _blue;

    private TextMeshProUGUI _feedback;
    private CanvasGroup _canvasGroup;
    private bool _hasFinished;

    private void Awake()
    {
        _feedback = GetComponentInChildren<TextMeshProUGUI>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        _red.OnUnitDown += ShowWinner;
        _blue.OnUnitDown += ShowWinner;
        GameManager.Instance.OnMatchReady += ShowMatchReady;
    }

    private void OnDisable()
    {
        _red.OnUnitDown -= ShowWinner;
        _blue.OnUnitDown -= ShowWinner;
        GameManager.Instance.OnMatchReady -= ShowMatchReady;
    }

    private void ShowMatchReady()
    {
        StartCoroutine(MatchReadyCo());
    }

    private IEnumerator MatchReadyCo()
    {
        _canvasGroup.alpha = 1;
        _feedback.text = "Match Start!";
        yield return new WaitForSeconds(1.5f);
        _canvasGroup.alpha = 0;
    }

    private void ShowWinner(IUnit unit)
    {
        if (_hasFinished) return;
        _hasFinished = true;

        _canvasGroup.alpha = 1;
        var bWinner = unit.TeamID == 1;
        _feedback.text = $"{(bWinner ? "Red" : "Blue")} team wins!";
        _feedback.color = bWinner ? Color.red : Color.blue;
        
        Invoke(nameof(GoBackToMainMenu), 3f);
    }

    private void GoBackToMainMenu()
    {
        GameManager.Instance.EndMatch();
    }
}
