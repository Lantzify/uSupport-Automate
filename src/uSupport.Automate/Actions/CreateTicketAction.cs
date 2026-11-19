using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Settings;
using Umbraco.Cms.Core.Security;
using uSupport.Constants;
using uSupport.Helpers;
using uSupport.Migrations.Schemas;
using uSupport.Services.Interfaces;

namespace uSupport.Automate.Actions;

[Action("uSupport.createTicket", "Create Ticket",
    Description = "Creates a uSupport ticket with the default status. The workspace's service account is the author.",
    Group = "uSupport",
    Icon = "icon-ticket",
    RequiredSections = [uSupportConstants.SectionAlias])]
public sealed class CreateTicketAction(
    ActionInfrastructure infrastructure,
    uSupportAutomateHelper helper,
    IuSupportTicketService ticketService,
    IuSupportTicketTypeService typeService,
    IuSupportTicketStatusService statusService,
    IHtmlSanitizer htmlSanitizer)
    : ActionBase<CreateTicketSettings, TicketOutput>(infrastructure)
{
    public override async Task<ActionResult> ExecuteAsync(ActionContext context, CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<CreateTicketSettings>();

        if (string.IsNullOrWhiteSpace(settings.Title))
        {
            return uSupportAutomateHelper.Invalid("Title is required.");
        }

        var type = string.IsNullOrWhiteSpace(settings.Type) ? null
            : Guid.TryParse(settings.Type, out var typeId) ? typeService.Get(typeId)
            : typeService.GetTypeIdFromName(settings.Type) is { } id ? typeService.Get(id)
            : null;
        if (type is null)
        {
            return uSupportAutomateHelper.Invalid($"Ticket type '{settings.Type}' not found.");
        }

        var status = statusService.GetDefaultStatus();
        if (status is null)
        {
            return ActionResult.Failed(
                new InvalidOperationException("uSupport has no default ticket status. Set one in the uSupport section."),
                StepRunErrorCategory.ConfigurationError);
        }

        if (await helper.GetServiceAccountAsync(context) is not { } user)
        {
            return uSupportAutomateHelper.NoServiceAccount();
        }

        // Same fields uSupport's own create endpoint sets. Summary can come from a binding
        // (a webhook body, say), so it's sanitized just like an editor's input.
        var ticket = ticketService.Create(new uSupportTicketSchema
        {
            Title = settings.Title,
            Summary = htmlSanitizer.Sanitize(settings.Summary),
            TypeId = type.Id,
            StatusId = status.Id,
            AuthorId = user.Id,
            LastUpdatedBy = user.Name,
            ExternalTicketId = uSupportExternalIdHelper.Generate(),
        });

        context.LogInfo($"Created ticket {ticket.ExternalTicketId}");
        return Success(helper.MapTicket<TicketOutput>(ticket));
    }
}

public sealed class CreateTicketSettings
{
    [Field(Label = "Title", Description = "The ticket title.", SupportsBindings = true)]
    public string Title { get; set; } = string.Empty;

    [Field(
        Label = "Summary",
        Description = "What the ticket is about. HTML is allowed and gets sanitized.",
        SortOrder = 1,
        SupportsBindings = true,
        EditorUiAlias = "Umb.PropertyEditorUi.TextArea")]
    public string Summary { get; set; } = string.Empty;

    [Field(
        Label = "Ticket type",
        Description = "The ticket type's name (e.g. General question) or ID.",
        SortOrder = 2,
        SupportsBindings = true)]
    public string Type { get; set; } = string.Empty;
}
