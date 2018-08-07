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
using SnapshotAndAirdrop.Handle;
using SnapshotAndAirdrop.Helper;

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
            this.assetId_airdrop.Items.Clear();

            foreach (string key in Config.Ins.nep5s.Keys)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = key;
                this.assetId_airdrop.Items.Add(comboBoxItem);
                ComboBoxItem comboBoxItem2 = new ComboBoxItem();
                comboBoxItem2.Content = key;
                this.assetId_award.Items.Add(comboBoxItem2);
            }
            this.assetId.SelectedIndex = 0;
        }

        private void assetType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.assetId.Items.Clear();
            if (this.assetType.SelectedIndex == 0)
            {//全局资产

                foreach (string key in Config.Ins.assets.Keys)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = key;
                    this.assetId.Items.Add(comboBoxItem);
                }
            }
            else if (this.assetType.SelectedIndex == 1)
            {//nep5资产
                foreach (string key in Config.Ins.nep5s.Keys)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = key;
                    this.assetId.Items.Add(comboBoxItem);
                }
            }
            this.assetId.SelectedIndex = 0;

        }

        private bool _lock = false;


        private void StartSnapshot(object sender, RoutedEventArgs e)
        {
            if (_lock)
            {
                MessageBox.Show("正在处理中~");
                return;
            }
            _lock = true;
            this.details.Items.Clear();
            ListBoxItem item = new ListBoxItem();
            item.Content = "";
            this.details.Items.Add(item);

            ListBoxItem item2 = new ListBoxItem();
            item2.Content = "";
            this.details.Items.Add(item2);

            if (this.assetType.SelectedIndex == 0)
            {//全局资产分析
                AssetSnapshotTask assetSnapshotTask = new AssetSnapshotTask();
                assetSnapshotTask.deleRuntime = null;
                assetSnapshotTask.deleResult = null;
                assetSnapshotTask.deleRuntime += RuntimeCallBack_Snapshot;
                assetSnapshotTask.deleResult += ResultCallBack_Snapshot;
                var assetid = Config.Ins.assets[(this.assetId.SelectedItem as ComboBoxItem).Content.ToString()].ToString();
                var height = this.height.Text;
                var snapshotColl = this.snapshotColl.Text;
                if (string.IsNullOrEmpty(assetid) || string.IsNullOrEmpty(height) || string.IsNullOrEmpty(snapshotColl))
                    return;

                assetSnapshotTask.StartTask(assetid, height, snapshotColl);
            }
            else
            {//nep5资产分析
                Nep5SnapshotTask nep5SnapshotTask = new Nep5SnapshotTask();
                nep5SnapshotTask.deleResult = null;
                nep5SnapshotTask.deleRuntime = null;
                nep5SnapshotTask.deleRuntime += RuntimeCallBack_Snapshot;
                nep5SnapshotTask.deleResult += ResultCallBack_Snapshot;

                var assetid = Config.Ins.nep5s[(this.assetId.SelectedItem as ComboBoxItem).Content.ToString()].ToString();
                var height = this.height.Text;
                var snapshotColl = this.snapshotColl.Text;
                if (string.IsNullOrEmpty(assetid) || string.IsNullOrEmpty(height) || string.IsNullOrEmpty(snapshotColl))
                    return;
                nep5SnapshotTask.StartTask(assetid, height, snapshotColl);
            }
        }

        private void height_LostFocus(object sender, RoutedEventArgs e)
        {
            this.snapshotColl.Text = "Snapshot"+"_"+(this.assetId.SelectedItem as ComboBoxItem).Content.ToString()+"_" + this.height.Text;
        }

        private void assetId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.assetId.SelectedItem != null)
                this.snapshotColl.Text = "Snapshot" + "_" + (this.assetId.SelectedItem as ComboBoxItem).Content.ToString() + "_" + this.height.Text;
        }

        private void RuntimeCallBack_Snapshot(params string[] args)
        {
            Dispatcher.Invoke((Action)delegate () {
                (this.details.Items[0] as ListBoxItem).Content = args[0];
            });
        }

        private void ResultCallBack_Snapshot(params string[] args)
        {
            Dispatcher.Invoke((Action)delegate () {
                _lock = false;
                (this.details.Items[1] as ListBoxItem).Content = args[0];
            });
        }

        private void RuntimeCallBack_Airdrop(params string[] args)
        {
            Dispatcher.Invoke((Action)delegate () {
                (this.details_airdrop.Items[0] as ListBoxItem).Content = args[0];
            });
        }

        private void ResultCallBack_Airdrop(params string[] args)
        {
            Dispatcher.Invoke((Action)delegate () {
                _lock = false;
                (this.details_airdrop.Items[1] as ListBoxItem).Content = args[0];
            });
        }

        private void RuntimeCallBack_Award(params string[] args)
        {
            Dispatcher.Invoke((Action)delegate () {
                (this.details_award.Items[0] as ListBoxItem).Content = args[0];
            });
        }

        private void ResultCallBack_Award(params string[] args)
        {
            Dispatcher.Invoke((Action)delegate () {
                _lock = false;
                (this.details_award.Items[1] as ListBoxItem).Content = args[0];
            });
        }

        private void StartAirdrop(object sender, RoutedEventArgs e)
        {
            if (_lock)
            {
                MessageBox.Show("正在处理中~");
                return;
            }
            _lock = true;

            this.details_airdrop.Items.Clear();
            ListBoxItem item = new ListBoxItem();
            item.Content = "";
            this.details_airdrop.Items.Add(item);

            ListBoxItem item2 = new ListBoxItem();
            item2.Content = "";
            this.details_airdrop.Items.Add(item2);


            string assetid = "";
            if (Config.Ins.nep5s.ContainsKey((this.assetId_airdrop.SelectedItem as ComboBoxItem).Content.ToString()))
            {
                assetid = Config.Ins.nep5s[(this.assetId_airdrop.SelectedItem as ComboBoxItem).Content.ToString()].ToString();
            }

            var ratio = this.ratio.Text;
            var snapshotColl = this.snapshotColl_airdrop.Text;

            if (priKey == null || string.IsNullOrEmpty(assetid) || string.IsNullOrEmpty(ratio) || string.IsNullOrEmpty(snapshotColl))
            {
                _lock = false;
                MessageBox.Show("参数错误");
                return;
            }

            //重新刷一下余额
            Getbalance();

            //需要空头出去的钱
            var value = decimal.Parse(ratio) * decimal.Parse(this.total.Content.ToString());

            var mb = MessageBox.Show("需要空投：" + value + "  当前拥有：" + this.balance.Content.ToString(),"",MessageBoxButton.OKCancel);
            if (mb == MessageBoxResult.OK)
            {

                AirdropTask airdropTask = new AirdropTask();
                airdropTask.deleResult = null;
                airdropTask.deleRuntime = null;
                airdropTask.deleRuntime += RuntimeCallBack_Airdrop;
                airdropTask.deleResult += ResultCallBack_Airdrop;
                airdropTask.StartTask(priKey, assetid, ratio, snapshotColl);
            }

        }

        byte[] priKey;

        public bool Lock { get => _lock; set => _lock = value; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 使用一个IntPtr类型值来存储加密字符串的起始点  
                IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.wif.SecurePassword);

                // 使用.NET内部算法把IntPtr指向处的字符集合转换成字符串  
                string wif = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);

                priKey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
                byte[] pubKey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(priKey);
                string address = ThinNeo.Helper.GetAddressFromPublicKey(pubKey);
                this.address.Content = address;
            }
            catch
            {
                MessageBox.Show("请输入正确的wif");
            }
        }

        private void Verification(object sender, RoutedEventArgs e)
        {
            var snapshotColl = this.snapshotColl_airdrop.Text;

            if (string.IsNullOrEmpty(snapshotColl))
            {
                MessageBox.Show("填写正确的库名");
            }

            decimal balance = 0;

            var count = mongoHelper.GetDataCount(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotColl);
            var looptime = count / 10000 + 1;
            for (var i = 1; i < looptime + 1; i++)
            {
                MyJson.JsonNode_Array Ja_addressInfo = mongoHelper.GetDataPages(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotColl, "{}", 10000, i);
                for (var ii = 0; ii < Ja_addressInfo.Count; ii++)
                {
                    if (Ja_addressInfo[ii].AsDict().ContainsKey("balance"))
                    {
                        Snapshot snapshot = JsonConvert.DeserializeObject<Snapshot>(Ja_addressInfo[ii].AsDict().ToString());
                        balance += decimal.Parse(snapshot.balance);
                        Console.WriteLine(snapshot.addr + "   " + decimal.Parse(snapshot.balance) + "    " + balance);
                    }
                }
            }

            this.total.Content = balance;
        }

        private void AssetId2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Getbalance();
        }

        private void Getbalance()
        {
            string address = this.address.Content.ToString();

            if (string.IsNullOrEmpty(address))
            {
                MessageBox.Show("请先输入地址");
                return;
            }

            try
            {
                string assetid = "";
                if (Config.Ins.nep5s.ContainsKey((this.assetId_airdrop.SelectedItem as ComboBoxItem).Content.ToString()))
                {
                    assetid = Config.Ins.nep5s[(this.assetId_airdrop.SelectedItem as ComboBoxItem).Content.ToString()].ToString();
                    byte[] postdata;
                    var url = HttpHelper.MakeRpcUrlPost(Config.Ins.url, "getnep5balanceofaddress", out postdata, new MyJson.JsonNode_ValueString(assetid), new MyJson.JsonNode_ValueString(address));
                    var result = HttpHelper.HttpPost(url, postdata);
                    MyJson.JsonNode_Array jsonArray = MyJson.Parse(result).AsDict()["result"].AsList();
                    this.balance.Content = jsonArray[0].AsDict()["nep5balance"];
                }
            }
            catch
            {

            }

        }

        private void Button_Click_award(object sender, RoutedEventArgs e)
        {
            try
            {
                // 使用一个IntPtr类型值来存储加密字符串的起始点  
                IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.wif_award.SecurePassword);

                // 使用.NET内部算法把IntPtr指向处的字符集合转换成字符串  
                string wif = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);

                priKey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
                byte[] pubKey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(priKey);
                string address = ThinNeo.Helper.GetAddressFromPublicKey(pubKey);
                this.address_award.Content = address;
            }
            catch
            {
                MessageBox.Show("请输入正确的wif");
            }
        }

        private void StartNnaAward(object sender, RoutedEventArgs e)
        {
            if (_lock)
            {
                MessageBox.Show("正在处理中~");
                return;
            }
            _lock = true;

            this.details_award.Items.Clear();
            ListBoxItem item = new ListBoxItem();
            item.Content = "";
            this.details_award.Items.Add(item);

            ListBoxItem item2 = new ListBoxItem();
            item2.Content = "";
            this.details_award.Items.Add(item2);


            string assetid = "";
            string assetName = (this.assetId_award.SelectedItem as ComboBoxItem).Content.ToString();
            if (Config.Ins.nep5s.ContainsKey(assetName))
            {
                assetid = Config.Ins.nep5s[assetName].ToString();
            }
            else
            {
                MessageBox.Show("资产不存在");
            }

            var value = this.value_award.Text;

            var mb = MessageBox.Show("每个符合要求的地址都将奖励"+value+ assetName, "", MessageBoxButton.OKCancel);
            if (mb == MessageBoxResult.OK)
            {

                NnsAwardTask nnsAwardTask = new NnsAwardTask();
                nnsAwardTask.deleResult = null;
                nnsAwardTask.deleRuntime = null;
                nnsAwardTask.deleRuntime += RuntimeCallBack_Award;
                nnsAwardTask.deleResult += ResultCallBack_Award;
                nnsAwardTask.StartTask(priKey, assetid, value);
            }
        }

        private void AssetId3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string address = this.address_award.Content.ToString();

            if (string.IsNullOrEmpty(address))
            {
                MessageBox.Show("请先输入地址");
                return;
            }
        }
    }
}
