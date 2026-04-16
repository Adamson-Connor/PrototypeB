using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class HudController : MonoBehaviour
{
    // private bool isPaused;
    // private bool isLosing;
    // private int LostCount = 0;
    [SerializeField]
    private FadeController fadeController;
   [SerializeField] private PlayerInput playerInput;
    public InputActionAsset inputActions;
    public static HudController instance;
    [SerializeField] GameObject SpoonHud;
    [SerializeField] GameObject LoseUI;
    [SerializeField] GameObject PauseUI;
    public ProgressBar ProgBar;
   // public TeleportManager TeleportManager;
    public StumbleScript StumbleScript;
    [SerializeField] int StairVal;
    [SerializeField] int DoorVal;
    [SerializeField] int StumbleVal = 1;

    private int MaxObjective;
    [SerializeField] int CurrentObjective;
    private void Awake()
    {
        instance = this;
    }

    [SerializeField] TMP_Text interactiontext;

    private void Start()
    {
        SpoonHud.GetComponent<ProgressBar>();
        ProgBar.GetProgress();
        //As prototype is short with no saves I can start with full bar and status but if tweaking in future make sure status is known on level start
    }
    private void Update()
    {
        
    }
    public void Pause()
    {
       // isPaused = true;
        PauseUI.SetActive(true);
        Time.timeScale = 0;
        ChangeActionMaptoUI();
    }
    private void Updateplayerstatus()
    {
        ProgBar.GetProgress();
        ProgBar.PainStatus();

    }
    public void StumbleInteraction()
    {
        ProgBar.ChangeResourceAmount(StumbleVal);
        // ProgBar.Spoon = StumbleVal;
        //  ProgBar.DecreaseProgress();
        Updateplayerstatus();
    }
    public void DoorInteraction()
    {
        //  TeleportManager.DestinationTP();
        // ProgBar.Spoon = DoorVal;
        //ProgBar.DecreaseProgress();

        ProgBar.ChangeResourceAmount(DoorVal);   
        Updateplayerstatus();
    }
    public void StairInteraction()
    {
        ProgBar.ChangeResourceAmount(StairVal);
      //  ProgBar.Spoon = StairVal;
      //  ProgBar.DecreaseProgress();
        Updateplayerstatus();
        //  TeleportManager.DestinationTP();
    }
    /*private void UpdateObjective()
    {
        if (CurrentObjective == MaxObjective)
            return;
        CurrentObjective++;
    }*/
    public void PauseTime()
    {
        Time.timeScale = 0;
    }
    public void ResumeTime()
    {
        Time.timeScale = 1;
    }
    public void Lose()
    {
        fadeController.LoadScene("MainMenu");
    }
    public void ChangeActionMaptoUI()
    { // use to to switch between UI and not UI
       
        
            Cursor.lockState = CursorLockMode.None;
            playerInput.SwitchCurrentActionMap("UI");
            Debug.Log("switched to ui action map");
    }
    public void ChangeActionMaptoPlayer()
    {
        playerInput.SwitchCurrentActionMap("Player");
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Changed AM To Player");
    }
    public void Resume()
    {
     /* if (isLosing == true)
        {
            LostCount += 1;
           
        }*/
     //isPaused = false;
      //  isLosing = false;
        Time.timeScale = 1;
        LoseUI.SetActive (false);
        PauseUI.SetActive(false);
        ChangeActionMaptoPlayer();
        /*If Open, Close Lose/pause Screen
        Increase progress by x amount that can be changed in editor for balancing
        animation? after a lose
        */
    }
    public void EnableInteractionText(string text)
    {
        interactiontext.text = text + " (Interact) ";
        interactiontext.gameObject.SetActive(true);
    }
    public void DisableInteractionText()
    {
        interactiontext.gameObject.SetActive(false);
    }
}




    