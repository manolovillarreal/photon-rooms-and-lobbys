using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCtrl : MonoBehaviourPunCallbacks
{

    public const string MAP_PROP_KEY = "map";
    public const string GAME_MODE_PROP_KEY = "gm";


    public int maxPlayers = 4;
    public bool EstrictSearch { get; set; }
    private bool flag ;
    #region SerializeFields
    [SerializeField]
    private Transform RoomsContainer; 
    [SerializeField]
    private GameObject prefabRoomItem; 

    #endregion

    private Dictionary<RoomInfo,GameObject> RoomsList; //lista de salas

    //Match Options
    private RoomOptions roomOptions;
    private string RoomName;
    private int Map = 0;
    private int GameMode = 0;

    private int expectedMap = -1;
    private int expectedGameMode = -1;
    private int expectedMaxPlayers = 0;



    #region MonoBehaviour Callbacks 
    void Awake()
    {
        RoomsList = new Dictionary<RoomInfo, GameObject>();
        roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayers;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
    }

    #endregion

    #region Photon Callbacks 

    public override void OnJoinedLobby()
    {
        Debug.Log("Se unio al lobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("update room list of " + roomList.Count);
        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.CustomProperties);
            ListRoom(room);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No se encontro partida aleatoria");
        if (EstrictSearch  || flag)
        {
            flag = false;
            CreateRoom();
        }
        else
        {
            flag = true;
            PhotonNetwork.JoinRandomRoom();
        }
        
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Creaste el Cuarto de nombre : "+ PhotonNetwork.CurrentRoom.Name);
    }
    public override void OnJoinedRoom()
    {

        Debug.Log("Se unio a un cuarto");
        UIManager.Instance.GotoRoom();

    }
    public override void OnCreateRoomFailed(short returnCode, string message) //si la sala existe
    {
        Debug.Log("Fallo en crear una nueva sala \n"+message);
    }
    #endregion

    #region Private Methods

    void RemoveRoomsFromList()
    {
        for (int i = RoomsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(RoomsContainer.GetChild(i).gameObject);
        }
    }
    void ListRoom(RoomInfo room) 
    {
        if (room.IsOpen && room.IsVisible)
        {
            GameObject roomListItem;

            if (!RoomsList.ContainsKey(room))
            {
                roomListItem = Instantiate(prefabRoomItem, RoomsContainer);
                RoomsList.Add(room, roomListItem);
            }
            else
            {
                roomListItem = RoomsList[room];
                if (room.RemovedFromList)
                {
                    RoomsList.Remove(room);
                    Destroy(roomListItem);
                }
            }

            RoomItem tempButton = roomListItem.GetComponent<RoomItem>();
            tempButton.SetRoom(room.Name, room.MaxPlayers, room.PlayerCount);
        }
    }

    #endregion

    public void CreateRoom()
    {
        Debug.Log("Creando nueva sala: " + RoomName);

        roomOptions.CustomRoomPropertiesForLobby = new string[] { MAP_PROP_KEY, GAME_MODE_PROP_KEY };
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { MAP_PROP_KEY, Map }, { GAME_MODE_PROP_KEY, GameMode } };       
        PhotonNetwork.CreateRoom(RoomName, roomOptions);
    }
    public void FindMatch()
    {
        var expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable();

        if (expectedMap >= 0)
        {
            expectedCustomRoomProperties.Add(MAP_PROP_KEY, expectedMap);
        }
        if (expectedGameMode >= 0)
        {
            expectedCustomRoomProperties.Add(GAME_MODE_PROP_KEY, expectedGameMode);
        }
        if (expectedCustomRoomProperties.Count > 0)
        {
            Debug.Log("Buscando partida con filtros|  Map: " + expectedMap + " , Players: " + expectedMaxPlayers);
            PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, (byte)expectedMaxPlayers);
        }
        else
        {
            Debug.Log("Buscando partida sin filtros");
            PhotonNetwork.JoinRandomRoom();
        }
    }
    public void LeaveLobby() //Se enlace al botón cancelar. Se usa para retornar al menú principal
    {
        UIManager.Instance.PanelLobby.SetActive(false);
        UIManager.Instance.PanelConnect.SetActive(false);

        PhotonNetwork.LeaveLobby();
    }

    public void SetMaxPlayers(string size)
    {
        int _maxPlayers = int.Parse(size);
        if (string.IsNullOrEmpty(size))
            expectedMaxPlayers = 0;
        else
            expectedMaxPlayers = _maxPlayers;

        roomOptions.MaxPlayers = (byte)_maxPlayers;
    }
    public void SetRoomName(string roomName)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            name = null;
        }
        RoomName = roomName;
    }
    public void SetRoomIsVisible(bool visible)
    {
        roomOptions.IsVisible = visible;
    }
    public void SetMap(int map)
    {
        expectedMap = map - 1;
        if (map == 0)
            this.Map = Random.Range(0, 2);
        else
            this.Map = map - 1;
    }


}
