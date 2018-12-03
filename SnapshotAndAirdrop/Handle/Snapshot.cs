using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapshotAndAirdrop.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Numerics;
using MongoDB.Bson;
using System.Globalization;

namespace SnapshotAndAirdrop.Handle
{
    class AssetSnapshotTask:PTask
    {
        public override void Handle(params object[] args)
        {
            //分析参数
            string assetid = (string)args[0];
            UInt32 height = UInt32.Parse((string)args[1]);
            string sendAssetid = (string)args[2];
            decimal sendCount = decimal.Parse((string)args[3]);
            string snapshopColl = (string)args[4];

            //获取快照资产的详情
            AssetInfo assetInfo = GetAssetInfo(assetid);

            //获取已有地址个数 分段处理
            var count = mongoHelper.GetDataCount(Config.Ins.Address_Conn, Config.Ins.Address_DB, Config.Ins.Address_Coll);
            var looptime = count / 10000 + 1;
            for (var i = 1; i < looptime+1; i++)
            {
                MyJson.JsonNode_Array Ja_addressInfo = mongoHelper.GetDataPages(Config.Ins.Address_Conn, Config.Ins.Address_DB, Config.Ins.Address_Coll, "{}", 10000, i);
                for (var ii = 0; ii < Ja_addressInfo.Count; ii++)
                {
                    string address = JsonConvert.DeserializeObject<Address>(Ja_addressInfo[ii].ToString()).addr;
                    //获取这个address的所有的utxo
                    string findFliter =ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new UTXO() { addr=address,asset=assetid}));
                    MyJson.JsonNode_Array utxos = mongoHelper.GetData(Config.Ins.UTXO_Conn, Config.Ins.UTXO_DB, Config.Ins.UTXO_Coll, findFliter);

                    decimal value = 0;
                    foreach (MyJson.JsonNode_Object j in utxos)
                    {
                        UTXO uTXO = JsonConvert.DeserializeObject<UTXO>(j.ToString());
                         if (uTXO.createHeight <= height && (uTXO.useHeight > height || uTXO.useHeight == -1))
                            value += (decimal)uTXO.value;
                    }
                    deleRuntime(((i - 1) * 10000 + ii) + "/" + count);
                    if (value > 0)
                    {
                        Snapshot snapshot = new Snapshot();
                        snapshot.addr = address;
                        snapshot.assetid = assetid;
                        snapshot.balance = BsonDecimal128.Create(value);
                        snapshot.send = BsonDecimal128.Create(((value / assetInfo.totoalSupply) * sendCount).ToString("F8"));
                        snapshot.totalSend = BsonDecimal128.Create(sendCount);
                        snapshot.sendAssetid = sendAssetid;
                        snapshot.txid = "";
                        snapshot.height = height;
                        snapshot.applied = false;
                        findFliter =ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new Snapshot() { addr = address }));
                        mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, findFliter, BsonDocument.Create(snapshot));
                    }
                }
            }


            //更新最新的分红数据表
            mongoHelper.DelData(Config.Ins.Snapshot_Conn,Config.Ins.Snapshot_DB,Config.Ins.CurrentDb_Coll, "{}");
            mongoHelper.InsetOne(Config.Ins.Snapshot_Conn,Config.Ins.Snapshot_DB,Config.Ins.CurrentDb_Coll, BsonDocument.Parse("{CurrentColl:\""+ snapshopColl + "\"}"));
            deleResult("完成");
        }



    }

    class Nep5SnapshotTask : PTask
    {
        public override void Handle(params object[] args)
        {
            //分析参数
            string assetid = (string)args[0];
            UInt32 height = UInt32.Parse((string)args[1]);
            string sendAssetid = (string)args[2];
            decimal sendCount = decimal.Parse((string)args[3]);
            string snapshopColl = (string)args[4];

            //获取快照资产的详情
            AssetInfo assetInfo = GetAssetInfo(assetid);

            //获取这个资产的所有的nep5交易来进行资产的分析
            NEP5Transfer nEP5Transfer = new NEP5Transfer();
            nEP5Transfer.asset = assetid;
            //先清除原有数据
            mongoHelper.DelData(Config.Ins.Snapshot_Conn,Config.Ins.Snapshot_DB,snapshopColl,"{}");

            //分批录入数据
            string findFilter =Helper.ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(nEP5Transfer));
            var count = mongoHelper.GetDataCount(Config.Ins.NEP5Transfer_Conn, Config.Ins.NEP5Transfer_DB, Config.Ins.NEP5Transfer_Coll, findFilter);
            var looptime = count / 1000 + 1;
            for (var i = 1; i < looptime+1; i++)
            {
                findFilter =ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new NEP5Transfer() { asset=assetid}));
                MyJson.JsonNode_Array Ja_Nep5transferInfo = mongoHelper.GetDataPages(Config.Ins.NEP5Transfer_Conn, Config.Ins.NEP5Transfer_DB, Config.Ins.NEP5Transfer_Coll, "{}", 1000, i,findFilter);
                for (var ii = 0; ii < Ja_Nep5transferInfo.Count; ii++)
                {
                    var str = Ja_Nep5transferInfo[ii].ToString();
                    decimal blockindex =JsonConvert.DeserializeObject<NEP5Transfer>(str).blockindex;
                    if (blockindex <= height)
                    {

                        string from = JsonConvert.DeserializeObject<NEP5Transfer>(str).from;
                        string to = JsonConvert.DeserializeObject<NEP5Transfer>(str).to;
                        decimal value =decimal.Parse(JsonConvert.DeserializeObject<NEP5Transfer>(str).value,NumberStyles.Float);

                        //更新from在snapshot中的value
                        if (!string.IsNullOrEmpty(from))
                        {
                            string findFliter =ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new Snapshot() { addr = from }));
                            MyJson.JsonNode_Array JA_SnapshotInfo = mongoHelper.GetData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, findFliter);

                            decimal balance = 0;
                            if (JA_SnapshotInfo.Count <= 0)
                            {
                                balance -= value;
                            }
                            else
                            {
                                balance =decimal.Parse(JA_SnapshotInfo[0].AsDict()["balance"].AsDict()["$numberDecimal"].ToString(), NumberStyles.Float);
                                balance = balance - value;
                            }

                            string whereFliter = findFliter;
                            var findFliter_Snapshot = new Snapshot()
                            {
                                addr = from,
                                balance = BsonDecimal128.Create(balance),
                                height = height,
                                assetid = assetid,
                                sendAssetid = sendAssetid,
                                txid = "",
                                applied = false,
                                totalSend = BsonDecimal128.Create(sendCount),
                                send = BsonDecimal128.Create(((balance / assetInfo.totoalSupply) * sendCount).ToString("F8"))
                            };
                            if (balance <= 0)
                                mongoHelper.DelData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, whereFliter);
                            else
                                mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, whereFliter, findFliter_Snapshot);
                        }
                        if (!string.IsNullOrEmpty(to))
                        {
                            //更新to在snapshot中的value
                            string findFliter =ToolHelper.RemoveUndefinedParams(JsonConvert.SerializeObject(new Snapshot() { addr = to }));
                            MyJson.JsonNode_Array JA_SnapshotInfo = mongoHelper.GetData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, findFliter);
                            decimal balance = 0;
                            if (JA_SnapshotInfo.Count <= 0)
                            {
                                balance += value;
                            }
                            else
                            {
                                balance = decimal.Parse(JA_SnapshotInfo[0].AsDict()["balance"].AsDict()["$numberDecimal"].ToString(), NumberStyles.Float);
                                balance = balance + value;
                            }
                            string whereFliter = findFliter;
                            var findFliter_Snapshot = new Snapshot()
                            {
                                addr = to,
                                balance = BsonDecimal128.Create(balance),
                                assetid = assetid,
                                height = height,
                                sendAssetid = sendAssetid,
                                txid = "",
                                applied = false,
                                totalSend = BsonDecimal128.Create(sendCount),
                                send = BsonDecimal128.Create(((balance / assetInfo.totoalSupply) * sendCount).ToString("F8"))
                            };

                            if (balance <= 0)
                                mongoHelper.DelData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, whereFliter);
                            else
                                mongoHelper.ReplaceData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, snapshopColl, whereFliter, findFliter_Snapshot);
                        }
                    }

                    deleRuntime(((i - 1) * 1000 + ii+1) + "/" + count);
                }
            }


            //更新最新的分红数据表
            mongoHelper.DelData(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, Config.Ins.CurrentDb_Coll, "{}");
            CurrentDb currentDb = new CurrentDb();
            currentDb.currentColl = snapshopColl;
            currentDb.height = height;
            currentDb.totalValue = BsonDecimal128.Create(sendCount);
            currentDb.assetid = assetid;
            currentDb.sendAssetid = sendAssetid;

            mongoHelper.InsetOne(Config.Ins.Snapshot_Conn, Config.Ins.Snapshot_DB, Config.Ins.CurrentDb_Coll, currentDb);

            deleResult("完成");
        }
    }
}
