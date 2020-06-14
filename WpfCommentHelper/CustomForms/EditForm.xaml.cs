using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfCommentHelper.CustomForms
{
    public partial class EditForm : Window
    {
        public EditForm()
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
        }

        private Control EditedControl;

        public EditForm(Control control)
            : this()
        {
            EditedControl = control;
            switch (control)
            {
                case TaskBox box:
                    RangePanel.Visibility = Visibility.Collapsed;
                    Title.Text = box.TitleBox.Content as string;
                    Desc.Text = box.DescBox.Text;
                    Score.Text = box.Tag as string;
                    break;
                case CheckBox check:
                    RangePanel.Visibility = Visibility.Collapsed;
                    DescPanel.Visibility = Visibility.Collapsed;
                    Title.Text = check.Content as string;
                    Score.Text = check.Tag as string;
                    break;
                case RadioButton radio:
                    RangePanel.Visibility = Visibility.Collapsed;
                    DescPanel.Visibility = Visibility.Collapsed;
                    Title.Text = radio.Content as string;
                    Score.Text = radio.Tag as string;
                    break;
                case MarkBox mark:
                    DescPanel.Visibility = Visibility.Collapsed;
                    ScorePanel.Visibility = Visibility.Collapsed;
                    Title.Text = mark.TitleBox.Content as string;
                    Range.Text = mark.Tag as string;
                    break;
            }
        }

        public static bool Edit(Control control)
        {
            var form = new EditForm(control);
            form.ShowDialog();

            return form.DialogResult.Value;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            if (EditedControl is null)
            {
                DialogResult = false;
                Close();
                return;
            }
            switch (EditedControl)
            {
                case TaskBox box:
                    box.TitleBox.Content = Title.Text;
                    box.DescBox.Text = Desc.Text;
                    box.Tag = Score.Text;
                    box.CalculateScore(box, null);
                    break;
                case CheckBox check:
                    check.Content = Title.Text;
                    check.Tag = Score.Text;
                    break;
                case RadioButton radio:
                    radio.Content = Title.Text;
                    radio.Tag = Score.Text;
                    break;
                case MarkBox mark:
                    mark.TitleBox.Content = Title.Text;
                    mark.ParseRange(Range.Text);
                    int score = int.Parse(mark.ScoreBox.Text);
                    if (score > mark.Max) score = mark.Max;
                    else if (score < mark.Min) score = mark.Min;
                    mark.ScoreBox.Text = score.ToString();
                    break;
            }
            DialogResult = true;
            Close();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Modify_Click(this, null);
            }
        }
    }
}
