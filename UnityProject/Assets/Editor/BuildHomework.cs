using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.SpatialTracking;
using WebXR;
using WebXR.XRITBridge;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public static class BuildHomework
{
    public static void Run()
    {
        Directory.CreateDirectory("Assets/Settings");
        foreach (string path in new[] {"Assets/Settings/XRGeneralSettings.asset", "Assets/Settings/WebXRSettings.asset", "Assets/Settings/Cube.mat"})
            AssetDatabase.DeleteAsset(path);
        var perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
        AssetDatabase.CreateAsset(perTarget, "Assets/Settings/XRGeneralSettings.asset");
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
        perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.WebGL);
        var general = perTarget.SettingsForBuildTarget(BuildTargetGroup.WebGL);
        general.InitManagerOnStart = true;
        if (!XRPackageMetadataStore.AssignLoader(general.Manager, "WebXR.WebXRLoader", BuildTargetGroup.WebGL))
            throw new System.Exception("Could not assign WebXR loader");
        var settings = ScriptableObject.CreateInstance<WebXRSettings>();
        settings.DisableXRDisplaySubsystem = true;
        settings.ARRequiredReferenceSpace = WebXRSettings.ReferenceSpaceTypes.local;
        settings.AROptionalFeatures = 0;
        settings.AutoLoadWebXRInputSystem = false;
        AssetDatabase.CreateAsset(settings, "Assets/Settings/WebXRSettings.asset");
        EditorBuildSettings.AddConfigObject("WebXR.Settings", settings, true);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var rig = new GameObject("WebXRCameraSet");
        var main = new GameObject("Main Camera");
        main.transform.SetParent(rig.transform);
        main.tag = "MainCamera";
        var camera = main.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.1f, 0.14f, 1);
        camera.nearClipPlane = .01f;
        var pose = main.AddComponent<TrackedPoseDriver>();
        pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);
        pose.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
        pose.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
        pose.UseRelativeTransform = false;
        var webCamera = main.AddComponent<WebXRCamera>();
        var serialized = new SerializedObject(webCamera);
        serialized.FindProperty("cameraMain").objectReferenceValue = camera;
        foreach (string name in new[] {"cameraL", "cameraR", "cameraARL", "cameraARR"})
        {
            var go = new GameObject(name);
            go.transform.SetParent(main.transform);
            var eye = go.AddComponent<Camera>();
            eye.enabled = false;
            eye.nearClipPlane = .01f;
            eye.clearFlags = CameraClearFlags.SolidColor;
            eye.backgroundColor = Color.clear;
            serialized.FindProperty(name).objectReferenceValue = eye;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        foreach (var hand in new[] {WebXRControllerHand.LEFT, WebXRControllerHand.RIGHT})
        {
            var go = new GameObject("Controller " + hand);
            go.transform.SetParent(rig.transform);
            go.AddComponent<WebXRController>().hand = hand;
        }
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Interactive Cube";
        cube.transform.localScale = Vector3.one * .2f;
        cube.transform.position = Vector3.forward;
        var physics = cube.AddComponent<Rigidbody>();
        physics.useGravity = false; physics.isKinematic = true;
        var grab = cube.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grab.throwOnDetach = false; grab.trackRotation = false; grab.trackScale = false;
        grab.useDynamicAttach = true;
        var feedback = cube.AddComponent<InteractiveObject>();
        new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
        var input = new GameObject("Interaction Input");
        var bridge = input.AddComponent<WebXRControllerAdapter>();
        var gestures = input.AddComponent<ScreenGestureInput>();
        gestures.bridge = bridge; gestures.target = feedback;
        foreach (string usage in new[] {"Screen", "LeftHand", "RightHand"})
        {
            var pointer = new GameObject(usage + " Ray");
            var ray = pointer.AddComponent<XRRayInteractor>();
            ray.maxRaycastDistance = 5; ray.enableUIInteraction = false;
            ray.allowAnchorControl = false; ray.useForceGrab = false;
            pointer.AddComponent<BridgeRayInput>().deviceUsage = usage;
        }
        var material = new Material(Shader.Find("Standard"));
        material.color = new Color(.15f, .55f, .95f);
        material.SetFloat("_Metallic", .35f);
        material.SetFloat("_Glossiness", .65f);
        AssetDatabase.CreateAsset(material, "Assets/Settings/Cube.mat");
        cube.GetComponent<Renderer>().sharedMaterial = material;
        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.transform.rotation = Quaternion.Euler(40, -30, 0);
        RenderSettings.ambientLight = Color.gray;
        EditorSceneManager.SaveScene(scene, "Assets/Homework.unity");
        PlayerSettings.companyName = "MIREA";
        PlayerSettings.productName = "WebAR PR7";
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.WebGL.template = "PROJECT:WebARInteractive";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.initialMemorySize = 128;
        PlayerSettings.WebGL.maximumMemorySize = 512;
        AssetDatabase.SaveAssets();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] {"Assets/Homework.unity"}, locationPathName = "../unity-pr7-build",
            target = BuildTarget.WebGL, options = BuildOptions.None
        });
        File.WriteAllText("../unity-pr7-build-result.txt", report.summary.result + "\n" + report.summary.totalSize + " bytes\n" + report.summary.totalTime);
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("Build failed");
    }
}
