using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using Photon.Pun;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
    /// and overlays some information as well as the source Texture2D on top of the
    /// detected image.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class TrackedImageInfoManager : MonoBehaviour
    {
        //Mobile Test stuff
        private Vector3 qrPos;
        private Vector3 phonePos;
        public GameObject phone;
        private Vector3 anchorPos;
        private bool hasScannedQR = false;

        [SerializeField]
        [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
        Camera m_WorldSpaceCanvasCamera;

        /// <summary>
        /// The prefab has a world space UI canvas,
        /// which requires a camera to function properly.
        /// </summary>
        public Camera worldSpaceCanvasCamera
        {
            get { return m_WorldSpaceCanvasCamera; }
            set { m_WorldSpaceCanvasCamera = value; }
        }

        //[SerializeField]
        //[Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
        //Texture2D m_DefaultTexture;

        ///// <summary>
        ///// If an image is detected but no source texture can be found,
        ///// this texture is used instead.
        ///// </summary>
        //public Texture2D defaultTexture
        //{
        //    get { return m_DefaultTexture; }
        //    set { m_DefaultTexture = value; }
        //}

        ARTrackedImageManager m_TrackedImageManager;

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

            //check existing anchor pos on network position
            //gameObject.transform.GetChild(0).GetChild(0)
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
            // Set canvas camera
            //var canvas = trackedImage.GetComponentInChildren<Canvas>();
            //canvas.worldCamera = worldSpaceCanvasCamera;

            qrPos = trackedImage.transform.position;
            phonePos = phone.transform.position;
            Debug.LogFormat("QRPos: {0}, PhonePos: {1}", qrPos, phonePos);
            if (!hasScannedQR)
            {
                //start the Launcher.Connect() process
                FindObjectOfType<CoLocated_MobileAR.Launcher>().Connect();

                Debug.LogFormat("QRPos: {0}, PhonePos: {1}", qrPos, phonePos);

                if (PhotonNetwork.IsConnected)
                {
                    //set anchorPos of NetworkPosition component
                    Vector3 arCamPos = gameObject.transform.GetChild(0).transform.position;
                    Quaternion arCamRot = gameObject.transform.GetChild(0).transform.rotation;
                    GameObject clientPrefab = PhotonNetwork.Instantiate("ClientPrefab", arCamPos, arCamRot);
                    clientPrefab.transform.parent = gameObject.transform.GetChild(0).transform;
                    clientPrefab.GetComponent<CoLocated_MobileAR.NetworkPosition>().anchorPos = qrPos;

                    Debug.LogFormat("arCamPos: {0}, arCamRot: {1}, anchorPos: {2}", arCamPos, arCamRot, clientPrefab.GetComponent<CoLocated_MobileAR.NetworkPosition>().anchorPos);

                    //make sure this only happens once -- TODO: make it so that if they lose tracking they can reset
                    hasScannedQR = true;
                }
            }


            //// Update information about the tracked image
            //var text = canvas.GetComponentInChildren<Text>();
            //text.text = string.Format(
            //    "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm\nQR Pos: {5}\nPhone Pos: {6}",
            //    trackedImage.referenceImage.name,
            //    trackedImage.trackingState,
            //    trackedImage.referenceImage.guid,
            //    trackedImage.referenceImage.size * 100f,
            //    trackedImage.size * 100f,
            //    qrPos,
            //    phonePos);

            //var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
            //var planeGo = planeParentGo.transform.GetChild(0).gameObject;

            // Disable the visual plane if it is not being tracked
            //if (trackedImage.trackingState != TrackingState.None)
            //{
            //    planeGo.SetActive(true);

            //    // The image extents is only valid when the image is being tracked
            //    trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

            //    // Set the texture
            //    var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
            //    material.mainTexture = trackedImage.referenceImage.texture; //changed from defaultTexture
            //}
            //else
            //{
            //    planeGo.SetActive(false);
            //}
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

                UpdateInfo(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
                UpdateInfo(trackedImage);
        }
    }
}