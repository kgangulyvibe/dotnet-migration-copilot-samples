using System.Web.Mvc;

namespace ContosoUniversity
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Require authentication for all controllers via Microsoft Entra ID
            filters.Add(new AuthorizeAttribute());
        }
    }
}
