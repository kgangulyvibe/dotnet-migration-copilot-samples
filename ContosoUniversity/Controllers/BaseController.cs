using System;
using System.Security.Claims;
using System.Web.Mvc;
using ContosoUniversity.Services;
using ContosoUniversity.Models;
using ContosoUniversity.Data;

namespace ContosoUniversity.Controllers
{
    public abstract class BaseController : Controller
    {
        protected SchoolContext db;
        protected NotificationService notificationService = new NotificationService();

        public BaseController()
        {
            db = SchoolContextFactory.Create();
        }

        protected void SendEntityNotification(string entityType, string entityId, EntityOperation operation)
        {
            SendEntityNotification(entityType, entityId, null, operation);
        }

        protected void SendEntityNotification(string entityType, string entityId, string entityDisplayName, EntityOperation operation)
        {
            try
            {
                // Use authenticated user's name from Microsoft Entra ID claims
                var userName = User?.Identity?.IsAuthenticated == true
                    ? User.Identity.Name ?? GetClaimValue("preferred_username") ?? "Unknown"
                    : "Anonymous";
                notificationService.SendNotification(entityType, entityId, entityDisplayName, operation, userName);
            }
            catch (Exception ex)
            {
                // Log the error but don't break the main operation
                System.Diagnostics.Debug.WriteLine($"Failed to send notification: {ex.Message}");
            }
        }

        private string GetClaimValue(string claimType)
        {
            var identity = User?.Identity as ClaimsIdentity;
            return identity?.FindFirst(claimType)?.Value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
                notificationService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
