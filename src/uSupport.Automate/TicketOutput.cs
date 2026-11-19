using Json.Schema.Generation;

namespace uSupport.Automate;

/// <summary>
/// A uSupport ticket as later automation steps see it.
/// </summary>
public class TicketOutput
{
    [Description("The ticket's unique ID.")]
    public Guid TicketId { get; set; }

    [Description("The human-friendly ticket ID, e.g. ABC-DEFG-123.")]
    public string? ExternalTicketId { get; set; }

    [Description("The ticket title.")]
    public string? Title { get; set; }

    [Description("The ticket summary (HTML).")]
    public string? Summary { get; set; }

    [Description("The ticket type's ID.")]
    public Guid TypeId { get; set; }

    [Description("The ticket type's name.")]
    public string? TypeName { get; set; }

    [Description("The ticket status's ID.")]
    public Guid StatusId { get; set; }

    [Description("The ticket status's name.")]
    public string? StatusName { get; set; }

    [Description("Whether the ticket's status is a resolved (inactive) status.")]
    public bool IsResolved { get; set; }

    [Description("The name of the user who submitted the ticket.")]
    public string? AuthorName { get; set; }

    [Description("The email address of the user who submitted the ticket.")]
    public string? AuthorEmail { get; set; }

    [Description("When the ticket was submitted.")]
    public DateTime Submitted { get; set; }

    [Description("When the ticket was resolved. Empty while it's open.")]
    public DateTime? Resolved { get; set; }

    [Description("The name of the user who last updated the ticket.")]
    public string? LastUpdatedBy { get; set; }

    [Description("When the ticket was last updated.")]
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// A ticket plus the comment that was just added to it.
/// </summary>
public sealed class TicketCommentOutput : TicketOutput
{
    [Description("The comment's unique ID.")]
    public Guid CommentId { get; set; }

    [Description("The comment text (HTML).")]
    public string? Comment { get; set; }

    [Description("The name of the user who wrote the comment.")]
    public string? CommentAuthorName { get; set; }

    [Description("The email address of the user who wrote the comment.")]
    public string? CommentAuthorEmail { get; set; }
}

/// <summary>
/// A ticket plus the status it had before the change.
/// </summary>
public sealed class TicketStatusChangedOutput : TicketOutput
{
    [Description("The ID of the status the ticket had before.")]
    public Guid PreviousStatusId { get; set; }

    [Description("The name of the status the ticket had before.")]
    public string? PreviousStatusName { get; set; }
}
