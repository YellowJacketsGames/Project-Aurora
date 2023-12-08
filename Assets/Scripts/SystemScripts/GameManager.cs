using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//We will use a static class for the GameManager as it's usually a singleton
//It will hold static variables that are easily accessable in each scene
public class GameManager : MonoBehaviour
{
    //Singleton static variable
    public static GameManager instance;


    //This is the code necessary to create a singleton
    #region Singleton
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);


    }

    #endregion

    [Header("GameManager Variables")]
    public CameraManager currentCameraManager;
    public PlayerController currentController;
    public TagManager speakerManager; //This is a placeholder, it should be another scriptable object with a list of all the speakers in game so that it doesn't depend on references.

}
