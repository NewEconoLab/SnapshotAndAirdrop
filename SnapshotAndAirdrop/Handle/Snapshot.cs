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
            BigInteger height = (BigInteger)args[1];
            string snapshopColl = (string)args[2];
            //获取已有地址个数 分段处理
            var count = mongoHelper.GetDataCount(Config.Address_Conn, Config.Address_DB, Config.Address_Coll);
            var looptime = count / 1000 + 1;
            for (var i = 0; i < looptime; i++)
            {
                MyJson.JsonNode_Array Ja_addressInfo = mongoHelper.GetDataPages(Config.Address_Conn, Config.Address_DB, Config.Address_Coll, "{}", 1000, i);
                for (var ii = 0; ii < Ja_addressInfo.Count; ii++)
                {
                    string address = JsonConvert.DeserializeObject<Address>(Ja_addressInfo[i].ToString())._Addr;
                    //获取这个address的所有的utxo
                    string findFliter = JsonConvert.SerializeObject(new UTXO() { addr=address,asset=assetid});
                    MyJson.JsonNode_Array utxos = mongoHelper.GetData(Config.UTXO_Conn, Config.UTXO_DB, Config.UTXO_Coll, findFliter);

                    BigInteger value = 0;
                    foreach (MyJson.JsonNode_Object j in utxos)
                    {
                        UTXO uTXO = JsonConvert.DeserializeObject<UTXO>(j.ToString());
                         if (uTXO._CreateHeight <= height && (uTXO._UseHeight > height || uTXO._UseHeight == -1))
                            value += uTXO._UseHeight;
                    }
                    if (value > 0)
                    {
                        Snapshot snapshot = new Snapshot();
                        snapshot.addr = address;
                        snapshot.balance = value;
                        findFliter = JsonConvert.SerializeObject(new Snapshot() {addr = address });
                        mongoHelper.ReplaceData(Config.Snapshot_Conn, Config.Snapshot_DB, snapshopColl, findFliter, JsonConvert.SerializeObject(snapshot));
                        deleRuntime((i*1000+ii)+"/" + count,address+":"+value);
                    }
                }
            }
            deleResult();
        }
    }

    class Nep5SnapshotTask : PTask
    {
        public override void Handle(params object[] args)
        {
            //分析参数
            string assetid = (string)args[0];
            BigInteger height = (BigInteger)args[1];
            string snapshopColl = (string)args[2];

            //获取这个资产的所有的nep5交易来进行资产的分析
            NEP5Transfer nEP5Transfer = new NEP5Transfer();
            nEP5Transfer.asset = assetid;
            var count = mongoHelper.GetDataCount(Config.Address_Conn, Config.Address_DB, Config.Address_Coll,JsonConvert.SerializeObject(nEP5Transfer));
            var looptime = count / 1000 + 1;
            for (var i = 0; i < looptime; i++)
            {

            }

        }
    }
}
