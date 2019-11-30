// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 指定したキャパシティの分だけ保持するキャッシュテーブル
    /// </summary>
    public class CacheMemory<TKey, TValue> where TKey : class where TValue : class
    {
        /// <summary>
        /// データ1
        /// </summary>
        private ConditionalWeakTable<TKey, TValue> hashtable1;

        /// <summary>
        /// データ2
        /// </summary>
        private ConditionalWeakTable<TKey, TValue> hashtable2;

        /// <summary>
        /// カレント
        /// </summary>
        private ConditionalWeakTable<TKey, TValue> currentTable;

        private int currentCount;

        /// <summary>
        /// 以前
        /// </summary>
        private ConditionalWeakTable<TKey, TValue> oldTable;

        private int oldCount;

        private long capacity;

        public long Capacity
        {
            get
            {
                return capacity;
            }
            set
            {
                capacity = value;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                var val = currentTable.GetValue(key, (k) => { return null; });
                val = oldTable.GetValue(key, (k) => { return null; });
                if (val != null)
                {
                    currentTable.Add(key, val);
                    currentCount++;
                }
                return default(TValue);
            }
            set
            {
                currentTable.Add(key, value);
                currentCount++;

                changeTable();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="capacity">最小限保障するキャッシュ個数</param>
        public CacheMemory(int capacity)
        {
            this.capacity = capacity;
            hashtable1 = new ConditionalWeakTable<TKey, TValue>();
            hashtable2 = new ConditionalWeakTable<TKey, TValue>();
            currentTable = hashtable1;
            oldTable = hashtable2;
        }


        public void Add(TKey key, TValue value)
        {
            currentTable.Add(key, value);

            changeTable();
        }

        public void Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            currentTable.Remove(key);
            oldTable.Remove(key);
        }

        private void changeTable()
        {
            if (currentCount >= capacity)
            {
                if (currentTable == hashtable1)
                {
                    hashtable2 = new ConditionalWeakTable<TKey, TValue>();
                    currentTable = hashtable2;
                    oldTable = hashtable1;
                    oldCount = currentCount;
                    currentCount = 0;
                }
                else if (currentTable == hashtable2)
                {
                    hashtable1 = new ConditionalWeakTable<TKey, TValue>();
                    currentTable = hashtable1;
                    oldTable = hashtable2;
                    oldCount = currentCount;
                    currentCount = 0;
                }
            }
        }

       
        public bool ContainsKey(TKey key)
        {
            var val = currentTable.GetValue(key, (k) => { return null; });
            if (val != null)
                return true;

            val = oldTable.GetValue(key, (k) => { return null; });
            return (val != null);
        }


        public void Clear()
        {
            hashtable1 = new ConditionalWeakTable<TKey, TValue>();
            hashtable2 = new ConditionalWeakTable<TKey, TValue>();
            currentTable = hashtable1;
            currentCount = 0;
            oldTable = hashtable2;
            oldCount = 0;
        }

    }

}
