using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;

namespace uSupport.Automate.Triggers;

/// <summary>
/// Settings shared by the uSupport triggers. Without the loop setting, an automation whose
/// action changes a ticket would fire its own trigger again and again.
/// </summary>
public sealed class TicketTriggerSettings : IAutomationOriginatedEventBehavior
{
    [Field(
        Label = "When triggered by another automation",
        Description = "How to handle ticket changes made by another automation.",
        EditorUiAlias = "Umb.PropertyEditorUi.Dropdown",
        EditorConfig = """
            [{ "alias": "items", "value": [
                { "name": "Always run", "value": "Run" },
                { "name": "Skip if this would loop", "value": "SkipOnCycle" },
                { "name": "Skip entirely", "value": "SkipAlways" }
            ] }]
            """,
        Group = "Advanced")]
    public string OnAutomationOriginatedEvent { get; set; } = nameof(AutomationOriginatedEventBehavior.SkipOnCycle);

    AutomationOriginatedEventBehavior IAutomationOriginatedEventBehavior.OnAutomationOriginated
        => Enum.TryParse<AutomationOriginatedEventBehavior>(OnAutomationOriginatedEvent, ignoreCase: true, out var value)
            ? value
            : AutomationOriginatedEventBehavior.SkipOnCycle;
}
