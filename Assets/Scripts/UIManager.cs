using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : Photon.PunBehaviour
{
    public static GameObject localPlayer;

    public GameManager gameManager;

    public GameObject lobbyUI;

    public GameObject joinRoomListRowPrefab;

    public GameObject roomListPanel;

    public GameObject question;

    public GameObject mainLog;

    public GameObject StartGameButton;

    private List<GameObject> joinRoomButtonPool = new List<GameObject>();

    private string[] questionArray = new string[] { "한국", "게임", "침대", "축구", "농구", "수박", "국회의원", "졸업", "철학", "대학생", "촛불", "바이올린" };

    private float time;

    private void Awake()
    {

 
    }
    // Use this for initialization
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("ARCatchmind_v1.0");
        question.GetComponentInChildren<Text>().text = "";
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnClickClear()
    {
        mainLog.GetComponentInChildren<Text>().text = "Clear All Lines!";
        GameObject.Find("PlayerPoint").GetComponent<PlayerController>().ClearLines();
    }

    public void OnClickStart()
    {
        System.Random random = new System.Random();
        int randomNumber = random.Next(questionArray.Length);
        string randomQuestion = questionArray[randomNumber];

        question.GetComponentInChildren<Text>().text = randomQuestion;

        GameObject.Find("PlayerPoint").GetComponent<PlayerController>().StartGame(randomQuestion);
    }

    public void StartTimer(string question)
    {
        time = 15f;
        StartGameButton.SetActive(false);
        StartCoroutine(timerCoroutine(question));
    }

    IEnumerator timerCoroutine(string questionStr)
    {
        while (time > 0f)
        {
            mainLog.GetComponentInChildren<Text>().text = "Time left: " + time.ToString();
            yield return new WaitForSeconds(1f);
            time -= 1f;
        }

        time = 5f;
        mainLog.GetComponentInChildren<Text>().text = "Guess What?";
        question.GetComponentInChildren<Text>().text = time.ToString();
        StartCoroutine(ShowQuestion(questionStr));

        yield return null;
    }

    IEnumerator ShowQuestion(string questionStr)
    {
        Debug.Log("Show Question");
        while (time >= 0f)
        {
            yield return new WaitForSeconds(1f);
            question.GetComponentInChildren<Text>().text = time.ToString();
            time -= 1f;
        }
        time = 5f;
        mainLog.GetComponentInChildren<Text>().text = "Answer!";
        question.GetComponentInChildren<Text>().text = questionStr;
        StartCoroutine(ClearQuestion());
        yield return null;
    }

    IEnumerator ClearQuestion()
    {
        Debug.Log("Clear Question");
        while (time > 0f)
        {
            yield return new WaitForSeconds(1f);
            time -= 1f;
        }
        time = 0f;
        mainLog.GetComponentInChildren<Text>().text = "";
        question.GetComponentInChildren<Text>().text = "";
        StartGameButton.SetActive(true);
        yield return null;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.GetRoomList().Length);
        for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
        {
            mainLog.GetComponentInChildren<Text>().text = PhotonNetwork.GetRoomList().Length.ToString();
            RoomInfo game = PhotonNetwork.GetRoomList()[i];
            GameObject button = Instantiate(joinRoomListRowPrefab);

            button.transform.SetParent(roomListPanel.transform, false);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100 - (200 * i));


            button.GetComponentInChildren<Text>().text = game.Name;
            button.GetComponentInChildren<Button>().onClick.AddListener(() =>
                                                                        OnJoinRoomClicked(game.Name));
            button.SetActive(true);

            joinRoomButtonPool.Add(button);

            //Debug.Log(game.PlayerCount);
            //Debug.Log(game.MaxPlayers);
        }
    }

    public void CreateGameRoom()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(null, options, null);
    }

    public void RefreshGameRoom()
    {
        joinRoomButtonPool.Clear();

        mainLog.GetComponentInChildren<Text>().text = "Number of rooms: " + PhotonNetwork.GetRoomList().Length.ToString();

        for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
        {
            RoomInfo game = PhotonNetwork.GetRoomList()[i];
            GameObject button = Instantiate(joinRoomListRowPrefab);

            button.transform.SetParent(roomListPanel.transform, false);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100 - (200 * i));


            button.GetComponentInChildren<Text>().text = game.Name;
            button.GetComponentInChildren<Button>().onClick.AddListener(() => 
                                                                        OnJoinRoomClicked(game.Name));
            button.SetActive(true);

            joinRoomButtonPool.Add(button);

            //Debug.Log(game.PlayerCount);
            //Debug.Log(game.MaxPlayers);
        }

        foreach (GameObject button in joinRoomButtonPool)
        {
            button.SetActive(false);
            button.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            button.GetComponentInChildren<Text>().text = string.Empty;
        }

        for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
        {
            RoomInfo game = PhotonNetwork.GetRoomList()[i];
            GameObject button = joinRoomButtonPool[i];

            button.GetComponentInChildren<Text>().text = game.Name;
            button.GetComponentInChildren<Button>().onClick.AddListener(() => 
                                                                        OnJoinRoomClicked(game.Name));
            button.SetActive(true);
        }

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        ChangeLobbyVisibility(false);

        mainLog.GetComponentInChildren<Text>().text = "Joined the room!";

        localPlayer = PhotonNetwork.Instantiate(
           "PlayerPoint",
           new Vector3(0, 0, 0),
           Quaternion.identity, 0);
    }

    private void OnJoinRoomClicked(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);

        gameManager.OnEnterResolvingModeClick();
    }

    private void ChangeLobbyVisibility(bool visible)
    {
        lobbyUI.gameObject.SetActive(visible);
    }

}
