using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.Scripting;
using UnityEngine.XR;
using WebXR;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WebXR.XRITBridge
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(stateType = typeof(XRSimulatedControllerState), commonUsages = new[] {"LeftHand", "RightHand"}, isGenericTypeOfDevice = false, displayName = "WebXR Controller", updateBeforeRender = true)]
    [Preserve]
    public class WebXRInteractionController : XRController
    {
#if UNITY_EDITOR
        static WebXRInteractionController()
        {
            InputSystem.RegisterLayout<WebXRInteractionController>(
                matches: new InputDeviceMatcher()
                    .WithInterface("WebXRController"));
        }
#endif

        internal WebXRControllerHand ident;
        internal bool screenDevice;

        public void SubmitScreenRay(Vector3 position, Quaternion rotation, bool pressed, bool tracked)
        {
            InputSystem.QueueStateEvent(this, new XRSimulatedControllerState {
                devicePosition = position, deviceRotation = rotation,
                isTracked = tracked,
                trackingState = tracked ? (int)(InputTrackingState.Position | InputTrackingState.Rotation) : 0,
                trigger = pressed ? 1f : 0f,
                buttons = (ushort)(pressed ? 1 << (int)ControllerButton.TriggerButton : 0)
            });
        }

        protected override void OnRemoved()
        {

            WebXRManager.OnControllerUpdate -= OnControllerUpdate;
            base.OnRemoved();
        }

        protected override void FinishSetup()
        {
            base.FinishSetup();


            WebXRManager.OnControllerUpdate += OnControllerUpdate;
        }

        private const float pressMargin = .5f;

        private void OnControllerUpdate(WebXRControllerData data)
        {
            if (screenDevice || (WebXRControllerHand)data.hand != ident)
                return;

            ushort buttons = 0;

            if (data.squeeze >= 1f - pressMargin)
                buttons |= 1 << (int)ControllerButton.GripButton;
            if (data.trigger >= 1f - pressMargin)
                buttons |= 1 << (int)ControllerButton.TriggerButton;
            if (data.buttonATouched)
                buttons |= 1 << (int)ControllerButton.PrimaryTouch;
            if (data.buttonBTouched)
                buttons |= 1 << (int)ControllerButton.SecondaryTouch;
            if (data.thumbstickTouched)
                buttons |= 1 << (int)ControllerButton.Primary2DAxisTouch;
            if (data.touchpadTouched)
                buttons |= 1 << (int)ControllerButton.Secondary2DAxisTouch;

            buttons |= 1 << (int)ControllerButton.UserPresence;

            var state = new XRSimulatedControllerState
            {
                primary2DAxis = new(data.thumbstickX, data.thumbstickY),
                trigger = data.trigger,
                grip = data.squeeze,
                secondary2DAxis = new(data.touchpadX, data.touchpadY),
                buttons = buttons,
                batteryLevel = 0f,
                trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation),
                isTracked = data.enabled,
                devicePosition = data.position,
                deviceRotation = data.rotation
            };

            InputSystem.QueueStateEvent(this, state);
        }
    }

    public class WebXRControllerAdapter : MonoBehaviour
    {
        public WebXRInteractionController Screen { get; private set; }
        private WebXRInteractionController left, right;
        private void Awake()
        {
            InputSystem.RegisterLayout<WebXRInteractionController>();
            left = InputSystem.AddDevice<WebXRInteractionController>("WebXR Left");
            left.ident = WebXRControllerHand.LEFT;
            InputSystem.SetDeviceUsage(left, UnityEngine.InputSystem.CommonUsages.LeftHand);
            right = InputSystem.AddDevice<WebXRInteractionController>("WebXR Right");
            right.ident = WebXRControllerHand.RIGHT;
            InputSystem.SetDeviceUsage(right, UnityEngine.InputSystem.CommonUsages.RightHand);
            Screen = InputSystem.AddDevice<WebXRInteractionController>("WebXR Screen");
            Screen.screenDevice = true;
            InputSystem.SetDeviceUsage(Screen, "Screen");
        }
        private void OnDestroy()
        {
            if (left != null && left.added) InputSystem.RemoveDevice(left);
            if (right != null && right.added) InputSystem.RemoveDevice(right);
            if (Screen != null && Screen.added) InputSystem.RemoveDevice(Screen);
        }
    }
}
