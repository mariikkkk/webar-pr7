using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using WebXR;
using WebXR.XRITBridge;

// DOM overlay -> bridge virtual device -> Input System -> XRI ray -> XR Grab.
[DefaultExecutionOrder(-200)]
public sealed class ScreenGestureInput : MonoBehaviour
{
    [Serializable] public class Gesture { public float x, y, ratio = 1; public bool pressed, tracked; }
    public WebXRControllerAdapter bridge;
    public InteractiveObject target;
    Gesture gesture = new Gesture();
    string lastStatus;
    [DllImport("__Internal")] static extern void PR7Publish(string json);
    [Serializable] class Snapshot
    {
        public bool selected, hovered;
        public float scale;
    }
    void Awake() { InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; }
    void OnEnable() { WebXRManager.OnXRChange += ModeChanged; }
    void OnDisable() { WebXRManager.OnXRChange -= ModeChanged; Cancel(); }
    IEnumerator Start() { return PlaceNextFrame(); }
    void ModeChanged(WebXRState state, int count, Rect left, Rect right)
    {
        Cancel(); StartCoroutine(PlaceNextFrame());
    }
    IEnumerator PlaceNextFrame() { yield return null; yield return null; Place(); }
    public void Pointer(string json)
    {
        Gesture next = JsonUtility.FromJson<Gesture>(json);
        if (next == null || float.IsNaN(next.x) || float.IsNaN(next.y)) return;
        gesture = next;
        target.ScaleBy(next.ratio);
        Submit();
    }
    void Update()
    {
        if (gesture.tracked) Submit();
        Publish();
    }
    void Submit()
    {
        Camera camera = Camera.main;
        if (camera == null || bridge == null || bridge.Screen == null) return;
        Ray ray = camera.ViewportPointToRay(new Vector3(Mathf.Clamp01(gesture.x), 1 - Mathf.Clamp01(gesture.y), 0));
        bridge.Screen.SubmitScreenRay(ray.origin, Quaternion.LookRotation(ray.direction, camera.transform.up), gesture.pressed, gesture.tracked);
    }
    public void Cancel()
    {
        gesture = new Gesture();
        Submit();
    }
    public void ResetObject() { Cancel(); StartCoroutine(PlaceNextFrame()); }
    void Place()
    {
        Camera camera = Camera.main;
        if (camera == null || target == null) return;
        target.transform.position = camera.transform.position + camera.transform.forward * 1f;
        target.transform.rotation = camera.transform.rotation * Quaternion.Euler(20, 30, 0);
        target.ResetScale();
    }
    void Publish()
    {
        if (target == null || target.Grab == null) return;
        string json = JsonUtility.ToJson(new Snapshot {
            selected = target.Grab.isSelected, hovered = target.Grab.isHovered, scale = target.ScaleFactor
        });
        if (json == lastStatus) return;
        lastStatus = json;
#if UNITY_WEBGL && !UNITY_EDITOR
        PR7Publish(json);
#endif
    }
}
