using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfCommentHelper.CustomForms;

namespace WpfCommentHelper
{
    public partial class TaskBox : UserControl
    {
        public TaskBox Owner;

        public TaskBox()
        {
            InitializeComponent();
        }
        public TaskBox(string label, string score, string type, string desc = null) : this()
        {
            TitleBox.Content = label;
            Tag = score;
            Type = type;
            if (desc is null)
            {
                DescBox.Text = "";
                DescBox.Visibility = Visibility.Collapsed;
            }
            else
                DescBox.Text = Regex.Unescape(desc);
            switch (Type)
            {
                case "task":
                    Left = Environment.NewLine;
                    Right = "";
                    break;
                case "subtask":
                    Left = "";
                    Right = "";
                    break;
                case "group":
                    Left = "";
                    Right = ";";
                    break;
            }
        }
        /// <summary>
        /// 是否显示详细信息
        /// </summary>
        public static bool Verbose { get; set; } = false;
        /// <summary>
        /// 输出评语时，出现在文本左侧的内容（默认为换行符）
        /// </summary>
        public string Left { get; set; }
        /// <summary>
        /// 输出评语时，出现在文本左侧的内容（默认为空）
        /// </summary>
        public string Right { get; set; }
        /// <summary>
        /// TaskBox 的类型
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// 获取当前题目的分数
        /// </summary>
        public string Score
        {
            get
            {
                CalculateScore(this, null);
                return ScoreBox.Text;
            }
        }
        /// <summary>
        /// 获取该题目的评语
        /// </summary>
        public string Comment
        {
            get
            {
                if (!HasComment) return "";
                var sb = new StringBuilder();
                string comment;
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
                            comment = GetComment(c);
                            if (!string.IsNullOrEmpty(comment))
                                sb.AppendLine(comment);
                            break;
                        case RadioButton r:
                            comment = GetComment(r);
                            if (!string.IsNullOrEmpty(comment))
                                sb.AppendLine(comment);
                            break;
                        case MarkBox m:
                            comment = GetComment(m);
                            if (!string.IsNullOrEmpty(comment))
                                sb.AppendLine(comment);
                            break;
                    }
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 判断该题目是否拥有可显示的评语
        /// </summary>
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
                            if (!CanIgnore(c)) return true;
                            if (c.IsChecked.Value && Type == "group") return true;
                            break;
                        case RadioButton r:
                            if (!CanIgnore(r)) return true;
                            break;
                        case MarkBox m:
                            if (!CanIgnore(m)) return true;
                            break;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 计算当前题目的分数（会在任意子项目的分数发生变化时被调用）
        /// </summary>
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
                    // TaskBox 的分数为自己的基础分数加上所有子项的分数
                    case TaskBox b:
                        if (int.TryParse(b.Score, out value)) score += value;
                        break;
                    // CheckBox 只有在被选中时，所对应的分值才会产生意义
                    case CheckBox c:
                        score += GetScore(c);
                        break;
                    // 同上，RadioButton 只有在被选中时，所对应的分值才会产生意义
                    case RadioButton r:
                        score += GetScore(r);
                        break;
                    // 不管是否被选中，MarkBox 的分数总是有意义的
                    case MarkBox m:
                        score += GetScore(m);
                        break;
                }
            }
            ScoreBox.Text = score.ToString();
        }

        /// <summary>
        /// 为当前题目添加新的子项目
        /// </summary>
        public void InsertChild(Control elem, int index = -1)
        {
            switch (elem)
            {
                case CheckBox check:
                    check.Checked += CalculateScore;
                    check.Unchecked += CalculateScore;
                    check.ToolTip = check.Tag;
                    check.Click += ((MainWindow)Application.Current.MainWindow).UpdateComment;
                    AddContextMenu(check);
                    break;
                case RadioButton radio:
                    radio.Checked += CalculateScore;
                    radio.Unchecked += CalculateScore;
                    radio.ToolTip = radio.Tag;
                    radio.Click += ((MainWindow)Application.Current.MainWindow).UpdateComment;
                    AddContextMenu(radio);
                    break;
                case TaskBox box:
                    box.ScoreBox.TextChanged += CalculateScore;
                    box.Owner = this;
                    break;
                case MarkBox mark:
                    mark.ScoreBox.TextChanged += CalculateScore;
                    mark.ScoreSlider.ValueChanged += CalculateScore;
                    mark.TitleBox.Click += ((MainWindow)Application.Current.MainWindow).UpdateComment;
                    mark.ScoreSlider.ValueChanged += ((MainWindow)Application.Current.MainWindow).UpdateComment;
                    AddContextMenu(mark);
                    break;
            }
            if (index == -1)
                Container.Children.Add(elem);
            else
                Container.Children.Insert(index, elem);
            CalculateScore(this, null);
        }

        /// <summary>
        /// 为一个 Check，Radio，Mark 子项添加右键菜单
        /// </summary>
        private void AddContextMenu(Control control)
        {
            var menu = new ContextMenu();

            var edit = new MenuItem { Header = "编辑" };
            edit.Click += (sender2, e2) =>
            {
                if (EditForm.Edit(control))
                {
                    CalculateScore(control, null);
                    ((MainWindow)Application.Current.MainWindow).UpdateComment(control, null);
                }
            };
            menu.Items.Add(edit);

            var insertAbove = new MenuItem { Header = "上方添加..." };
            insertAbove.Click += (sender4, e4) =>
            {
                int index = this.Container.Children.IndexOf(control);
                var newItem = NewForm.Add(this);
                if (newItem is null)
                    return;
                this.InsertChild(newItem, index);
            };
            menu.Items.Add(insertAbove);

            var insertBelow = new MenuItem { Header = "下方添加..." };
            insertBelow.Click += (sender5, e5) =>
            {
                int index = this.Container.Children.IndexOf(control) + 1;
                var newItem = NewForm.Add(this);
                if (newItem is null)
                    return;
                this.InsertChild(newItem, index);
            };
            menu.Items.Add(insertBelow);

            var delete = new MenuItem { Header = "删除" };
            delete.Click += (sender3, e3) =>
            {
                this.Container.Children.Remove(control);
            };
            menu.Items.Add(delete);
            control.ContextMenu = menu;
        }

        /// <summary>
        /// 获取单个元件的批语
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetComment(Control element)
        {
            string comment = "";
            switch (element)
            {
                case CheckBox c:
                    if (CanIgnore(c)) return null;
                    if (Verbose)
                    {
                        comment += $"{c.Content.ToString().TrimEnd('*')}";
                        if (c.Tag != null)
                            comment += $" ({c.Tag})";
                        comment += ";";
                        return comment;
                    }
                    else
                        return $"{c.Content};";
                case RadioButton r:
                    if (CanIgnore(r)) return null;
                    if (Verbose)
                    {
                        comment += $"{r.Content.ToString().TrimEnd('*')}";
                        if (r.Tag != null)
                            comment += $" ({r.Tag})";
                        comment += ";";
                        return comment;
                    }
                    else
                        return $"{r.Content};";
                case MarkBox m:
                    if (CanIgnore(m)) return null;
                    if (Verbose)
                        return $"{m.Comment.TrimEnd('*')} ({m.Score});";
                    else
                        return $"{m.Comment};";
            }
            return null;
        }
        /// <summary>
        /// 获取单个元件的分数
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private int GetScore(Control element)
        {
            switch (element)
            {
                case CheckBox c:
                    if (c.IsChecked.Value)
                        if (c.Tag is null) return 0;
                        else if (int.TryParse(c.Tag.ToString(), out int score1))
                            return score1;
                    break;
                case RadioButton r:
                    if (r.IsChecked.Value)
                        if (r.Tag is null) return 0;
                        else if (int.TryParse(r.Tag.ToString(), out int score2))
                            return score2;
                    break;
                case MarkBox m:
                    if (int.TryParse(m.Score, out int score3))
                        return score3;
                    break;
            }
            return 0;
        }
        /// <summary>
        /// 判断某个选项栏是否被忽略（即没有被选中，或内容以“*”结尾）
        /// </summary>
        private bool CanIgnore(Control element)
        {
            switch (element)
            {
                case CheckBox c:
                    // 如果没有被选中，则可以忽略
                    if (!c.IsChecked.Value) return true;
                    // 如果以“*”结尾，而且不要求详细，则可以忽略
                    else if (((string)c.Content).EndsWith("*") && !Verbose) return true;
                    break;
                case RadioButton r:
                    if (!r.IsChecked.Value) return true;
                    else if (((string)r.Content).EndsWith("*") && !Verbose) return true;
                    break;
                case MarkBox m:
                    // 如果被选中，则不能忽略
                    if (m.TitleBox.IsChecked.Value) return false;
                    // 如果没有被选中，但要求详细，则不能忽略
                    else if (m.Score != m.Max.ToString() && Verbose) return false;
                    else return true;
            }
            return false;
        }

        /// <summary>
        /// 编辑
        /// </summary>
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditForm.Edit(this);

            e.Handled = true;
        }
        /// <summary>
        /// 删除
        /// </summary>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            // 这里用一个很笨拙的方法来实现删除功能
            // 如果父元素是一个 StackPanel，说明上级是一个 TaskBox
            // 否则如果是个 Grid，则认为当前的 Task 是根节点，不可删除
            if (parent is StackPanel p)
            {
                p.Children.Remove(this);
            }
            else
            {
                MessageBox.Show("当前为根节点，无法删除。");
            }
        }
        /// <summary>
        /// 添加
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var child = NewForm.Add(this);
            if (child is null)
                return;
            this.InsertChild(child);
        }
        /// <summary>
        /// 下方添加
        /// </summary>
        private void InsertBelow_Click(object sender, RoutedEventArgs e)
        {
            if (this.Owner is null)
                return;
            var child = NewForm.Add(this.Owner);
            if (child is null)
                return;
            int index = this.Owner.Container.Children.IndexOf(this) + 1;
            this.Owner.InsertChild(child, index);
        }
        /// <summary>
        /// 上方添加
        /// </summary>
        private void InsertAbove_Click(object sender, RoutedEventArgs e)
        {
            if (this.Owner is null)
                return;
            var child = NewForm.Add(this.Owner);
            if (child is null)
                return;
            int index = this.Owner.Container.Children.IndexOf(this);
            this.Owner.InsertChild(child, index);
        }
        /// <summary>
        /// 获取元件的父节点
        /// </summary>
        public static TaskBox GetParentTaskBox(DependencyObject c)
        {
            int depth = 4;
            DependencyObject parent = c;
            while (depth-- > 0)
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent is TaskBox box)
                    return box;
            }
            return null;
        }
    }
}
