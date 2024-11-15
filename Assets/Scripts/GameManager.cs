using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject menu;
    public GameObject lobby;
    // Start is called before the first frame update
    void Start()
    {
        menu.SetActive(true);
        lobby.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void CreateLobby()
    {
        menu.SetActive(false);
        lobby.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }    
}
