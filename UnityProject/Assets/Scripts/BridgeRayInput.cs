using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[DefaultExecutionOrder(-150)]
[RequireComponent(typeof(XRRayInteractor))]
public sealed class BridgeRayInput : MonoBehaviour
{
    public string deviceUsage = "Screen";
    InputAction position, rotation, tracked, select, value;
    XRRayInteractor ray;
    void Awake()
    {
        ray = GetComponent<XRRayInteractor>();
        string path = "<WebXRInteractionController>{" + deviceUsage + "}/";
        position = new InputAction("Position", InputActionType.Value, path + "devicePosition", expectedControlType:"Vector3");
        rotation = new InputAction("Rotation", InputActionType.Value, path + "deviceRotation", expectedControlType:"Quaternion");
        tracked = new InputAction("Tracked", InputActionType.Button, path + "isTracked");
        select = new InputAction("Select", InputActionType.Button, path + "triggerButton");
        value = new InputAction("Select value", InputActionType.Value, path + "trigger");
        ray.selectInput = new XRInputButtonReader {
            inputSourceMode = XRInputButtonReader.InputSourceMode.InputAction,
            inputActionPerformed = select, inputActionValue = value
        };
        ray.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.State;
        position.Enable(); rotation.Enable(); tracked.Enable(); select.Enable(); value.Enable();
    }
    void Update()
    {
        bool active = tracked.IsPressed();
        if (active)
        {
            transform.SetPositionAndRotation(position.ReadValue<Vector3>(), rotation.ReadValue<Quaternion>());
        }
        else transform.SetPositionAndRotation(new Vector3(10000, 10000, 10000), Quaternion.identity);
        // Keep actions enabled: a ray enabled during pointerdown would miss that first press.
    }
    void OnDestroy()
    {
        position?.Dispose(); rotation?.Dispose(); tracked?.Dispose(); select?.Dispose(); value?.Dispose();
    }
}
