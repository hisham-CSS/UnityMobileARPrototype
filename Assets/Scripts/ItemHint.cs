using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHint : MonoBehaviour
{
    public string hint = "This is a default hint";

    public void Hint()
    {
        Debug.Log(hint);
    }
}
