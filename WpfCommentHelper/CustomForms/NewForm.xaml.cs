using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;

namespace WpfCommentHelper.CustomForms
{
    public partial class NewForm : Window
    {
        public NewForm()
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
        }

        private TaskBox container;
        public Control NewItem;

        public NewForm(TaskBox container)
            : this()
        {
            this.container = container;

            this.TypeSelection.SelectedIndex = 0;
            this.Title.Focus();
            this.Title.SelectAll();
        }

        public static Control Add(TaskBox container)
        {
            var form = new NewForm(container);
            // 更新一下界面显示
            form.TypeSelection_SelectionChanged(form.TypeSelection, null);
            form.ShowDialog();
            if (form.DialogResult.Value)
                return form.NewItem;

            return null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (container is null)
            {
                DialogResult = false;
                Close();
                return;
            }
            var type = ((string)((ComboBoxItem)TypeSelection.SelectedItem).Content).ToLower();
            switch (type)
            {
                case "task":
                case "subtask":
                case "group":
                    TaskBox box = new TaskBox(Title.Text, Score.Text, type, Desc.Text);
                    box.FontSize = container.FontSize > 15 ? container.FontSize - 1 : 15;
                    NewItem = box;
                    break;
                case "check":
                    CheckBox check = new CheckBox
                    {
                        Content = Title.Text,
                        Tag = Score.Text,
                        FontWeight = Emphasis.IsChecked.Value ? FontWeights.Bold : FontWeights.Normal
                    };
                    NewItem = check;
                    break;
                case "radio":
                    CheckBox radio = new CheckBox
                    {
                        Content = Title.Text,
                        Tag = Score.Text,
                        FontWeight = Emphasis.IsChecked.Value ? FontWeights.Bold : FontWeights.Normal
                    };
                    NewItem = radio;
                    break;
                case "mark":
                    MarkBox mark = new MarkBox(Title.Text, Range.Text);
                    mark.FontWeight = Emphasis.IsChecked.Value ? FontWeights.Bold : FontWeights.Normal;
                    NewItem = mark;
                    break;
            }
            DialogResult = true;
            Close();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Add_Click(this, null);
            }
        }

        private void TypeSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (TypeSelection?.SelectedIndex ?? 0)
            {
                case 0:
                case 1:
                case 2:
                    DescPanel.Visibility = Visibility.Visible;
                    ScorePanel.Visibility = Visibility.Visible;
                    RangePanel.Visibility = Visibility.Collapsed;
                    Emphasis.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                case 4:
                    DescPanel.Visibility = Visibility.Collapsed;
                    ScorePanel.Visibility = Visibility.Visible;
                    RangePanel.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    DescPanel.Visibility = Visibility.Collapsed;
                    ScorePanel.Visibility = Visibility.Collapsed;
                    RangePanel.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
