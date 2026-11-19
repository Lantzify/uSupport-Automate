using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Settings;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;
using uSupport.Constants;
using uSupport.Helpers;
using uSupport.Migrations.Schemas;
using uSupport.Notifications;
using uSupport.Services.Interfaces;

namespace uSupport.Automate.Actions;

[Action("uSupport.addTicketComment", "Add Ticket Comment",
    Description = "Adds a comment to a uSupport ticket. The workspace's service account is the comment's author.",
    Group = "uSupport",
    Icon = "icon-chat",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class AddTicketCommentAction(
    ActionInfrastructure infrastructure,
    uSupportAutomateHelper helper,
    IuSupportTicketService ticketService,
    IuSupportTicketCommentService commentService,
    IHtmlSanitizer htmlSanitizer,
    IEventAggregator eventAggregator)
    : ActionBase<AddTicketCommentSettings, TicketCommentOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<AddTicketCommentSettings>();

        if (helper.GetTicket(settings.TicketId) is not { } ticket)
        {
            return uSupportAutomateHelper.Invalid($"Ticket '{settings.TicketId}' not found.");
        }

        if (string.IsNullOrWhiteSpace(settings.Comment))
        {
            return uSupportAutomateHelper.Invalid("Comment is required.");
        }

        if (await helper.GetServiceAccountAsync(context) is not { } user)
        {
            return uSupportAutomateHelper.NoServiceAccount();
        }

        // Mirrors uSupport's comment endpoint: save the comment, touch the ticket, then raise
        // the notification that writes the ticket history (and fires "Ticket Comment Added").
        var comment = commentService.Create(new uSupportTicketCommentSchema
        {
            TicketId = ticket.Id,
            UserId = user.Id,
            Comment = htmlSanitizer.Sanitize(settings.Comment),
        });

        var ticketSchema = ticket.ConvertDtoToSchema();
        ticketSchema.LastUpdatedBy = user.Name;
        ticketSchema.LastUpdated = DateTime.Now;
        var updatedTicket = ticketService.Update(ticketSchema);

        await eventAggregator.PublishAsync(new AddTicketCommentNotification(updatedTicket, comment), cancellationToken);

        context.LogInfo($"Added a comment to ticket {updatedTicket.ExternalTicketId}");
        return Success(helper.MapComment(updatedTicket, comment));
    }
}

public sealed class AddTicketCommentSettings
{
    [Field(Label = "Ticket ID", Description = "The ticket's unique ID, e.g. from a uSupport trigger.", SupportsBindings = true)]
    public string TicketId { get; set; } = string.Empty;

    [Field(
        Label = "Comment",
        Description = "The comment text. HTML is allowed and gets sanitized.",
        SortOrder = 1,
        SupportsBindings = true,
        EditorUiAlias = "Umb.PropertyEditorUi.TextArea")]
    public string Comment { get; set; } = string.Empty;
}
