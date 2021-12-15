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
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Controls.DataVisualization;
using Npgsql;
using Milestone2.Entities;

namespace Milestone2
{
    /// <summary>
    /// Interaction logic for Checkins.xaml
    /// </summary>
    public partial class Checkins : Window
    {
        MainWindow parent;
        private List<string> listOfNumbers;
        public Checkins(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.listOfNumbers = new List<string>();
            columnChart();
        }

        public void columnChart()
        {
            listOfNumbers.Clear();

            Business business = parent.BusinessGrid.SelectedItem as Business;
            if (business != null)
            {
                string sqlquery = $"Select business_id, Count(*), month from Checkins_table WHERE business_id = {business.bid.ToString()} Group by month;";
                Utility.executeQuery(sqlquery, updateGraphHelper);


                List<KeyValuePair<string, int>> myData = new List<KeyValuePair<string, int>>();

                myData.Add(new KeyValuePair<string, int>("January", Int32.Parse(listOfNumbers[0])));
                myData.Add(new KeyValuePair<string, int>("Febuary", Int32.Parse(listOfNumbers[1])));
                myData.Add(new KeyValuePair<string, int>("March", Int32.Parse(listOfNumbers[3])));
                myData.Add(new KeyValuePair<string, int>("April", Int32.Parse(listOfNumbers[4])));
                myData.Add(new KeyValuePair<string, int>("May", Int32.Parse(listOfNumbers[5])));
                myData.Add(new KeyValuePair<string, int>("June", Int32.Parse(listOfNumbers[4])));
                myData.Add(new KeyValuePair<string, int>("July", Int32.Parse(listOfNumbers[6])));
                myData.Add(new KeyValuePair<string, int>("Auguest", Int32.Parse(listOfNumbers[7])));
                myData.Add(new KeyValuePair<string, int>("September", Int32.Parse(listOfNumbers[8])));
                myData.Add(new KeyValuePair<string, int>("October", Int32.Parse(listOfNumbers[9])));
                myData.Add(new KeyValuePair<string, int>("November", Int32.Parse(listOfNumbers[10])));
                myData.Add(new KeyValuePair<string, int>("December", Int32.Parse(listOfNumbers[11])));
                MyChart.DataContext = myData;
            }
       
        }
        private void updateGraphHelper(NpgsqlDataReader reader)
        {

            string numbers = reader.GetInt64(1).ToString();
 
            foreach(string word in numbers.Split('\n'))
            {
                listOfNumbers.Add(word);
            }
            
        }

    }
}
   