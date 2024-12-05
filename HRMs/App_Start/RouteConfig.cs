using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HRMs
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Custom routes
            routes.MapRoute(
                name: "Dashboard",
                url: "dashboard",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: "AttendanceReport",
                url: "attendance/report/{date}",
                defaults: new { controller = "Attendance", action = "Report", date = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LeaveApproval",
                url: "leave/approve/{id}",
                defaults: new { controller = "Leave", action = "Approve" }
            );

            routes.MapRoute(
                name: "EmployeeProfile",
                url: "employee/profile/{id}",
                defaults: new { controller = "Employee", action = "Profile" }
            );

            // Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
