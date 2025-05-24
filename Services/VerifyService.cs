using Microsoft.EntityFrameworkCore;
using PROJECT_TEMPLATE.Database;
using PROJECT_TEMPLATE.Models;
using Serilog;

namespace PROJECT_TEMPLATE.Services;

public class VerifyService(NxDbContext context)
{
    
    public async Task<bool> VerifyInvitationAsync(Guid invitationId)
    {
        if (invitationId == Guid.Empty)
        {
            Log.Warning("VerifyInvitationAsync received an empty InvitationId.");
            throw new ArgumentException("Invalid invitation ID.");
        }

        try
        {
            var invitation = await context.Invitations.FirstOrDefaultAsync(i => i.Id == invitationId);

            if (invitation == null)
            {
                Log.Warning("Invitation not found for ID: {InvitationId}", invitationId);
                return false;
            }

            if (invitation.InvitationAccepted == true)
            {
                Log.Information("Invitation already accepted for ID: {InvitationId}", invitationId);
                return true;
            }

            invitation.InvitationAccepted = true;
            await context.SaveChangesAsync();

            Log.Information("Invitation successfully verified and accepted for ID: {InvitationId}", invitationId);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error verifying invitation with ID: {InvitationId}", invitationId);
            throw new ApplicationException("An error occurred while verifying the invitation.", ex);
        }
    }
    
    public async Task<InvitationContactInfo?> GetInvitationDetailsAsync(Guid invitationId)
    {
        if (invitationId == Guid.Empty)
        {
            Log.Warning("GetInvitationDetailsAsync received an empty InvitationId.");
            throw new ArgumentException("Invalid invitation ID.");
        }

        try
        {
            var invitation = await context.Invitations
                .Where(i => i.Id == invitationId)
                .Select(i => new InvitationContactInfo
                {
                    Email = i.WorkEmail,
                    FirstName = i.FirstName,
                    LastName = i.LastName
                })
                .FirstOrDefaultAsync();

            if (invitation == null)
            {
                Log.Warning("No invitation found for ID: {InvitationId}", invitationId);
                return null;
            }

            return invitation;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving invitation details for ID: {InvitationId}", invitationId);
            throw new ApplicationException("An error occurred while retrieving invitation details.", ex);
        }
    }
}