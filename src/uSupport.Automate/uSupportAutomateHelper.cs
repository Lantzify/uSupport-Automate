using uSupport.Dtos.Tables;
using Umbraco.Cms.Core.Services;
using uSupport.Services.Interfaces;
using Umbraco.Automate.Core.Actions;
using Umbraco.Cms.Core.Models.Membership;

namespace uSupport.Automate;

/// <summary>
/// Lookups and mapping shared by the uSupport triggers and actions.
/// Public only because their public constructors take it; everything on it is internal.
/// </summary>
public sealed class uSupportAutomateHelper
{
    private readonly IUserService _userService;
    private readonly IuSupportTicketService _ticketService;
    private readonly IuSupportTicketTypeService _typeService;
    private readonly IuSupportTicketStatusService _statusService;

    public uSupportAutomateHelper(
        IUserService userService,
        IuSupportTicketService ticketService,
        IuSupportTicketTypeService typeService,
        IuSupportTicketStatusService statusService)
    {
        _userService = userService;
        _ticketService = ticketService;
        _typeService = typeService;
        _statusService = statusService;
    }

    internal T MapTicket<T>(uSupportTicket ticket) where T : TicketOutput, new()
    {
        // Type and Status aren't reliably filled in on the ticket, so look them up by ID.
        var status = ticket.Status ?? _statusService.Get(ticket.StatusId);
        var author = _userService.GetUserById(ticket.AuthorId);

        return new T
        {
            TicketId = ticket.Id,
            ExternalTicketId = ticket.ExternalTicketId,
            Title = ticket.Title,
            Summary = ticket.Summary,
            TypeId = ticket.TypeId,
            TypeName = (ticket.Type ?? _typeService.Get(ticket.TypeId))?.Name,
            StatusId = ticket.StatusId,
            StatusName = status?.Name,
            IsResolved = status is { Active: false },
            AuthorName = author?.Name,
            AuthorEmail = author?.Email,
            Submitted = ticket.Submitted,
            Resolved = ticket.Resolved,
            LastUpdatedBy = ticket.LastUpdatedBy,
            LastUpdated = ticket.LastUpdated,
        };
    }

    internal TicketCommentOutput MapComment(uSupportTicket ticket, uSupportTicketComment comment)
    {
        var output = MapTicket<TicketCommentOutput>(ticket);
        var commentAuthor = _userService.GetUserById(comment.UserId);
        output.CommentId = comment.Id;
        output.Comment = comment.Comment;
        output.CommentAuthorName = commentAuthor?.Name;
        output.CommentAuthorEmail = commentAuthor?.Email;
        return output;
    }

    internal TicketStatusChangedOutput MapStatusChange(uSupportTicket oldTicket, uSupportTicket newTicket)
    {
        var output = MapTicket<TicketStatusChangedOutput>(newTicket);
        output.PreviousStatusId = oldTicket.StatusId;
        output.PreviousStatusName = (oldTicket.Status ?? _statusService.Get(oldTicket.StatusId))?.Name;
        return output;
    }

    internal uSupportTicketStatus? GetStatus(Guid id) => _statusService.Get(id);

    internal uSupportTicket? GetTicket(string? id)
        => Guid.TryParse(id, out var key) ? _ticketService.Get(key) : null;

    internal async Task<IUser?> GetServiceAccountAsync(ActionContext context)
        => context.ExecutionContext?.ServiceAccountKey is { } key ? await _userService.GetAsync(key) : null;

    internal static ActionResult Invalid(string message)
        => ActionResult.Failed(new ArgumentException(message), StepRunErrorCategory.Validation);

    internal static ActionResult NoServiceAccount()
        => ActionResult.Failed(
            new InvalidOperationException("No service account to act as. Run the automation from a workspace that has one."),
            StepRunErrorCategory.ConfigurationError);
}
