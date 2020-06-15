using System;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using WpfCommentHelper.CustomForms;

namespace WpfCommentHelper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 默认的程序标题
        /// </summary>
        public static readonly string ProgramTitle = "作业批改助手";
        public static readonly string LineBreak = Environment.NewLine;

        private string OutputMarkdownFileName = "";

        public MainWindow()
        {
            InitializeComponent();

            ReadXml("template.xml");

            Title = ProgramTitle;
        }

        /// <summary>
        /// 如果已经打开了 XML 文档，则保存文件路径，用于重新读取（重置）
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// 读取 XML 文件，生成对应的批改界面
        /// </summary>
        /// <param name="filename"></param>
        ///
        public void ReadXml(string filename)
        {
            XDocument doc = XDocument.Load(filename);
            // root 为一个 task，一定没有分数
            XElement root = doc.Root;

            TaskBox rootTask = new TaskBox(root.Attribute("title").Value,
                root.Attribute("score")?.Value,
                root.Name.ToString(),
                root.Attribute("desc")?.Value);
            rootTask.FontSize = 18;
            // 根节点不能并列添加，不能删除
            for (int i = 2; i < rootTask.ContextMenu.Items.Count; i++)
            {
                ((MenuItem)rootTask.ContextMenu.Items[i]).IsEnabled = false;
            }

            AddChildrenFromXElement(rootTask, root);

            CommentPanel.Children.Clear();
            CommentPanel.Children.Add(rootTask);
            UpdateComment(this, null);

            FileName = filename;
            // 修改程序的标题
            Title = $"{ProgramTitle} - {Path.GetFileNameWithoutExtension(filename)}";

        }

        /// <summary>
        /// 将现有批改界面保存为 XML 文件，包含当前的分数
        /// </summary>
        /// <param name="filename"></param>
        /// 
        public void WriteXml(string filename)
        {
            XDocument doc = new XDocument();
            TaskBox rootTask = CommentPanel.Children[0] as TaskBox;
            XElement root = new XElement("task");
            AddXElementsFromTaskBox(root, rootTask);
            doc.Add(root);
            doc.Save(filename);
        }

        /// <summary>
        /// 更新评语内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateComment(object sender, RoutedEventArgs e)
        {
            if (CommentPanel.Children.Count == 0) return;
            TaskBox t = (TaskBox)CommentPanel.Children[0];
            // 去掉开头结尾的空白（尤其是换行符）
            string str = t.Comment.Trim();
            // 将评语结尾的分号改为句号
            if (str.EndsWith(";"))
                str = str.TrimEnd(';') + ".";
            // 如果显示详细信息，则在评语开头添加分数
            if (TaskBox.Verbose)
                str = $"Overall Score [{t.Score}]{LineBreak}{str}";
            CommentBox.Text = str;

            // 在标题栏也添加一个分数信息
            if (!Regex.IsMatch(Title, @"\[\d+\]$"))
                Title += $" [{t.Score}]";
            else
                Title = Regex.Replace(Title, @"\[\d+\]$", $"[{t.Score}]");
        }

        /// <summary>
        /// 将界面中的 XElement 转为对应 TaskBox
        /// </summary>
        /// <param name="task"></param>
        /// <param name="root"></param>
        private void AddChildrenFromXElement(TaskBox task, XElement root)
        {
            if (root.HasElements)
            {
                foreach (var elem in root.Elements())
                {
                    string type = elem.Name.ToString();
                    string title = elem.Attribute("title")?.Value;
                    string score = elem.Attribute("score")?.Value;
                    string desc = elem.Attribute("desc")?.Value;
                    string check = elem.Attribute("ischecked")?.Value;
                    bool emphasis = elem.Attribute("emphasis")?.Value == "True";
                    switch (elem.Name.ToString())
                    {
                        // 一个大标题，标题前有空行
                        case "task":
                        // 一个小标题，标题前没有空行
                        case "subtask":
                        // 一个批语分组，标题前没有空行，标题后有分号
                        case "group":
                            TaskBox t = new TaskBox(title, score, type, desc);
                            t.FontSize = task.FontSize > 15 ? task.FontSize - 1 : 15;
                            AddChildrenFromXElement(t, elem);
                            task.InsertChild(t);
                            break;
                        case "check":
                            CheckBox c = new CheckBox
                            {
                                Content = title,
                                Tag = score
                            };
                            c.FontWeight = emphasis ? FontWeights.Bold : FontWeights.Normal;
                            if (check != null)
                                c.IsChecked = bool.Parse(check);
                            task.InsertChild(c);
                            break;
                        case "radio":
                            RadioButton r = new RadioButton
                            {
                                Content = title,
                                Tag = score
                            };
                            r.FontWeight = emphasis ? FontWeights.Bold : FontWeights.Normal;
                            if (check != null)
                                r.IsChecked = bool.Parse(check);
                            task.InsertChild(r);
                            break;
                        case "mark":
                            MarkBox m = new MarkBox(title, elem.Attribute("range").Value, score);
                            m.FontWeight = emphasis ? FontWeights.Bold : FontWeights.Normal;
                            if (check != null)
                                m.TitleBox.IsChecked = bool.Parse(check);
                            task.InsertChild(m);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 将界面中的 TaskBox 转为对应 XElement
        /// </summary>
        /// <param name="root"></param>
        /// <param name="task"></param>
        private void AddXElementsFromTaskBox(XElement root, TaskBox task)
        {
            // 当前结点的其他信息
            if (task.TitleBox.Content != null)
                root.SetAttributeValue("title", task.TitleBox.Content);
            if (task.Tag != null)
                root.SetAttributeValue("score", task.Tag);
            if (!string.IsNullOrEmpty(task.DescBox.Text))
                root.SetAttributeValue("desc", task.DescBox.Text);
            foreach (var child in task.Container.Children)
            {
                XElement xelem;
                switch (child)
                {
                    case TaskBox t:
                        xelem = new XElement(t.Type);
                        AddXElementsFromTaskBox(xelem, t);
                        root.Add(xelem);
                        break;
                    case CheckBox c:
                        xelem = new XElement("check");
                        xelem.SetAttributeValue("title", c.Content);
                        xelem.SetAttributeValue("score", c.Tag);
                        if (c.IsChecked.Value)
                            xelem.SetAttributeValue("ischecked", true);
                        root.Add(xelem);
                        break;
                    case RadioButton r:
                        xelem = new XElement("radio");
                        xelem.SetAttributeValue("title", r.Content);
                        xelem.SetAttributeValue("score", r.Tag);
                        if (r.IsChecked.Value)
                            xelem.SetAttributeValue("ischecked", true);
                        root.Add(xelem);
                        break;
                    case MarkBox m:
                        xelem = new XElement("mark");
                        xelem.SetAttributeValue("title", m.TitleBox.Content);
                        xelem.SetAttributeValue("range", m.Tag);
                        xelem.SetAttributeValue("score", m.ScoreBox.Text);
                        if (m.TitleBox.IsChecked.Value)
                            xelem.SetAttributeValue("ischecked", true);
                        root.Add(xelem);
                        break;
                }
            }
        }

        /// <summary>
        /// 获取完整的 Markdown 格式的批语信息
        /// </summary>
        /// <returns></returns>
        private (string comment, string score) GetFullComments()
        {
            bool originVerbose = TaskBox.Verbose;
            TaskBox t = (TaskBox)CommentPanel.Children[0];

            TaskBox.Verbose = false;
            string simple = t.Comment.Trim();
            if (simple.EndsWith(";")) simple = simple.TrimEnd(';') + ".";
            simple = string.Join(LineBreak,
                from line in simple.Split(new string[] { LineBreak }, StringSplitOptions.None)
                select $"> {line}"
                );

            TaskBox.Verbose = true;
            string detail = t.Comment.Trim();
            if (detail.EndsWith(";")) detail = detail.TrimEnd(';') + ".";
            detail = $"Overall Score [{t.Score}]{LineBreak}{detail}";
            detail = $"```{LineBreak}{detail}{LineBreak}```";

            TaskBox.Verbose = originVerbose;

            return (simple + LineBreak + LineBreak + detail, t.Score);
        }

        #region 界面事件

        /// <summary>
        /// 打开 XML 文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog open = new Forms.OpenFileDialog();
            open.Filter = "Comment files (*.xml)|*.xml";
            if (open.ShowDialog() == Forms.DialogResult.OK)
            {
                ReadXml(open.FileName);
                // 打开了新的 XML 文件，认为需要重新设定 Markdown 文件的路径
                OutputMarkdownFileName = "";
            }
        }

        /// <summary>
        /// 保存当前的批改为 XML 文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Forms.SaveFileDialog save = new Forms.SaveFileDialog();
            //save.FileName = FileName;
            save.InitialDirectory = Path.GetDirectoryName(FileName);
            save.FileName = Path.GetFileNameWithoutExtension(FileName);
            save.Filter = "Comment files (*.xml)|*.xml";
            if (save.ShowDialog() == Forms.DialogResult.OK)
            {
                //FileName = save.FileName;
                WriteXml(save.FileName);
            }
        }

        /// <summary>
        /// 将当前的作业另存为 XML 文件
        /// </summary>
        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            Forms.SaveFileDialog save = new Forms.SaveFileDialog();
            //save.FileName = FileName;
            save.InitialDirectory = Path.GetDirectoryName(FileName);
            save.FileName = Path.GetFileNameWithoutExtension(FileName);
            save.Filter = "Comment files (*.xml)|*.xml";
            if (save.ShowDialog() == Forms.DialogResult.OK)
            {
                //FileName = save.FileName;
                WriteXml(save.FileName);
            }
        }

        /// <summary>
        /// 新建一个作业
        /// </summary>
        private void New_Click(object sender, RoutedEventArgs e)
        {
            var rootTask = new TaskBox("Assignment", "", "task", "this is a new assignment");
            rootTask.FontSize = 18;
            // 根节点不能并列添加，不能删除
            for (int i = 2; i < rootTask.ContextMenu.Items.Count; i++)
            {
                ((MenuItem)rootTask.ContextMenu.Items[i]).IsEnabled = false;
            }

            CommentPanel.Children.Clear();
            CommentPanel.Children.Add(rootTask);
            UpdateComment(this, null);

            FileName = "";
            // 修改程序的标题
            Title = ProgramTitle;
        }

        /// <summary>
        /// 快速将批语导出为 Markdown 语法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var result = GetFullComments();

            Clipboard.SetText(result.comment);
        }

        /// <summary>
        /// 重置左侧所有选项（其实是重新读取对应 XML 文档）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FileName)) return;
            // 清空并重置
            ReadXml(FileName);
            // 回到顶部
            CommentScroll.ScrollToHome();
        }

        /// <summary>
        /// 是否显示详尽版批语
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Verbose_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            string content = b.Content as string;
            if (content == "详细")
            {
                TaskBox.Verbose = false;
                b.Content = "简略";
            }
            else if (content == "简略")
            {
                TaskBox.Verbose = true;
                b.Content = "详细";
            }
            UpdateComment(this, null);
        }

        /// <summary>
        /// 处理拖入的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (Path.GetExtension(filePaths[0]) == ".xml")
                {
                    ReadXml(filePaths[0]);
                }
            }
        }

        /// <summary>
        /// 拖入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Link;
            else
                e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// 导出批语到 Markdown 文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Output_Click(object sender, RoutedEventArgs e)
        {
            // 如果没有 Markdown 文件的路径
            if (string.IsNullOrEmpty(OutputMarkdownFileName))
            {
                var save = new Forms.SaveFileDialog
                {
                    FileName = "score.md",
                    Filter = "文本文件|*.md"
                };
                if (save.ShowDialog() != Forms.DialogResult.OK)
                {
                    return;
                }
                OutputMarkdownFileName = save.FileName;
            }
            // 到此，应该是有 Markdown 文件路径的
            string studentName = InputBox.Input("请输入学生姓名：", "提示");
            if (string.IsNullOrWhiteSpace(studentName))
                return;
            (string comment, string score) = GetFullComments();
            string result = $"## {studentName} {score}" + LineBreak + LineBreak + comment + LineBreak;

            using (var writer = new StreamWriter(OutputMarkdownFileName, true, Encoding.UTF8))
            {
                writer.WriteLine(result);
            }

            // 导出之后顺便清空评分
            Cut_Click(this, null);
        }

        /// <summary>
        /// 点击文件按钮，展开菜单
        /// </summary>
        private void File_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.IsOpen = true;
        }

        #endregion

    }
}
