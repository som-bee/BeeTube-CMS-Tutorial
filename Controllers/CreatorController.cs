using BeeTube.Models;
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
                var CreatorStatistics = GetCreatorStatistics(userProfile.UserId);
                var creatorProfile = new DashboardViewModel() { 
                    UserProfile = userProfile ,
                    CreatorStatistics = CreatorStatistics
                };
                return View(creatorProfile);

            }

                
        }
        public ActionResult EditProfile()
        {
            string userId = User.Identity.GetUserId();
            using (var dbContext = new BeeTubeEntities())
            {
                var userProfile = dbContext.UserProfiles.Where(u => u.UserId == userId).FirstOrDefault();
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

        [HttpGet]
        public ActionResult Videos()
        {
            string creatorId = User.Identity.GetUserId(); ;
            using (var dbContext = new BeeTubeEntities())
            {
                var userProfile = dbContext.UserProfiles.Where(u => u.UserId == creatorId).FirstOrDefault();
                var CreatorStatistics = GetCreatorStatistics(userProfile.UserId);
                var creatorProfile = new DashboardViewModel()
                {
                    UserProfile = userProfile,
                    CreatorStatistics = CreatorStatistics
                };
                return View(CreatorStatistics.UploadedVideos);

            }
        }




        private CreatorStatistics GetCreatorStatistics(string userId)
        {
            using( var dbContext = new BeeTubeEntities())
            {
                var videos = dbContext.Videos.Where(v=>v.CreatorId == userId).ToList();
                // Retrieve and calculate the creator statistics from the data source
                var totalVideos = videos.Count();
               // var totalViews = 
               // var totalSubscribers = 

                // Create a new instance of CreatorStatistics and populate the properties
                var creatorStatistics = new CreatorStatistics
                {
                    UploadedVideos=videos,
                    TotalVideos = totalVideos,
                    //TotalViews = totalViews,
                    //TotalSubscribers = totalSubscribers,
                    // Set other properties as needed
                };

                return creatorStatistics;
            }


           
        }


    }
}