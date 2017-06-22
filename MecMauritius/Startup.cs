using MecMauritius.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MecMauritius.Startup))]
namespace MecMauritius
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();
        }

        // In this method we will create default User roles and Admin user for login   
        private static void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            var userStore = new UserStore<ApplicationUser>(context);
            var UserManager = new UserManager<ApplicationUser>(userStore);

            //var adminO365 = UserManager.FindByEmail("admin@mieazure.onmicrosoft.com");
            //var msrimec = UserManager.FindByEmail("msri.mec@outlook.com");
            //var skyman = UserManager.FindByEmail("skymanaditya1@gmail.com");
            //var chirantan31 = UserManager.FindByEmail("chirantan31@gmail.com");
            //UserManager.AddToRole(adminO365.Id, "Admin");
            //UserManager.AddToRole(msrimec.Id, "Admin");
            //UserManager.AddToRole(chirantan31.Id, "Admin");

            // var adityapes = UserManager.FindByEmail("adityaagarwal@pesit.pes.edu");
            // UserManager.AddToRole(adityapes.Id, "Admin");


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {
                // first we create Admin rool   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                //var user = new ApplicationUser();
                //user.UserName = "admin@mecmauritius.com";
                //user.Email = "admin@mecmauritius.com";

                //string userPWD = "Admin123*";

                //var chkUser = UserManager.Create(user, userPWD);

                ////Add default User to Role Admin   
                //if (chkUser.Succeeded)
                //{
                //    var result1 = UserManager.AddToRole(user.Id, "Admin");

                //}
            }

            // creating Creating Manager role    
            if (!roleManager.RoleExists("Student"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Student";
                roleManager.Create(role);
            }

            // creating Creating Employee role    
            if (!roleManager.RoleExists("Educator"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Educator";
                roleManager.Create(role);
            }
        }
    }
}
