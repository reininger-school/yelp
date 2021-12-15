using Milestone2.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Milestone2
{
    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class Tips : Window
    {
        private Tip activeTip;

        MainWindow parent;
        public Tips(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.AddColumns2Tips();
            this.AddColumns2FriendTips();
            this.LoadTips(this.parent.activeBusiness);
        }

        private void AddColumns2Tips()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("date");
            col1.Header = "Date";
            this.dataGrid_Tips.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("name");
            col2.Header = "User Name";
            this.dataGrid_Tips.Columns.Add(col2);
            
            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("likes");
            col3.Header = "Likes";
            this.dataGrid_Tips.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Binding = new Binding("text");
            col4.Header = "Text";
            this.dataGrid_Tips.Columns.Add(col4);
        }

        private void AddColumns2FriendTips()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("name");
            col1.Header = "User Name";
            this.dataGrid_FriendTips.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("date");
            col2.Header = "Date";
            this.dataGrid_FriendTips.Columns.Add(col2);
            
            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("text");
            col3.Header = "Text";
            this.dataGrid_FriendTips.Columns.Add(col3);
        }

        // load tips into business grid
        public void LoadTips(Business B)
        {
            this.dataGrid_Tips.Items.Clear();
            void AddRow(NpgsqlDataReader reader)
            {
                this.dataGrid_Tips.Items.Add(new Tip() { date = reader.GetDateTime(0).ToString(), name = reader.GetString(1), likes = reader.GetInt32(2).ToString(), text = reader.GetString(3), userID = reader.GetString(4) });
            }
            if (B != null)
            {
                string sqlstr = $"SELECT t.tipdate, u.name, t.likes, t.tiptext, t.user_id FROM tip_table AS t, users_table AS u WHERE t.user_id = u.user_id AND t.business_id = '{B.bid}' ORDER BY t.tipdate DESC";
                Utility.executeQuery(sqlstr, AddRow);

                //friends
                this.dataGrid_FriendTips.Items.Clear();
                void AddFriendRow(NpgsqlDataReader reader)
                {
                    this.dataGrid_FriendTips.Items.Add(new Tip() { name = reader.GetString(0), date = reader.GetDateTime(1).ToString(), text = reader.GetString(2) });
                }

                sqlstr = $"SELECT friend_of, tipdate, tiptext FROM (SELECT friend_of FROM friends_table WHERE friend_for = '{this.parent.userSelectionHandler.User.userID}') AS friends INNER JOIN tip_table ON friends.friend_of = tip_table.user_id INNER JOIN users_table ON friends.friend_of = users_table.user_id WHERE business_id = '{B.bid}'";
                Utility.executeQuery(sqlstr, AddFriendRow);
            }
        }

        private void button_AddTip_Click(object sender, RoutedEventArgs e)
        {
            string sqlstr = $"INSERT INTO tip_table VALUES ('{DateTime.Now}', '{this.parent.activeBusiness.bid}', '{this.parent.userSelectionHandler.User.userID}', '{this.textBox_NewTip.Text}', {0})";
            Utility.executeQuery(sqlstr, null);
            this.LoadTips(this.parent.activeBusiness);
        }

        private void dataGrid_Tips_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid_Tips.SelectedIndex > -1)
            {
                Tip T = dataGrid_Tips.Items[dataGrid_Tips.SelectedIndex] as Tip;
                if (T.name != null && T.name != "")
                {
                    this.activeTip = T;
                }
            }
        }

        private void button1_Like_Click(object sender, RoutedEventArgs e)
        {
            string sqlstr = $"UPDATE tip_table SET likes = likes + 1 WHERE business_id = '{this.parent.activeBusiness.bid}' AND tipdate = '{this.activeTip.date}' AND user_id = '{this.activeTip.userID}'";
            Utility.executeQuery(sqlstr, null);
            Console.WriteLine(sqlstr);
            this.LoadTips(this.parent.activeBusiness);
        }
    }
}
