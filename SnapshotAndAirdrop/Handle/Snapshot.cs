using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapshotAndAirdrop.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Numerics;

namespace SnapshotAndAirdrop.Handle
{
    class AssetSnapshotTask:PTask
    {
        public override void Handle(params object[] args)
        {
            //分析参数
            string assetid = (string)args[0];
            decimal height = decimal.Parse((string)args[1]);
            string snapshopColl = (string)args[2];
            //获取已有地址个数 分段处理
            var count = mongoHelper.GetDataCount(Config.Ins.Address_Conn, Config.Ins.Address_DB, Config.Ins.Address_Coll);
            var looptime = count / 10000 + 1;
            for (var i = 1; i < looptime+1; i++)
            {
                MyJson.JsonNode_Array Ja_addressInfo = mongoHelper.GetDataPages(Config.Ins.Address_Conn, Config.Ins.Address_DB, Config.Ins.Address_Coll, "{}", 10000, i);
                for (var ii = 0; ii < Ja_addressInfo.Count; ii++)
                {
                    string address = JsonConvert.DeserializeObject<Address>(Ja_addressInfo[ii].ToString()).__Addr;
                    //获取这个address的所有的utxo
                    string findFliter = JsonConvert.SerializeObject(new UTXO() { __Addr=address,__Asset=assetid});
                    findFliter =ToolHelper.RemoveUndefinedParams(MyJson.Parse(findFliter).AsDict());

                    MyJson.JsonNode_Array utxos = mongoHelper.GetData(Config.Ins.UTXO_Conn, Config.Ins.UTXO_DB, Config.Ins.UTXO_Coll, findFliter);

                    decimal value = 0;
                    foreach (MyJson.JsonNode_Object j in utxos)
                    {
                        UTXO uTXO = JsonConvert.DeserializeObject<UTXO>(j.ToString());
                         if (uTXO.__CreateHeight <= height && (uTXO.__UseHeight > height || uTXO.__UseHeight == -1))
                            value +=decimal.Parse(uTXO.__Value);
                    }
                    deleRuntime(((i - 1) * 10000 + ii) + "/" + count);
                    if (value > 0)
                    {
                        Snapshot snapshot = new Snapshot();
                        snapshot.__Addr = address;
                        snapshot.__Balance = value.ToString();
                        findFliter = JsonConvert.SerializeObject(new Snapshot() {__Addr = address });
                        findFliter = ToolHelper.RemoveUndefinedParams(MyJson.Parse(findFliter).AsDict());
                        mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, findFliter, ToolHelper.RemoveUndefinedParams(MyJson.Parse(JsonConvert.SerializeObject(snapshot)).AsDict()));
                    }
                }
            }
            deleResult("完成");
        }
    }

    class Nep5SnapshotTask : PTask
    {
        public override void Handle(params object[] args)
        {
            //分析参数
            string assetid = (string)args[0];
            decimal height = decimal.Parse((string)args[1]);
            string snapshopColl = (string)args[2];

            //获取这个资产的所有的nep5交易来进行资产的分析
            NEP5Transfer nEP5Transfer = new NEP5Transfer();
            nEP5Transfer.__Asset = assetid;
            //先清除原有数据
            mongoHelper.DelData(Config.Ins.Snapshot_Conn,Config.Ins.Snapshot_DB,snapshopColl,"{}");

            //分批录入数据
            string findFilter = JsonConvert.SerializeObject(nEP5Transfer);
            findFilter = ToolHelper.RemoveUndefinedParams(MyJson.Parse(findFilter).AsDict());
            var count = mongoHelper.GetDataCount(Config.Ins.NEP5Transfer_Conn, Config.Ins.NEP5Transfer_DB, Config.Ins.NEP5Transfer_Coll, findFilter);
            var looptime = count / 1000 + 1;
            for (var i = 1; i < looptime+1; i++)
            {
                //var time1 = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                findFilter = JsonConvert.SerializeObject(new NEP5Transfer() { __Asset=assetid});
                findFilter = ToolHelper.RemoveUndefinedParams(MyJson.Parse(findFilter).AsDict());
                MyJson.JsonNode_Array Ja_Nep5transferInfo = mongoHelper.GetDataPages(Config.Ins.NEP5Transfer_Conn, Config.Ins.NEP5Transfer_DB, Config.Ins.NEP5Transfer_Coll, "{}", 1000, i,findFilter);
                for (var ii = 0; ii < Ja_Nep5transferInfo.Count; ii++)
                {
                    var str = Ja_Nep5transferInfo[ii].ToString();
                    decimal blockindex =JsonConvert.DeserializeObject<NEP5Transfer>(str).__Blockindex;
                    if (blockindex <= height)
                    {

                        string from = JsonConvert.DeserializeObject<NEP5Transfer>(str).__From;
                        string to = JsonConvert.DeserializeObject<NEP5Transfer>(str).__To;
                        decimal value = decimal.Parse(JsonConvert.DeserializeObject<NEP5Transfer>(str).__Value);


                        //更新from在snapshot中的value
                        if (!string.IsNullOrEmpty(from))
                        {
                            string findFliter = JsonConvert.SerializeObject(new Snapshot() { __Addr = from });
                            findFliter = ToolHelper.RemoveUndefinedParams(MyJson.Parse(findFliter).AsDict());
                            MyJson.JsonNode_Array JA_SnapshotInfo = mongoHelper.GetData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, findFliter);

                            decimal balance = 0;
                            if (JA_SnapshotInfo.Count <= 0)
                            {
                                balance -= value;
                            }
                            else
                            {
                                balance = decimal.Parse(JsonConvert.DeserializeObject<Snapshot>(JA_SnapshotInfo[0].AsDict().ToString()).__Balance);
                                balance = balance - value;
                                Console.WriteLine(from + "    " + value + "  " + balance);
                            }

                            string whereFliter = findFliter;
                            findFliter = JsonConvert.SerializeObject(new Snapshot() { __Addr = from, __Balance = balance.ToString() });
                            findFliter = ToolHelper.RemoveRedundantParams(MyJson.Parse(findFliter).AsDict());
                            mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, whereFliter, findFliter);
                        }
                        if (!string.IsNullOrEmpty(to))
                        {
                            //更新to在snapshot中的value
                            string findFliter = JsonConvert.SerializeObject(new Snapshot() { __Addr = to });
                            findFliter = ToolHelper.RemoveUndefinedParams(MyJson.Parse(findFliter).AsDict());
                            MyJson.JsonNode_Array JA_SnapshotInfo = mongoHelper.GetData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, findFliter);
                            decimal balance = 0;
                            if (JA_SnapshotInfo.Count <= 0)
                            {
                                balance += value;
                            }
                            else
                            {
                                balance = decimal.Parse(JsonConvert.DeserializeObject<Snapshot>(JA_SnapshotInfo[0].AsDict().ToString()).__Balance);
                                balance = balance + value;
                            }
                            string whereFliter = findFliter;
                            findFliter = JsonConvert.SerializeObject(new Snapshot() { __Addr = to, __Balance = balance.ToString() });
                            findFliter = ToolHelper.RemoveRedundantParams(MyJson.Parse(findFliter).AsDict());
                            mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, whereFliter, findFliter);
                        }
                    }

                    deleRuntime(((i - 1) * 1000 + ii+1) + "/" + count);
                }
            }
            deleResult("完成");
        }
    }
}
