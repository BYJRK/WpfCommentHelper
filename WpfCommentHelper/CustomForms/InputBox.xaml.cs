using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Forms = System.Windows.Forms;

namespace WpfCommentHelper.CustomForms
{
    /// <summary>
    /// InputBox.xaml 的交互逻辑
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();

            Title = "";
            Message.Text = "";
            UserInput.Text = "";
            UserInput.Focus();
            UserInput.SelectAll();

            this.Owner = Application.Current.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public static string Input(string message, string title = "")
        {
            var box = new InputBox();
            box.Title = title;
            box.Message.Text = message;
            box.UserInput.Text = "";
            box.ShowDialog();
            if (box.DialogResult.Value)
                return box.UserInput.Text;
            else
                return "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch ((string)btn.Content)
            {
                case "确定":
                    DialogResult = true;
                    break;
                case "取消":
                    DialogResult = false;
                    break;
            }
            this.Close();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
                this.Close();
            }
        }
    }
}
