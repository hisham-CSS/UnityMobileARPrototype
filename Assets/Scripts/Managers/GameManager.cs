using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Button CreateGameButton;
    public Button JoinGameButton;

    public GameObject menu;
    public GameObject lobby;
    // Start is called before the first frame update
    void Start()
    {
        menu.SetActive(true);
        lobby.SetActive(false);

        CreateGameButton.onClick.AddListener(CreateLobby);
        JoinGameButton.onClick.AddListener(JoinGame);
    }


    // Update is called once per frame
    void Update()
    {

    }

    async void CreateLobby()
    {
        await QuickLobbyManager.Instance.CreateLobby();

        menu.SetActive(false);
        lobby.SetActive(true);
    }

    async void JoinGame()
    {
        await QuickLobbyManager.Instance.JoinLobby();

        menu.SetActive(false);
        lobby.SetActive(true);
    }

    void LoadScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }    
}
