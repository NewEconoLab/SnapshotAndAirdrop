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
            { "SDT", "0xa4f408df2a1ec2a950ec5fd06d7b9dc5f83b9e73" },
            { "NNC_test","0x8826206138420e79936123a18176c597cd173173"}
        };

        public static string url = "https://api.nel.group/api/testnet";

        //Address
        public static string Address_Conn = "mongodb://nelData_mainnet:NELqingmingzi3345@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockData_mainnet?replicaSet=mgset-4977005";
        public static string Address_DB = "NeoBlockData_mainnet";
        public static string Address_Coll = "address";
        //UTXO
        public static string UTXO_Conn = "mongodb://nelData_mainnet:NELqingmingzi3345@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockData_mainnet?replicaSet=mgset-4977005";
        public static string UTXO_DB = "NeoBlockData_mainnet";
        public static string UTXO_Coll = "utxo";
        //NEP5Transfer
        public static string NEP5Transfer_Conn = "mongodb://nelData_mainnet:NELqingmingzi3345@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/NeoBlockData_mainnet?replicaSet=mgset-4977005";
        public static string NEP5Transfer_DB = "NeoBlockData_mainnet";
        public static string NEP5Transfer_Coll = "NEP5transfer";
        //快照
        public static string Snapshot_Conn = "mongodb://snapshot:qmzNEL3345@dds-bp1b36419665fdd41167-pub.mongodb.rds.aliyuncs.com:3717,dds-bp1b36419665fdd42489-pub.mongodb.rds.aliyuncs.com:3717/SnapshotAndAirdrop?replicaSet=mgset-4977005";
        public static string Snapshot_DB = "SnapshotAndAirdrop";
    }


    //地址参数格式
    public struct Address
    {
        //根据实际情况修改字段名称  例如在数据库中address对应的key是addres 就将下面的addr替换成address
        public string addr;

        public string __Addr
        {
            get
            {
                return addr;
            }
            set
            {
                addr = value;
            }
        }

    }

    //Utxo参数格式
    public struct UTXO
    {
        //根据实际情况修改字段名称
        public string addr;
        public string asset;
        public decimal value;
        public decimal createHeight;
        public decimal useHeight;


        public string __Addr
        {
            get
            {
                return addr;
            }
            set
            {
                addr = value;
            }
        }
        public string __Asset
        {
            get
            {
                return asset;
            }
            set
            {
                asset = value;
            }
        }
        public decimal __Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }
        public decimal __CreateHeight
        {
            get
            {
                return createHeight;
            }
            set
            {
                createHeight = value;
            }
        }
        public decimal __UseHeight
        {
            get
            {
                return useHeight;
            }
            set
            {
                useHeight = value;
            }
        }
    }

    //Nep5Transfer
    public struct NEP5Transfer
    {
        public decimal blockindex;
        public string asset;
        public string from;
        public string to;
        public decimal value;


        public decimal __Blockindex
        {
            get
            {
                return blockindex;
            }
            set
            {
                blockindex = value;
            }
        }

        public string __Asset
{
            get
            {
                return asset;
            }
            set
            {
                asset = value;
            }
        }

        public string __From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
            }
        }

        public string __To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
            }
        }
        
        public decimal __Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }
    }

    //快照
    public struct Snapshot
    {
        public string addr;
        public decimal balance;
        public decimal send;
        public string txid;


        public string __Addr
        {
            get
            {
                return addr;
            }
            set
            {
                addr = value;
            }
        }
        public string __Txid
        {
            get
            {
                return txid;
            }
            set
            {
                txid = value;
            }
        }
        public decimal __Balance
        {
            get
            {
                return balance;
            }
            set
            {
                balance = value;
            }
        }
        public decimal __Send
        {
            get
            {
                return send;
            }
            set
            {
                send = value;
            }
        }
    }
}
