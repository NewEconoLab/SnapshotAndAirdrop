﻿using Newtonsoft.Json;
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
using MongoDB.Bson;

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
            this.assetType2.SelectedIndex = 0;
            this.assetId_airdrop2.Items.Clear();

            foreach (string key in Config.Ins.nep5s.Keys)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = key;
                ComboBoxItem comboBoxItem2 = new ComboBoxItem();
                comboBoxItem2.Content = key;
                this.assetId_award.Items.Add(comboBoxItem2);
                ComboBoxItem comboBoxItem3 = new ComboBoxItem();
                comboBoxItem3.Content = key;
                this.assetId_airdrop2.Items.Add(comboBoxItem3);
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

        private void assetType2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.assetId2.Items.Clear();
            if (this.assetType2.SelectedIndex == 0)
            {//全局资产

                foreach (string key in Config.Ins.assets.Keys)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = key;
                    this.assetId2.Items.Add(comboBoxItem);
                }
            }
            else if (this.assetType2.SelectedIndex == 1)
            {//nep5资产
                foreach (string key in Config.Ins.nep5s.Keys)
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = key;
                    this.assetId2.Items.Add(comboBoxItem);
                }
            }
            this.assetId2.SelectedIndex = 0;
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
                var sendAssetid = Config.Ins.assets[(this.assetId2.SelectedItem as ComboBoxItem).Content.ToString()].ToString();
                var sendCount = this.sendCount.Text;
                var height = this.height.Text;
                var snapshotColl = this.snapshotColl.Text;
                if (string.IsNullOrEmpty(assetid) || string.IsNullOrEmpty(height) || string.IsNullOrEmpty(snapshotColl))
                    return;

                assetSnapshotTask.StartTask(assetid, height, sendAssetid, sendCount,snapshotColl);
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
                var sendAssetid = Config.Ins.nep5s[(this.assetId2.SelectedItem as ComboBoxItem).Content.ToString()].ToString();
                var sendCount = this.sendCount.Text;
                var snapshotColl = this.snapshotColl.Text;
                if (string.IsNullOrEmpty(assetid) || string.IsNullOrEmpty(height) || string.IsNullOrEmpty(snapshotColl))
                    return;
                nep5SnapshotTask.StartTask(assetid, height, sendAssetid, sendCount, snapshotColl);
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

            var snapshotColl = this.snapshotColl_airdrop.Text;

            if (priKey == null || string.IsNullOrEmpty(snapshotColl))
            {
                _lock = false;
                MessageBox.Show("参数错误");
                return;
            }

            //需要空头出去的钱
            var appliedSend = this.appliedSendTotal.Content;
            var send = this.sendTotal.Content;

            var mb = MessageBox.Show("需要空投：" + appliedSend, "",MessageBoxButton.OKCancel);
            if (mb == MessageBoxResult.OK)
            {
                AirdropTask airdropTask = new AirdropTask();
                airdropTask.deleResult = null;
                airdropTask.deleRuntime = null;
                airdropTask.deleRuntime += RuntimeCallBack_Airdrop;
                airdropTask.deleResult += ResultCallBack_Airdrop;
                airdropTask.StartTask(priKey, send, snapshotColl);
            }
            else
            {
                _lock = false;

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
            decimal send = 0;
            decimal appliedSend = 0;

            var count = mongoHelper.GetDataCount(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotColl);
            var looptime = count / 10000 + 1;
            for (var i = 1; i < looptime + 1; i++)
            {
                MyJson.JsonNode_Array Ja_addressInfo = mongoHelper.GetDataPages(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshotColl, "{}", 10000, i);
                for (var ii = 0; ii < Ja_addressInfo.Count; ii++)
                {
                    if (Ja_addressInfo[ii].AsDict().ContainsKey("balance"))
                    {
                        balance += decimal.Parse(Ja_addressInfo[ii].AsDict()["balance"].AsDict()["$numberDecimal"].ToString(),System.Globalization.NumberStyles.Float);
                    }
                    if (Ja_addressInfo[ii].AsDict().ContainsKey("send"))
                    {
                        send += decimal.Parse(Ja_addressInfo[ii].AsDict()["send"].AsDict()["$numberDecimal"].ToString(), System.Globalization.NumberStyles.Float);
                    }
                    if (Ja_addressInfo[ii].AsDict().ContainsKey("applied") && Ja_addressInfo[ii].AsDict()["applied"].AsBool())
                    {
                        appliedSend += decimal.Parse(Ja_addressInfo[ii].AsDict()["send"].AsDict()["$numberDecimal"].ToString(), System.Globalization.NumberStyles.Float);
                    }
                }
            }

            this.snapshotTotal.Content = balance;
            this.sendTotal.Content = send;
            this.appliedSendTotal.Content = appliedSend;
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

        private void AssetId4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GetBalanceOf(object sender, RoutedEventArgs e)
        {
            try
            {
                var asset = Config.Ins.nep5s[(this.assetId_airdrop2.SelectedItem as ComboBoxItem).Content.ToString()].ToString();
                var height = 0;
                int.TryParse(this.snapshot_height.Text, out height);
                var address = this.snapshot_address.Text;
                if (string.IsNullOrEmpty(asset) || height == 0 || string.IsNullOrEmpty(address))
                {
                    MessageBox.Show("请填入正确的参数");
                    return;
                }
                var balance = decimal.Zero;
                MyJson.JsonNode_Array Ja_Nep5transferInfo = mongoHelper.GetData(Config.Ins.NEP5Transfer_Conn, Config.Ins.NEP5Transfer_DB, Config.Ins.NEP5Transfer_Coll, ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new NEP5Transfer() { asset = asset, from = address })));
                for (var i = 0; i < Ja_Nep5transferInfo.Count; i++)
                {
                    var str = Ja_Nep5transferInfo[i].ToString();
                    int blockindex = JsonConvert.DeserializeObject<NEP5Transfer>(str).blockindex;
                    if (blockindex <= height)
                        balance -= decimal.Parse(JsonConvert.DeserializeObject<NEP5Transfer>(str).value, System.Globalization.NumberStyles.Float);

                }

                Ja_Nep5transferInfo = mongoHelper.GetData(Config.Ins.NEP5Transfer_Conn, Config.Ins.NEP5Transfer_DB, Config.Ins.NEP5Transfer_Coll, ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new NEP5Transfer() { asset = asset, to = address })));
                for (var i = 0; i < Ja_Nep5transferInfo.Count; i++)
                {
                    var str = Ja_Nep5transferInfo[i].ToString();
                    int blockindex = JsonConvert.DeserializeObject<NEP5Transfer>(str).blockindex;
                    if (blockindex <= height)
                        balance += decimal.Parse(JsonConvert.DeserializeObject<NEP5Transfer>(str).value, System.Globalization.NumberStyles.Float);

                }

                this.balanceof.Content = "余额：" + balance;
            }
            catch
            {
                MessageBox.Show("参数异常");
            }
        }
    }
}
