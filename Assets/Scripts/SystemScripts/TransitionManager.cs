using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ImageEffects;

//This script will handle transitions from level to level and different areas of the game
public class TransitionManager : MonoBehaviour
{
    [Header("Transition Variables")]

    //Bools to check which function to do
    bool fadeIn;
    bool fadeOut;
    Effects transitionEffects;
    [SerializeField] private Image transitionImage;
    [Range(0f, 1f)]
    [SerializeField] float transitionDuration;

    [SerializeField] float transitionTime = 0;

    
    private void Start()
    {
        //Give reference to Game Manager
        GameManager.instance.currentTransitionManager = this;

        //We begin with a fade out transition
        SetFadeOut();

        transitionEffects = new Effects();
    }
    private void Update()
    {
        if (fadeIn) //If faden in has been set, execute the method 
        {
            if(transitionEffects.FadeIn(transitionImage, transitionDuration))
            {
                fadeIn = false; //When the transition is finished, we make sure the transition stops
            }
        }

        if (fadeOut) //If faden out in has been set, execute the method 
        {
            if (transitionEffects.FadeOut(transitionImage, transitionDuration))
            {
                fadeOut = false; //When the transition is finished, we make sure the transition stops

                //Since we're exiting the transition, we also need to change to player state to idle
                if(GameManager.instance.currentController != null)
                GameManager.instance.currentController.ChangeState(PlayerState.Idle);
            }
        }
    }

    #region Transition Methods

    public void SetFadeIn() //Set the transition to fade In
    {
        if(GameManager.instance.currentController != null)
        GameManager.instance.currentController.ChangeState(PlayerState.Transition); //Since we're entering a transition, we set the player state to transition

        if (!fadeOut)
            fadeIn = true;
    }

    public void SetFadeOut() //Set the transition to fade out
    {
        if(!fadeIn)
        fadeOut = true;
    }
    public bool ReturnTransitionStatus() //This returns true if either transition is playing and false if none are
    {
        if(fadeIn || fadeOut)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
