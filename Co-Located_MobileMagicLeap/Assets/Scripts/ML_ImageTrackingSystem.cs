using System.Collections;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class ImageTargetInfo
{
    public string Name;
    public Texture2D Image;
    public float LongerDimension;
}

public enum ImageTrackingStatus
{
    Inactive,
    PrivilegeDenied,
    ImageTrackingActive,
    CameraUnavailable
}

public class ML_ImageTrackingSystem : MonoBehaviour
{
#if PLATFORM_LUMIN
    private MLImageTracker.Target _imageTarget;
    public ImageTargetInfo TargetInfo;
    public GameObject TrackedImageFollower;

    public delegate void TrackingStatusChanged(ImageTrackingStatus status);
    public static TrackingStatusChanged OnImageTrackingStatusChanged;
    public ImageTrackingStatus CurrentStatus;

    public Vector3 ImagePos = Vector3.zero;
    public Quaternion ImageRot = Quaternion.identity;

    //for photon / network pos
    private Vector3 qrPos;
    private Quaternion qrRot;
    private bool hasScannedQR = false;
    public GameObject clientPrefab;

#region Unity Methods
    private void Awake()
    {
        UpdateImageTrackingStatus(ImageTrackingStatus.Inactive);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
        {
            StopImageTracking(true);
        }
        else
        {
            StartImageTracking();
        }
    }

    private void OnDestroy()
    {
        StopImageTracking(false);
    }

    void Start()
    {
        ActivatePrivileges();
    }

#endregion

#region Privilege Methods
    private void ActivatePrivileges()
    {
        if (CurrentStatus != ImageTrackingStatus.PrivilegeDenied)
        {
            MLPrivilegeRequesterBehavior requesterBehavior = GetComponent<MLPrivilegeRequesterBehavior>();
            if (requesterBehavior == null)
            {
                requesterBehavior = gameObject.AddComponent<MLPrivilegeRequesterBehavior>();
                requesterBehavior.Privileges = new MLPrivileges.RuntimeRequestId[]
                {
                    MLPrivileges.RuntimeRequestId.CameraCapture
                };
            }
            requesterBehavior.OnPrivilegesDone += HandlePrivilegesDone;
            requesterBehavior.enabled = true;
        }
    }

    private void HandlePrivilegesDone(MLResult result)
    {
        GetComponent<MLPrivilegeRequesterBehavior>().OnPrivilegesDone -= HandlePrivilegesDone;

        if (result.IsOk)
        {
            StartImageTracking();
        }
        else
        {
            Debug.LogError("Camera Privilege Denied or Not Present in Manifest");
            UpdateImageTrackingStatus(ImageTrackingStatus.PrivilegeDenied);
        }
    }
#endregion

#region Image Tracking Methods

    private void UpdateImageTrackingStatus(ImageTrackingStatus status)
    {
        CurrentStatus = status;
        OnImageTrackingStatusChanged?.Invoke(CurrentStatus);
    }

    public void StartImageTracking()
    {
        // Only start Image Tracking if privilege wasn't denied
        if (CurrentStatus != ImageTrackingStatus.PrivilegeDenied)
        {
            // Hasn't been started and attempt of start fails is likely due to the camera already being in use:
            if (!MLImageTracker.IsStarted && !MLImageTracker.Start().IsOk) //deprecated but fine for now
            {
                Debug.LogError("Image Tracker Could Not Start");
                UpdateImageTrackingStatus(ImageTrackingStatus.CameraUnavailable);
                return;
            }

            // MLImageTracker would have been started by previous If statement at this point, so enable it. 
            if (MLImageTracker.Enable().IsOk)
            {
                // Add the target image to the tracker and set the callback
                _imageTarget = MLImageTracker.AddTarget(TargetInfo.Name, TargetInfo.Image,
                                                        TargetInfo.LongerDimension, HandleImageTracked);
                UpdateImageTrackingStatus(ImageTrackingStatus.ImageTrackingActive);
            }
            else
            {
                Debug.LogError("Image Tracker Could Not Start");
                UpdateImageTrackingStatus(ImageTrackingStatus.CameraUnavailable);
                return;
            }
        }
    }

    public void StopImageTracking(bool pause)
    {
        if (MLImageTracker.IsStarted)
        {
            if (pause == true) // Temporarily disable the Image Tracker
            {
                MLImageTracker.RemoveTarget(TargetInfo.Name);
                MLImageTracker.Disable();
            }
            else
            {
                MLImageTracker.Stop();
            }
        }
    }

    private void HandleImageTracked(MLImageTracker.Target imageTarget,
                                    MLImageTracker.Target.Result imageTargetResult)
    {
        // If tracked, update position / rotation and move following object to that position / rotation
        switch (imageTargetResult.Status)
        {
            case MLImageTracker.Target.TrackingStatus.Tracked:

                ImagePos = imageTargetResult.Position;
                ImageRot = imageTargetResult.Rotation;

                if (TrackedImageFollower != null)
                {
                    TrackedImageFollower.transform.position = ImagePos;
                    TrackedImageFollower.transform.rotation = ImageRot;
                }

                // photon stuff -- on scan, instantiate network prefab, then update pos/rot each subsequent scan
                qrPos = imageTargetResult.Position;
                qrRot = imageTargetResult.Rotation;

                if (!PhotonNetwork.IsConnected)
                {
                    FindObjectOfType<CoLocated_MobileAR.Launcher>().Connect();
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

                        //set anchorPos of NetworkPosition component -- same as this gameobject (ML Headpose)
                        Vector3 MLPos = gameObject.transform.position;
                        Quaternion MLRot = gameObject.transform.rotation;
                        clientPrefab = PhotonNetwork.Instantiate("ClientPrefab", MLPos, MLRot);
                        clientPrefab.transform.parent = gameObject.transform;

                        //Debug.LogFormat("ClientPrefab Instantiated\n\n\narCamPos: {0}, arCamRot: {1}, anchorPos: {2}",
                        //    MLPos,
                        //    MLRot,
                        //    clientPrefab.GetComponent<CoLocated_MobileAR.NetworkPosition>().anchorPos);

                        //make sure instantiation only happens once
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


                break;

            case MLImageTracker.Target.TrackingStatus.NotTracked:
                // Additional Logic can be added here for when the image is not detected
                break;
        }
    }

#endregion
#endif
}