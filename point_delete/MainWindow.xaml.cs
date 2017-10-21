using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace point_delete
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "文本文档|*.txt";
            openFile.Multiselect = false;
            if (openFile.ShowDialog() != null)
            {
                InputPath.Text = openFile.FileName;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = "result";
            saveFile.DefaultExt = ".txt";
            saveFile.Filter = "文本文档(.txt)|*.txt";
            if (saveFile.ShowDialog() != null)
            {
                OutputPath.Text = saveFile.FileName;
            }
        }

        private async void deal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = InputPath.Text;
                string output = OutputPath.Text;
                double size = Convert.ToDouble(size_inut.Text);
                Task task = Task.Run(() => {
                    compute deal = new compute();
                    deal.Read(input);
                    deal.Deal(size);
                    deal.Output(output);
                });
                await task;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"错误");
            }
        }

        private void MenuItem1_Checked(object sender, RoutedEventArgs e)
        {
            menu2.IsChecked = false; menu3.IsChecked = false;
            Point start=new Point(0,0);
            Point end = new Point(0, 1);
            grid1.Background = new LinearGradientBrush(Color.FromRgb(255,255,255),Color.FromRgb(160,160,160),start,end);
        }

        private void MenuItem2_Checked(object sender, RoutedEventArgs e)
        {
            menu1.IsChecked = false; menu3.IsChecked = false;
            Point start = new Point(0, 0);
            Point end = new Point(0, 1);
            grid1.Background = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(255, 88, 88), start, end);
        }

        private void MenuItem3_Checked(object sender, RoutedEventArgs e)
        {
            menu2.IsChecked = false; menu1.IsChecked = false;
            Point start = new Point(0, 0);
            Point end = new Point(0, 1);
            grid1.Background = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(255, 200, 80), start, end);
        }

        private void help(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("你自己摸索着用吧，这是一个没用的对话框。","错误");
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("你自己摸索着用吧，这是一个没用的对话框", "错误");
        }
    }

    class compute
    {
        //sourcePoint为具有所有原始数据的点，point为只具有x,y,z坐标的表示点的数组
        SortedList<int, string> sourcePoint = new SortedList<int, string>();
        int i = 0;
        string line;
        List<int> resultPoint = new List<int>();
        double[,] point = new double[1000000, 2];
        public void Read(string path)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            while ((line = sr.ReadLine()) != null)
            {
                sourcePoint.Add(i, line);
                i++;
            }
            sourcePoint.Remove(0);
            try
            {
                foreach (KeyValuePair<int, string> element in sourcePoint)
                {
                    int a = element.Key;
                    string b = element.Value;
                    b = Regex.Replace(b.Trim(), "\\s+", " ");
                    var temp = b.Split(' ');
                    point[a, 0] = Convert.ToSingle(temp[0]);
                    point[a, 1] = Convert.ToSingle(temp[1]);
                }
            }
            catch (FormatException e)
            {
                MessageBox.Show(e.Message,"错误");
            }
            finally
            {
                sr.Close();
            }

        }
        public void Deal(double size)
        {
            double xMax = point[1, 0], yMax = point[1, 1], xMin = point[1, 0], yMin = point[1, 0];
            for (int j = 1; j < i; j++)
            {
                if (point[j, 0] > xMax) xMax = point[j, 0];
                else if (point[j, 0] < xMin) xMin = point[j, 0];
                if (point[j, 1] > yMax) yMax = point[j, 1];
                else if (point[j, 1] < yMin) yMin = point[j, 1];
            }
            //A，B，C为空间立方体的三边长，size为取样区间,numABC为三边小立方体数量
            double A = xMax - xMin, B = yMax - yMin;
            int numA, numB;
            numA = (int)(A / size) + 1;
            numB = (int)(B / size) + 1;
            List<int>[,] points = new List<int>[numA, numB];
            for (int aa = 0; aa < numA; aa++)
            {
                for (int bb = 0; bb < numB; bb++)
                {
                    points[aa, bb] = new List<int>();
                }
            }
            for (int j = 1; j < i; j++)
            {
                int a, b;
                a = (int)((point[j, 0] - xMin) / size);
                b = (int)((point[j, 1] - yMin) / size);
                points[a, b].Add(j);
            }
            for (int a = 0; a < numA; a++)
            {
                for (int b = 0; b < numB; b++)
                {
                    int num = points[a, b].Count();
                    if (num == 0) continue;
                    double xp = 0, yp = 0;
                    foreach (int element in points[a, b])
                    {
                        xp = xp + point[element, 0];
                        yp = yp + point[element, 1];
                    }
                    double D = int.MaxValue; int min = 0;
                    foreach (int element in points[a, b])
                    {
                        double d = (point[element, 0] - xp / num) * (point[element, 0] - xp / num) + (point[element, 1] - yp / num) * (point[element, 1] - yp / num);
                        if (D > d)
                        {
                            D = d; min = element;
                        }
                    }
                    resultPoint.Add(min);
                }
            }
        }
        public void Output(string resultPath)
        {

            FileStream fs = new FileStream(resultPath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                foreach (int element in resultPoint)
                {
                    sw.WriteLine(sourcePoint[element]);
                }
                MessageBox.Show(string.Format("处理完成！原始点数目为{0}，处理后点数为{1}",i, resultPoint.Count), "提示");

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "错误");
            }
            finally
            {
                sw.Flush(); sw.Close(); fs.Close();
            }
        }
    }
}
