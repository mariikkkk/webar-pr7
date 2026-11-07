Local adaptation for assignment 7, based on upstream 0.1.2.
- Preserve controller Input System layout and WebXR controller event mapping.
- Add a Screen virtual controller and SubmitScreenRay for DOM overlay gestures.
- Use WebXR poses unchanged; remove VR-only height subtraction and 30-degree tilt.
- Clean up event subscriptions/devices on removal.
- Use assembly names instead of hard-coded GUID dependencies.
- Do not instantiate WebXRHeadsetAdapter: WebXRCamera from exporter 0.25.0 owns AR cameras.
Original Apache-2.0 license retained. This is an adapted bridge, not upstream mobile support.
- Adapt legacy headset callback signature to the exporter 0.25.0 matrix-only event; this headset adapter remains unused.
- Use XR Interaction Toolkit 3.7.0 and Input System 1.20.1 with Unity 6000.6.
