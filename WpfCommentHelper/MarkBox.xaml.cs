using System.Windows.Controls;
using System.Windows.Input;

namespace WpfCommentHelper
{
    /// <summary>
    /// MarkBox.xaml 的交互逻辑
    /// </summary>
    public partial class MarkBox : UserControl
    {
        public MarkBox()
        {
            InitializeComponent();
        }
        public int Max { get; set; }
        public int Min { get; set; }
        public MarkBox(string title, string range, string score) : this()
        {
            TitleBox.Content = title;
            Tag = range;
            // score 形如 1,10
            string[] scores = range.Split(',');
            Min = int.Parse(scores[0]);
            Max = int.Parse(scores[1]);
            // 默认最高分
            ScoreBox.Text = score;
        }

        private void ScoreBox_KeyUp(object sender, KeyEventArgs e)
        {
            int score = int.Parse(ScoreBox.Text);
            switch (e.Key)
            {
                case Key.Up:
                case Key.Right:
                    if (score < Max) score += 1;
                    break;
                case Key.Down:
                case Key.Left:
                    if (score > Min) score -= 1;
                    break;
                case Key.Delete:
                    ScoreBox.Text = (string)Tag;
                    return;
            }
            ScoreBox.Text = score.ToString();
        }
    }
}
