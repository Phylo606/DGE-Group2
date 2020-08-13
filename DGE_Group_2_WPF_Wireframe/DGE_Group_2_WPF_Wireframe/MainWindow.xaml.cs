using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (XmlElement item in entities.GetElementsByTagName("room"))
            {
                var n = new Room()
                {
                    Id = item.InnerText,
                    Teacher = item.GetAttribute("teacher"),
                    Type = (RoomType)(int.Parse(item.GetAttribute("type")) - 1)
                };
                rooms.Add(n);
            }
            foreach (XmlElement item in entities.GetElementsByTagName("person"))
            {
                var n = new Person()
                {
                    Id = item.InnerText
                };
                people.Add(n);
            }
            foreach (XmlElement item in data.GetElementsByTagName("check"))
            {
                var n = new Check()
                {
                    DateIn = DateTime.Parse(item.GetAttribute("in")),
                    DateOut = DateTime.Parse(item.GetAttribute("out")),
                    Room = rooms.Find(o => o.Id == item.GetAttribute("room")),
                    User = people.Find(o => o.Id == item.GetAttribute("user"))

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

            foreach (var item in rooms)
            {
                _searchrooms.Items.Add(item);
            }
            FilterRooms();
            Filter();
        }
        /// <summary>
        /// Filters the search page
        /// </summary>
        public void Filter()
        {
            _history.Items.Clear();

            //placeholder code
            foreach (var item in checktemp)
            {
                _history.Items.Add(item);
            }


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
                    //do stuff...
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
                    Teacher = item.Room.Teacher,
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

        /// <summary>
        /// A room
        /// </summary>
        public class Room
        {
            public RoomType Type { get; set; }
            public string Id { get; set; }
            public string Teacher { get; set; }
            public override string ToString()
            {
                return Id;
            }
        }
        /// <summary>
        /// The list of rooms
        /// </summary>
        public List<Room> rooms = new List<Room>();
        /// <summary>
        /// The list of people
        /// </summary>
        public List<Person> people = new List<Person>();
        /// <summary>
        /// The list of undisplayable records
        /// </summary>
        public List<Check> checks = new List<Check>();
        /// <summary>
        /// A list of displayable records used as backup for turning on or off filters. USE THIS INSTEAD OF EDITING THE GRIDVIEW ITEMS, then read off this (using <c>for each</c> or <c>for each ... .FindAll(...)</c> etc.) to display the filtered or unfiltered gridview items.
        /// </summary>
        public List<_check> checktemp = new List<_check>();

        private void _roomtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRooms();
        }

        private void _searchrooms_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_getusers.SelectedIndex < 1) return;
            //if the text is still "(Quick Add)" don't do anything (prevents misfiring and infinite loops)
            var l = _searchusers.Text.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries).Cast<string>().ToList();
            //output each item in the field as a string with no empties [literally 'let l be the textbox text but with its text split by the character ',' but remove empty strings, and cast it to an enumerable of 'string' and cast it to a list']
            l.Add(e.AddedItems[0].ToString());
            //add that new selection (had to use e because the text kept changing back before being read) [literally 'add to l "the first new thing you are talking about but a string"']
            _searchusers.Text = string.Join(", ", l.Select(o=>o.Trim()));
            //re-insert all items as a trimmed comma-seperated array of values [literally 'make text joined version of "l but everything is trimmed" by the string ", "']
            _getusers.SelectedIndex = 0;
            //go back to "(Quick Add)"
        }

        private void _searchusers_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ifsearchusers.IsChecked = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _searchusers.Text = "";
        }
    }
}
