using UnityEngine;
using TMPro; 

public class EndGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerTextLabel;

    private void Start()
    {
        if (ResultManager.Instance != null)
        {
            winnerTextLabel.text = ResultManager.Instance.WinnerMessage;
        }
        else
        {
            Debug.LogWarning("Nie znaleziono ResultManagera w scenie końcowej!");
        }
    }
}