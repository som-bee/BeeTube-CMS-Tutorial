using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeeTube.Controllers
{
    [Authorize]
    public class CreatorController : Controller
    {
        // GET: Creator
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            string creatorId = User.Identity.GetUserId(); ;
            using (var dbContext = new BeeTubeEntities())
            {
                var userProfile = dbContext.UserProfiles.Where(u => u.UserId == creatorId).FirstOrDefault();
                return View(userProfile);

            }

                
        }
        public ActionResult EditProfile(int id)
        {
            using (var dbContext = new BeeTubeEntities())
            {
                var userProfile = dbContext.UserProfiles.Where(u => u.Id == id).FirstOrDefault();
                return View(userProfile);

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                // Update the user profile in the database
                // Assuming you have access to the database context or repository

                // Example: Update the profile in the database
                using (var dbContext = new BeeTubeEntities())
                {
                    dbContext.UserProfiles.Attach(userProfile);
                    dbContext.Entry(userProfile).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }

                // Redirect to the profile details page or any other appropriate action
                return RedirectToAction("Dashboard");
            }

            // If the model state is not valid, return the view with validation errors
            return View(userProfile);
        }

    }
}