using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This handles the objects that the player has in his inventory
//This keeps both the progression items and the key items

public class PlayerInventory : PlayerComponent
{
    //This two list hold the obtainable objects in the player inventory
    private List<ObjectClass> keyObjectInventory = new List<ObjectClass>();
    private List<ObjectClass> typewriterObjectInventory = new List<ObjectClass>();


    //This region holds every method to add items to the inventory
    #region Add to Inventory
    private void AddObjectToInventory(ObjectClass newObject, List<ObjectClass> currentInventory)
    {
        //First we check if the player already has this object in his inventory

        foreach(ObjectClass o in currentInventory)
        {
            if (o.CompareId(newObject)) //if some object in the inventory is the same, it doesn't store it and ends the method
                return;
        }

        currentInventory.Add(newObject);
    }


    public void AddObjectToKeyInventory(ObjectClass newObject)
    {
        AddObjectToInventory(newObject, keyObjectInventory);
    }

    public void AddObjectToTypewriterInventory(ObjectClass newObject)
    {
        AddObjectToInventory(newObject, typewriterObjectInventory);
    }

    #endregion

    public void UseItem(ObjectClass newObject)
    {
        //First we check if the object we want to remove is in the player inventory
        if (CheckIfObjectIsInInventory(newObject))
            keyObjectInventory.Remove(newObject); //We remove the item from the list

        else
            Debug.LogError("The object you tried to use is not in the player inventory");
    }

    public bool CheckIfObjectIsInInventory(ObjectClass newObject)
    {
        foreach (ObjectClass o in keyObjectInventory)
        {
            if (o.CompareId(newObject)) //if some object in the inventory is the same, we break the loop
                return true;
        }

        return false;
    }
}


