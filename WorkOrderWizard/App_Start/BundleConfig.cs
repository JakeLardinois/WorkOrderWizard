using System.Web;
using System.Web.Optimization;

namespace WorkOrderWizard
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true; // when configuring the bundles, if there are minified or CDN versions found then the files don't show up on the page; This forces them to display...
            bundles.UseCdn = true; //Enables CDN Support.

            //bundles.Add(new ScriptBundle("~/bundles/jquery", "http://ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.min.js") //used in release mode
            //    .Include("~/Scripts/jquery-{version}.js")); //used in debug mode unless no files are found...
            bundles.Add(new ScriptBundle("~/bundles/jquery", "http://ajax.googleapis.com/ajax/libs/jquery/2.1.3/jquery.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui", "http://ajax.googleapis.com/ajax/libs/jqueryui/1.11.3/jquery-ui.js")); //Note that you must put your version of JqueryUI in jquery.themeswitcher.js or jquery.themeswitcher.min.js...

            //http://jqueryvalidation.org/
            bundles.Add(new ScriptBundle("~/bundles/jqueryvalidate", "http://ajax.aspnetcdn.com/ajax/jquery.validate/1.13.1/jquery.validate.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryvalidateAdditionalMethods", "http://ajax.aspnetcdn.com/ajax/jquery.validate/1.13.1/additional-methods.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/themeswitcher").Include(
                        "~/Scripts/jquery.themeswitcher.js"));

            bundles.Add(new ScriptBundle("~/bundles/impromptu").Include(
                        "~/Scripts/jquery-impromptu.js"));

            bundles.Add(new ScriptBundle("~/bundles/multiselect").Include(
                        "~/Scripts/jquery.multiselect.js",
                        "~/Scripts/jquery.multiselect.filter.js"));

            bundles.Add(new ScriptBundle("~/bundles/autogrow").Include(
                        "~/Scripts/autogrow.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/Scripts/DataTables-1.10.2/media/js/jquery.dataTables.min.js",
                        "~/Scripts/DataTables-1.10.2/extensions/TableTools/js/dataTables.tableTools.min.js",
                        "~/Scripts/DataTables-1.10.2/extensions/ColReorder/js/dataTables.colReorder.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/datetimeformatter").Include(
                        "~/Scripts/DateTimeFormatter.js"));

            bundles.Add(new ScriptBundle("~/bundles/index").Include(
                        "~/Scripts/index.js"));
            bundles.Add(new ScriptBundle("~/bundles/createworkorders").Include(
                        "~/Scripts/createworkorders.js"));
            bundles.Add(new ScriptBundle("~/bundles/viewworkorders").Include(
                        "~/Scripts/viewworkorders.js"));

            bundles.Add(new ScriptBundle("~/bundles/jeditable").Include(
                        "~/Scripts/JEditable-1.7.3/jquery.jeditable.js"));



            bundles.Add(new StyleBundle("~/Content/impromptu").Include(
                "~/Content/jquery-impromptu.css"));

            bundles.Add(new StyleBundle("~/Content/multiselect").Include(
                "~/Content/jquery.multiselect.css",
                "~/Content/jquery.multiselect.filter.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/reset.css",
                "~/Content/html5-reset.css",
                "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/datatables").Include(
                "~/Scripts/DataTables-1.10.2/media/css/jquery.dataTables_themeroller.css",
                "~/Scripts/DataTables-1.10.2/extensions/TableTools/css/dataTables.tableTools.css",
                "~/Scripts/DataTables-1.10.2/extensions/ColReorder/css/dataTables.colReorder.css"));

        }
    }
}