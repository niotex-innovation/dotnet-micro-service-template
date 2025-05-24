using Microsoft.EntityFrameworkCore;
using NxSuite.Interfaces;
using PROJECT_TEMPLATE.Database;
using PROJECT_TEMPLATE.Models;
using System.Web;
using Serilog;

namespace PROJECT_TEMPLATE.Services;

public class TryForFreeService(NxDbContext context, IMqService mqService, ISystemSettings settings, IServiceScopeFactory scopeFactory)
{
    public async Task AddAsync(TryForFreeModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FirstName))
        {
            Log.Warning("AddAsync was called with invalid data: {@Request}", request);
            throw new ArgumentException("Invalid request data.");
        }

        try
        {
            var invitationId = Guid.NewGuid();

            var invitation = new Invitation
            {
                Id = invitationId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                WorkEmail = request.Email,
                FirmSize = request.FirmSize,
                ReferralCode = request.ReferralCode,
                PromotionalCode = request.PromotionalCode,
                CreatedAt = DateTime.UtcNow
            };

            await context.Invitations.AddAsync(invitation);
            await context.SaveChangesAsync();

            Log.Information("Invitation created for {Email} with ID {InvitationId}", request.Email, invitationId);

            await SendWelcomeEmailAsync(request.Email, request.FirstName, request.LastName, invitationId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while processing AddAsync for {Email}", request.Email);
            throw new ApplicationException("An error occurred while processing the request.", ex);
        }
    }

    public async Task<object> GetAsync()
    {
        try
        {
            var results = await context.Invitations
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.PhoneNumber,
                    x.WorkEmail,
                    x.FirmSize,
                    x.ReferralCode,
                    x.PromotionalCode,
                    x.CreatedAt
                })
                .Take(10)
                .ToListAsync();

            Log.Information("Retrieved latest {Count} invitations", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while retrieving invitations");
            throw;
        }
    }

    private async Task SendWelcomeEmailAsync(string email, string firstName, string lastName, Guid invitationId)
    {
        try
        {
            var templateId = settings.GetSetting<string>("Email:TemplateIdWelcome");
            var bccEmail = settings.GetSetting<string>("Email:BccDefault") ?? "admin@nxsuite.com";

            var tokenEncoded = HttpUtility.UrlEncode(invitationId.ToString("D"));
            var link = $"https://www.nxsuite.com/verify?token={tokenEncoded}";

            var emailPayload = new
            {
                TemplateId = templateId,
                Subject = "Welcome to NxSuite!",
                SenderName = "NxSuite",
                FromEmail = "support@nxsuite.com",
                To = new[] {email},
                Bcc = new[] {bccEmail},
                Tokens = new Dictionary<string, string>
                {
                    ["firstName"] = firstName,
                    ["lastName"] = lastName,
                    ["link"] = link
                }
            };

            await mqService.PublishMessageAsync("email.send", emailPayload, async r =>
            {
                using var scope = scopeFactory.CreateScope();
                var scopedDbContext = scope.ServiceProvider.GetRequiredService<NxDbContext>();
                var e = await scopedDbContext.Invitations.SingleOrDefaultAsync(x => x.Id == invitationId);
                if (e == null) return;
                e.OutboundEmailId = r.id;
                await scopedDbContext.SaveChangesAsync();
            });

            Log.Information("Welcome email queued for {Email}", email);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send welcome email to {Email}", email);
            // Optionally don't throw here to avoid breaking the flow for the user
        }
    }
}