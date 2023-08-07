using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using Image = UnityEngine.UI.Image;

namespace ThronefallTAS.UI;

public class InfoPanel : UniverseLib.UI.Panels.PanelBase
{
    public override string Name => "Info Panel";
    public override int MinWidth => 200;
    public override int MinHeight => 0;
    public override Vector2 DefaultAnchorMin => new(0.0f, 0.8f);
    public override Vector2 DefaultAnchorMax => new(0.06f, 1.0f);

    public override Vector2 DefaultPosition => new(
        -Owner.Canvas.renderingDisplaySize.x / 2,
        Owner.Canvas.renderingDisplaySize.y / 2
    );

    public override bool CanDragAndResize => false;

    private Text _frameCounter;
    private Text _state;

    public InfoPanel(UIBase owner) : base(owner)
    {
    }

    protected override void ConstructPanelContent()
    {
        uiRoot.GetComponent<Image>().color = Color.clear;
        ContentRoot.GetComponent<Image>().color = Color.clear;

        var main = UIFactory.CreateUIObject("main", ContentRoot);
        UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
            main,
            true,
            false,
            true,
            true,
            padLeft: 5,
            spacing: 5,
            childAlignment: TextAnchor.UpperLeft
        );
        UIFactory.SetLayoutElement(main, preferredWidth: 120, preferredHeight: 60);

        _frameCounter = CreateEntry(main, "frame count", "Frame: ", "Idle");
        _state = CreateEntry(main, "state", "State: ", "Idle");

        LayoutRebuilder.ForceRebuildLayoutImmediate(uiRoot.GetComponent<RectTransform>());
    }

    private Text CreateEntry(GameObject parent, string name, string defaultLabel, string defaultValue)
    {
        var panel = UIFactory.CreateUIObject($"{name} panel", parent);
        UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(
            panel,
            true,
            false,
            true,
            true,
            childAlignment: TextAnchor.MiddleLeft
        );

        Object.Destroy(panel.GetComponent<Image>());
        UIFactory.SetLayoutElement(panel);

        var label = UIFactory.CreateLabel(panel, $"{name} label", defaultLabel);
        label.color = UIManager.TextColor;
        UIFactory.SetLayoutElement(label.gameObject, minHeight: 20);

        var value = UIFactory.CreateLabel(panel, $"{name}", defaultValue);
        value.color = UIManager.ButtonTextColor;
        value.alignment = TextAnchor.MiddleRight;
        UIFactory.SetLayoutElement(value.gameObject, minHeight: 20);

        return value;
    }

    public override void Update()
    {
        _frameCounter.text = Plugin.Instance.CurrentFrame.ToString();
        if (Plugin.Instance.Running)
        {
            _state.text = Plugin.Instance.Paused ? "Paused" : "Running";
        }
        else if (Plugin.Instance.Recording)
        {
            _state.text = "Recording";
        }
        else
        {
            _state.text = "Idle";
        }
    }
}