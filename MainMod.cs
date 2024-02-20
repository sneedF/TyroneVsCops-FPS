using MelonLoader;
using UnityEngine;
using HarmonyLib;
using Invector.vCamera;
using Invector.vCharacterController;
using System;

namespace TyroonVsChuds
{
    public class MainMod : MelonMod
    {
        private static Camera camera;
        private static Rigidbody Tyrone;
        private static bool isFirstPerson = true;
        private float verticalRotation = 0f;
        private static vHeadTrack headTrack;
        private float FOV = 120f; // Initial FOV value

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            HarmonyInstance.PatchAll();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            LoggerInstance.Msg($"SCENE LOADED {sceneName}");
            if (sceneName == "tyrone_vs_cops_game")
            {
                GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
                camera = cameraObject.GetComponent<Camera>();
                GameObject tyroneObject = GameObject.FindGameObjectWithTag("Player");
                Tyrone = tyroneObject.GetComponent<Rigidbody>();
                headTrack = GameObject.Find("vShooterController_tyrone_sr").GetComponent<vHeadTrack>();
            }
        }


        [HarmonyPatch(typeof(vThirdPersonCamera))]
        [HarmonyPatch(nameof(vThirdPersonCamera.RotateCamera))]
        [HarmonyPatch(MethodType.Normal)]
        public class CameraPatch
        {
            [HarmonyPrefix]
            static bool Prefix(ref float x, ref float y)
            {
                if (isFirstPerson)
                {
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(vThirdPersonCamera))]
        [HarmonyPatch("CameraMovement")]
        [HarmonyPatch(MethodType.Normal)]
        public class CameraPatch2
        {
            [HarmonyPrefix]
            static bool Prefix(ref bool forceUpdate)
            {
                if (isFirstPerson)
                {
                    return false;
                }
                return true;
            }
        }



        [HarmonyPatch(typeof(vThirdPersonMotor))]
        [HarmonyPatch("RotateToDirection")]
        [HarmonyPatch(new System.Type[] {typeof(Vector3), typeof(float)})]
        [HarmonyPatch(MethodType.Normal)]
        public class CameraPatch3
        {
            [HarmonyPrefix]
            static bool Prefix(ref Vector3 direction, float rotationSpeed)
            {
                if (isFirstPerson)
                {
                    return false;
                }
                return true;
            }
        }
        public override void OnLateUpdate()
        {
            base.OnLateUpdate();


            if (Input.GetKeyDown(KeyCode.Delete))
            {
                isFirstPerson = !isFirstPerson;
                LoggerInstance.Msg($"isFirstPerson set to {isFirstPerson}");
            }


            if (camera != null && Tyrone != null && isFirstPerson)
            {
                HandleFirstPersonView();
            }
        }

        private void HandleFirstPersonView()
        {
            float mouseX = Input.GetAxis("Mouse X") * PlayerPrefs.GetFloat("sensvalue", 2f);
            float mouseY = Input.GetAxis("Mouse Y") * PlayerPrefs.GetFloat("sensvalue", 2f);

            // Rotate the player object left and right (yaw)
            Tyrone.transform.Rotate(Vector3.up * mouseX);

            // Calculate the new vertical rotation (pitch), and clamp it to prevent over-rotation
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            // Calculate desired camera position
            //Vector3 desiredCameraPosition = MovePositionForward(camera.transform.position, Quaternion.Euler(verticalRotation, Tyrone.transform.rotation.eulerAngles.y, 0), distanceFromHead);
            camera.transform.position = headTrack.head.position;

            // Set camera rotation directly without smoothing
            camera.transform.rotation = Quaternion.Euler(verticalRotation, Tyrone.transform.rotation.eulerAngles.y, 0);

            // Set near clip plane and field of view
            camera.nearClipPlane = 0.25f;
            camera.fieldOfView = FOV;
        }
    }


}

