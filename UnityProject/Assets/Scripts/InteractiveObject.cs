using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public sealed class InteractiveObject : MonoBehaviour
{
    public float ScaleFactor { get; private set; } = 1f;
    public XRGrabInteractable Grab { get; private set; }
    Material material;
    readonly Color idle = new Color(.15f, .55f, .95f);
    void Awake()
    {
        Grab = GetComponent<XRGrabInteractable>();
        material = GetComponent<Renderer>().material;
        Grab.selectEntered.AddListener(_ => Refresh());
        Grab.selectExited.AddListener(_ => Refresh());
        Grab.hoverEntered.AddListener(_ => Refresh());
        Grab.hoverExited.AddListener(_ => Refresh());
    }
    void Refresh()
    {
        material.color = Grab.isSelected ? new Color(1f, .65f, .05f)
            : Grab.isHovered ? new Color(.15f, .95f, .8f) : idle;
    }
    public void ScaleBy(float ratio)
    {
        if (!Grab.isSelected || float.IsNaN(ratio) || float.IsInfinity(ratio)) return;
        ScaleFactor = Mathf.Clamp(ScaleFactor * Mathf.Clamp(ratio, .5f, 2f), .4f, 3f);
        transform.localScale = Vector3.one * (.2f * ScaleFactor);
    }
    public void ResetScale()
    {
        ScaleFactor = 1f;
        transform.localScale = Vector3.one * .2f;
    }
    void OnDestroy() { if (material != null) Destroy(material); }
}
