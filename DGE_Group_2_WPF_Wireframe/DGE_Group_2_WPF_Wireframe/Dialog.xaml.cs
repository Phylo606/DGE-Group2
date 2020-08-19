using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DGE_Group_2_WPF_Wireframe
{

    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public MessageBoxResult MessageBoxResult = MessageBoxResult.Cancel;
        public int MessageBoxCode { get; set; } = 0;
        public void NewButton(string txt, MessageBoxResult messageBoxResult)
        {
            var d = new Button() { MinWidth = 75, Height = 25, Content=txt, Margin = new Thickness(5) };
            d.Tag = messageBoxResult;
            d.Click += D_Click;
            btns.Children.Add(d);
        }

        public MessageBoxResult ShowMessageBox()
        {
            ShowDialog();
            return MessageBoxResult;
        }

        private void D_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = (MessageBoxResult) (sender as Button).Tag;
            Close();
        }

        public void NewButton(string txt, RoutedEventHandler eventHandler, bool thenClose = true)
        {
            var d = new Button() { MinWidth = 75, Height = 25, Content = txt, Margin = new Thickness(5) };
           if (thenClose)  d.Click += D_Click1;
            d.Click += eventHandler;
            d.Tag = this.Tag;
            btns.Children.Add(d);
        }
        public void NewButton(string txt, int outputCode)
        {
            var d = new Button() { MinWidth = 75, Height = 25, Content = txt, Margin = new Thickness(5) };
            d.Click += D_Click2;
            d.Click += D_Click1;
            d.Tag = outputCode;
            btns.Children.Add(d);
        }

        private void D_Click2(object sender, RoutedEventArgs e)
        {
          MessageBoxCode = (int) ((Button)sender).Tag;
        }

        private void D_Click1(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public System.Drawing.Icon Icon { get; set; }
        public Dialog(string title, string heading, string text, System.Drawing.Icon image)
        {
            InitializeComponent();
            Title = title;
            t.Text = heading;
            c.Text = text;
            Icon = image;
            img.Source = ToImageSource(image);
            if (text == "") c.Visibility = Visibility.Collapsed;

        }

        public ImageSource ToImageSource(Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());



            return wpfBitmap;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
          //  System.Media.SystemSounds.Exclamation.Play();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Icon == System.Drawing.SystemIcons.Exclamation)
                System.Media.SystemSounds.Exclamation.Play();
            if (Icon == System.Drawing.SystemIcons.Error)
                System.Media.SystemSounds.Hand.Play();
        }
    }
}
