using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{

    public Button resetButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resetButton.onClick.AddListener(ResetGame);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    
}
