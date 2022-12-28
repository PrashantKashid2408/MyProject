using System.Web;
using System.Web.Optimization;

namespace AdaniCall
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-2.1.3.js",
                "~/Scripts/jquery-ui-1.11.4.min.js",
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/modernizr-2.6.2.js"                
            ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/plugins/bootstrap/js/bootstrap.bundle.min.js",
                "~/Scripts/plugins/sweetalert2/sweetalert2.min.js",
                //"~/Scripts/plugins/bs4/popper.min.js",
                //"~/Scripts/plugins/bs4/bootstrap.min.js",
                "~/Scripts/plugins/bs4/bootstrap-dialog.js", //Custom JS BS3 to BS4
                "~/Scripts/plugins/bs4/bootstrap-dialog-custom-bs4.js", //Custom JS BS3 to BS4
                //"~/Scripts/plugins/bootstrap-dialog/bootstrap-dialog.min.js", //Custom JS BS3 to BS4
                //"~/Scripts/plugins/bootstrap-dialog/bootstrap-dialog-custom.js" //Custom JS BS3 to BS4
                "~/Scripts/plugins/bootstrap-datepicker/bootstrap-datepicker.js",
                "~/Scripts/plugins/overlayScrollbars/js/jquery.overlayScrollbars.min.js",
                //"~/Scripts/dist/js/adminlte.js",
                "~/Scripts/dist/js/demo.js",
                "~/Scripts/respond.js"
            ));

            /* bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*")); */

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            /* bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*")); */

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/plugins/fontawesome-free/css/all.min.css",
                //"~/Content/plugins/font-awesome/font-awesome.css",
                "~/Content/plugins/sweetalert2-theme-bootstrap-4/bootstrap-4.min.css",
                "~/Content/plugins/tempusdominus-bootstrap-4/css/tempusdominus-bootstrap-4.min.css",
                "~/Content/plugins/icheck-bootstrap/icheck-bootstrap.min.css",
                "~/Content/dist/css/adminlte.min.css",
                "~/Content/plugins/overlayScrollbars/css/OverlayScrollbars.min.css",
                //"~/Content/plugins/daterangepicker/daterangepicker.css",
                "~/Content/plugins/bootstrap-datepicker/bootstrap-datepicker.min.css",
                "~/Content/plugins/bootstrap-dialog/bootstrap-dialog.min.css",
                //"~/Content/AdminLTE/AdminLTE.css",
                "~/Content/Site.css"
            //"~/Content/plugins/bs4/bootstrap.min.css" //Custom CSS BS3 to BS4 
            ));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = false;
        }
    }
}
