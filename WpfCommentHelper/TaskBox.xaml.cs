using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfCommentHelper
{
    public partial class TaskBox : UserControl
    {
        public TaskBox()
        {
            InitializeComponent();
        }

        public static bool Verbose { get; set; } = false;
        public string Left { get; set; } = Environment.NewLine;
        public string Right { get; set; } = string.Empty;

        public TaskBox(string label, string score, string desc = null, string left = null, string right = null) : this()
        {
            TitleBox.Content = label;
            Tag = score;
            if (desc is null)
                DescBox.Visibility = Visibility.Collapsed;
            else
                DescBox.Text = Regex.Unescape(desc);
            if (!(left is null))
                Left = left;
            if (!(right is null))
                Right = right;
            CalculateScore(this, null);
        }

        public string Comment
        {
            get
            {
                if (!HasComment) return "";
                var sb = new StringBuilder();
                foreach (var item in Container.Children)
                {
                    switch (item)
                    {
                        case TaskBox t:
                            if (t.HasComment)
                            {
                                sb.Append($"{t.Left}{t.TitleBox.Content}{t.Right}");
                                if (Verbose)
                                    sb.Append($" [{t.ScoreBox.Text}]");
                                sb.AppendLine();
                                sb.Append(t.Comment);
                            }
                            break;
                        case CheckBox c:
                            if (Verbose && c.IsChecked.Value)
                            {
                                sb.Append(c.Content.ToString().TrimEnd('*'));
                                if (!(c.Tag is null))
                                    if (!((string)c.Tag).StartsWith("-"))
                                        sb.Append($" (+{c.Tag})");
                                    else
                                        sb.Append($" ({c.Tag})");
                                sb.AppendLine(";");
                            }
                            else if (!IsIgnored(c))
                                sb.AppendLine($"{c.Content};");
                            break;
                        case RadioButton r:
                            if (Verbose && r.IsChecked.Value)
                            {
                                sb.Append(r.Content.ToString().TrimEnd('*'));
                                if (!(r.Tag is null))
                                    if (!((string)r.Tag).StartsWith("-"))
                                        sb.Append($" (+{r.Tag})");
                                    else
                                        sb.Append($" ({r.Tag})");
                                sb.AppendLine(";");
                            }
                            else if (!IsIgnored(r))
                                sb.AppendLine($"{r.Content};");
                            break;
                        case MarkBox m:
                            if (Verbose && m.TitleBox.IsChecked.Value)
                            {
                                sb.Append(m.TitleBox.Content.ToString().TrimEnd('*'));
                                if (!m.ScoreBox.Text.StartsWith("-"))
                                    sb.Append($" (+{m.ScoreBox.Text})");
                                else
                                    sb.Append($" ({m.ScoreBox.Text})");
                                sb.AppendLine(";");
                            }
                            else if (!IsIgnored(m.TitleBox))
                                sb.AppendLine($"{m.TitleBox.Content};");
                            break;
                    }
                }
                return sb.ToString();
            }
        }
        public bool HasComment
        {
            get
            {
                foreach (var item in Container.Children)
                {
                    switch (item)
                    {
                        case TaskBox t:
                            if (t.HasComment) return true;
                            break;
                        case CheckBox c:
                            if (c.IsChecked.Value) return true;
                            break;
                        case RadioButton r:
                            if (!IsIgnored(r)) return true;
                            break;
                        case MarkBox m:
                            if (!IsIgnored(m.TitleBox)) return true;
                            break;
                    }
                }
                return false;
            }
        }

        public void CalculateScore(object sender, RoutedEventArgs e)
        {
            int score = 0;
            if (int.TryParse((string)Tag, out int tagValue))
                score += tagValue;
            foreach (Control item in Container.Children)
            {
                int value = 0;
                switch (item)
                {
                    case CheckBox c:
                        if (!c.IsChecked.Value) continue;
                        else if (int.TryParse((string)c.Tag, out value)) score += value;
                        break;
                    case RadioButton r:
                        if (!r.IsChecked.Value) continue;
                        else if (int.TryParse((string)r.Tag, out value)) score += value;
                        break;
                    case TaskBox b:
                        if (int.TryParse(b.ScoreBox.Text, out value)) score += value;
                        break;
                    case MarkBox m:
                        if (int.TryParse(m.ScoreBox.Text, out value)) score += value;
                        break;
                }
            }
            ScoreBox.Text = score.ToString();
        }
        public void AddChildren(params Control[] children)
        {
            foreach (var elem in children)
            {
                switch (elem)
                {
                    case CheckBox c:
                        c.Checked += CalculateScore;
                        c.Unchecked += CalculateScore;
                        c.ToolTip = c.Tag;
                        break;
                    case RadioButton r:
                        r.Checked += CalculateScore;
                        r.Unchecked += CalculateScore;
                        r.ToolTip = r.Tag;
                        break;
                    case TaskBox b:
                        b.ScoreBox.TextChanged += CalculateScore;
                        break;
                    case MarkBox m:
                        m.ScoreBox.TextChanged += CalculateScore;
                        break;
                }
                Container.Children.Add(elem);
            }
            CalculateScore(this, null);
        }
        public bool IsIgnored(ToggleButton button)
        {
            string comment = (string)button.Content;
            if (button.IsChecked.Value)
                if (comment.EndsWith("*") && !Verbose) return true;
                else return false;
            else return true;
        }
    }
}
