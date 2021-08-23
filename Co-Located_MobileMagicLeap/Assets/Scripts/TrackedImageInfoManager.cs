using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
    /// and overlays some information as well as the source Texture2D on top of the
    /// detected image.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class TrackedImageInfoManager : MonoBehaviour
    {
        //Mobile Test stuff -- haven't yet rewritten this from the ARF sample, so formatting is different from my other scripts
        private Vector3 qrPos;
        private Quaternion qrRot;
        private bool hasScannedQR = false;
        public GameObject clientPrefab;

        ARTrackedImageManager m_TrackedImageManager;

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            if (trackedImage == null)
            {
                Debug.Log("no tracked image?");
                return;
            }

            qrPos = trackedImage.transform.position;
            qrRot = trackedImage.transform.rotation;
            //Debug.LogFormat("QRPos: {0}, PhonePos: {1}", qrPos, phonePos);

            if (!PhotonNetwork.IsConnected)
            {
                //start the Launcher.Connect() process
                FindObjectOfType<CoLocated_MobileAR.Launcher>().Connect();

                //Debug.LogFormat("QRPos: {0}, PhonePos: {1}",
                //    qrPos,
                //    phonePos);
            }
            else
            {
                //only instantiate prefab on first scan, but relocalize on any scan
                if (!hasScannedQR)
                {
                    //set my qrPos so all clients know where it is in my world space
                    Hashtable prop = new Hashtable();
                    prop.Add("anchorPos", qrPos);
                    prop.Add("anchorRot", qrRot);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(prop);

                    if (!PhotonNetwork.InRoom)
                    {
                        Debug.Log("waiting for room");
                        return;
                    }

                    //set anchorPos of NetworkPosition component -- TODO: eventually move components to ARCamera
                    Vector3 arCamPos = gameObject.transform.GetChild(0).transform.position;
                    Quaternion arCamRot = gameObject.transform.GetChild(0).transform.rotation;
                    clientPrefab = PhotonNetwork.Instantiate("ClientPrefab", arCamPos, arCamRot);
                    clientPrefab.transform.parent = gameObject.transform.GetChild(0).transform;

                    //Debug.LogFormat("ClientPrefab Instantiated\n\n\narCamPos: {0}, arCamRot: {1}, anchorPos: {2}",
                    //    arCamPos,
                    //    arCamRot,
                    //    clientPrefab.GetComponent<CoLocated_MobileAR.NetworkPosition>().anchorPos);

                    //make sure this only happens once
                    hasScannedQR = true;
                }
                else
                {
                    //update the qrPos, qrRot, and client prefab pos
                    PhotonNetwork.LocalPlayer.CustomProperties["anchorPos"] = qrPos;
                    PhotonNetwork.LocalPlayer.CustomProperties["anchorRot"] = qrRot;

                    //Debug.Log("relocalizing on scan");
                }

            }
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                UpdateInfo(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
                UpdateInfo(trackedImage);
        }
    }
}