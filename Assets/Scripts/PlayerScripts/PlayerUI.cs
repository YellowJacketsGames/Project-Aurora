using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

//This script will handle most of the player HUD and UI
//Like contexual controls and UI animations
public class PlayerUI : PlayerComponent
{
    private Gamepad gamepad; //This variable is used to see if the player is using a gamepad
    private Keyboard keyboard; //This variable is used to see if the player is using a gamepad

    private DialogueLayoutClass currentDialogueLayout; //Variable to hold the current layout displayed to 


    [Header("Dialogue Variables")]
    [SerializeField] private float typingSpeed; //Variable to change the speed of which the text appear in dialogue boxes.
    bool typingLine;
    bool skipValue;


    [Space]
    [Header("Inventory Variables")]
    [SerializeField] private float inventoryPopUpPosition;
    [SerializeField] private float inventoryPopUpTime;


    //Region to hold the show context control methods UI
    #region Context Controls
    private void ShowKeyboardControls() //This methods changes the current display controls to the keyboard
    {
        _parent.keyboardControls.SetActive(true);
        _parent.gamepadControls.SetActive(false);

    }

    private void ShowGamepadControls() //This methods changes the current display controls to the gamepad
    {
        _parent.gamepadControls.SetActive(true);
        _parent.keyboardControls.SetActive(false);
    }

    #endregion

    //This region holds the methods related to interact prompt
    #region Interact Prompt
    public void ShowInteractPrompt(InteractableElement element)
    {
        _parent.interactablePromptObject.gameObject.SetActive(true); //First we set the parent object of the interaction prompt active

        switch (element.GetElementType()) //We check which type of element is the current one to put the correct text
        {
            case InteractionType.PickUp:
                _parent.interactableText.SetText("Pick up " + element.GetElementName());
                break;
            case InteractionType.Location:
                DoorElement door = element.GetComponent<DoorElement>();

                if (door != null)
                {
                    if (door.GetKeyValue())
                    {
                        if (door.CheckIfCanOpen())
                        {
                            _parent.interactableText.SetText("Open door");
                        }
                        else
                        {
                            _parent.interactableText.SetText("Door locked");
                        }
                    }
                    else
                    {
                        _parent.interactableText.SetText("Go to " + element.GetElementName());
                    }
                }

                break;
            case InteractionType.Conversation:
                _parent.interactableText.SetText("Talk to " + element.GetElementName());
                break;
            default:
                break;
        }

        //We deactivate both controls so that we don't keep a wrong control type active
        _parent.controllerInteractPrompt.SetActive(false);
        _parent.keyboardInteractPrompt.SetActive(false);

        //And we activate the correct one depending on the current control type

        switch (_parent.currentControl)
        {
            case ControlType.Gamepad:
                _parent.controllerInteractPrompt.SetActive(true);
                break;
            case ControlType.Keyboard:
                _parent.keyboardInteractPrompt.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void HideInteractPrompt()
    {
        _parent.interactablePromptObject.gameObject.SetActive(false); //We set the parent object of the interaction prompt to not active
    }

    #endregion

    //This region holds all methods related to conversations UI
    #region Conversations

    //Method to activate the conversation box and set the appropiate control layout
    public void ShowConversationBox()
    {
        _parent.dialogueBox.SetActive(true);

        switch (_parent.currentControl)
        {
            case ControlType.Gamepad:
                _parent.controllerContinueButton.SetActive(true);
                _parent.keyboardContinueButton.SetActive(false);
                break;
            case ControlType.Keyboard:
                _parent.keyboardContinueButton.SetActive(true);
                _parent.controllerContinueButton.SetActive(false);
                break;
            default:
                break;
        }
    }
    //Method to deactivate the conversation box
    public void HideConversationBox()
    {
        _parent.dialogueBox.SetActive(false);
    }

    //Method to change the dialogue text, we will replace this in the future with a coroutine that displays text with each letter appearing one at a time
    public void ChangeConversationText(string newText)
    {
        _parent.dialogueText.SetText(newText);
    }

    //Method to set the dialogue layout to the current speaker.
    public void SetDialogueLayout(Speaker currentSpeaker)
    {
        if(currentDialogueLayout != null)
        {
            if (currentDialogueLayout != _parent.dialogueLayouts[currentSpeaker.layoutOrder])
            {
                currentDialogueLayout.DeactivateLayout();
                currentDialogueLayout = _parent.dialogueLayouts[currentSpeaker.layoutOrder];
            }
        }
        else
        {
            currentDialogueLayout = _parent.dialogueLayouts[currentSpeaker.layoutOrder];
        }

        currentDialogueLayout.FillLayout(currentSpeaker);
    }


    //Coroutine to show the dialogue text one letter at a time like a typewriter.
    public IEnumerator DisplayLine(string line)
    {
        typingLine = true; //Setting the typing status to true
        _parent.dialogueText.text = ""; //First we empty the component for the next line

        //We then through every character in our line in a loop

        foreach(char letter in line.ToCharArray())
        {
            _parent.dialogueText.text += letter;
            AudioManager.instance.PlayWithRandomPitch(0.8f, 1.3f, "Typewriter");
            yield return new WaitForSeconds(typingSpeed);
        }

        //When we're done, we display any choices avaible
        _parent.playerConversationComponent.DisplayChoices();

        //Setting the typing status to false
        typingLine = false;
    }

    #region Typing Status
    public bool ReturnTypingStatus() //Method to get the typing status
    {
        return typingLine;
    }
    public void SetTypingStatus(bool value) //Method to Set the typing status
    {
        typingLine = value;
    }
    #endregion

    #endregion


    //This region holds all methods related to inventory UI

    #region Inventory

    public void ShowObjectObtained(ObjectClass newObj)
    {
        _parent.objectName.text = "You've obtained " + newObj.GetObjectName();
        _parent.objectIcon.sprite = newObj.GetIcon();
        _parent.inventoryAnimations.SetTrigger("popUp");
    }

    public void ShowObjectUsed(ObjectClass obj)
    {
        _parent.objectName.text = "You've used " + obj.GetObjectName();
        _parent.objectIcon.sprite = obj.GetIcon();
        _parent.inventoryAnimations.SetTrigger("popUp");
    }

    #endregion
    private void Update()
    {
        if (_parent.seeControls)
        {
            switch (_parent.currentControl)
            {
                case ControlType.Gamepad:
                    ShowGamepadControls();
                    break;
                case ControlType.Keyboard:
                    ShowKeyboardControls();
                    break;
                default:
                    break;
            }
        }
    }

}
