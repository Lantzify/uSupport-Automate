using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Settings;
using uSupport.Constants;

namespace uSupport.Automate.Actions;

[Action("uSupport.getTicket", "Get Ticket",
    Description = "Loads a uSupport ticket so later steps can use its fields.",
    Group = "uSupport",
    Icon = "icon-search",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class GetTicketAction(ActionInfrastructure infrastructure, uSupportAutomateHelper helper)
    : ActionBase<GetTicketSettings, TicketOutput>(infrastructure)
{
    public override Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<GetTicketSettings>();

        return Task.FromResult(helper.GetTicket(settings.TicketId) is { } ticket
            ? Success(helper.MapTicket<TicketOutput>(ticket))
            : uSupportAutomateHelper.Invalid($"Ticket '{settings.TicketId}' not found."));
    }
}

public sealed class GetTicketSettings
{
    [Field(Label = "Ticket ID", Description = "The ticket's unique ID.", SupportsBindings = true)]
    public string TicketId { get; set; } = string.Empty;
}
