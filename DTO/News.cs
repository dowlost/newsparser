using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MyScrapper.DTO
{
    public class News
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime? PubDate { get; set; }
        public string Author { get; set; }
        public string MainImage { get; set; }
        public List<string> SecondaryImages { get; set; }
        public string Keywords { get; set; }
        public List<string> Videos { get; set; }
        public string Icon { get; set; }
    }
}
