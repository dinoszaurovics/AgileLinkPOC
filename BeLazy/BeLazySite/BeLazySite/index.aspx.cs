using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BeLazy;


namespace BeLazySite
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            BeLazy.Program.Main(new string[] { "Sync" });
            Response.Redirect("index.aspx");
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            BeLazy.Program.Main(new string[] { "Onboarding" });
            Response.Redirect("index.aspx");
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            BeLazy.Program.Main(new string[] { "ClearLog" });
            Response.Redirect("index.aspx");
        }
    }
}