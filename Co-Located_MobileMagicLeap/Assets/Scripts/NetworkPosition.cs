using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace CoLocated_MobileAR
{
    public class NetworkPosition : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields

        /// <summary>
        /// The position of the client that owns this component, in their relative world space.
        /// </summary>
        Vector3 networkPos;

        /// <summary>
        /// Was having trouble getting rotation to sync with AR Camera in normal Transform View, so doing this now.
        /// </summary>
        Quaternion networkRot;

        /// <summary>
        /// The vector between the client's anchor and their position, used to replicate their position in another world space.
        /// </summary>
        Vector3 offset;

        /// <summary>
        /// The relative rotation of the client to their own anchor rotation, used to replicate their rotation in our world space.
        /// </summary>
        Quaternion relativeRot;

        /// <summary>
        /// a toggle just to ensure the debug timer coroutine only starts after we have variables filled out
        /// </summary>
        bool firstPassDone = false;

        /// <summary>
        /// Uses Line Renderer to simulate Debug.DrawLine but on mobile
        /// </summary>
        public GameObject debugLinePrefab;
        GameObject debugLine;
        GameObject camCheckLine;

        #endregion

        #region Public Fields

        /// <summary>
        /// Variable to store localPos of the anchor that clientPos should be relative to in *this* world space.
        /// For this demo, refers to the QRCode pos, but can be extrapolated to any network anchor.
        /// Is set on recognition of the image, before connection to ensure variable is set before we need it here.
        /// </summary>
        public Vector3 anchorPos;

        /// <summary>
        /// Have to store anchor rotation too to be able to have relative rotation of client prefab.
        /// </summary>
        public Quaternion anchorRot;

        #endregion

        #region IPunObservabale Callbacks

        /// <summary>
        /// Creating Custom Position Sync, sending our position and rotation instead of using PhotonTransformView
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(gameObject.transform.position);
                stream.SendNext(gameObject.transform.rotation);
            }
            else
            {
                networkPos = (Vector3)stream.ReceiveNext();
                networkRot = (Quaternion)stream.ReceiveNext();
            }
        }

        #endregion

        #region MonoBehaviour Callbacks

        /// <summary>
        /// Set the anchorPos to our world space anchor pos, same for rotation.
        /// </summary>
        void Start()
        {
            Debug.Log("start start");
            Debug.Log(PhotonNetwork.LocalPlayer.UserId);
            anchorPos = (Vector3)PhotonNetwork.LocalPlayer.CustomProperties["anchorPos"];
            Debug.Log(anchorPos);
            anchorRot = (Quaternion)PhotonNetwork.LocalPlayer.CustomProperties["anchorRot"];

            Debug.LogFormat("Network Position Start \n\n AnchorPos: {0}, AnchorRot: {1}\n\n",
                anchorPos,
                anchorRot);

            //debugging line 1) green if qr to this phone, 2) red if qr to other client
            debugLine = Instantiate(debugLinePrefab);
            debugLine.GetComponent<LineRenderer>().SetPositions(new Vector3[2] { anchorPos, gameObject.transform.position });

            //testing cam pos on client
            //camCheckLine = Instantiate(debugLinePrefab);
            //camCheckLine.GetComponent<LineRenderer>().SetPositions(new Vector3[2] { gameObject.transform.parent.transform.position, gameObject.transform.parent.transform.position + new Vector3 (0, 1f, 0)});
            //camCheckLine.GetComponent<LineRenderer>().startColor = Color.blue;

            if (photonView.IsMine)
            {
                debugLine.GetComponent<LineRenderer>().startColor = Color.green;

            }
            else
            {
                debugLine.GetComponent<LineRenderer>().startColor = Color.red;
            }
        }

        /// <summary>
        /// Here we take the anchorPos of the other client in their world space,
        /// use that to find the offset vector of their current position,
        /// then apply that vector to our anchor Pos to get their position in our world space.
        /// Same for rotation.
        /// </summary>
        void FixedUpdate()
        {
            anchorPos = (Vector3)PhotonNetwork.LocalPlayer.CustomProperties["anchorPos"];
            anchorRot = (Quaternion)PhotonNetwork.LocalPlayer.CustomProperties["anchorRot"];
            debugLine.GetComponent<LineRenderer>().SetPositions(new Vector3[2] { anchorPos, gameObject.transform.position });
            //camCheckLine.GetComponent<LineRenderer>().SetPositions(new Vector3[2] { gameObject.transform.parent.transform.position, gameObject.transform.parent.transform.position + new Vector3(0, 1f, 0)});

            if (!photonView.IsMine)
            {
                offset = networkPos - (Vector3)photonView.Controller.CustomProperties["anchorPos"];
                gameObject.transform.position = anchorPos + offset;

                relativeRot = Quaternion.Inverse((Quaternion)photonView.Controller.CustomProperties["anchorRot"]) * networkRot;
                gameObject.transform.rotation = anchorRot * relativeRot;

                // just a big debug log, not needed for final demo, uncomment for info.
                
                if (!firstPassDone)
                {
                    Debug.Log("starting log timer coroutine");
                    StartCoroutine( LogTimer() );
                    firstPassDone = true;
                }
                
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// Just a big debug log that I didn't want to clog update with, so having it log every second.
        /// </summary>
        /// <returns></returns>
        IEnumerator LogTimer()
        {
            while(true)
            {
                Debug.LogFormat("NetworkPos: {0}\nTheir AnchorPos: {1}\nOur AnchorPos: {2}\nOffset: {3}\nResult: {4}\nCam Pos: {5}",
                    networkPos,
                    (Vector3)photonView.Controller.CustomProperties["anchorPos"],
                    anchorPos,
                    offset,
                    gameObject.transform.position,
                    gameObject.transform.parent.transform.position);
                yield return new WaitForSeconds(1f);
            }

        }

        #endregion

    }

}

