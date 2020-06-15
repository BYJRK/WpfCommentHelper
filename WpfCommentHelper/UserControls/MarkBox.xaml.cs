using System;
using System.Windows.Controls;

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
            Comment = title;
            ParseRange(range);
            // 默认最高分
            if (string.IsNullOrEmpty(score))
                Score = Max.ToString();
            else
                Score = score;
        }

        public MarkBox(string title, string range) : this()
        {
            Comment = title;
            ParseRange(range);
            Score = Max.ToString();
        }

        public void ParseRange(string range)
        {
            string[] scores = range.Split(',');
            if (scores.Length == 3)
            {
                Min = int.Parse(scores[0]);
                Step = int.Parse(scores[1]);
                Max = int.Parse(scores[2]);
            }
            else if (scores.Length == 2)
            {
                Min = int.Parse(scores[0]);
                Max = int.Parse(scores[1]);
            }
            else if (scores.Length == 1)
            {
                var temp = int.Parse(range);
                Min = Math.Min(temp, 0);
                Max = Math.Max(temp, 0);
            }
            else
                throw new Exception("MarkBox range 参数有误。");
            Tag = $"{Min},{Step},{Max}";
            ToolTip = Tag;
            // 分数拖动条
            ScoreSlider.Minimum = Min;
            ScoreSlider.Maximum = Max;
            double[] sliderRange = new double[Max - Min + 1];
            for (int i = 0; i < Max - Min + 1; i += Step)
                sliderRange[i] = Min + i;
            ScoreSlider.Ticks = new System.Windows.Media.DoubleCollection(sliderRange);
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
        /// 获取该打分项的批语
        /// </summary>
        public string Comment
        {
            get => TitleBox.Content.ToString();
            set => TitleBox.Content = value;
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
        /// 递进步数
        /// </summary>
        public int Step { get; set; } = 1;
    }
}
