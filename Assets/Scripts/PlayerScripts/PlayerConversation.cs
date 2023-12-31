using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UINavigation;

//This script handles dialogue and conversations the player might have at any moment in the level
//It checks the input of the UI to choose options and to read lines.

public class PlayerConversation : PlayerComponent
{
    [Header("Dialogue Components")]
    private Story currentDialogue;

    //This are variables to fix a small bug with the input of both action maps clashing when the dialogue is finished.
    [SerializeField] float storyFinishedTimer = 2f;
    bool storyfinished; //bool to check if the story is done
    [Header("Choices Components")]
    [SerializeField] private List<ChoiceClass> dialogueChoices;
    private NavigateOptions choiceNavigation = new NavigateOptions();

    private Coroutine displayLine; //Variable to stop the coroutine
    public bool canContinue;

    private void Start()
    {
        canContinue = true;
    }
    public void BeginStory()
    {
        #region Set external functions

        currentDialogue.BindExternalFunction("CheckIfHasItem", (string itemId) =>
        {
            if (_parent.playerInventoryComponent.CheckIfObjectIsInInventory(itemId))
            {
                currentDialogue.variablesState["hasItem"] = true;
            }
            else
            {
                currentDialogue.variablesState["hasItem"] = false;
            }

        });

        currentDialogue.BindExternalFunction("GoToNextObjective", (string none) =>
        {
            GameManager.instance.questManager.UpdateObjective();
        });

        currentDialogue.BindExternalFunction("CallEvent", (int eventIndex) =>
        {
            GameManager.instance.currentLevelManager.TriggerEvent(eventIndex);
        });

        currentDialogue.BindExternalFunction("StopTyping", (string none) =>
        {
            canContinue = false;
        });

        #endregion 

        if (currentDialogue.canContinue)
        {
            displayLine = StartCoroutine(_parent.playerUIComponent.DisplayLine(currentDialogue.Continue()));
            //DisplayChoices();
            HandleTags();
        }
    }

    public void FixedUpdate()
    {       
        if (_parent.CurrentPlayerState == PlayerState.Conversation) //We only execute this if we're in the conversation state
        {
            if (!GameManager.instance.currentLevelManager.IsCurrentEventRunning())
            {
                if (_parent.playerInputHandlerComponent.GetAcceptInput()) //If we input the continue button, it will continue the story
                {
                    if (canContinue)
                    {
                        ContinueStory();
                    }
                }

                if (storyfinished) //if the story is done, we clean it up
                {
                    FinishStory();
                }
            }
        }
    }

    #region Story 
    public void ContinueStory() //Method to make story continue
    {
        if (currentDialogue.canContinue) // The first condition is if the story can continue to the next line
        {
            if (!_parent.playerUIComponent.ReturnTypingStatus()) //Check to make sure we're not overwriting 
            {
                //If we're not, we begin the write text coroutine
                displayLine = StartCoroutine(_parent.playerUIComponent.DisplayLine(currentDialogue.Continue()));

                //We check which speaker is actually talking to set the portraits
                HandleTags();
            }
            else
            {
                SkipLine();
            }
        }
        else
        {
            //This part of the continue story method is reserved for finishing the story or waiting for choices to be answered

            if (_parent.playerUIComponent.ReturnTypingStatus()) //If this is the last line before the story can move on, we display it fully
            {
                if (displayLine != null) //We make sure to stop the coroutine so that we don't start another one and they overlap, it could cause bugs
                {
                    StopCoroutine(displayLine);
                    displayLine = null;
                }

                //We reset the dialogue to the current text
                _parent.dialogueText.text = currentDialogue.currentText;

                //Since we're no longer typing, we stop the bool in the player UI
                _parent.playerUIComponent.SetTypingStatus(false);

                //If there are any choices avaible, we display them
                DisplayChoices();
            }
            else
            {
                if (currentDialogue.currentChoices.Count <= 0) //If there are no choices to be answered (since that also makes the dialogue not able to continue) we finish the story
                {
                    _parent.playerUIComponent.HideConversationBox(); //Deactivating the dialogue ui

                    storyfinished = true;
                }
            }

        }
    }
    
    public void SkipLine()
    {
        //If we want to continue the story but the text is appearing currently we skip over to the next line

        if (displayLine != null) //We make sure to stop the coroutine so that we don't start another one and they overlap, it could cause bugs
        {
            StopCoroutine(displayLine);
            displayLine = null;
        }

        //We reset the dialogue to the current text
        _parent.dialogueText.text = currentDialogue.currentText;

        //Since we're no longer typing, we stop the bool in the player UI
        _parent.playerUIComponent.SetTypingStatus(false);

        //If there are any choices avaible, we display them
        DisplayChoices();
    }
    public void FinishStory() //This method is only performed when there is no more dialogue in the current story
    {
        //We set a timer so that the accept and jump input don't overlap
        storyFinishedTimer -= Time.deltaTime;

        if (storyFinishedTimer <= 0) //When it's done, we reset the timer and set the new state
        {
            storyFinishedTimer = 2f;
            storyfinished = false;
            _parent.ChangeState(PlayerState.Idle);
        }
    }
    public void SetCurrentDialogue(Story dialogue) //Method to change the Current Dialogue
    {
        currentDialogue = dialogue;
    }

    #endregion

    #region Choices
    public void DisplayChoices() //Method to display the different choices
    {
        if(currentDialogue.currentChoices.Count > 0) //Execute only if there are choices to display
        {
            foreach(ChoiceClass c in dialogueChoices)
            {
                c.ReturnParent().SetActive(false);
            }

            for (int i = 0; i < currentDialogue.currentChoices.Count; i++) //Go through every choice avaible and fill out the variables
            {
                
                if (dialogueChoices[i] == null)
                    break;
                dialogueChoices[i].SetChoice(currentDialogue.currentChoices[i]);
            }

            StartCoroutine(choiceNavigation.SelectFirstOption(dialogueChoices[0].ReturnParent()));
        }
    }
    
    public void AssignChoices(int index) //Method to execute the coices when it appears
    {
        currentDialogue.ChooseChoiceIndex(index);

        foreach (ChoiceClass c in dialogueChoices)
        {
            c.ReturnParent().SetActive(false);
        }
    }


    #endregion

    #region Speakers

    private void HandleTags() //This is a method to get the speakers in a dialogue and give them the reference to the UI
    {
        if(currentDialogue.currentTags.Count > 0) //if we have a tag, we perform the 
        {
            foreach (string tag in currentDialogue.currentTags)
            {
                string[] splitTag = tag.Split(":");

                string tagKey = splitTag[0];
                string valueKey = splitTag[1];

                switch (tagKey)
                {
                    case "speaker":
                        _parent.playerUIComponent.SetDialogueLayout(GameManager.instance.speakerManager.ReturnSpeaker(valueKey));
                        Debug.Log("Current Speaker: " + valueKey);
                        break;
                    case "give_item":
                        ObjectClass obj = new ObjectClass();
                        obj = obj.CreateObject(valueKey);
                        _parent.playerInventoryComponent.AddObjectToKeyInventory(obj);
                        break;
                    case "take_item":
                        if (_parent.playerInventoryComponent.CheckIfObjectIsInInventory(valueKey))
                        {
                            _parent.playerInventoryComponent.UseItem(valueKey);
                        }
                        break;
                    default:
                        Debug.LogWarning("Tag not found use for");
                        break;
                }
            }
        }
    }
    
    #endregion
}
