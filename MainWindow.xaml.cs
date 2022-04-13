using DnsTest.Data;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
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
            InitializeComponent();
            urlBox.Text = RDZ.Tools.FileHelper.LoadStringFromFile("urlList.txt");
        }


       
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
            BlockingCollection<ItemVM> Cachelist = new BlockingCollection<ItemVM>(4);

            var addTask = new Task(() =>
            {
                foreach (var item in list)
                {
                    Cachelist.Add(item);
                }
                Cachelist.CompleteAdding();
            }, TaskCreationOptions.LongRunning);
            addTask.Start();

          

            Task.Run(async () =>
           {
               await Task.Delay(500);
               while (Cachelist.Count > 0|| Cachelist.IsAddingCompleted==false)
               {
                   var item = Cachelist.Take();

                   var eq = item == list[0];
                   Task.Run(() =>
                   {
                       var res = DnsTask.DoDnsTask(dnsServer, item.Url).Result;
                       item.IP = res.IP;
                       item.Time = res.MiliSecondTime;
                       item.IsFailed = res.IsFailed;
                   });
               }
           });

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
            Application.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }

}
