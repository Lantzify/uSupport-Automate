using Umbraco.Automate.Core.Triggers;
using uSupport.Constants;
using uSupport.Notifications;

namespace uSupport.Automate.Triggers;

// Each trigger listens for a notification uSupport already publishes. Automate finds them
// through [Trigger] and wires up the notification handlers itself.
// All of them need the workspace's service account to have the uSupport section.

[Trigger("uSupport.ticketCreated", "Ticket Created",
    Description = "Fires when a uSupport ticket is created.",
    Group = "uSupport",
    Icon = "icon-ticket",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class TicketCreatedTrigger(TriggerInfrastructure infrastructure, uSupportAutomateHelper helper)
    : NotificationTriggerBase<TicketTriggerSettings, TicketOutput, CreateTicketNotification>(infrastructure)
{
    public override IEnumerable<TriggerEvent> MapEvent(CreateTicketNotification notification)
    {
        yield return TicketTriggerEvent.Create(Alias, $"{notification.Ticket.Id}", helper.MapTicket<TicketOutput>(notification.Ticket));
    }
}

[Trigger("uSupport.ticketUpdated", "Ticket Updated",
    Description = "Fires whenever a uSupport ticket is saved, including when a comment is added to it.",
    Group = "uSupport",
    Icon = "icon-edit",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class TicketUpdatedTrigger(TriggerInfrastructure infrastructure, uSupportAutomateHelper helper)
    : NotificationTriggerBase<TicketTriggerSettings, TicketOutput, UpdateTicketNotification>(infrastructure)
{
    public override IEnumerable<TriggerEvent> MapEvent(UpdateTicketNotification notification)
    {
        var ticket = notification.Ticket;
        yield return TicketTriggerEvent.Create(Alias, TicketTriggerEvent.UpdateKey(ticket.Id, ticket.LastUpdated), helper.MapTicket<TicketOutput>(ticket));
    }
}

[Trigger("uSupport.ticketStatusChanged", "Ticket Status Changed",
    Description = "Fires when a uSupport ticket moves to another status.",
    Group = "uSupport",
    Icon = "icon-flag",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class TicketStatusChangedTrigger(TriggerInfrastructure infrastructure, uSupportAutomateHelper helper)
    : NotificationTriggerBase<TicketTriggerSettings, TicketStatusChangedOutput, TicketHistoryNotification>(infrastructure)
{
    public override IEnumerable<TriggerEvent> MapEvent(TicketHistoryNotification notification)
    {
        if (notification.OldTicket.StatusId == notification.NewTicket.StatusId)
        {
            yield break;
        }

        var ticket = notification.NewTicket;
        yield return TicketTriggerEvent.Create(Alias, TicketTriggerEvent.UpdateKey(ticket.Id, ticket.LastUpdated), helper.MapStatusChange(notification.OldTicket, ticket));
    }
}

[Trigger("uSupport.ticketResolved", "Ticket Resolved",
    Description = "Fires when an open uSupport ticket moves to a resolved status.",
    Group = "uSupport",
    Icon = "icon-check",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class TicketResolvedTrigger(TriggerInfrastructure infrastructure, uSupportAutomateHelper helper)
    : NotificationTriggerBase<TicketTriggerSettings, TicketStatusChangedOutput, TicketHistoryNotification>(infrastructure)
{
    // Not UpdateTicketResolvedNotification: uSupport raises that on every save of a ticket
    // that is already resolved, not only when it becomes resolved.
    public override IEnumerable<TriggerEvent> MapEvent(TicketHistoryNotification notification)
    {
        if (notification.OldTicket.StatusId == notification.NewTicket.StatusId
            || helper.GetStatus(notification.OldTicket.StatusId) is { Active: false })
        {
            yield break;
        }

        var output = helper.MapStatusChange(notification.OldTicket, notification.NewTicket);
        if (output.IsResolved)
        {
            yield return TicketTriggerEvent.Create(Alias, TicketTriggerEvent.UpdateKey(output.TicketId, output.LastUpdated), output);
        }
    }
}

[Trigger("uSupport.ticketCommentAdded", "Ticket Comment Added",
    Description = "Fires when a comment is added to a uSupport ticket.",
    Group = "uSupport",
    Icon = "icon-chat",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class TicketCommentAddedTrigger(TriggerInfrastructure infrastructure, uSupportAutomateHelper helper)
    : NotificationTriggerBase<TicketTriggerSettings, TicketCommentOutput, AddTicketCommentNotification>(infrastructure)
{
    public override IEnumerable<TriggerEvent> MapEvent(AddTicketCommentNotification notification)
    {
        yield return TicketTriggerEvent.Create(Alias, $"{notification.Comment.Id}", helper.MapComment(notification.Ticket, notification.Comment));
    }
}

[Trigger("uSupport.ticketDeleted", "Ticket Deleted",
    Description = "Fires when a uSupport ticket is deleted.",
    Group = "uSupport",
    Icon = "icon-trash",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class TicketDeletedTrigger(TriggerInfrastructure infrastructure, uSupportAutomateHelper helper)
    : NotificationTriggerBase<TicketTriggerSettings, TicketOutput, DeleteTicketNotification>(infrastructure)
{
    public override IEnumerable<TriggerEvent> MapEvent(DeleteTicketNotification notification)
    {
        yield return TicketTriggerEvent.Create(Alias, $"{notification.Ticket.Id}", helper.MapTicket<TicketOutput>(notification.Ticket));
    }
}

internal static class TicketTriggerEvent
{
    // The key lets Automate drop a notification that fires twice for the same change.
    internal static TriggerEvent Create<TOutput>(string alias, string? key, TOutput output) where TOutput : class
        => new TriggerEvent<TOutput>
        {
            TriggerAlias = alias,
            InitiatorType = TriggerInitiatorType.System,
            IdempotencyKey = key is null ? null : $"{alias}:{key}",
            Output = output,
        };

    // Without a LastUpdated there's nothing to tell two saves apart, so skip deduplication.
    internal static string? UpdateKey(Guid ticketId, DateTime? lastUpdated)
        => lastUpdated is { } date ? $"{ticketId}:u{date.Ticks}" : null;
}
