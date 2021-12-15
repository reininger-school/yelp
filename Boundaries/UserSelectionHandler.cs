using Milestone2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Windows.Controls;

namespace Milestone2.Boundaries
{
    /// <summary>
    /// Handles selecting new user.
    /// </summary>
    public class UserSelectionHandler
    {
        private string userID;
        private User user = null;

        public string UserID
        {
            get => this.userID;
            set
            {
                this.userID = value;
                this.nameChanged();
            }
        }

        public User User
        {
            get => this.user;
            set => this.user = value;
        }

        public DataGrid Friends
        {
            get; set;
        }

        public DataGrid FriendTips { get; set; }

        // handles name change.
        private void nameChanged()
        {
            string sqlstr = $"SELECT * FROM users_table WHERE user_id = '{userID}'";
            Utility.executeQuery(sqlstr, this.ReadUser);
            this.LoadFriends();
            this.LoadTips();
        }

        private void ReadUser(NpgsqlDataReader reader)
        {
            if (User == null) User = new User();

            User.userID = reader.GetString(0);
            User.name = reader.GetString(1);
            User.averagestars = reader.GetDouble(2);
            User.fans = reader.GetInt32(3);
            User.cool = reader.GetInt32(4);
            User.funny = reader.GetInt32(5);
            User.tipCount = reader.GetInt32(6);
            User.totalLikes = reader.GetInt32(7);
            User.useful = reader.GetInt32(8);
            User.latitude = reader.GetDouble(9);
            User.longitude = reader.GetDouble(10);
            User.yelpingSince = reader.GetDateTime(11);
        }

        private void LoadFriends()
        {
            this.Friends.Items.Clear();
            string sqlstr = $"SELECT name, average_stars, totallikes, yelping_since FROM (SELECT friend_for FROM users_table AS users INNER JOIN friends_table ON users.user_id = friends_table.friend_of WHERE users.user_id = '{user.userID}') AS friends INNER JOIN users_table ON friends.friend_for = users_table.user_id";
            Utility.executeQuery(sqlstr, this.AddRow);
        }

        private void AddRow(NpgsqlDataReader reader)
        {
            this.Friends.Items.Add(new Friend() { name = reader.GetString(0), avgStars = Math.Round(reader.GetDouble(1), 1).ToString(), totalLikes = reader.GetInt32(2).ToString(), yelpingSince = reader.GetDateTime(3).ToString() });
        }

        private void LoadTips()
        {
            this.FriendTips.Items.Clear();
            string sqlstr = $"SELECT users_table.name, business_table.name, business_table.city, tiptext, tips.tipdate FROM (SELECT friends.friend_for, MAX(tipdate) FROM (SELECT friend_for FROM users_table AS users INNER JOIN friends_table ON users.user_id = friends_table.friend_of WHERE users.user_id = '{this.user.userID}') AS friends Inner Join tip_table ON friends.friend_for = tip_table.user_id GROUP BY friends.friend_for) AS dates INNER JOIN tip_table AS tips ON max = tips.tipdate INNER JOIN business_table ON tips.business_id = business_table.business_id INNER JOIN users_table ON users_table.user_id = dates.friend_for";
            Utility.executeQuery(sqlstr, this.AddTipRow);
        }

        private void AddTipRow(NpgsqlDataReader reader)
        {
            this.FriendTips.Items.Add(new Tip() { business = reader.GetString(1), city = reader.GetString(2), date = reader.GetDateTime(4).ToString(), name = reader.GetString(0), text = reader.GetString(3) });
        }
    }
}
