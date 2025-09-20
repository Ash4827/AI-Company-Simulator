using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuBehavior : MonoBehaviour
{
    public Button StartButton;
    public Button FinishedButton;
    public GameObject companyNamePanel;
    string companyName;

    bool startButtonPressed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        companyNamePanel.SetActive(false);
        StartButton.onClick.AddListener(OnStartClick);
        FinishedButton.onClick.AddListener(OnFinishedClick);
        if (PlayerPrefs.GetInt("GameStarted", 0) == 1)
        {
            StartButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Resume Game";
            
        }
        else
        {
            StartButton.GetComponentInChildren<TMPro.TMP_Text>().text = "New Game";
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnStartClick()
    {
        if (PlayerPrefs.GetInt("GameStarted", 0) == 1) //if game already started, skip to main scene
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            return;
        }
        startButtonPressed = true;
        if (startButtonPressed)
        {
            PlayerPrefs.SetInt("GameStarted", 1);
            companyNamePanel.SetActive(true);
            return;
        }        
    }

    void OnFinishedClick()
    {
        companyName = companyNamePanel.GetComponentInChildren<TMPro.TMP_InputField>().text;

        if (companyName == "")
        {
            companyName = "MyCompany";
        }
        PlayerPrefs.SetString("CompanyName", companyName);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
