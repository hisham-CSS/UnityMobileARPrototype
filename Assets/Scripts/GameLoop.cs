using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    //stores all the objects that are required for this savenger hunt
    public List<GameObject> huntList;

    public void FoundItem(GameObject item)
    {
        if (huntList[0] == item)
        {
            Debug.Log($"{item.name} was found!");
            item.GetComponent<ItemHint>().Hint();
            //item was found correctly - we can add some extra effects here when we find an item
            huntList.Remove(item);
            
            //win condition - this should also have some sort of effect when we win to make the player feel good
            if (huntList.Count <= 0)
                Debug.Log($"You have completed the hunt in {Time.time}");
        }
        else
        {
            //maybe play a sound effect to inform users that you are not scanning the correct item at the current moment!
            Debug.Log($"You are scanning items out of order!");
        }

    }

}
