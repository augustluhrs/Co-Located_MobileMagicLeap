# Co-Located_MobileAR
*Basic Unity Template for using ARFoundation and Photon for Co-Located Mobile Localization*

![Gif of two phones, one iPhone, one Android, connecting to same scene and tracking each other's position and rotation](MobileLocalization_QRDemo1.gif)

### Why Co-Located AR is So Tricky




### Setup Steps For Deploying Demo to Devices
1. Download / Clone Repo
2. Open Project in Unity 2020.3
3. Setup Photon App using the Wizard -- TODO: Instructions
3. Build MobileQR_Test Scene to iOS or Android.
4. Open the image located at Assets/Scripts/TrackedImages/qrcode to use as the scanned image.

*note: The Client needs to hold the phone very still between first recognition of the qr code and when the network instantiation happens, or else the offset will be off.*



### Setup Steps for Using This Project as a Starting Template
1.
*note: This project just covers connecting different mobile devices, but to include shared content, you can follow standard photon guides and just replace the NetworkPosition custom TransformView for any network sync stuff -- to ensure positions are relative.*



### Setup Steps For Replicating This From Scratch
1. Create a new Unity 2020.3 project and default scene (MobileQR_Test).
2. Install the packages as found here: [ARFoundation Packages](https://github.com/Unity-Technologies/arfoundation-samples), minus "ARKit Face Tracking". Add AR Session Origin and AR Session, delete Main Camera, set AR Camera as main.
3. Install Photon (PUN) according to the [set up guide](https://doc.photonengine.com/en-us/pun/v2/demos-and-tutorials/pun-basics-tutorial/intro). Log in / setup account with PUN Wizard to get AppID. Make sure the AppID is correct in Resources/PhotonServerSettings.
4. Setup the basic networking scripts and gameobjects (Launcher.cs and PUN_Launcher) with basic connection to random room.
5. Setup the prefab representing the connected clients and attach Photon View components.
6. Create custom Network Position script component for sending relative position and rotation to QRCode via Photon View.
7. Import the ImageTracking scripts from ARFoundation. On image found, call Launcher.Connect(), then instantiate the ClientPrefab and set the anchorPos/Rot variables of our NetworkPosition component via a Custom Property of our Photon Player.
8. Create the XR Reference Image Library of images to track, for now just one QR Code png.
9. Update Build and Player settings for Android and iOS.
10. Get twisted up in a bunch of bugs and slight tweaks, go into a rabbit hole, and emerge two days later with a working project and no recollection of how you got there.