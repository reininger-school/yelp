using Milestone2.Boundaries;
using Milestone2.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Milestone2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UserHandler userHandler;
        public UserSelectionHandler userSelectionHandler = new UserSelectionHandler();
        public Tips tipsWindow;
        public Checkins checkinwindow;
        public Business activeBusiness;

        private static Dictionary<string, int> PriceRangeNameMappings = new Dictionary<string, int>
        {
            { nameof(PriceFilterLow),     1 },
            { nameof(PriceFilterMed),     2 },
            { nameof(PriceFilterHigh),    3 },
            { nameof(PriceFilterLavish),  4 }
        };

        /// <summary>
        /// Maps control names to names in DB.
        /// </summary>
        private static Dictionary<string, string> AttributeNameMappings = new Dictionary<string, string>
        {
            { nameof(AcceptsCreditCards_Checkbox),   "BusinessAcceptsCreditCards" },
            { nameof(TakesReservations_Checkbox),    "RestaurantsReservations" },
            { nameof(WheelchairAccessible_Checkbox), "WheelchairAccessible" },
            { nameof(OutdoorSeating_Checkbox),       "OutdoorSeating" },
            { nameof(GoodForKids_Checkbox),          "GoodForKids" },
            { nameof(GoodForGroups_Checkbox),        "RestaurantsGoodForGroups" },
            { nameof(Delivery_Checkbox),             "RestaurantsDelivery" },
            { nameof(Takeout_Checkbox),              "RestaurantsTakeout" },
            { nameof(FreeWiFi_Checkbox),             "WiFi" },
            { nameof(BikeParking_Checkbox),          "BikeParking" }
        };

        private static List<(string, string)> ColumnNameMappings = new List<(string, string)>
        {
            ("Business Name", "name"),
            ( "State", "state"),
            ( "City", "city"),
            ( "Zipcode", "zipcode"),
            ( "Address", "address"),
            ( "Distance", "distance"),
            ( "Tip Count", "numtips"),
            ( "Checkin Count", "numcheckins"),
            ( "Rating", "stars")
        };

        /// <summary>
        /// Maps control names to names in DB.
        /// </summary>
        private static Dictionary<string, string> MealNameMappings = new Dictionary<string, string>
        {
            { nameof(Breakfast_Checkbox), "breakfast" },
            { nameof(Brunch_Checkbox), "brunch" },
            { nameof(Lunch_Checkbox), "lunch" },
            { nameof(Dinner_Checkbox), "dinner" },
            { nameof(LateNight_Checkbox), "latenight" }
        };

        public MainWindow()
        {
            InitializeComponent();

            ExecuteQuery("CREATE EXTENSION IF NOT EXISTS cube;", reader => { });
            ExecuteQuery("CREATE EXTENSION IF NOT EXISTS earthdistance;", reader => { });
            InitializeStatesList();
            InitializeGridColumns();
            InitializeOrderBySelection();
            InitializeCategorySelection();
            
            this.userHandler = new UserHandler(this.listBox);
            this.userSelectionHandler.Friends = this.dataGrid_Friends;
            this.userSelectionHandler.FriendTips = this.dataGrid_FriendTips;
            this.tipsWindow = new Tips(this);
            this.checkinwindow = new Checkins(this);
            AddColumns2Friends();
            AddColumns2Tips();
        }

        private void InitializeStatesList()
        {
            string queryText = "SELECT DISTINCT state FROM business_table ORDER BY state";
            ExecuteQuery(queryText, reader => StateSelection.Items.Add(reader.GetString(0)));
        }

        private void InitializeGridColumns()
        {
            foreach ((string, string) columnMapping in ColumnNameMappings)
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Binding = new Binding(columnMapping.Item2);
                col.Header = columnMapping.Item1;
                col.Width = 150;
                BusinessGrid.Columns.Add(col);
            }
        }

        private void InitializeOrderBySelection()
        {
            foreach ((string, string) columnMapping in ColumnNameMappings)
            {
                OrderBySelection.Items.Add(columnMapping.Item1);
            }
            OrderBySelection.SelectedIndex = 0;
        }

        private void InitializeCategorySelection()
        {
            string queryText = "SELECT DISTINCT category_name FROM categories_table ORDER BY category_name;";
            ExecuteQuery(queryText, reader => CategoryFilterSelection.Items.Add(reader.GetString(0)));
        }

        private void UpdateCitySelection(string selectedState)
        {
            string queryText = $"SELECT DISTINCT city FROM business_table WHERE state = '{selectedState}' ORDER BY city;";
            ExecuteQuery(queryText, reader => CitySelection.Items.Add(reader.GetString(0)));
        }

        private void UpdateZipcodeSelection(string selectedState, IEnumerable<string> selectedCities)
        {
            string citiesFilter = $"({string.Join(" OR ", selectedCities.Select(c => $"city = '{c}'"))})";
            string queryText = $"SELECT DISTINCT zipcode FROM business_table WHERE state = '{selectedState}' AND {citiesFilter} ORDER BY zipcode;";
            ExecuteQuery(queryText, reader => ZipcodeSelection.Items.Add(reader.GetInt32(0).ToString()));
        }

        private void StateSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedState = ((ComboBox)sender).SelectedItem as string;

            CitySelection.Items.Clear();
            if (!string.IsNullOrWhiteSpace(selectedState))
                UpdateCitySelection(selectedState);
        }

        private void CitySelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedState = StateSelection.SelectedItem as string;
            IEnumerable<string> selectedCities = ((ListBox)sender).SelectedItems.Cast<string>();

            ZipcodeSelection.Items.Clear();
            if (!string.IsNullOrWhiteSpace(selectedState) && selectedCities.Any())
                UpdateZipcodeSelection(selectedState, selectedCities);
        }

        private void AddCategoryFilter_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> selectedFilters = CategoryFilterSelection.SelectedItems.Cast<string>();
            foreach (string filter in selectedFilters)
            {
                if (!SelectedCategoryFilters.Items.Contains(filter))
                    SelectedCategoryFilters.Items.Add(filter);
            }
        }

        private void RemoveCategoryFilter_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> selectedFilters = CategoryFilterSelection.SelectedItems.Cast<string>();
            foreach (string filter in selectedFilters)
            {
                if (SelectedCategoryFilters.Items.Contains(filter))
                    SelectedCategoryFilters.Items.Remove(filter);
            }
        }

        private void SearchBusinessesButton_Click(object sender, RoutedEventArgs e)
        {
            BusinessGrid.Items.Clear();
         
            // Filter for selected state
            List<string> filters = new List<string>();
            if (StateSelection.SelectedItem is string selectedState)
            {
                filters.Add($"state = '{selectedState}'");
            }

            // Filter for selected cities
            IEnumerable<string> selectedCities = CitySelection.SelectedItems.Cast<string>();
            if (selectedCities.Any())
            {
                filters.Add($"({string.Join(" OR ", selectedCities.Select(c => $"city = '{c}'"))})");
            }

            // Filter for selected zipcodes
            IEnumerable<string> selectedZipcodes = ZipcodeSelection.SelectedItems.Cast<string>();
            if (selectedZipcodes.Any())
            {
                filters.Add($"({string.Join(" OR ", selectedZipcodes.Select(z => $"zipcode = '{z}'"))})");
            }

            // Filter the selected categories
            IEnumerable<string> selectedCategories = SelectedCategoryFilters.Items.Cast<string>();
            if (selectedCategories.Any())
            {
                string categoryArray = $"ARRAY[{string.Join(", ", selectedCategories.Select(c => $"'{c}'"))}]::varchar[]";
                string categoryFilter = $"{categoryArray} <@ (SELECT ARRAY(" +
                                        $"  SELECT category_name " +
                                        $"  FROM categories_table c " +
                                        $"  WHERE b.business_id = c.business_id" +
                                        $"))";
                filters.Add(categoryFilter);
            }

            // Filter the selected acceptable prices
            List<int> deselectedPriceFilters = new List<int>();
            foreach (CheckBox c in PriceFilterSelection.Children.Cast<CheckBox>().Where(c => !c.IsChecked.HasValue || !c.IsChecked.Value))
                deselectedPriceFilters.Add(PriceRangeNameMappings[c.Name]);
            if (deselectedPriceFilters.Any())
            {
                string subqueryFilter = $"({string.Join(" OR ", deselectedPriceFilters.Select(f => $"attr_value = '{f}'"))})";
                string priceFilter = $"NOT EXISTS (SELECT business_id FROM attributes_table a WHERE b.business_id = a.business_id AND attr_name = 'RestaurantsPriceRange2' AND {subqueryFilter})";
                filters.Add(priceFilter);
            }

            // Filter on selected attributes
            List<string> selectedAttributeFilters = new List<string>();
            foreach (CheckBox c in AttributeFilterSelection.Children.Cast<CheckBox>().Where(c => c.IsChecked.HasValue && c.IsChecked.Value))
                selectedAttributeFilters.Add(AttributeNameMappings[c.Name]);
            if (selectedAttributeFilters.Any())
            {
                string attributeArray = $"ARRAY[{string.Join(", ", selectedAttributeFilters.Select(a => $"'{a}'"))}]::varchar[]";

                Func<string, string> nameToQuery = name =>
                {
                    if (name == "WiFi") return $"(attr_name = '{name}' AND attr_value = 'free')";
                    else                return $"(attr_name = '{name}' AND attr_value = 'True')";
                };
                string subqueryFilter = $"({string.Join(" OR ", selectedAttributeFilters.Select(nameToQuery))})";
                string attributeFilter = $"{attributeArray} <@ (SELECT ARRAY(" +
                                         $"  SELECT attr_name" +
                                         $"  FROM attributes_table a" +
                                         $"  WHERE b.business_id = a.business_id AND {subqueryFilter}" +
                                         $"))";
                filters.Add(attributeFilter);
            }

            // Filter on selected meals
            List<string> selectedMealFilters = new List<string>();
            foreach (CheckBox c in MealFilterSelection.Children.Cast<CheckBox>().Where(c => c.IsChecked.HasValue && c.IsChecked.Value))
                selectedMealFilters.Add(MealNameMappings[c.Name]);
            if (selectedMealFilters.Any())
            {
                string attributeArray = $"ARRAY[{string.Join(", ", selectedMealFilters.Select(a => $"'{a}'"))}]::varchar[]";

                Func<string, string> nameToQuery = name =>
                {
                    return $"(attr_name = '{name}' AND attr_value = 'True')";
                };
                string subqueryFilter = $"({string.Join(" OR ", selectedMealFilters.Select(nameToQuery))})";
                string attributeFilter = $"{attributeArray} <@ (SELECT ARRAY(" +
                                         $"  SELECT attr_name" +
                                         $"  FROM attributes_table a" +
                                         $"  WHERE b.business_id = a.business_id AND {subqueryFilter}" +
                                         $"))";
                filters.Add(attributeFilter);
            }

            string whereFilter = string.Join(" AND ", filters);
            if (string.IsNullOrWhiteSpace(whereFilter))
            {
                whereFilter = "TRUE";
            }

            double currentUserLongitude = this.userSelectionHandler.User?.longitude ?? 0.0;
            double currentUserLatitude = this.userSelectionHandler.User?.latitude ?? 0.0;
            string currentUserPoint = $"point({currentUserLongitude},{currentUserLatitude})";
            string queryText = $"SELECT business_id, name, state, city, zipcode, address, latitude, longitude, " +
                               $"       ({currentUserPoint} <@> point(longitude,latitude)) as distance, " +
                               $"       numtips, numcheckins, is_open, stars " +
                               $"FROM business_table b " +
                               $"WHERE {whereFilter} " +
                               $"ORDER BY {ColumnNameMappings.Where(t => t.Item1 == OrderBySelection.SelectedItem as string).Single().Item2};";
            ExecuteQuery(queryText, reader => BusinessGrid.Items.Add(Business.CreateFromReader(reader, this.userSelectionHandler.User != null)));
        }

        private void ExecuteQuery(string query, Action<NpgsqlDataReader> action)
        {
            using (var connection = new NpgsqlConnection(Utility.BuildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = query;
                    try
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            action(reader);
                        }
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        MessageBox.Show("SQL Error - " + ex.Message.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void AddColumns2Friends()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("name");
            col1.Header = "Name";
            dataGrid_Friends.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("totalLikes");
            col2.Header = "Total Likes";
            dataGrid_Friends.Columns.Add(col2);
            
            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("avgStars");
            col3.Header = "Avg Stars";
            dataGrid_Friends.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Binding = new Binding("yelpingSince");
            col4.Header = "Yelping Since";
            dataGrid_Friends.Columns.Add(col4);
        }
        private void AddColumns2Tips()
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Binding = new Binding("name");
            col1.Header = "User Name";
            dataGrid_FriendTips.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("business");
            col2.Header = "Business";
            dataGrid_FriendTips.Columns.Add(col2);
            
            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("city");
            col3.Header = "City";
            dataGrid_FriendTips.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Binding = new Binding("text");
            col4.Header = "Text";
            dataGrid_FriendTips.Columns.Add(col4);
            
            DataGridTextColumn col5 = new DataGridTextColumn();
            col5.Binding = new Binding("date");
            col5.Header = "Date";
            dataGrid_FriendTips.Columns.Add(col5);
        }
        
        // Fires when name search changes.
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox userTextBox = sender as TextBox;
            this.userHandler.Name = userTextBox.Text;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox userListBox = sender as ListBox;
            this.userSelectionHandler.UserID = userListBox.SelectedItem.ToString();

            this.textBox_username.Text = this.userSelectionHandler.User.name;
            this.textBox_stars.Text = Math.Round(this.userSelectionHandler.User.averagestars, 1).ToString();
            this.textBox_fans.Text = this.userSelectionHandler.User.fans.ToString();
            this.textBox_yelpSince.Text = this.userSelectionHandler.User.yelpingSince.ToString();
            this.textBox_funny.Text = this.userSelectionHandler.User.funny.ToString();
            this.textBox_cool.Text = this.userSelectionHandler.User.cool.ToString();
            this.textBox_useful.Text = this.userSelectionHandler.User.useful.ToString();
            this.textBox_tipCount.Text = this.userSelectionHandler.User.tipCount.ToString();
            this.textBox_totalTipLikes.Text = this.userSelectionHandler.User.totalLikes.ToString();
            this.textBox_lat.Text = this.userSelectionHandler.User.latitude.ToString();
            this.textBox_long.Text = this.userSelectionHandler.User.longitude.ToString();
        }

        private void button_edit_Click(object sender, RoutedEventArgs e)
        {
            this.textBox_username.IsReadOnly = !this.textBox_username.IsReadOnly;
            this.textBox_lat.IsReadOnly = !this.textBox_lat.IsReadOnly;
            this.textBox_long.IsReadOnly = !this.textBox_long.IsReadOnly;

            this.textBox_username.IsEnabled = !this.textBox_username.IsEnabled;
            this.textBox_lat.IsEnabled = !this.textBox_lat.IsEnabled;
            this.textBox_long.IsEnabled = !this.textBox_long.IsEnabled;
        }

        private void button_update_Click(object sender, RoutedEventArgs e)
        {
            this.userSelectionHandler.User.name = this.textBox_username.Text;
            this.userSelectionHandler.User.latitude = double.Parse(this.textBox_lat.Text);
            this.userSelectionHandler.User.longitude = double.Parse(this.textBox_long.Text);
            this.userSelectionHandler.User.Update();

            // disable fields again
            this.button_edit_Click(null, null);
        }

        private void BusinessGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Hours hours = new Hours() { close = "", open ="" };

            void gethours(NpgsqlDataReader reader)
            {
                hours = new Hours() { open = reader.GetTimeSpan(1).ToString(), close = reader.GetTimeSpan(0).ToString() };
            }

            void AddAttributeRow(NpgsqlDataReader reader)
            {
                listBox_CategoriesAttributes.Items.Add(reader.GetString(0));
            }

            void AddCategoryRow(NpgsqlDataReader reader)
            {
                listBox_CategoriesAttributes.Items.Add($"{reader.GetString(0)}");
            }

            if (BusinessGrid.SelectedIndex > -1)
            {
                Business B = BusinessGrid.Items[BusinessGrid.SelectedIndex] as Business;
                if ((B.bid != null) && B.bid.ToString().CompareTo("") != 0)
                {
                    int dayNumberOfWeek = (int)DateTime.Today.DayOfWeek;
                    string sqlstr = $"SELECT open, close FROM hours_table WHERE business_id = '{B.bid}' AND dayofweek = {dayNumberOfWeek}";
                    Utility.executeQuery(sqlstr, gethours);
                    textBlock_Name.Text = B.name;
                    textBlock_Address.Text = B.address;
                    textBlock_Hours.Text = $"Today ({DateTime.Today.DayOfWeek}): Opens: {hours.open}    Closes: {hours.close}";


                    listBox_CategoriesAttributes.Items.Clear();

                    // add attributes
                    sqlstr = $"SELECT attr_name FROM attributes_table WHERE business_id = '{B.bid}' AND (attr_value = 'True' OR (attr_name = 'WiFi' AND attr_value = 'free'));";
                    Utility.executeQuery(sqlstr, AddAttributeRow);

                    // add categories
                    sqlstr = $"SELECT category_name FROM categories_table WHERE business_id = '{B.bid}';";
                    Utility.executeQuery(sqlstr, AddCategoryRow);

                    this.activeBusiness = B;
                }
            }
        }

        private void button_Tips_Click(object sender, RoutedEventArgs e)
        {
            this.tipsWindow = new Tips(this);
            this.tipsWindow.Show();
        }
        private void button_Checkins_Click(object sender, RoutedEventArgs e)
        {
            checkinwindow.columnChart();
            this.checkinwindow.Show();
        }
    }
}
