using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Numerics;


namespace SnapshotAndAirdrop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            this.assetType.SelectedIndex = 0;

            Address address = new Address();
        }

        private void assetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.assetId.Items.Clear();
            if (this.assetType.SelectedIndex == 0)
            {//全局资产

                foreach (string key in Config.assets.Keys)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = key;
                    this.assetId.Items.Add(comboBoxItem);
                }
            }
            else if (this.assetType.SelectedIndex == 1)
            {//nep5资产
                foreach (string key in Config.nep5s.Keys)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = key;
                    this.assetId.Items.Add(comboBoxItem);
                }
            }
            this.assetId.SelectedIndex = 0;

        }




        private void StartSnapshot(object sender, RoutedEventArgs e)
        {
            if (this.assetType.SelectedIndex == 0)
            {//全局资产分析
                Console.WriteLine("全局资产分析");
            }
            else
            {//nep5资产分析
                Console.WriteLine("nep5资产分析");
            }
        }

    }
}
