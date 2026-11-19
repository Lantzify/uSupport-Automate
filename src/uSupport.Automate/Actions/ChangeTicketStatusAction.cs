using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Settings;
using Umbraco.Cms.Core.Events;
using uSupport.Constants;
using uSupport.Helpers;
using uSupport.Notifications;
using uSupport.Services.Interfaces;

namespace uSupport.Automate.Actions;

[Action("uSupport.changeTicketStatus", "Change Ticket Status",
    Description = "Moves a uSupport ticket to another status, e.g. to resolve it.",
    Group = "uSupport",
    Icon = "icon-flag",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class ChangeTicketStatusAction(
    ActionInfrastructure infrastructure,
    uSupportAutomateHelper helper,
    IuSupportTicketService ticketService,
    IuSupportTicketStatusService statusService,
    IEventAggregator eventAggregator)
    : ActionBase<ChangeTicketStatusSettings, TicketOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<ChangeTicketStatusSettings>();

        if (helper.GetTicket(settings.TicketId) is not { } ticket)
        {
            return uSupportAutomateHelper.Invalid($"Ticket '{settings.TicketId}' not found.");
        }

        var status = string.IsNullOrWhiteSpace(settings.Status) ? null
            : statusService.Get(Guid.TryParse(settings.Status, out var statusId) ? statusId : statusService.GetStatusIdFromName(settings.Status));
        if (status is null)
        {
            return uSupportAutomateHelper.Invalid($"Ticket status '{settings.Status}' not found.");
        }

        if (status.Id == ticket.StatusId)
        {
            context.LogInfo($"Ticket {ticket.ExternalTicketId} already has status {status.Name}");
            return Success(helper.MapTicket<TicketOutput>(ticket));
        }

        if (await helper.GetServiceAccountAsync(context) is not { } user)
        {
            return uSupportAutomateHelper.NoServiceAccount();
        }

        // Mirrors uSupport's update endpoint, including the notifications that write the
        // ticket history and fire "Ticket Status Changed" / "Ticket Resolved".
        var ticketSchema = ticket.ConvertDtoToSchema();
        ticketSchema.StatusId = status.Id;
        ticketSchema.Resolved = status.Active ? null : DateTime.Now;
        ticketSchema.LastUpdatedBy = user.Name;
        ticketSchema.LastUpdated = DateTime.Now;
        var updatedTicket = ticketService.Update(ticketSchema);

        if (!status.Active)
        {
            await eventAggregator.PublishAsync(new UpdateTicketResolvedNotification(updatedTicket), cancellationToken);
        }

        await eventAggregator.PublishAsync(new TicketHistoryNotification(user.Id, ticket, updatedTicket), cancellationToken);

        context.LogInfo($"Moved ticket {updatedTicket.ExternalTicketId} to {status.Name}");
        return Success(helper.MapTicket<TicketOutput>(updatedTicket));
    }
}

public sealed class ChangeTicketStatusSettings
{
    [Field(Label = "Ticket ID", Description = "The ticket's unique ID, e.g. from a uSupport trigger.", SupportsBindings = true)]
    public string TicketId { get; set; } = string.Empty;

    [Field(
        Label = "Status",
        Description = "The new status's name (e.g. Resolved) or ID.",
        SortOrder = 1,
        SupportsBindings = true)]
    public string Status { get; set; } = string.Empty;
}
