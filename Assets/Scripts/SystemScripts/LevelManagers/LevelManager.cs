using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

//This manager stores all the different events in each level.
//It also executes the events through the trigger event method.
public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelEvent[] levelEvents; //All the events of the current event
    private LevelEvent currentEvent; //variable to check the current event is running;
    [SerializeField] string levelMusic;
    public Quest levelQuest;


    bool playtest;
    bool playtestingMoveScene;
    private void Awake()
    {
        GameManager.instance.currentLevelManager = this;
       
    }
    private void Start()
    {
        //Set GameManager reference
        SetLevelMusic();
        GameManager.instance.questManager.SetCurrentQuest();
        playtestingMoveScene = false;
    }

    public virtual void SetLevelMusic()
    {
        AudioManager.instance.FadeIn(levelMusic, true);
    }
    public virtual void EndLevelMusic()
    {
        AudioManager.instance.FadeOut(levelMusic);
    }
    public void TriggerEvent(int eventToTrigger) //Method to trigger an specific event
    {
        if(levelEvents.Length <= 0) //If there are no events stored in the list, we don't execute the method.
        {
            Debug.Log("There are no events in this level manager");
            return;
        }

        if(eventToTrigger >= levelEvents.Length || levelEvents[eventToTrigger] == null) //if the event we're trying to call is not on the list, we don't execute the method.
        {
            Debug.Log("The event you tried to trigger does not exist");
            return;
        }
        if (!levelEvents[eventToTrigger].HasBeenTriggered()) //if the event has not been triggered yet, we call it.
        {
            currentEvent = levelEvents[eventToTrigger];
            levelEvents[eventToTrigger].OnEvent();
        }
    }

    private void Update()
    {
        if (!playtestingMoveScene)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                GameManager.instance.currentTransitionManager.NextLevel();
                playtestingMoveScene = true;
            }
        }
    }
    public bool IsCurrentEventRunning()
    {
        if(currentEvent != null)
        {
            if (currentEvent.IsEventRunning())
            {
                return true;
            }
            else
            {
                currentEvent = null;
                return false;
            }
        }
        else
        {
            Debug.Log("No event is active");
            return false;
        }
    }
}