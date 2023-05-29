using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeeTube.Models
{
    public class DashboardViewModel
    {
        public UserProfile UserProfile { get; set; }
        public CreatorStatistics CreatorStatistics { get; set; }
    }


    public class CreatorStatistics
    {
        public List<Video> UploadedVideos { get; set; }
        public int TotalVideos { get; set; }
        public int TotalViews { get; set; }
        public int TotalSubscribers { get; set; }
        // Add more properties as needed
    }



}