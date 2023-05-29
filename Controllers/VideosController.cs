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
                ViewBag.VideoCategory = "all";
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

        [HttpGet]
        [Route("video/categories")]
        public ActionResult Categories()
        {

            using (var dbContext = new BeeTubeEntities())
            {
                // Assuming you have a database context named 'dbContext' and a 'Category' entity
                List<Category> categories = dbContext.Categories.ToList();

                return View(categories);

            }
            return View();
               
        }

        [HttpGet]
        [Route("videos/category/{id}")]
        public ActionResult CategoryVideos(int id)
        {
            using (var dbContext = new BeeTubeEntities())
            {
                var category = dbContext.Categories.Include("Videos").FirstOrDefault(c => c.Id == id);
                if (category == null)
                {
                    return HttpNotFound();
                }
                ViewBag.VideoCategory = category.Name;
                var videos = category.Videos.ToList();

                return View("Videos",videos);
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