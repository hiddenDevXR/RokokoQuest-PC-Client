using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public UserElement[] usersArray;
    public GameObject[] dancers;
    public static Vector3 offsetPosition;
    public static Vector3 offsetRotation;
    public LevelManager levelManager;
    public string roomName = "DanceFloor";

    public Text statusText;
    public Color normalMessageColor;
    public Color errorMessageColor;
    public Color warningMessageColor;
    
    private ActorEntity currentAvatar;
    private PhotonView myPhotonView;

    bool userWasAknowledge = false;
    
    void Start()
    {
        offsetPosition = transform.position + Vector3.up * 0.1f;
        offsetPosition = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        ConnectedToServer();
        myPhotonView = GetComponent<PhotonView>();
        currentAvatar = dancers[0].GetComponent<ActorEntity>();
    }

    private void ConnectedToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        SetStatus("Try Connect to Server...", normalMessageColor);
    }

    public override void OnConnectedToMaster()
    {
        SetStatus("Connected to Server.", normalMessageColor);
        base.OnConnectedToMaster();
        EnterRoom();
    }

    public void EnterRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 0;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        // roomOptions.PlayerTtl = 60000;
        // roomOptions.EmptyRoomTtl = 60000;

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        SetStatus("Joined a Room.", normalMessageColor);
        PhotonNetwork.NickName = "Unity Server";
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        SetStatus("A new player joined the Room.", normalMessageColor);
        if(userWasAknowledge)
            myPhotonView.RPC("ChangeDancer", RpcTarget.Others, currentAvatar.index);
        

        StartCoroutine(UpdateUserArray(5f));
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        SetStatus("A player left the Room.", warningMessageColor);
        userIsAknowledge = false;
        base.OnPlayerLeftRoom(otherPlayer);
        StartCoroutine(UpdateUserArray(5f));
    }

    public void ChangeAvatar(int index)
    {
        CleanUp();

        foreach (var dancer in dancers)
            dancer.SetActive(false);

        dancers[index].SetActive(true);
        currentAvatar = dancers[index].GetComponent<ActorEntity>();
        
        myPhotonView.RPC("ChangeDancer", RpcTarget.OthersBuffered, index);
    }
    
    public void EnableRotationCalibration()
    {
        levelManager.rotationPoint.transform.position = new Vector3(
            currentAvatar.customBodyBones[0].transform.position.x, 0,
            currentAvatar.customBodyBones[0].transform.position.z);
        levelManager.ParentToPivot();
        
        myPhotonView.RPC("SetCalibrationPoint", RpcTarget.Others, null);
    }
    
    public void DisableRotationCalibration()
    {
        levelManager.UnparentFromPivot();
        levelManager.rotationPoint.transform.position = Vector3.zero;
        myPhotonView.RPC("FreeFromCalibrationPoint", RpcTarget.Others, null);
    }

    private bool isHidden = false;
    public void HideScenery()
    {
        isHidden = !isHidden;
        myPhotonView.RPC("HideScenery", RpcTarget.Others, isHidden);
    }


    private int currentAknowledgeUser;
    public static bool userIsAknowledge;
    [PunRPC]
    public void AknowledgeUser(int index)
    {
        SetStatus("Aknowledging user Num: " + index.ToString(), normalMessageColor);
        userWasAknowledge = true;
        currentAknowledgeUser = index;
        userIsAknowledge = true;
    }

    public AudioSource pingSFX;
    
    [PunRPC]
    public void PlayPingSFX()
    {
        pingSFX.Play();
    }

    public void CallNextPlayer(int index)
    {
        CleanUp();
        myPhotonView.RPC("EnableVoidWall", RpcTarget.Others, index);
    }

    public void PlayAudioToUser()
    {
        myPhotonView.RPC("PlayAudioClip", RpcTarget.Others, currentAknowledgeUser);
    }

   
    public void DissolveScenery()
    {
        myPhotonView.RPC("DissolveScenery", RpcTarget.Others, isHidden);
    }

    private void Update()
    {
            float horizontal = Input.GetAxis("Horizontal") * Time.deltaTime * 1.5f;
            float vertical = Input.GetAxis("Vertical") * Time.deltaTime * 1.5f;
            transform.position = new Vector3(transform.position.x - horizontal, offsetPosition.y, transform.position.z - vertical);
            offsetPosition = new Vector3(horizontal, 0f, vertical);

            if (Input.GetKey(KeyCode.Q))
                RotateLevelOnBothEnds(-0.5f);
            else if (Input.GetKey(KeyCode.E))
                RotateLevelOnBothEnds(0.5f);

            // else
            //     myPhotonView.RPC("RotateLevel", RpcTarget.Others, 0.0f);
        
    }

    void RotateLevelOnBothEnds(float angle)
    {
        myPhotonView.RPC("RotateLevel", RpcTarget.Others, angle);
        levelManager.rotationPoint.eulerAngles += Vector3.up * angle;
    }

    IEnumerator WaitForStableConnection(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        myPhotonView.RPC("ChangeDancer", RpcTarget.Others, currentAvatar.index);
    }
    
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SetStatus("Connection Lost...", warningMessageColor);
        if (CanRecoverFromDisconnect(cause))
        {
            StartCoroutine(Recover());
        }
        else
        {
            Debug.Log(cause);
        }
    }

    private bool CanRecoverFromDisconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            // the list here may be non exhaustive and is subject to review
            case DisconnectCause.Exception:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return true;
        }
        return false;
    }

    private IEnumerator Recover()
    {
        while (PhotonNetwork.CurrentRoom == null)
        {
            if (!PhotonNetwork.ReconnectAndRejoin())
            {
                // Debug.LogError("ReconnectAndRejoin failed, trying Reconnect");
                Debug.Log("ReconnectAndRejoin failed, trying Enter room as new user");

                EnterRoom();

                // if (!PhotonNetwork.Reconnect())
                // {
                //     Debug.LogError("Reconnect failed, trying ConnectUsingSettings");
                //     if (!PhotonNetwork.ConnectUsingSettings())
                //     {
                //         Debug.LogError("ConnectUsingSettings failed");
                //     }
                // } else if (!PhotonNetwork.RejoinRoom(roomName))
                // {
                //     Debug.LogError("RejoinRoom failed");
                // }
            }

            yield return new WaitForSeconds(2);
        }
    }

    void SetStatus(string message, Color statusColor)
    {
        statusText.color = statusColor;
        statusText.text = message;
    }

    IEnumerator UpdateUserArray(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        int index = PhotonNetwork.PlayerList.Length;

        foreach (UserElement userElement in usersArray)
            userElement.SetData("!", 0);


        for (int i = 0; i < index; i++)
        {
            Player currentPlayer = PhotonNetwork.PlayerList[i];
            usersArray[i].SetData(currentPlayer.NickName, currentPlayer.ActorNumber);
        }
    }

    void CleanUp()
    {
        PhotonNetwork.OpCleanRpcBuffer(myPhotonView);
        PhotonNetwork.RemoveBufferedRPCs();
    }
}