using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MouseFix;

public class MouseFix : ModBehaviour {
    public static MouseFix Instance;

    public void Awake() {
        Instance = this;
    }

    public void Start() {
        // Starting here, you'll have access to OWML's mod helper.
        ModHelper.Console.WriteLine($"{nameof(MouseFix)} is loaded!", MessageType.Success);

        new Harmony("Novaenia.MouseFix").PatchAll(Assembly.GetExecutingAssembly());
    }
}

[HarmonyPatch]
public class MouseFixPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AbstractInputAction), nameof(AbstractInputAction.TryApplyMouseProcessing))]
    public static bool AbstractInputAction_TryApplyMouseSmoothing_Prefix(AbstractInputAction __instance, ref float value) {
        value *= 0.05f;
        return false;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.UpdateInput))]
    public static void PlayerCameraController_UpdateInput_Postfix(PlayerCameraController __instance, float deltaTime) {
        // same as vanilla UpdateInput code but this time we're doing degreesX all da time
        bool freeLook = __instance._shipController != null && __instance._shipController.AllowFreeLook() && OWInput.IsPressed(InputLibrary.freeLook, 0f);
        bool inputMode = OWInput.IsInputMode(InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam | InputMode.PatchingSuit);
        if (__instance._isSnapping || __instance._isLockedOn || (PlayerState.InZeroG() && PlayerState.IsWearingSuit()) || (!inputMode && !freeLook))
            return;

        bool inAlarmSequence = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
        Vector2 vector = Vector2.one;
        vector *= ((__instance._zoomed || inAlarmSequence) ? PlayerCameraController.ZOOM_SCALAR : 1f);
        vector *= __instance._playerCamera.fieldOfView / __instance._initFOV;
        if (Time.timeScale > 1f)
            vector /= Time.timeScale;
        float num = InputUtil.IsMouseMoveAxis(InputLibrary.look.AxisID) ? 0.01666667f : deltaTime;
        float lookRate = OWInput.UsingGamepad() ? PlayerCameraController.GAMEPAD_LOOK_RATE_Y : PlayerCameraController.LOOK_RATE;
        __instance._degreesX += OWInput.GetAxisValue(InputLibrary.look, InputMode.All).x * lookRate * vector.x * num;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.UpdateRotation))]
    public static bool PlayerCameraController_UpdateRotation_Prefix(PlayerCameraController __instance) {
        bool freeLook = __instance._shipController != null && __instance._shipController.AllowFreeLook() && OWInput.IsPressed(InputLibrary.freeLook, 0f);
        __instance._degreesX %= 360f;
        __instance._degreesY %= 360f;
        if (!__instance._isSnapping) {
            if (freeLook) {
                __instance._degreesX = Mathf.Clamp(__instance._degreesX, -60f, 60f);
                __instance._degreesY = Mathf.Clamp(__instance._degreesY, -35f, 80f);
            } else {
                __instance._degreesY = Mathf.Clamp(__instance._degreesY, -89.999f, 89.999f);
            }
        }
        __instance._rotationX = Quaternion.AngleAxis(__instance._degreesX, Vector3.up);
        __instance._rotationY = Quaternion.AngleAxis(__instance._degreesY, -Vector3.right);
        Quaternion quaternion;
        if (freeLook || !Time.inFixedTimeStep || (PlayerState.InZeroG() && PlayerState.IsWearingSuit()))
            quaternion = __instance._rotationX * __instance._rotationY * Quaternion.identity;
        else {
            quaternion = __instance._rotationY * Quaternion.identity;
            PlayerCharacterController character = __instance._characterController;
            character.transform.rotation = Quaternion.AngleAxis(__instance._degreesX, character.transform.up) * character.transform.rotation;
            __instance._degreesX = 0.0f;
        }
        __instance._playerCamera.transform.localRotation = quaternion;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCharacterController), nameof(PlayerCharacterController.UpdateTurning))]
    public static bool PlayerCameraController_UpdateTurning_Prefix(PlayerCharacterController __instance) {
        float num = __instance._playerCam.fieldOfView / __instance._initFOV;
        float num2 = OWInput.GetAxisValue(InputLibrary.look, InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam).x * num;
        __instance._lastTurnInput = num2;
        bool flag = Locator.GetAlarmSequenceController() != null && Locator.GetAlarmSequenceController().IsAlarmWakingPlayer();
        if (__instance._isGrounded && __instance._groundBody != null) {
            Vector3 vector = ((__instance._movingPlatform != null) ? __instance._movingPlatform.GetAngularVelocity() : __instance._groundBody.GetAngularVelocity());
            int num4 = (int)Mathf.Sign(Vector3.Dot(vector, __instance._transform.up));
            __instance._baseAngularVelocity = Vector3.Project(vector, __instance._transform.up).magnitude * (float)num4;
        } else {
            __instance._baseAngularVelocity *= 0.995f;
        }
        Locator.GetPlayerCameraController()._degreesX += __instance._baseAngularVelocity * 180f / 3.1415927f * Time.fixedDeltaTime;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.Update))]
    public static bool PlayerCameraController_Update_Prefix(PlayerCameraController __instance) {
        if (__instance._shipController != null && __instance._shipController.AllowFreeLook() && OWInput.IsNewlyReleased(InputLibrary.freeLook, InputMode.All))
            __instance.CenterCameraOverSeconds(0.33f, true);
        // now always update the camera, even when reading:
        __instance.UpdateCamera(Time.unscaledDeltaTime);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerCameraController), nameof(PlayerCameraController.FixedUpdate))]
    public static bool PlayerCameraController_FixedUpdate_Prefix(PlayerCameraController __instance) {
        __instance.UpdateRotation();
        return false;
    }
}