using BeeTube.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

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

        [Authorize]
        [HttpGet]
        //[Route("video/upload")]
        public ActionResult UploadVideo()
        {
            using (var context = new BeeTubeEntities())
            {
                // Retrieve the list of categories from the database
                var categories = context.Categories.ToList();

                // Pass the list of categories to the view
                ViewBag.Categories = new SelectList(categories, "Id", "Name");
            }

            return View();
        }

        [Authorize]
        [HttpPost]
        //[Route("video/upload")]
        public ActionResult UploadVideo(VideoUploadModel model)
        {
            if (ModelState.IsValid)
            {
                var videoModel = new BeeTube.Video();
                TimeSpan videoDuration = TimeSpan.FromSeconds(0);

                //store the uploaded video file
                // Check if a file was uploaded
                if (model.VideoFile != null && model.VideoFile.ContentLength > 0)
                {
                    // Generate a unique filename for the uploaded file
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.VideoFile.FileName);

                    // Specify the path where the file will be stored
                    var filePath = Path.Combine(Server.MapPath("~/Uploads/video-files"), fileName);

                    // Save the file to the specified path
                    model.VideoFile.SaveAs(filePath);

                    // Return the file path
                    videoModel.FilePath = "Uploads/video-files/" + fileName;

                    ////getting video duration
                    //string videoFilePath = "~/"+videoModel.FilePath;

                    //using (var videoFile = new VideoFileReader())
                    //{
                    //    videoFile.Open(videoFilePath);

                    //    // Get the duration in seconds
                    //    double durationInSeconds = (double)(videoFile.FrameCount / videoFile.FrameRate);

                    //    // Convert the duration to the desired format (e.g., TimeSpan)
                    //    TimeSpan duration = TimeSpan.FromSeconds(durationInSeconds);

                    //    // Use the duration as needed
                    //    videoDuration = duration;
                    //}





                }
                //store the uploaded video thumbnail file
                if (model.ThumbnailFile != null && model.ThumbnailFile.ContentLength > 0)
                {
                    // Generate a unique filename for the uploaded file
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ThumbnailFile.FileName);

                    // Specify the path where the file will be stored
                    var filePath = Path.Combine(Server.MapPath("~/Uploads/video-thumbnails"), fileName);

                    // Save the file to the specified path
                    model.ThumbnailFile.SaveAs(filePath);

                    // Return the file path
                    videoModel.ThumbnailUrl = "Uploads/video-thumbnails/" + fileName;
                }

                //assigning uploaded filr powerty to video model
                videoModel.Title = model.Title;
                videoModel.Description = model.Description;
                videoModel.CategoryID = model.CategoryID;
                videoModel.UploadDate = DateTime.Now;
                videoModel.CreatorId = User.Identity.GetUserId();
                videoModel.Duration = model.Duration;

                using (var context = new BeeTubeEntities())
                {
                    context.Videos.Add(videoModel);
                    context.SaveChanges();
                }
                return RedirectToAction("Videos", "Creator");
                //return View("UploadSuccess");
            }
            return View();
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
        [HttpGet]
        public ActionResult VideoDetails(int id)
        {

            int videoId = id;
            using (var context = new BeeTubeEntities())
            {

                // var video = context.Videos.Find(videoId);
                //var comments = context.Comments.Where(c => c.VideoID == id).ToList();

                var video = GetVideoByVideoId(videoId);

                if(video.User.Id == User.Identity.GetUserId())
                {
                    var comments = GetCommentsByVideoId(videoId);

                    // Create a view model to hold the video and comments
                    var viewModel = new VideoViewModel
                    {
                        Video = video,
                        Comments = comments,
                    };

                    return View(viewModel);
                }
                ViewBag.ErrorMsg = "Unauthozied action";
                return View("Error");

            }
        }

        [HttpGet]
        public ActionResult EditVideo(int id)
        {
            int videoId = id;
            using (var context = new BeeTubeEntities())
            {
                var video = GetVideoByVideoId(videoId);

                if (video.User.Id == User.Identity.GetUserId())
                {
                    // Retrieve the list of categories from the database
                    var categories = context.Categories.ToList();

                    // Pass the list of categories to the view
                    ViewBag.Categories = new SelectList(categories, "Id", "Name");
                    ViewBag.ThumbnailUrl = video.ThumbnailUrl;

                    VideoUploadModel videoModel = new VideoUploadModel()
                    {
                        Id= video.Id,
                        Title = video.Title,
                        Description = video.Description,
                        CategoryID = video.CategoryID,
   
                    };



                    return View(videoModel);
                }
                ViewBag.ErrorMsg = "Unauthozied action";
                return View("Error");

            }
        }

        [HttpGet]
        public ActionResult DeleteVideo(int id)
        {
            //int videoId = id;
            //using (var dbContext = new BeeTubeEntities())
            //{
            //    var video = dbContext.Videos.FirstOrDefault(v => v.Id == id);

            //    if (video != null && video.User.Id == User.Identity.GetUserId())
            //    {

            //        return PartialView("_DeleteConfirmation", video);


            //    }
            //    ViewBag.ErrorMsg = "Unauthozied action";
            //    return View("Error");

            //}
            using (var dbContext = new BeeTubeEntities())
            {
                var video = dbContext.Videos.FirstOrDefault(v => v.Id == id);
                if (video != null && video.User.Id == User.Identity.GetUserId())
                {


                    // Delete the video and associated files
                    // ...
                    //removing video and thunail files
                    // Remove the video file
                    var videoFilePath = Server.MapPath("~/" + video.FilePath);
                    if (System.IO.File.Exists(videoFilePath))
                    {
                        System.IO.File.Delete(videoFilePath);
                    }

                    // Remove the thumbnail file
                    var thumbnailFilePath = Server.MapPath("~/" + video.ThumbnailUrl);
                    if (System.IO.File.Exists(thumbnailFilePath))
                    {
                        System.IO.File.Delete(thumbnailFilePath);
                    }

                    // Remove the video
                    dbContext.Videos.Remove(video);

                    // Remove the associated comments
                    var comments = dbContext.Comments.Where(c => c.VideoID == id);
                    dbContext.Comments.RemoveRange(comments);

                    // Save the changes
                    dbContext.SaveChanges();

                    return RedirectToAction("Videos");


                }
                ViewBag.ErrorMsg = "Unauthozied action";
                return View("Error");


            }
        }
       

        [HttpPost]
        public ActionResult EditVideo(VideoUploadModel model)
        {
            int videoId = model.Id;
            using (var context = new BeeTubeEntities())
            {
                var video = context.Videos.Where(v => v.Id == videoId).FirstOrDefault();
                if (video.User.Id == User.Identity.GetUserId())
                {
                   video.Title= model.Title;
                    video.Description= model.Description;
                    video.CategoryID = model.CategoryID;

                    //store the uploaded video thumbnail file
                    if (model.ThumbnailFile != null && model.ThumbnailFile.ContentLength > 0)
                    {
                        // Generate a unique filename for the uploaded file
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ThumbnailFile.FileName);

                        // Specify the path where the file will be stored
                        var filePath = Path.Combine(Server.MapPath("~/Uploads/video-thumbnails"), fileName);

                        // Save the file to the specified path
                        model.ThumbnailFile.SaveAs(filePath);

                        // Return the file path
                        video.ThumbnailUrl = "Uploads/video-thumbnails/" + fileName;
                    }

                    context.SaveChanges();



                    return RedirectToAction("VideoDetails", new {id=video.Id});
                }
                ViewBag.ErrorMsg = "Unauthozied action";
                return View("Error");

            }
        }

       

        [HttpGet]
        [Route("DeleteComment/{videoId}/{id}")]
        public ActionResult DeleteComment(int id, int videoId)
        {
            using( var dbContext = new BeeTubeEntities())
            {
                // Retrieve the comment from the database based on the provided id
                var comment = dbContext.Comments.FirstOrDefault(c => c.Id == id);

                if (comment != null)
                {
                    // Remove the comment from the database
                    dbContext.Comments.Remove(comment);
                    dbContext.SaveChanges();

                    // Optionally, perform any additional logic or processing

                    // Redirect to the appropriate action or view
                    return RedirectToAction("VideoDetails",new {id = videoId }); // Replace "Index" with the desired action name
                }
                else
                {
                    // Comment not found, handle the error or return a specific view
                    return HttpNotFound();
                }
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

        public Video GetVideoByVideoId(int videoId)
        {
            using (var context = new BeeTubeEntities())
            {
                var video = context.Videos.Where(v => v.Id == videoId).FirstOrDefault();

                var user = context.Users.Where(u => u.Id == video.CreatorId).FirstOrDefault();

                video.User = user;
                return video;

            }
        }
        public List<Comment> GetCommentsByVideoId(int videoId)
        {
            using (var context = new BeeTubeEntities())
            {
                var comments = context.Comments.Where(c => c.VideoID == videoId).ToList();
                var userIds = comments.Select(c => c.UserID).Distinct().ToList();
                var users = context.Users.Where(u => userIds.Contains(u.Id)).ToList();

                var joinedComments = from c in comments
                                     join u in users on c.UserID equals u.Id
                                     select new Comment
                                     {
                                         Id = c.Id,
                                         VideoID = c.VideoID,
                                         UserID = c.UserID,
                                         Content = c.Content,
                                         CreatedAt = c.CreatedAt,
                                         User = u
                                     };
                return joinedComments.ToList();

            }



        }


    }
}