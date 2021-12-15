using Milestone2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milestone2.Boundaries
{
    // Updates user table.
    static class UserUpdateHandler
    {
        public static void Update(this User user)
        {
            string sqlstr = $"UPDATE users_table SET name = '{user.name}', user_latitude = '{user.latitude}', user_longitude = '{user.longitude}' WHERE user_id = '{user.userID}'";
            Utility.executeQuery(sqlstr, null);
        }
    }
}
