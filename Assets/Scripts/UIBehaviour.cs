using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBehaviour : MonoBehaviour
{
    // --- Core Stats ---
    public string companyName;
    public long companyCash = 7000000;
    public int infrastructure = 0;
    public int engineers = 0;
    public int marketers = 0;
    public int employees => engineers + marketers;
    public int aiLevel = 0;
    private int nextAILevel = 1;
    public int currentAILearningHours = 0;
    public int totalAILearningHours = 0;
    int users = 0;

    // --- Costs & Balancing ---
    [Header("Balancing")]
    [SerializeField] private int baseInfrastructure = 1;
    [SerializeField] private int baseEmployees = 5;
    [SerializeField] private int baseCost = 500000;
    [SerializeField] private int infrastructureCost = 750000;
    [SerializeField] private int dailyCostPerEmployee = 320;

    // --- UI References ---
    [Header("Top Bar")]
    [SerializeField] private GameObject companyNameText;
    [SerializeField] private GameObject companyCashText;

    [Header("Panels")]
    [SerializeField] private GameObject buildAIPanel;
    [SerializeField] private GameObject employeePanel;
    [SerializeField] private GameObject infrastructurePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject manageAIPanel;
    [SerializeField] private GameObject aiChatPanel;

    [Header("Buttons")]
    [SerializeField] private Button aiChatButton;
    [SerializeField] private Button releaseAIButton;

    [Header("Counts & Requirements")]
    [SerializeField] private GameObject infrastructureCount;
    [SerializeField] private GameObject employeeCount;
    [SerializeField] private GameObject aiIntelCount;
    [SerializeField] private GameObject timeCount;
    [SerializeField] private GameObject infraRequirementsText;
    [SerializeField] private GameObject employeeRequirementsText;
    [SerializeField] private GameObject costRequirementsText;
    [SerializeField] private GameObject infrastructureCostRequirementsText;
    [SerializeField] private GameObject userCountText;
    [SerializeField] public GameObject currentLearningHoursText;
    [SerializeField] public GameObject totalLearningHoursText;

    private GameObject activePanel;

    // --- In-game Time ---
    private float timeCounter = 0f;
    private float secondsPerDay = 5f;
    private int currentDay = 1;
    private int currentYear = 1;
    private int lastDayCharged = -1;

    // ---------------- Unity Lifecycle ----------------
    void Start()
    {
        // All panels start closed
        buildAIPanel.SetActive(false);
        employeePanel.SetActive(false);
        infrastructurePanel.SetActive(false);
        settingsPanel.SetActive(false);
        manageAIPanel.SetActive(false);

        LoadGame();
        RefreshUI();
    }

    void Update()
    {
        releaseAIButtonHandler();
        AIChatHandeler();
        // In-game
        // time + costs logic unchanged
        timeCounter += Time.deltaTime;
        if (timeCounter >= secondsPerDay)
        {
            Payout();
            users += (int)(users * 0.05f); // 5% daily growth
            if (users < 0) users = 0; // prevent negative users
            
            currentDay++;
            timeCounter = 0f;

            if (currentDay > 365)
            {
                currentYear++;
                currentDay = 1;
            }

            SaveGame();
            RefreshUI();

        }

        if (currentDay > lastDayCharged)
        {
            DeductDailyEmployeeCost();
            InfrastructureUpkeep();
            lastDayCharged = currentDay;
            SaveGame();
        }

        timeCount.GetComponent<TMPro.TMP_Text>().text =
            $"Year: {currentYear} Day: {currentDay}";
    }

    void OnApplicationQuit() => SaveGame();
    void OnApplicationPause(bool pause) { if (pause) SaveGame(); }

    // ---------------- UI Methods ----------------
    public void OpenPanel(GameObject panel)
    {
        if (activePanel != null) activePanel.SetActive(false);
        panel.SetActive(true);
        activePanel = panel;
    }

    public void CloseActivePanel()
    {
        if (activePanel != null)
        {
            activePanel.SetActive(false);
            activePanel = null;
        }
    }

    void Payout()
            { 
                int payout = users * 5; // $5 per user
                companyCash += payout;
                Debug.Log($"A payout of ${payout} received from {users} users.");
            }

    public void BuildAI()
    {
        int requiredInfrastructure = baseInfrastructure * (int)Mathf.Pow(2, aiLevel);
        int requiredEmployees = baseEmployees * (int)Mathf.Pow(2, aiLevel);
        int requiredCost = baseCost * (int)Mathf.Pow(2, aiLevel);

        if (infrastructure >= requiredInfrastructure &&
            employees >= requiredEmployees &&
            companyCash >= requiredCost)
        {
            companyCash -= requiredCost;
            aiLevel++;
            UpdateRequirementsText();
            SaveGame();
            RefreshUI();
        }
        else
        {
            Debug.Log($"Requirements not met. Need: {requiredInfrastructure} infra, {requiredEmployees} employees, ${requiredCost:N0} cash");
        }
    }

    void UpdateRequirementsText()
    {
        int requiredInfrastructure = baseInfrastructure * (int)Mathf.Pow(2, aiLevel);
        int requiredEmployees = baseEmployees * (int)Mathf.Pow(2, aiLevel);
        int requiredCost = baseCost * (int)Mathf.Pow(2, aiLevel);

        infraRequirementsText.GetComponent<TMPro.TMP_Text>().text = $"Infrastructure Required: {requiredInfrastructure}";
        employeeRequirementsText.GetComponent<TMPro.TMP_Text>().text = $"Employees Required: {requiredEmployees}";
        costRequirementsText.GetComponent<TMPro.TMP_Text>().text = $"-${requiredCost:N0}";
    }

    public void BuildInfrastructure()
    {
        int cost = infrastructureCost * (infrastructure + 1);
        if (companyCash >= cost)
        {
            companyCash -= cost;
            infrastructure++;
            UpdateInfrastructureCost();
            SaveGame();
            RefreshUI();
        }
        else
        {
            Debug.Log("Not enough cash to build infrastructure.");
        }
    }

    void UpdateInfrastructureCost()
    {
        int nextCost = infrastructureCost * (infrastructure + 1);
        infrastructureCostRequirementsText.GetComponentInChildren<TMPro.TMP_Text>().text =
            $"-${nextCost:N0}";
    }

    private void InfrastructureUpkeep()
    {
        int upkeepCost = (int)(infrastructureCost * infrastructure * 0.01f);
        companyCash -= upkeepCost;
        Debug.Log($"Infrastructure upkeep deducted: ${upkeepCost}");
        RefreshUI();
    }

    public void HireEngineer()
    {
        int hireCost = 50000;
        if (companyCash >= hireCost)
        {
            companyCash -= hireCost;
            engineers++;
            SaveGame();
            RefreshUI();
        }
        else
        {
            Debug.Log("Not enough money to hire!");
        }
    }

    private void DeductDailyEmployeeCost()
    {
        int totalDailyCost = employees * dailyCostPerEmployee;
        companyCash -= totalDailyCost;
        Debug.Log($"Daily employee cost deducted: ${totalDailyCost}");
        RefreshUI();
    }

    public void ReleaseAI()
    {

        bool canReleaseByLevel = aiLevel >= 1 && aiLevel == nextAILevel;
        bool canReleaseByTraining = currentAILearningHours >= 1;
        

        if (canReleaseByLevel || canReleaseByTraining)
        {
            // Update state
            float baseGrowth = Mathf.Pow(aiLevel, 2) * (currentAILearningHours + 1) * 1000;
            float variation = Random.Range(0.8f, 5.0f); // random variation

            if (canReleaseByTraining)
            {
                totalAILearningHours += currentAILearningHours;
                currentAILearningHours = 0;
                users = (int)(users * variation);
            }


            if (canReleaseByLevel)
            {

                users = (int)(users + (baseGrowth * variation));
                nextAILevel++;
                SaveGame();
                RefreshUI();
                // only bump next level when releasing a new AI
            }
            SaveGame();
            RefreshUI();

            // Record training so only further progress counts


            Debug.Log($"AI released at level {aiLevel}, users now {users}");
        }
        else
        {
            Debug.Log("No new AI to release. Train more or build next level.");
        }
    }

    private void releaseAIButtonHandler()
    {
        TextMeshProUGUI buttonText = releaseAIButton.GetComponentInChildren<TextMeshProUGUI>();
        if (aiLevel < nextAILevel && currentAILearningHours == 0)
        {
            releaseAIButton.interactable = false;
            buttonText.color = new Color32(255, 255, 255, 25); // faded
        }
        else
        {
            releaseAIButton.interactable = true;
            buttonText.color = new Color32(255, 255, 255, 255); //normal
        }
    }

    void AIChatHandeler()
    { 
        if (aiLevel >= 1)
        {
            aiChatButton.interactable = true;
            
        }
        else
        {
            aiChatButton.interactable = false;
            
            
        }
    }
    // ---------------- Persistence ----------------
    public void SaveGame()
    {
        PlayerPrefs.SetString("CompanyName", companyName);
        PlayerPrefs.SetInt("CompanyCash", (int)companyCash);
        PlayerPrefs.SetInt("Infrastructure", infrastructure);
        PlayerPrefs.SetInt("employees", employees);
        PlayerPrefs.SetInt("Engineers", engineers);
        PlayerPrefs.SetInt("Marketers", marketers);
        PlayerPrefs.SetInt("AILevel", aiLevel);
        PlayerPrefs.SetInt("NextAILevel", nextAILevel);
        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.SetInt("CurrentYear", currentYear);
        PlayerPrefs.SetInt("LastDayCharged", lastDayCharged);
        PlayerPrefs.SetInt("Users", users);
        PlayerPrefs.SetInt("CurrentAILearningHours", currentAILearningHours);
        PlayerPrefs.SetInt("TotalAILearningHours", totalAILearningHours);
        


        // Save real-world time
        PlayerPrefs.SetString("LastSaveTime", System.DateTime.Now.ToBinary().ToString());

        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        companyName = PlayerPrefs.GetString("CompanyName", "My Company");
        companyCash = PlayerPrefs.GetInt("CompanyCash", (int)companyCash);
        infrastructure = PlayerPrefs.GetInt("Infrastructure", 0);
        engineers = PlayerPrefs.GetInt("Engineers", 0);
        marketers = PlayerPrefs.GetInt("Marketers", 0);
        // employees is now a property, so no need to load it
        aiLevel = PlayerPrefs.GetInt("AILevel", 0);
        nextAILevel = PlayerPrefs.GetInt("NextAILevel", 1);
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        currentYear = PlayerPrefs.GetInt("CurrentYear", 1);
        lastDayCharged = PlayerPrefs.GetInt("LastDayCharged", 0);
        users = PlayerPrefs.GetInt("Users", 0);
        currentAILearningHours = PlayerPrefs.GetInt("CurrentAILearningHours", 0);
        totalAILearningHours = PlayerPrefs.GetInt("TotalAILearningHours", 0);

        // Handle offline progression
        string lastSaveTimeString = PlayerPrefs.GetString("LastSaveTime", "");
        if (!string.IsNullOrEmpty(lastSaveTimeString))
        {
            long binaryTime = long.Parse(lastSaveTimeString);
            System.DateTime lastSaveTime = System.DateTime.FromBinary(binaryTime);
            System.TimeSpan timeAway = System.DateTime.Now - lastSaveTime;
 
            int daysAway = Mathf.FloorToInt((float)timeAway.TotalSeconds / secondsPerDay);
            if (daysAway > 0)
            {
                Debug.Log($"Player was away for {daysAway} in-game days. Applying upkeep...");
                for (int i = 0; i < daysAway; i++)
                {
                    currentDay++;
                    if (currentDay > 365)
                    {
                        currentYear++;
                        currentDay = 1;
                    }

                    DeductDailyEmployeeCost();
                    InfrastructureUpkeep();
                    Payout();
                    users += (int)(users * 0.05f); // 5% daily growth
                }
            }
        }
    }

    public void RefreshUI()
    {
        companyNameText.GetComponent<TMPro.TMP_Text>().text = companyName;
        companyCashText.GetComponent<TMPro.TMP_Text>().text = $"${companyCash:N0}";
        userCountText.GetComponent<TMPro.TMP_Text>().text = $"Users: {users:N0}";
        infrastructureCount.GetComponent<TMPro.TMP_Text>().text = $"Infrastructure Level: {infrastructure}";
        employeeCount.GetComponent<TMPro.TMP_Text>().text = $"Employees: {employees}";
        aiIntelCount.GetComponent<TMPro.TMP_Text>().text = $"AI Intelligence: V{aiLevel}";
        timeCount.GetComponent<TMPro.TMP_Text>().text = $"Year: {currentYear} Day: {currentDay}";
        currentLearningHoursText.GetComponent<TMPro.TMP_Text>().text = $"Current Learning Hours: {currentAILearningHours}";
        totalLearningHoursText.GetComponent<TMPro.TMP_Text>().text = $"Total Learning Hours: {totalAILearningHours}";

        UpdateRequirementsText();
        UpdateInfrastructureCost();
    }
}
