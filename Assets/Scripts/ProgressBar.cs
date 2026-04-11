using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header ("Main Settings")]
    [SerializeField] Image bar;
    [SerializeField] int CurrentValue = 100;
    [SerializeField] int MaxValue = 100;
    [SerializeField] int MinValue = 0;
    [Space]
    [SerializeField] private bool OverkillMode;

    [SerializeField]StumbleScript StumbleScript;
    [SerializeField] Interactable InteractionScript;
    [SerializeField] GameObject HudManagerObj;
    //public GameObject SpoonPrefab;
  //  public Color enabledColour;
 //   public Color disabledColour;
  //  public bool StartFull= false;
    [SerializeField] HudController hudManager;
    public int Spoon = 0;
   /* [SerializeField]
    GameObject Orange;
    [SerializeField]
    GameObject Yellow;
    [SerializeField]
    GameObject UnHurt;
    [SerializeField]
    GameObject Red;
    [SerializeField]
    int YellowNum;
    [SerializeField]
    int OrangeNum;
    */
   
   
   

    List<SpoonScript> Spoons = new List<SpoonScript>();
    List<Image> ProgressSteps;


    private void UpdateBar()
    {
        if (MaxValue <= 0)
        {
            bar.fillAmount = 0;
            return;
        }
        float fillamount = (float)CurrentValue / MaxValue;
        bar.fillAmount = fillamount;
    }
    /*
    public void PainStatus()
    {
        if (CurrentValue == MaxValue)
        {
            UnHurt.SetActive(true);

            Orange.SetActive(false);
            Yellow.SetActive(false);
        }
        if (CurrentValue <= YellowNum)
        {
            UnHurt.SetActive(false);
          
            Yellow.SetActive(true);
            Orange.SetActive(false) ;
        }
        if (CurrentValue <= OrangeNum)
        {
            UnHurt.SetActive(false);
        
            Yellow.SetActive(false);
            Orange.SetActive(true);
        }
        if (StumbleScript.IsStumbling == true)
        {
            UnHurt.SetActive(false);
            Orange.SetActive(false);
            Yellow.SetActive(false);
            Red.SetActive(true) ;
        }
        else
        {
            Red.SetActive(false);
        }
    }

    public void DrawSpoons()
    {
        ClearSpoon();

        //based of max health find how many we need
        float maxSpoonRemainder = MaxValue % 2;
        // to see if we are odd or even 
        int SpoonstoMake = (int)(MaxValue / 2 + maxSpoonRemainder);
        for (int i = 0; i < SpoonstoMake; i++)
        {
            CreateEmptySpoon();
        }
        for (int i = 0; i < Spoons.Count; i++)
        {
            int spoonStatusRemainder = (int)Mathf.Clamp(CurrentValue - (i * 2), 0, 2);
            Spoons[i].SetSpoonImage((SpoonStatus)spoonStatusRemainder);
        }
    }
    public void CreateEmptySpoon ()
    {
        GameObject newSpoon = Instantiate(SpoonPrefab);
        newSpoon.transform.SetParent (transform);

        SpoonScript spoonComponent = newSpoon.GetComponent<SpoonScript>();
        spoonComponent.SetSpoonImage(SpoonStatus.Empty);
        Spoons.Add(spoonComponent);
    }
   public void ClearSpoon()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);

        }
        Spoons = new List<SpoonScript>();
    }*/
    private void Start()
    {
        UpdateBar();
        //DrawSpoons();
        HudManagerObj.GetComponent<HudController>();
        //MaxValue = transform.childCount;
        /*ProgressSteps = new List<Image>();

        for (int i = 0; i < MaxValue; i++)
        {
            ProgressSteps.Add(transform.GetChild(i).GetComponent<Image>());
        }

        InititateProgressBar(StartFull);*/
    }
   /* void ChangeSpriteColour(int index, Color Newcolour)
    {
        ProgressSteps[index].color = Newcolour;
    }*/
   /* public void InititateProgressBar(bool IsFull)
    {
        if (IsFull)
        {
            for (int i = 0; i < MaxValue;i++)
            {
                ChangeSpriteColour(i, enabledColour);
            }
            CurrentValue = MaxValue;
        }
        else
        {
            for (int i = 0; i < MaxValue;i++)
            {
                ChangeSpriteColour(i, disabledColour);
            }
            CurrentValue = 0;
        }
    }
   */

    public bool ChangeResourceAmount(int amount)
    {
        if (!OverkillMode && CurrentValue + amount <0)
            return false;
        CurrentValue += amount;
        CurrentValue = Mathf.Clamp(CurrentValue, MinValue, MaxValue);

        bar.fillAmount = (float)CurrentValue / MaxValue;
        return true;
    }
  /*  public void IncreaseProgress()
    {
        if (CurrentValue == MaxValue)
            return;
        CurrentValue += Spoon; //add on value from hudmanager
                               // ChangeSpriteColour(CurrentValue - Spoon, enabledColour);
       // DrawSpoons();
    }

    public void DecreaseProgress()
    {
        if (CurrentValue == MinValue) return;
       // ChangeSpriteColour (CurrentValue - Spoon, disabledColour);
        CurrentValue-=Spoon;
       // DrawSpoons();
    }
    */
    public int GetProgress()
    {
        return CurrentValue;
    }

    private void Update()
    {
        
            if (CurrentValue == 0)
            {
                hudManager.Lose();
            }
        
    }
    
}

