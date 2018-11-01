using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SnapshotAndAirdrop
{
    class Config
    {
        public static Config Ins
        {
            get
            {
                if (ins == null)
                    ins = new Config();
                return ins;
            }
        }

        private static Config ins;

        //定义全局资产种类
        public MyJson.JsonNode_Object assets { private set; get; }
        //定义nep5资产种类
        public MyJson.JsonNode_Object nep5s { private set; get; }

        public string url { private set; get; }

        //Address
        public string Address_Conn { private set; get; }
        public string Address_DB { private set; get; }
        public string Address_Coll { private set; get; }
        //UTXO
        public string UTXO_Conn { private set; get; }
        public string UTXO_DB { private set; get; }
        public string UTXO_Coll { private set; get; }
        //NEP5Transfer
        public string NEP5Transfer_Conn { private set; get; }
        public string NEP5Transfer_DB { private set; get; }
        public string NEP5Transfer_Coll { private set; get; }
        //快照
        public string Snapshot_Conn { private set; get; }
        public string Snapshot_DB { private set; get; }
        //nns奖励发放
        public string Applyfornnc_Conn { private set; get; }
        public string Applyfornnc_DB { private set; get; }
        public string Applyfornnc_Coll { private set; get; }


        public Config()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()    //将配置文件的数据加载到内存中
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())   //指定配置文件所在的目录
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)  //指定加载的配置文件
                .Build();    //编译成对象  


            assets = MyJson.Parse(config["assets"]) as MyJson.JsonNode_Object;
            nep5s = MyJson.Parse(config["nep5s"]) as MyJson.JsonNode_Object;
            url = config["url"];
            Address_Conn = config["Address_Conn"];
            Address_DB = config["Address_DB"];
            Address_Coll = config["Address_Coll"];
            UTXO_Conn = config["UTXO_Conn"];
            UTXO_DB = config["UTXO_DB"];
            UTXO_Coll = config["UTXO_Coll"];
            NEP5Transfer_Conn = config["NEP5Transfer_Conn"];
            NEP5Transfer_DB = config["NEP5Transfer_DB"];
            NEP5Transfer_Coll = config["NEP5Transfer_Coll"];
            Snapshot_Conn = config["Snapshot_Conn"];
            Snapshot_DB = config["Snapshot_DB"];
            Applyfornnc_Conn = config["Applyfornnc_Conn"];
            Applyfornnc_DB = config["Applyfornnc_DB"];
            Applyfornnc_Coll = config["Applyfornnc_Coll"];
        }


    }


    //地址参数格式
    public struct Address
    {
        //根据实际情况修改字段名称  例如在数据库中address对应的key是addres 就将下面的addr替换成address
        public string addr;
    }

    //Utxo参数格式
    public struct UTXO
    {
        //根据实际情况修改字段名称
        public string addr;
        public string asset;
        public BsonDecimal128 value;
        public Int32 createHeight;
        public Int32 useHeight;
    }
    //Nep5Transfer
    public struct NEP5Transfer
    {
        public Int32 blockindex;
        public string asset;
        public string from;
        public string to;
        public string value;
    }
    //快照
    public struct Snapshot
    {
        public string addr;
        public BsonDecimal128 balance;
        public BsonDecimal128 send;
        public string txid;
        public string sendAssetId;
        public UInt32 height;
    }


    public struct NnsAward
    {
        public string maxBuyer;
        public BsonDecimal128 maxBonus;
        public string txid;
    }


    public struct TotalSnapshot
    {
        public string snapshotColl;
        public string assetid;
        public BsonDecimal128 totalValue;
        public UInt32 height;
    }
}
