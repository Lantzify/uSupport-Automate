<p align="center">
    <img src="assets/usupport.png" width="250">
</p>

![version](https://img.shields.io/nuget/v/uSupport.Automate?label=version)
[![Nuget](https://img.shields.io/nuget/dt/uSupport.Automate?color=2346c018&logo=Nuget)](https://www.nuget.org/packages/uSupport.Automate/)
[![Umbraco](https://img.shields.io/badge/marketplace-umbraco-%233544b1#f5c1bc)](https://marketplace.umbraco.com/package/usupport.automate)

[![Release](https://github.com/Lantzify/uSupport-automate/actions/workflows/main.yml/badge.svg)](https://github.com/Lantzify/uSupport-automate/actions/workflows/main.yml)

## What is uSupport.Automate
uSupport.Automate connects [uSupport](https://github.com/Lantzify/uSupport) to [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate). It adds uSupport triggers and actions to Automate, so you can build automations around your tickets right in the backoffice, without writing code.

## Why uSupport.Automate
Stop doing the same ticket chores by hand. For example:
* Reply to every new ticket with a "Thanks, we're on it" comment.
* Post to Slack when a ticket is resolved.
* Open a ticket when a monitoring tool calls a webhook.
* Email the author for feedback once their ticket is resolved.

### Key features
* 6 triggers: ticket created, updated, status changed, resolved, comment added and deleted
* 4 actions: create ticket, add comment, change status and get ticket
* Ticket fields (title, status, author email and more) available to every later step
* Changes made by automations show up in the ticket history
* Loop protection, so an automation doesn't keep triggering itself

## Requirements
* Umbraco 17.6 or later
* Umbraco Automate 17.6 or later
* uSupport 17 or later

## Getting started
1. Install Umbraco Automate and uSupport.Automate. uSupport comes along if you don't have it yet.
   ```
   dotnet add package Umbraco.Automate
   dotnet add package uSupport.Automate
   ```
2. Give the workspace's service account access to uSupport. In **Users**, open the user group the service account belongs to and add the **uSupport** section. Every uSupport trigger and action needs it, so without it they won't show up.
3. Create an automation. The triggers and actions are in the **uSupport** group.

> [!TIP]
> If Automate shows database errors like `no such table: umbracoAutomateWorkspace`, it's missing its connection string. To let it share the Umbraco database, add this to `appsettings.json` and restart:
> ```json
> "Umbraco": {
>   "Automate": {
>     "UseNamedConnectionString": "umbracoDbDSN"
>   }
> }
> ```

## Triggers
| Trigger | Fires when |
|---|---|
| Ticket Created | A ticket is created. |
| Ticket Updated | A ticket is saved, including when a comment is added to it. |
| Ticket Status Changed | A ticket moves to another status. |
| Ticket Resolved | An open ticket moves to a resolved status. |
| Ticket Comment Added | A comment is added to a ticket. |
| Ticket Deleted | A ticket is deleted. |

Every trigger passes the ticket on to later steps: its ID and ticket number (e.g. `ABC-DEFG-123`), title, summary, type, status, whether it's resolved, the author's name and email, and its dates. Ticket Status Changed and Ticket Resolved also pass the previous status. Ticket Comment Added also passes the comment and who wrote it.

## Actions
| Action | What it does |
|---|---|
| Create Ticket | Creates a ticket from a title, summary and ticket type. It gets the default status. |
| Add Ticket Comment | Adds a comment to a ticket. |
| Change Ticket Status | Moves a ticket to another status, e.g. Resolved. |
| Get Ticket | Loads a ticket so later steps can use its fields. |

Good to know:
* The workspace's service account is the author of the tickets and comments an automation creates, so the ticket history shows they came from an automation.
* Give the ticket type or status by name (e.g. `General question`) or by ID.
* The actions don't send uSupport's emails. Add Automate's Send Email step if you want one.
* HTML in summaries and comments is sanitized, the same as in the uSupport section.
* By default, a trigger skips changes that would loop back into the same automation. You can change this under **Advanced** in the trigger's settings.

## Useful links
- [uSupport](https://github.com/Lantzify/uSupport)
- [uSupport documentation](https://lantzify.github.io/uSupport-documentation/)
- [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate)

## Issues

If you find any issues with the package, feel free to [raise an issue](https://github.com/Lantzify/uSupport-automate/issues)!

## Screenshots
![uSupport triggers](assets/triggers.png "uSupport triggers")
![uSupport actions](assets/actions.png "uSupport actions")
![An automation that replies to new tickets](assets/automation.png "An automation that replies to new tickets")
![A run and its step outputs](assets/run.png "A run and its step outputs")
![The automation's comment in the ticket history](assets/ticket-history.png "The automation's comment in the ticket history")
