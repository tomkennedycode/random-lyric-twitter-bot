using System;

namespace yungleanlyrics.Models {
    public class Tweet {
        public long id { get; set; }
        public string id_str { get; set; }
        public DateTime created_at { get; set; }
        public long author_id { get; set; }
        public string text { get; set; }
    }
}
