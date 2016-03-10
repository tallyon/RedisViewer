using System;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Text;

namespace RedisViewer.LIB
{
    public class RedisDB
    {
        #region Data fields and properties
        private ConnectionMultiplexer redis;
        private IServer server;
        private IDatabase db;

        // Singleton
        private static RedisDB instance;
        public static RedisDB Instance
        {
            get
            {
                if (instance == null)
                    instance = new RedisDB();
                return instance;
            }
        }

        public string GetConnectionInfo
        {
            get
            {
                return string.Format("Connected to {0}", redis.Configuration.ToString());
            }
        }
        #endregion

        #region Constructors
        private RedisDB() { }

        //public RedisDB(string server, int port)
        //{
        //    // Connect to Redis
        //    redis = ConnectionMultiplexer.Connect(server);
        //    this.server = redis.GetServer(server, port);
        //    // Acces db0
        //    db = redis.GetDatabase();
        //}
        #endregion

        #region Methods
        //===============
        //= Connection  =
        //===============
        public bool Connect(string server, int port)
        {
            try
            {
                // Connect to Redis
                redis = ConnectionMultiplexer.Connect(server);
                this.server = redis.GetServer(server, port);
                // Acces db0
                db = redis.GetDatabase();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public string PrintDatabase()
        {
            StringBuilder sb = new StringBuilder(string.Format("Keys in {0}:\n", redis.Configuration));
            IEnumerable<RedisKey> keys = server.Keys();

            foreach (var key in keys)
            {
                switch (db.KeyType(key))
                {
                    case RedisType.String:
                        sb.Append(string.Format("{0}: {1}\n", key.ToString(), db.StringGet(key)));
                        break;

                    case RedisType.List:
                        sb.Append(string.Format("{0}:\n", key.ToString()));
                        RedisValue[] listValues = db.ListRange(key);
                        for (int i = 0; i < listValues.Length; i++)
                            sb.Append(string.Format("\t- {0}\n", listValues[i].ToString()));
                        break;

                    default:
                        break;
                }
            }

            return sb.ToString();
        }

        //===================
        //= GET operations  =
        //===================
        public string GetKeys()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<RedisKey> keys = server.Keys();

            foreach (var key in keys)
            {
                sb.Append(string.Format("{0}\n", key.ToString()));
            }

            return sb.ToString();
        }

        public List<KeyValuePair<string, string[]>> GetKeysList()
        {
            IEnumerable<RedisKey> keys = server.Keys();
            List<KeyValuePair<string, string[]>> keysList = new List<KeyValuePair<string, string[]>>();

            string[] keyValue;
            foreach (var key in keys)
            {
                switch (db.KeyType(key))
                {
                    case RedisType.String:
                        keyValue = new string[1];
                        keyValue[0] = db.StringGet(key);
                        keysList.Add(new KeyValuePair<string, string[]>(key.ToString(), keyValue));
                        break;

                    case RedisType.List:
                        keyValue = new string[db.ListLength(key)];
                        for (int i = 0; i < keyValue.Length; i++)
                        {
                            keyValue[i] = db.ListGetByIndex(key, i);
                        }
                        keysList.Add(new KeyValuePair<string, string[]>(key.ToString(), keyValue));
                        break;

                    default:
                        break;
                }
            }
            return keysList;
        }

        //===================
        //=  SET operations =
        //===================
        public bool SetKey(string key, string value)
        {
            return db.StringSet(key, value);
        }

        public bool SetKeyWithExpiration(string key, string value, int expirationInSeconds)
        {
            long expirationInTicks = expirationInSeconds * 10000000;
            TimeSpan expiry = new TimeSpan(expirationInTicks);
            return db.StringSet(key, value, expiry);
        }

        //===================
        //= DEL operations  =
        //===================
        public bool DelKey(string key)
        {
            return db.KeyDelete(key);
        }

        public long DelKeys(string[] keys)
        {
            // Construct RedisKey array from given string array of keys
            RedisKey[] rediskeys = new RedisKey[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                rediskeys[i] = keys[i];
            }

            return db.KeyDelete(rediskeys);
        }

        //===================
        //= TTL operations  =
        //===================
        public int? TTLKey(string key)
        {
            var ttl = db.KeyTimeToLive(key);

            if (ttl != null)
            {

                return Convert.ToInt32(ttl.Value.TotalSeconds);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
