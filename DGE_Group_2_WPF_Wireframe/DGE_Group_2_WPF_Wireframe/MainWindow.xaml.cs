using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace DGE_Group_2_WPF_Wireframe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _actiondate.SelectedDate = DateTime.Now;
            _actiontime.Text = DateTime.Now.ToString("HH:mm");
            GetData();
        }

        /// <summary>
        /// Opens the data (or a file if commanded)
        /// </summary>
        /// <param name="overrideFilename">Open a file instead of the resource file. Be careful when writing back to the file to not superfluously ovewrite the resource.</param>
        public void GetData(string overrideFilename = "")
        {
            //WIPE
            people.Clear();
            rooms.Clear();
            checks.Clear();
            checktemp.Clear();

            //OPEN
            var x = new XmlDocument();
            if (overrideFilename == "")
                x.LoadXml(Properties.Resources.test);
            else
                x.Load(overrideFilename);

            //PREPARE
            var xx = x.SelectSingleNode("testdata") as XmlElement;
            var entities = (XmlElement)xx.SelectSingleNode("testentities");
            var data = (XmlElement)xx.SelectSingleNode("testtimes");

            //RETRIEVE

            foreach (XmlElement item in entities.GetElementsByTagName("person"))
            {
                var n = new Person()
                {
                    Id = item.InnerText
                };
                people.Add(n);
            }
            foreach (XmlElement item in entities.GetElementsByTagName("teacher"))
            {
                var n = new Teacher()
                {
                    Id = item.InnerText,
                    Title = item.GetAttribute("title"),
                    Name = item.GetAttribute("name")
                };
                teachers.Add(n);
            }
            foreach (XmlElement item in entities.GetElementsByTagName("room"))
            {
                var n = new Room()
                {
                    Id = item.InnerText,
                    Type = (RoomType)(int.Parse(item.GetAttribute("type")) - 1)
                };
                rooms.Add(n);
            }
            foreach (XmlElement item in entities.GetElementsByTagName("class"))
            {
                var n = new Class()
                {
                    Id = item.InnerText,
                    Room = rooms.Find(o => o.Id == item.GetAttribute("room")),
                    Teacher = teachers.Find(o => o.Id == item.GetAttribute("teacher")),
                    Time = DateTime.Parse(item.GetAttribute("time"))
                };
                classes.Add(n);
            }
            foreach (XmlElement item in data.GetElementsByTagName("check"))
            {
                var n = new Check()
                {
                    DateIn = DateTime.Parse(item.GetAttribute("in")),
                    DateOut = DateTime.Parse(item.GetAttribute("out")),
                    Room = rooms.Find(o => o.Id == item.GetAttribute("room")),
                    User = people.Find(o => o.Id == item.GetAttribute("user")),
                    Class = classes.Find(o => o.Id == item.GetAttribute("class"))

                };
                checks.Add(n);
                checktemp.Add(Check.CreateUIItem(n));
            }


            //DISPLAY
            foreach (var item in people)
            {
                _userid.Items.Add(item);
                _getusers.Items.Add(item);
            }
            foreach (var item in teachers)
            {
                ddTeachers.Items.Add(item);
                //cbTeacher.Items.Add(item);
                //viewTeacher.Items.Add(item);
            }
            foreach (var item in rooms)
            {
                _searchrooms.Items.Add(item);
                //viewRoom.Items.Add(item);
                //editRoom.Items.Add(item);
            }
            FilterRooms();
            Filter();
        }
        /// <summary>
        /// Filters the search page
        /// </summary>
        public void Filter()
        {
            try
            {
                //clear existing picked visual records
                _history.Items.Clear();

                //if filters aren't identifed as used...
                if (!IsSearching)
                {
                    // for each item in all visual records
                    foreach (var item in checktemp)
                    {
                        //add it
                        _history.Items.Add(item);
                    }
                }
                //if filters are identifed as used...
                else
                {
                    // if check in date is ticked but null or if checkout date is ticked but null, throw error
                    if ((cbCheckIn.IsChecked == true && dpCheckIn.SelectedDate == null) || (cbCheckIn.IsChecked == true && dpCheckOut.SelectedDate == null)) throw new ArgumentException("Please select a respective date(s).");

                    // for each item in "all visual records but if users contains user if user is ticked but if room is room if room is ticked but if type is type if type is checked but if check in date is check in date but if checkout date is checkout date if checkout is ticked but if teacher is teacher if teacher is ticked"...
                    // [literally 'for each instance returned by a global search in checktemp for "where users contains user if user is ticked and if room is room if room is ticked and if type is type if type is checked and if check in date is check in date and if checkout date is checkout date if checkout is ticked and if teacher is teacher if teacher is ticked"']
                    foreach (var item in
                        checktemp.FindAll(o =>
                            (_ifsearchusers.IsChecked == false || _searchusers.Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList().Contains(o.User)) &&
                            (cbRoom.IsChecked == false || _searchrooms.Text == o.Room) &&
                            (cbType.IsChecked == false || txtType.Text == o.Type) &&
                            (cbCheckIn.IsChecked == false || o.In == dpCheckIn.SelectedDate.Value.ToString("dd/MM/yyyy HH:mm")) &&
                            (cbCheckOut.IsChecked == false || o.In == dpCheckOut.SelectedDate.Value.ToString("dd/MM/yyyy HH:mm")) &&
                            (cbTeachers.IsChecked == false || txtTeachers.Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList().Contains(o.Teacher)))
                        )
                    {
                        //add it
                        _history.Items.Add(item);

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + $" ({ex.GetType()})", "Invalid", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            tbResults.Text = _history.Items.Count.ToString() + " of " + checktemp.Count.ToString();
            tbSearching.Text = IsSearching.ToString();
            status.FontWeight = IsSearching ? FontWeights.Bold : FontWeights.Normal;

        }
        /// <summary>
        /// Filters the check-in page
        /// </summary>
        public void FilterRooms()
        {
            // if I don't know who this is, do nothing
            //(prevents InitializeComponent from firing this prematurely then crashing the app)
            if (_roomid == null) return;

            _roomid.Items.Clear();
            //clear the combobox

            foreach (var item in rooms.FindAll(o => (int)o.Type == _roomtype.SelectedIndex))
            //  for each item in 'rooms but only where the index of the type within the enum RoomType is the user-selected index of a combobox'..
            {
                _roomid.Items.Add(item);
                //add it
            }

            if (_roomid.Items.Count != 0) _roomid.SelectedIndex = 0;
            // as the clearing reset the combobox, if there is something to show in this case, show the first one

        }
        /// <summary>
        /// Use to indicate whether filters are being used
        /// </summary>
        public bool IsSearching { get; set; } = false;

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if I don't know who this is, do nothing
            if (_menutext == null) return;


            _menutext.Foreground = (_maintabs.SelectedIndex == 0) ? Brushes.Blue : Brushes.White;
            // if I am Menu, turn blue
        }

        private void _directioncombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _directioncombo.Foreground = _directioncombo.SelectedIndex == 0 ? Brushes.Green : Brushes.Red;
            // if I am Checking In, turn green
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //validate e.g.
                if (_actiondate.SelectedDate == null) //if date not selected
                {
                    throw new ArgumentException("Please select a date.");//throw an error and break to CATCH statement
                }
                if (!TimeSpan.TryParse(_actiontime.Text, out TimeSpan n)) //if time is not valid
                {
                    throw new ArgumentException("Please enter a time.");

                }
                //
                //
                //
                //
                Check checkinFound = null;

                if (_directioncombo.SelectedIndex == 1)
                {


                    foreach (var item in checks)
                    {
                        if (item.Class == _roomid.SelectedItem && item.User.Id == _userid.Text && item.DateIn.DayOfYear == _actiondate.SelectedDate.Value.DayOfYear && item.DateIn.TimeOfDay <= TimeSpan.Parse(_actiontime.Text) && item.DateOut == null)
                        {
                            var totalLength = (TimeSpan.Parse(_actiontime.Text) - item.DateIn.TimeOfDay);
                            var dd = (MessageBox.Show($"Is this what you are checking out of?\r\r" +
                                $"{item.Class}" +
                                $"{item.User}" +
                                $"check in since {item.DateIn}\r\r" +
                                $"WARNING: Make sure the check in time is correct before finishing. This may or may not be the desired block, as any block on the day with the check in date before the checkout date and no existing checkout date is highlighted.\r" +
                                $"Proposed attendence length: {totalLength.TotalHours} hours {totalLength.TotalMinutes} minutes.  \r" +
                                $"\r\r" +
                                $"Click Yes to finish" +
                                $"Click No to find next" +
                                $"Click Cancel to cancel search",
                                "Results", MessageBoxButton.YesNoCancel, MessageBoxImage.Information));

                            if (dd == MessageBoxResult.Cancel) return;
                            if (dd == MessageBoxResult.Yes)
                            {
                                checkinFound = item;
                                break;
                            }
                        }
                    }

                    if (checkinFound == null) throw new Exception("Either no results were found or the user skipped through the results. Please make sure that you have entered the correct data and a checkout time that is greater than or equal to a check in time on the same day.");





                }
                var d = MessageBox.Show(
                       $"Are you certain this what you are doing?\r" +
                       $"\r" +
                       $"User '{_userid.Text}' is   {((ComboBoxItem)_directioncombo.SelectedItem).Content.ToString().Remove(2)} \r" +
                       $"checking {(_directioncombo.SelectedIndex == 0 ? "INTO" : "OUT OF") } {_roomtype.Text.Remove(_roomtype.Text.Length - 3)} '{_roomid.Text}'\r" +
                       $"at '{_actiondate.SelectedDate.Value.ToLongDateString()} {_actiontime.Text}'\r" +
                       $"\r" +
                       $"Click OK to finish or Cancel to return.",
                       "Submit", MessageBoxButton.OKCancel, MessageBoxImage.Information);

                if (d == MessageBoxResult.OK)//if previous message box says OK
                {


                    if (_directioncombo.SelectedIndex == 0)
                    {

                        //do stuff (check in)

                    }
                    else
                    {

                        //do stuff (check out) (with checkinFound)

                    }


                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + $" ({ex.GetType()})", "Invalid", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// A version of a record to display in the report
        /// </summary>
        public class _check
        {
            public string User { get; set; }
            public string Teacher { get; set; }
            public string In { get; set; }
            public string Out { get; set; }
            public string Room { get; set; }
            public string Type { get; set; }
        }
        /// <summary>
        /// A record
        /// </summary>
        public class Check
        {
            public Class Class { get; set; }
            public Person User { get; set; }
            public Room Room { get; set; }
            public DateTime DateIn { get; set; }
            public DateTime DateOut { get; set; }

            /// <summary>
            /// Converts the record into a displayable record
            /// </summary>
            /// <param name="item">The inputted <c>Check</c> object.</param>
            /// <returns></returns>
            public static _check CreateUIItem(Check item)
            {
                return new _check
                {
                    User = item.User.Id,
                    Teacher = item.Class.Teacher.ToString(),
                    In = item.DateIn.ToString("dd/MM/yyyy HH:mm"),
                    Out = item.DateOut.ToString("dd/MM/yyyy HH:mm"),
                    Room = item.Room.Id,
                    Type = item.Room.Type.ToString()
                };
            }


        }

        /// <summary>
        /// A user
        /// </summary>

        public class Person
        {
            public string Id { get; set; }
            public override string ToString()
            {
                return Id;
            }
        }

        /// <summary>
        /// Type of room
        /// </summary>
        public enum RoomType
        {
            Class,
            Office
        }
        public class Teacher
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Title + " " + Name;
            }
        }

        /// <summary>
        /// A room
        /// </summary>
        public class Room
        {
            public RoomType Type { get; set; }
            public string Id { get; set; }
            public override string ToString()
            {
                return Id;
            }
        }

        public class Class
        {
            public string Id { get; set; }
            public Room Room { get; set; }
            public Teacher Teacher { get; set; }
            public DateTime Time { get; set; }

            public override string ToString()
            {
                return $"{Id} by {Teacher} in {Room} at {Time.ToString("HH:mm")}";
            }
        }

        /// <summary>
        /// The list of rooms
        /// </summary>
        public List<Room> rooms = new List<Room>();
        /// <summary>
        /// The list of classes
        /// </summary>
        public List<Class> classes = new List<Class>();
        /// <summary>
        /// The list of people
        /// </summary>
        public List<Person> people = new List<Person>();
        /// <summary>
        /// A list of teachers
        /// </summary>
        public List<Teacher> teachers = new List<Teacher>();
        /// <summary>
        /// The list of undisplayable records
        /// </summary>
        public List<Check> checks = new List<Check>();
        /// <summary>
        /// A list of displayable records used as backup for turning on or off filters. USE THIS INSTEAD OF EDITING THE GRIDVIEW ITEMS, then read off this (using <c>for each</c> or <c>for each ... .FindAll(...)</c> etc.) to display the filtered or unfiltered gridview items.
        /// </summary>
        public List<_check> checktemp = new List<_check>();

        public class ClassGenerator
        {
            public RoomType RoomType { get; set; }
            public Room Room { get; set; }
            public DayOfWeek Day { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public DateTime? StartDate { get; set; } // nullable DateTime
            public DateTime? EndDate { get; set; } // nullable DateTime
            public string Label { get; set; } = "";
            public Teacher Teacher { get; set; }

            public static _class CreateUIItem(ClassGenerator cg)
            {
                return new _class
                {
                    Teacher = cg.Teacher.ToString(),
                    Label = cg.Label,
                    Times = $"{cg.StartTime.ToString("HH:mm")} to {cg.EndTime.ToString("HH:mm")}",
                    Day = cg.Day.ToString(),
                    StartDate = (cg.StartDate == null ? "" : cg.StartDate.Value.ToShortDateString()),
                    EndDate = (cg.EndDate == null ? "" : cg.EndDate.Value.ToShortDateString())
                };

            }

        }

        public class _class
        {
            public string Teacher { get; set; }
            public string Label { get; set; }
            public string Room { get; set; }
            public string Times { get; set; }
            public string Day { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }

        }
        private void _roomtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRooms();
        }

        private void _searchrooms_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == _getusers)
            {
                if (_getusers.SelectedIndex < 1) return;
                //if the text is still "(Quick Add)" don't do anything (prevents misfiring and infinite loops)
                var l = _searchusers.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList();
                //output each item in the field as a string with no empties [literally 'let l be the textbox text but with its text split by the character ',' but remove empty strings, and cast it to an enumerable of 'string' and cast it to a list']
                l.Add(e.AddedItems[0].ToString());
                //add that new selection (had to use e because the text kept changing back before being read) [literally 'add to l "the first new thing you are talking about but a string"']
                _searchusers.Text = string.Join(", ", l.Select(o => o.Trim()));
                //re-insert all items as a trimmed comma-seperated array of values [literally 'make text joined version of "l but everything is trimmed" by the string ", "']
                _getusers.SelectedIndex = 0;
                //go back to "(Quick Add)"
            }
            else
            {
                if (ddTeachers.SelectedIndex < 1) return;
                //if the text is still "(Quick Add)" don't do anything (prevents misfiring and infinite loops)
                var l = txtTeachers.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList();
                //output each item in the field as a string with no empties [literally 'let l be the textbox text but with its text split by the character ',' but remove empty strings, and cast it to an enumerable of 'string' and cast it to a list']
                l.Add(e.AddedItems[0].ToString());
                //add that new selection (had to use e because the text kept changing back before being read) [literally 'add to l "the first new thing you are talking about but a string"']
                txtTeachers.Text = string.Join(", ", l.Select(o => o.Trim()));
                //re-insert all items as a trimmed comma-seperated array of values [literally 'make text joined version of "l but everything is trimmed" by the string ", "']
                ddTeachers.SelectedIndex = 0;
                //go back to "(Quick Add)"
            }

        }

        private void _searchusers_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == txtTeachers)
                cbTeachers.IsChecked = true;
            else
                _ifsearchusers.IsChecked = true;

            if (cbAuto == null) return;
            if (cbAuto.IsChecked == true) Filter();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _searchusers.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsSearching = true;
            Filter();
        }

        private void dpCheckIn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpCheckOut == null) return;
            if (cbAuto == null) return;

            cbCheckIn.IsChecked = dpCheckIn.SelectedDate != null;
            cbCheckOut.IsChecked = dpCheckOut.SelectedDate != null;
            if (cbAuto.IsChecked == true) Filter();

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            IsSearching = false;
            Filter();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _ifsearchusers.IsChecked = false;
            cbCheckIn.IsChecked = false;
            cbCheckOut.IsChecked = false;
            cbRoom.IsChecked = false;
            cbType.IsChecked = false;
            cbTeachers.IsChecked = false;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _ifsearchusers.IsChecked = false;
            cbCheckIn.IsChecked = false;
            cbCheckOut.IsChecked = false;
            cbRoom.IsChecked = false;
            cbType.IsChecked = false;
            _searchusers.Text = "";
            _searchrooms.Text = "";
            _roomtype.SelectedIndex = 0;
            dpCheckIn.SelectedDate = null;
            dpCheckOut.SelectedDate = null;
            ddCheckIn.SelectedIndex = 0;
            ddCheckOut.SelectedIndex = 0;
            cbTeachers.IsChecked = false;
            txtTeachers.Text = "";
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (cbAuto == null) return;

            txtTeachers.Text = "";
        }

        private void txtType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
            cbType.IsChecked = true;
        }

        private void _searchrooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
            cbRoom.IsChecked = true;
        }

        private void ddCheckIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
            cbCheckIn.IsChecked = true;
        }

        private void ddCheckOut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAuto == null) return;

            cbCheckOut.IsChecked = true;
        }

        private void cbAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAuto.IsChecked == true && !IsSearching)
            {
                if (MessageBox.Show("This action is not applicable while not Seeing Search. Do you wish to search now? Click No to manually activate AutoSearch using Search later.\r\rThe action should work automatically afterwards as long as you are Seeing Search.", "Search", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    IsSearching = true;
                    Filter();
                }

            }
        }

        private void _ifsearchusers_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
        }

        private void cbTeachers_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
        }

        private void cbType_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
        }

        private void cbRoom_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
        }

        private void cbCheckIn_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
        }

        private void cbCheckOut_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAuto == null) return;

            if (cbAuto.IsChecked == true) Filter();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (gbEdit == null) return;
        //    gbEdit.Visibility = _Edit.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        //    gbView.Margin = _Edit.IsChecked == true ? new Thickness(0, 0, 439, 0) : new Thickness(0);

        //}

        //private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (gbEdit == null) return;

        //    gbFilter.Visibility = _Filter.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        //    tbClass.Margin = _Filter.IsChecked == true ? new Thickness(0, 198, 0, 0) : new Thickness(0);
        //    svClass.Margin = _Filter.IsChecked == true ? new Thickness(0, 229, 0, 0) : new Thickness(0, 229 - 198, 0, 0);
        //}

        //private void btnGet_Click(object sender, RoutedEventArgs e)
        //{
        //    var loop = false;

        //    do
        //    {


        //        var d = new Dialog("Get", "What attribute of the selection would you like to copy to the editor?", "", System.Drawing.SystemIcons.Question);
        //        d.btns.Orientation = Orientation.Vertical;
        //        d.WindowStartupLocation = WindowStartupLocation.Manual;
        //        d.Top = 100;
        //        d.Left = 100;

        //        var lb = new ListBox();
        //        lb.Items.Add("Room type");
        //        lb.Items.Add("Room");
        //        lb.Items.Add("Day");
        //        lb.Items.Add("Time start");
        //        lb.Items.Add("Time end");
        //        lb.Items.Add("Date start");
        //        lb.Items.Add("Date end");
        //        lb.Items.Add("Teacher");
        //        lb.Items.Add("Label");

        //        d.btns.Children.Add(lb);

        //        d.NewButton("Apply", 1);
        //        d.NewButton("OK", 2);
        //        d.NewButton("Cancel", 3);

        //        d.btns.Children[1].IsEnabled = false;
        //        d.btns.Children[2].IsEnabled = false;
        //        getListBox = lb;
        //        getApplyButton = (Button)d.btns.Children[1];
        //        getOKButton = (Button)d.btns.Children[2];

        //        lb.SelectionChanged += Lb_SelectionChanged;
        //        d.ShowDialog();

        //        loop = d.MessageBoxCode == 1;


        //        //txtLabel.Text = "Test String";

        //    }
        //    while (loop);
        //}

        //ListBox getListBox;
        //Button getApplyButton;
        //Button getOKButton;



        //private void Lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    getApplyButton.IsEnabled = getListBox.SelectedIndex != -1;
        //    getOKButton.IsEnabled = getListBox.SelectedIndex != -1;
        //}
    }
}
