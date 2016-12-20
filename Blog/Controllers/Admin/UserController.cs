using Blog.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers.Admin
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {

        // GET: User
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //
        //Get: User/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var users = database.Users.ToList();

                var admins = GetAdminUsersName(users, database);
                ViewBag.Admins = admins;

                return View(users);

            }
        }

        private HashSet<string> GetAdminUsersName(List<ApplicationUser> users, BlogDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var admins = new HashSet<string>();
            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.UserName);
                }
            }

            return admins;
        }

        //GET:User/Edit
        public ActionResult Edit(string id)
        {
            //
            //Validate Id
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get user from database
                var user = database.Users.Where(u => u.Id == id).First();

                //Check if user exist
                if (user == null)
                {
                    return HttpNotFound();
                }

                //Create a view model
                var viewModel = new EditUserViewModel();
                viewModel.User = user;
                viewModel.Roles = GetUserRoles(user, database);
                return View(viewModel);



            }
        }

        private IList<Role> GetUserRoles(ApplicationUser user, BlogDbContext db)
        {
            //Get user manager
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            //Get all application roles
            var roles = db.Roles.Select(r => r.Name).OrderBy(r => r).ToList();

            //For each application role, check if the user has it
            var userRoles = new List<Role>();

            foreach (var roleName in roles)
            {
                var role = new Role { Name = roleName };
                if (userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }
                userRoles.Add(role);
            }

            return userRoles;
        }


        //POST: User/Edit

        [HttpPost]
        public ActionResult Edit(string id, EditUserViewModel viewModels)
        {
            //Check if model is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //Get user from the database
                    var user = database.Users.FirstOrDefault(u => u.Id == id);

                    //Check if user exist
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    //If password field is not empty,change password
                    if (!string.IsNullOrEmpty(viewModels.Password))
                    {
                        var hasher = new PasswordHasher();
                        var passwordHash = hasher.HashPassword(viewModels.Password);
                        user.PasswordHash = passwordHash;
                    }

                    //Set user providers
                    user.Email = viewModels.User.Email;
                    user.FullName = viewModels.User.FullName;
                    user.UserName = viewModels.User.Email;
                    this.SetUserRoles(viewModels, user, database);

                    //Save changes
                    database.Entry(user).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("List");
                }

            }
            return View(viewModels);
        }

        private void SetUserRoles(EditUserViewModel viewModels, ApplicationUser user, BlogDbContext context)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            foreach (var role in viewModels.Roles)
            {
                if (role.IsSelected && !userManager.IsInRole(user.Id, role.Name))
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected && userManager.IsInRole(user.Id, role.Name))
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }


        //
        //GET: User/Delete

        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get user from database
                var user = database.Users.Where(u => u.Id.Equals(id)).First();

                //Check if user exist
                if (user == null)
                {
                    return HttpNotFound();
                }

                return View(user);
            }
        }

        //
        //POST: User/DELETE

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed (string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                //Get user from database
                var user = database.Users.Where(u => u.Id.Equals(id)).First();

                //Check user articles from the database
                var userArticles = database.Articles.Where(a => a.Author.Id == user.Id);

                //Delete articles
                foreach (var article in userArticles)
                {
                    database.Articles.Remove(article);
                }

                //Delete user and save changes
                database.Users.Remove(user);
                database.SaveChanges();
              

                return RedirectToAction("List");
            }
        }
    }
}