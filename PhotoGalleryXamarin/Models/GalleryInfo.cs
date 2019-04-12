using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PhotoGalleryXamarin.Models
{
    public class GalleryInfo
    {
        [JsonProperty("photos")]
        public Gallery Gallery { get; set; }
    }

    public class Gallery
    {
        [JsonProperty("photo")]
        public List<GalleryItem> GalleryItems { get; set; }
    }

    public class GalleryItem
    {  
        [JsonProperty("title")]
        public string Caption { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url_s")]
        public string Url { get; set; } 
    }
}
