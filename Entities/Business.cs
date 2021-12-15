using Npgsql;

namespace Milestone2.Entities
{
    /// <summary>
    /// Represents a business entity.
    /// </summary>
    public class Business
    {
        public string bid { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public int zipcode { get; set; }
        public string address { get; set; }
        public object distance { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int numtips { get; set; }
        public int numcheckins { get; set; }
        public bool is_open { get; set; }
        public double stars { get; set; }

        public Business(string bid, string name, string state, string city,
                        int zipcode, string address, double latitude, double longitude, object distance,
                        int num_tips, int num_checkins, bool is_open, double stars)
        {
            this.bid = bid;
            this.name = name;
            this.state = state;
            this.city = city;
            this.zipcode = zipcode;
            this.address = address;
            this.latitude = latitude;
            this.longitude = longitude;
            this.distance = distance;
            this.numtips = num_tips;
            this.numcheckins = num_checkins;
            this.is_open = is_open;
            this.stars = stars;
        }

        public Business() { }

        public static Business CreateFromReader(NpgsqlDataReader reader, bool currentUserExists)
        {
            return new Business
            {
                bid = reader.GetString(0),
                name = reader.GetString(1),
                state = reader.GetString(2),
                city = reader.GetString(3),
                zipcode = reader.GetInt32(4),
                address = reader.GetString(5),
                latitude = reader.GetDouble(6),
                longitude = reader.GetDouble(7),
                distance = currentUserExists ? (object)reader.GetDouble(8) : "N/A",
                numtips = reader.GetInt32(9),
                numcheckins = reader.GetInt32(10),
                is_open = reader.GetBoolean(11),
                stars = reader.GetDouble(12)
            };
        }
    }
}
