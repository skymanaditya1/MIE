using System.Web;
using System.Web.Optimization;

namespace MecMauritius
{
    public static class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/jqueryval")
    .Include("~/Scripts/jquery.validate.js")
    .Include("~/Scripts/jquery.validate.unobtrusive.js"));



        }
    }
}
