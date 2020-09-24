using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RoomCtrl : MonoBehaviourPunCallbacks
{
    public const string MAP_PROP_KEY = "map";
    public const string GAME_MODE_PROP_KEY = "gm";

    [SerializeField]
    private Transform playersContainer;
    [SerializeField]
    private GameObject playerListItemPrefab;
    [SerializeField]
    private GameObject MasterPanel;
    [SerializeField]
    private Text RoomName;
    [SerializeField]
    private Dropdown DropdownMap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Photon Callbacks

    public override void OnJoinedRoom()
    {
        int map = (int)PhotonNetwork.CurrentRoom.CustomProperties[MAP_PROP_KEY];
        Debug.Log("Map : " + map);
        DropdownMap.value = map;
        RoomName.text = PhotonNetwork.CurrentRoom.Name;
        if (PhotonNetwork.IsMasterClient)
        {
            MasterPanel.SetActive(true);
        }
        else
        {
            MasterPanel.SetActive(false);
        }
        RemovePlayerFromList();
        FillPlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RemovePlayerFromList();
        FillPlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemovePlayerFromList();
        FillPlayerList();
        if (PhotonNetwork.IsMasterClient)
        {
            MasterPanel.SetActive(true);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("El Master Cliente Se fue \nEl nuevo Master Client es :" + newMasterClient.NickName);     
;    }

    #endregion

    #region private methods
    void FillPlayerList()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject tempListing = Instantiate(playerListItemPrefab, playersContainer);
            Text tempText = tempListing.transform.GetChild(0).GetComponent<Text>();
            tempText.text = player.NickName;            
        }
    }

    void RemovePlayerFromList()
    {
        // Borra cada entrada de la lista de jugadores
        for (int i = playersContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(playersContainer.GetChild(i).gameObject);
        }
    }
    #endregion

    #region public Methods
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel(1);
        }
    }

    public void LeaveRoom() // Retorna al lobby
    {
        UIManager.Instance.GoToLobby();
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        StartCoroutine(rejoinLobby());
    }
    #endregion

    IEnumerator rejoinLobby()
    {
        yield return new WaitForSeconds(2);
        // para forzar la actualización de la lista de salas 
        PhotonNetwork.JoinLobby();
    }

}
