using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace SnapshotAndAirdrop
{
    class Config
    {
        //定义全局资产种类
        public static Dictionary<string, string> assets = new Dictionary<string, string>()
        {
            { "NEO", "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b" },
            { "GAS", "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7" }
        };
        //定义nep5资产种类
        public static Dictionary<string, string> nep5s = new Dictionary<string, string>()
        {
            { "NNC", "0xfc732edee1efdf968c23c20a9628eaa5a6ccb934" },
            { "SDT", "0xa4f408df2a1ec2a950ec5fd06d7b9dc5f83b9e73" }
        };


        //Address
        public static string Address_Conn = "mongodb://nelDataStorage:NELqingmingzi1128@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockBaseData?replicaSet=mgset-4977005";
        public static string Address_DB = "NeoBlockBaseData";
        public static string Address_Coll = "address";
        //UTXO
        public static string UTXO_Conn = "mongodb://nelDataStorage:NELqingmingzi1128@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockBaseData?replicaSet=mgset-4977005";
        public static string UTXO_DB = "NeoBlockBaseData";
        public static string UTXO_Coll = "utxo";
        //NEP5Transfer
        public static string NEP5Transfer_Conn = "mongodb://nelDataStorage:NELqingmingzi1128@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockBaseData?replicaSet=mgset-4977005";
        public static string NEP5Transfer_DB = "NeoBlockBaseData";
        public static string NEP5Transfer_Coll = "NEP5transfer";
        //快照
        public static string Snapshot_Conn = "mongodb://nelDataStorage:NELqingmingzi1128@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockBaseData?replicaSet=mgset-4977005";
        public static string Snapshot_DB = "NeoAnalysisBaseData_testnet";
    }


    //地址参数格式
    public struct Address
    {
        //根据实际情况修改字段名称  例如在数据库中address对应的key是addres 就将下面的addr替换成address
        public string addr { private get; set; }

        public string _Addr
        {
            get
            {
                return addr;
            }
        }

    }

    //Utxo参数格式
    public struct UTXO
    {
        //根据实际情况修改字段名称
        public string addr { private get; set; }
        public string asset { private get; set; }
        public BigInteger value { private get; set; }
        public BigInteger createHeight { private get; set; }
        public BigInteger useHeight { private get; set; }


        public string _Addr
        {
            get
            {
                return addr;
            }
        }
        public string _Asset
        {
            get
            {
                return asset;
            }
        }
        public BigInteger @Value
        {
            get
            {
                return value;
            }
        }
        public BigInteger _CreateHeight
        {
            get
            {
                return createHeight;
            }
        }
        public BigInteger _UseHeight
        {
            get
            {
                return useHeight;
            }
        }
    }

    //Nep5Transfer
    public struct NEP5Transfer
    {
        public string blockindex { private get; set; }
        public string asset { private get; set; }
        public string from { private get; set; }
        public string to { private get; set; }
        public BigInteger value { private get; set; }



        public string _Blockindex
        {
            get
            {
                return blockindex;
            }
        }

        public string _Asset
{
            get
            {
                return asset;
            }
        }

        public string _From
        {
            get
            {
                return from;
            }
        }

        public string _To
        {
            get
            {
                return to;
            }
        }
        
        public BigInteger @Value
        {
            get
            {
                return value;
            }
        }
    }

    //快照
    public struct Snapshot
    {
        public string addr { private get; set; }
        public BigInteger balance { private get; set; }
        public BigInteger send { private get; set; }
        public string txid { private get; set; }


        public string _Addr
        {
            get
            {
                return addr;
            }
        }
        public string _Txid
        {
            get
            {
                return txid;
            }
        }
        public BigInteger _Balance
        {
            get
            {
                return balance;
            }
        }
        public BigInteger _Send
        {
            get
            {
                return send;
            }
        }
    }
}
