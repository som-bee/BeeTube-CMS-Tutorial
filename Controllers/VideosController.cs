using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using BeeTube.Models;
using BeeTube;
using Microsoft.AspNet.Identity;

namespace BeeTube.Controllers
{
    public class VideosController : Controller
    {
        // GET: Videos
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("video/all")]
        public ActionResult Videos()
        {
            using (var context = new BeeTubeEntities())
            {
                var videos = context.Videos.ToList();
                return View(videos);
            }
        }


        [HttpGet]
        [Route("video")]
        public ActionResult Video(int id)
        {
            int videoId = id;
            using (var context = new BeeTubeEntities())
            {
               // var video = context.Videos.Find(videoId);
                //var comments = context.Comments.Where(c => c.VideoID == id).ToList();

                var video= GetVideoByVideoId(videoId);
                var comments = GetCommentsByVideoId(videoId);

                // Create a view model to hold the video and comments
                var viewModel = new VideoViewModel
                {
                    Video = video,
                    Comments = comments,
                    Comment = new Comment() { VideoID = videoId}
                };

                return View(viewModel);

            }
        }
        [Authorize]
        [HttpGet]
        [Route("video/upload")]
        public ActionResult UploadVideo()
        {
            using(var context = new BeeTubeEntities())
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
        [Route("video/upload")]
        public ActionResult UploadVideo(VideoUploadModel model)
        {
            if(ModelState.IsValid)
            {
                var videoModel = new BeeTube.Video();


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

                using (var context = new BeeTubeEntities())
                {
                    context.Videos.Add(videoModel);
                    context.SaveChanges();
                }

                return View("UploadSuccess");
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        [Route("video")]
        public ActionResult AddComment(VideoViewModel model)
        {
            // Validate and process the comment submission
            if (ModelState.IsValid && User.Identity.IsAuthenticated)
            {

                using(var dbContext =  new BeeTubeEntities())
                {
                    // Add the comment to the database
                    var comment = new Comment
                    {
                        VideoID = model.Comment.VideoID,
                        UserID = User.Identity.GetUserId(),
                        Content = model.Comment.Content,
                        CreatedAt = DateTime.Now
                    };
                    
                    dbContext.Comments.Add(comment);
                    dbContext.SaveChanges();

                    // Construct the JSON response
                    var response = new
                    {
                        success = true,
                        comment = new
                        {
                            content = comment.Content,
                            author = User.Identity.GetUserName(),
                            date = comment.CreatedAt.ToString("dd MMM, yyyy")
                        }
                    };

                    // Return the JSON response
                    return Json(response);
                }
            
            }
            else
            {
                // Return validation errors if the comment submission is invalid
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var response = new { success = false, errors = errors };
                return Json(response);
            }

        }


        public Video GetVideoByVideoId(int videoId)
        {
            using (var context = new BeeTubeEntities())
            {
                var video = context.Videos.Where(v => v.Id == videoId).FirstOrDefault();
            
                var user = context.Users.Where(u => u.Id == video.CreatorId).FirstOrDefault()  ;

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