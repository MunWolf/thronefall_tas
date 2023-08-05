using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using Image = UnityEngine.UI.Image;

namespace ThronefallTAS.UI;

public class TasPanel : UniverseLib.UI.Panels.PanelBase
{
    public override string Name => "Network Panel";
    public override int MinWidth => 0;
    public override int MinHeight => 0;
    public override Vector2 DefaultAnchorMin => new(0.43f, 0.27f);
    public override Vector2 DefaultAnchorMax => new(0.57f, 0.62f);
    public override bool CanDragAndResize => false;
    
    private static readonly Color BackgroundColor = new(0.11f, 0.11f, 0.11f, 1.0f);
    private static readonly Color TextColor = new(0.78f, 0.65f, 0.46f, 1.0f);
    private static readonly Color ButtonTextColor = new(0.97f, 0.88f, 0.75f, 1.0f);
    private static readonly Color ButtonHoverTextColor = new(0.0f, 0.41f, 0.11f);
    private static readonly Color ExitButtonColor = new(0.176f, 0.165f, 0.149f);

    public TasPanel(UIBase owner) : base(owner)
    {
        Rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void AddButtonEvent(ButtonRef button, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        var trigger = button.GameObject.GetComponent<EventTrigger>();
        var entry = new EventTrigger.TriggerEvent();
        entry.AddListener(action);
        
        trigger.triggers.Add(new EventTrigger.Entry()
        {
            eventID = type,
            callback = entry
        });
    }

    protected override void ConstructPanelContent()
    {
        uiRoot.GetComponent<Image>().color = BackgroundColor;
        ContentRoot.GetComponent<Image>().color = BackgroundColor;
        
        var exit = UIFactory.CreateButton(ContentRoot, "exit", "X");
        UIFactory.SetLayoutElement(exit.GameObject, minWidth: 20, minHeight: 20);
        exit.Component.image.enabled = false;
        exit.GameObject.AddComponent<EventTrigger>();
        exit.ButtonText.color = ExitButtonColor;
        AddButtonEvent(exit, EventTriggerType.PointerExit, (data) =>
        {
            exit.ButtonText.fontSize = 14;
            exit.ButtonText.color = ExitButtonColor;
        });
        AddButtonEvent(exit, EventTriggerType.PointerEnter, (data) =>
        {
            exit.ButtonText.fontSize = 16;
            exit.ButtonText.color = ButtonHoverTextColor;
        });
        AddButtonEvent(exit, EventTriggerType.Select, (data) =>
        {
            exit.ButtonText.fontSize = 14;
            exit.ButtonText.color = ExitButtonColor;
            UIManager.CloseNetworkPanel();
        });
        
        var panel = UIFactory.CreateHorizontalGroup(
            ContentRoot,
            "panel",
            true,
            true,
            true,
            true,
            childAlignment: TextAnchor.MiddleCenter,
            padding: new Vector4(10, 30, 5, 5),
            bgColor: BackgroundColor
        );
        UIFactory.SetLayoutElement(panel, flexibleWidth: 500, flexibleHeight: 600);
        panel.SetActive(false);
        
        var main = UIFactory.CreateVerticalGroup(
            panel,
            "main",
            false,
            false,
            true,
            true,
            spacing: 20,
            childAlignment: TextAnchor.MiddleCenter,
            bgColor: BackgroundColor
        );
        
        // Main group
        {
            var hostButton = UIFactory.CreateButton(main, "host", "Host", BackgroundColor);
            UIFactory.SetLayoutElement(hostButton.GameObject, minWidth: 80, minHeight: 20);
            hostButton.ButtonText.color = ButtonTextColor;
            hostButton.GameObject.GetComponent<Image>().enabled = false;
            hostButton.GameObject.AddComponent<EventTrigger>();
            AddButtonEvent(hostButton, EventTriggerType.PointerExit, (data) =>
            {
                hostButton.ButtonText.fontSize = 14;
                hostButton.ButtonText.color = ButtonTextColor;
            });
            AddButtonEvent(hostButton, EventTriggerType.PointerEnter, (data) =>
            {
                hostButton.ButtonText.fontSize = 16;
                hostButton.ButtonText.color = ButtonHoverTextColor;
            });
            
            var connectButton = UIFactory.CreateButton(main, "connect", "Connect", BackgroundColor);
            UIFactory.SetLayoutElement(connectButton.GameObject, minWidth: 80, minHeight: 20);
            connectButton.ButtonText.color = ButtonTextColor;
            connectButton.GameObject.GetComponent<Image>().enabled = false;
            connectButton.GameObject.AddComponent<EventTrigger>();
            AddButtonEvent(connectButton, EventTriggerType.PointerExit, (data) =>
            {
                connectButton.ButtonText.fontSize = 14;
                connectButton.ButtonText.color = ButtonTextColor;
            });
            AddButtonEvent(connectButton, EventTriggerType.PointerEnter, (data) =>
            {
                connectButton.ButtonText.fontSize = 16;
                connectButton.ButtonText.color = ButtonHoverTextColor;
            });
        }

        panel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(uiRoot.GetComponent<RectTransform>());
    }

    public override void Update()
    {
        
    }
}