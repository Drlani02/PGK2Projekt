using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public GameObject[] models;

    public void SwitchModel(int index)
    {
        for (int i = 0; i < models.Length; i++)
        {
            models[i].SetActive(false);
        }
        if (index >= 0 && index < models.Length)
        {
            models[index].SetActive(true);
        }
    }
}