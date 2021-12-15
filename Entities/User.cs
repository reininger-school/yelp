using System;

namespace Milestone2.Entities
{
    /// <summary>
    /// Represents user entity.
    /// </summary>
    public class User
    {
        public string userID { get; set; }
        public string name { get; set; }
        public double averagestars { get; set; }
        public int fans { get; set; }
        public int cool { get; set; }
        public int funny { get; set; }
        public int tipCount { get; set; }
        public int totalLikes { get; set; }
        public int useful { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime yelpingSince { get; set; }
    }
}
