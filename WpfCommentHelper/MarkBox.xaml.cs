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

        /// <summary>
        /// 获取该打分项的分数
        /// </summary>
        public string Score
        {
            get => ScoreBox.Text;
            set => ScoreBox.Text = value;
        }
        /// <summary>
        /// 最高分数（同时也是默认值）
        /// </summary>
        public int Max { get; set; }
        /// <summary>
        /// 最低分数
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// 在分数栏被点选时，用方向键来调整分数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScoreBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!int.TryParse(ScoreBox.Text, out int score)) return;
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
                case Key.Escape:
                case Key.Delete:
                    string[] text = ((string)Tag).Split(',');
                    Score = text[text.Length - 1];
                    return;
            }
            ScoreBox.Text = score.ToString();
        }
    }
}
