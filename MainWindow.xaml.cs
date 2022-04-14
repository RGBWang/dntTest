using DnsTest.Data;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DnsTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            InitializeComponent();
            urlBox.Text = RDZ.Tools.FileHelper.LoadStringFromFile("urlList.txt");
        }



        static int GlobalControl = 0;

        private  void Button_Click(object sender, RoutedEventArgs e)
        {
            var list = urlBox.Text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => new ItemVM()
                        {
                            Url = s,
                        }).ToList();

            string dnsServer = dnsServerBox.Text;
            listBox.ItemsSource = list;
            listBox.Visibility = Visibility.Visible;

            GlobalControl++;



            int localControl = GlobalControl;
            BlockingCollection<ItemVM> Cachelist = new BlockingCollection<ItemVM>(16);

            var addTask = new Task(() =>
            {
                foreach (var item in list)
                {
                    if(localControl!= GlobalControl)
                    {
                        break;
                    }
                    Cachelist.Add(item);

                    Task.Run(() =>
                    {
                        var res = DnsTask.DoDnsTask(dnsServer, item.Url).Result;
                        Cachelist.Take();
                        item.IP = res.IP;
                        item.Time = res.MiliSecondTime;
                        item.IsFailed = res.IsFailed;
                    });
                }
         
                Cachelist.CompleteAdding();
            }, TaskCreationOptions.LongRunning);
            addTask.Start();

        }
    }

    public class ItemVM : INotifyPropertyChanged
    {
        public string Url { get; set; }

        string _IP;
        public string IP
        {
            get => _IP; set
            {
                _IP = value;
                RaisePropertyChanged(nameof(IP));
            }
        }

        double _Time;
        public double Time
        {
            get => _Time; set
            {
                _Time = value;
                RaisePropertyChanged(nameof(Time));
            }
        }

        bool isFailed;

        public bool IsFailed
        {
            get => isFailed; set
            {
                isFailed = value;
                RaisePropertyChanged(nameof(IsFailed));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }

}
