using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BeeTube.Models
{
    public class VideoUploadModel
    {
       
       

        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        [Required]
        public Nullable<int> CategoryID { get; set; }

        [Required]
        public HttpPostedFileBase ThumbnailFile { get; set; }

        [Required]
        public HttpPostedFileBase VideoFile { get; set; }
       

    }

    

}