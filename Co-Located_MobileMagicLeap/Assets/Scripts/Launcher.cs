using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace CoLocated_MobileAR
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields
        /// <summary>
        /// Maximum number of players per room.
        /// </summary>
        [Tooltip("When a room is full, it can't be joined by new players.")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;
        
        #endregion

        #region Private Fields

        /// <summary>
        /// Client's game version number
        /// </summary>
        string gameVersion = "1";

        #endregion

        #region MonoBehavior CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early init.
        /// </summary>
        void Awake()
        {
            //#Critical
            //This makes sure we can use PhotonNetwork.LoadLevel() on the main client
            //and all clients in the same room sync their level automatically.
            PhotonNetwork.AutomaticallySyncScene = false;
        }

        /// <summary>
        /// MonoBehavior Method called on GameObject by Unity during init.
        /// </summary>
        void Start()
        {
            //Connect(); //doing this on QRScan now
        }

        #endregion

        #region MonoBehaviorPunCallbacks Callbacks

        public override void OnConnectedToMaster()
        {
            //base.OnConnectedToMaster();
            Debug.Log("Launcher OnConnectedToMaster() called by PUN");

            //TODO: need this twice?
            //#Critical we need to attempt joining a random room. If we fail, we'll create a new room in OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            //base.OnJoinRandomFailed(returnCode, message);
            Debug.Log("Launcher OnJoinRandomRoomFailed() called by PUN");

            //#Critical: we failed to join random room, so either none exist or they're all full. Need to create one.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            //base.OnJoinedRoom();
            Debug.Log("Launcher OnJoinedRoom() called by PUN. Client in a room.");
            Debug.LogFormat("Client name: {0}", PhotonNetwork.LocalPlayer.UserId);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            //base.OnDisconnected(cause);
            Debug.Log("Launcher OnDisconnected() called by PUN");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// - If already connected, attempt joining a random room
        /// - If not yet connected, connect this app instance to Photon Cloud
        /// </summary>
        public void Connect()
        {
            //check if connected or not, join if so, else initiate connection
            if (PhotonNetwork.IsConnected)
            {
                //#Critical we need to attempt joining a random room. If we fail, we'll create a new room in OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                //#Critical, first, need to connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion

        

    }
}
