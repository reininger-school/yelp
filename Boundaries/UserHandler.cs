using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Milestone2.Boundaries
{
    // handles input to set current user group box.
    public class UserHandler
    {
        public ListBox listBox;
        private string name;

        public UserHandler(ListBox listBox)
        {
            this.listBox = listBox;
        }

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.nameChanged();
            }
        }

        private void nameChanged()
        {
            string newName = this.name;
            this.listBox.Items.Clear();
            string sqlStr = $"SELECT DISTINCT user_id FROM users_table WHERE name LIKE '{newName}%'";
            Utility.executeQuery(sqlStr, AddRow);
        }

        private void AddRow(NpgsqlDataReader reader)
        {
            this.listBox.Items.Add(reader.GetString(0));
            //Console.WriteLine(reader.GetString(0));
        }
    }
}
