using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TMPDropdownHandler : MonoBehaviour
{
    public UIBehaviour uiBehaviour;
    public TMP_Dropdown tmpDropdown;
    public Button beginMachineLearningButton;
    public TextMeshProUGUI mlTimer;

    private int selectedIndex;
    private bool isTraining = false;
    private DateTime trainingEndTime; 

    void Start()
    {
        tmpDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        beginMachineLearningButton.onClick.AddListener(BeginMachineLearning);
        mlTimer.text = "";

        selectedIndex = tmpDropdown.value;

        // Load persistent training data
        if (PlayerPrefs.HasKey("TrainingEndTime"))
        {
            string endTimeStr = PlayerPrefs.GetString("TrainingEndTime");
            trainingEndTime = DateTime.Parse(endTimeStr);
            selectedIndex = PlayerPrefs.GetInt("TrainingIndex", 0);

            if (DateTime.Now < trainingEndTime)
            {
                isTraining = true;
                StartCoroutine(SimulateMachineLearningPersistent());
            }
            else
            {
                FinishTraining(selectedIndex);
            }
        }

        UpdateButtonState();
    }

    void Update()
    {
        UpdateButtonState();
    }

    void OnDropdownValueChanged(int index)
    {
        selectedIndex = index;
        UpdateButtonState();
    }

    private (float duration, int cost) GetTrainingData()
    {
        switch (selectedIndex)
        {
            case 0: return (3600f, 250000);     // 1 hour
            case 1: return (10800f, 750000);    // 3 hours
            case 2: return (18000f, 1250000);   // 5 hours
            default: return (0f, 0);
        }
    }

    private void UpdateButtonState()
    {
        var (_, cost) = GetTrainingData();
        var buttonText = beginMachineLearningButton.GetComponentInChildren<TextMeshProUGUI>();

        if (uiBehaviour.companyCash < cost || uiBehaviour.aiLevel < 1 || isTraining)
        {
            beginMachineLearningButton.interactable = false;
            buttonText.color = new Color32(255, 255, 255, 25);
        }
        else
        {
            beginMachineLearningButton.interactable = true;
            buttonText.color = new Color32(255, 255, 255, 255);
        }
    }

    public void BeginMachineLearning()
    {
        var (duration, cost) = GetTrainingData();

        if (uiBehaviour.companyCash < cost) return;

        uiBehaviour.companyCash -= cost;
        uiBehaviour.SaveGame();

        trainingEndTime = DateTime.Now.AddSeconds(duration);
        PlayerPrefs.SetString("TrainingEndTime", trainingEndTime.ToString());
        PlayerPrefs.SetInt("TrainingIndex", selectedIndex);
        PlayerPrefs.Save();

        isTraining = true;
        tmpDropdown.interactable = false;
        beginMachineLearningButton.GetComponentInChildren<TextMeshProUGUI>().text = "Processing...";

        StartCoroutine(SimulateMachineLearningPersistent());
    }

    private IEnumerator SimulateMachineLearningPersistent()
    {
        while (DateTime.Now < trainingEndTime)
        {
            TimeSpan remaining = trainingEndTime - DateTime.Now;
            mlTimer.text = $"{remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            yield return new WaitForSeconds(1f);
        }

        FinishTraining(selectedIndex);
    }

    private void FinishTraining(int index)
    {
        mlTimer.text = "Training Complete!";
        beginMachineLearningButton.GetComponentInChildren<TextMeshProUGUI>().text = "Begin Machine Learning";
        beginMachineLearningButton.interactable = true;
        tmpDropdown.interactable = true;
        isTraining = false;

        // Clear PlayerPrefs
        PlayerPrefs.DeleteKey("TrainingEndTime");
        PlayerPrefs.DeleteKey("TrainingIndex");

        switch (index)
        {
            case 0: uiBehaviour.currentAILearningHours += 1; break;
            case 1: uiBehaviour.currentAILearningHours += 3; break;
            case 2: uiBehaviour.currentAILearningHours += 5; break;
        }

        uiBehaviour.currentLearningHoursText.GetComponent<TMP_Text>().text =
            $"New Learning Hours: {uiBehaviour.currentAILearningHours}";
        uiBehaviour.SaveGame();
        uiBehaviour.RefreshUI();
    }
}
