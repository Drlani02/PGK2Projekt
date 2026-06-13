using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Preview Setup")]
    public Image previewImage;
    public Sprite[] skinSprites;

    public void ChangePreviewSkin(int index)
    {
        CharacterData.SelectionIndex = index;

        if (index >= 0 && index < skinSprites.Length)
        {
            previewImage.sprite = skinSprites[index];
        }
    }

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
