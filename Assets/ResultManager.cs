using UnityEngine;
using TMPro;

public class ResultManager : MonoBehaviour
{
    public static ResultManager Instance;

    public string WinnerMessage { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetWinner(string message)
    {
        WinnerMessage = message;
    }
}