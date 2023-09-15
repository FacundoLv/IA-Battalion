using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField _redTeamUnitsInput;
    [SerializeField] private TMP_InputField _blueTeamUnitsInput;
    [SerializeField] private TextMeshProUGUI _prompt;
    [SerializeField] private Button _startButton;

    private int _redTeamAmount;
    private int _blueTeamAmount;

    public void StartMatch()
    {
        GameManager.Instance.StartMatch(_redTeamAmount, _blueTeamAmount);
    }

    public void ValidateInput()
    {
        var redTeamIsValid = ValidateEntry( _redTeamUnitsInput.text, out var redAmount);
        var blueTeamIsValid = ValidateEntry(_blueTeamUnitsInput.text, out var blueAmount);

        var isValidInput = redTeamIsValid && blueTeamIsValid;
        _startButton.interactable = isValidInput;
        if (!isValidInput) return;

        _redTeamAmount = redAmount;
        _blueTeamAmount = blueAmount;
        _prompt.text = "Ready to start match!";
    }

    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }

    private bool ValidateEntry(string entry, out int value)
    {
        if (!int.TryParse(entry, out value)) return false;

        var maxUnits = GameManager.Instance.MaxUnits;
        if (value > maxUnits)
        {
            _prompt.text = $"Max units allowed: {maxUnits}";
            return false;
        }

        return  true;
    }
}
