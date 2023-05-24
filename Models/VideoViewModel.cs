using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeeTube.Models
{
    public class VideoViewModel
    {
        public Video Video { get; set; }
        public List<Comment> Comments { get; set; }

        public Comment Comment { get; set; }
    }

}