using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerTextLabel;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (ResultManager.Instance != null)
        {
            winnerTextLabel.text = ResultManager.Instance.WinnerMessage;
        }
        else
        {
            Debug.LogWarning("Nie znaleziono ResultManagera w scenie końcowej!");
        }
    }
    public void Back()
    {
        SceneManager.LoadScene(0);
    }

}