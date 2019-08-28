// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SoundList<T> : IList<T> where T : SoundBase
    {
        private List<int> soundQueue = new List<int>();

        private List<T> soundList = new List<T>();

        public int Count
        {
            get
            {
                return soundList.Count;
            }
        }

        //
        // 概要:
        //     指定したインデックスにある要素を取得または設定します。
        //
        // パラメーター:
        //   index:
        //     取得または設定する要素の、0 から始まるインデックス番号。
        //
        // 戻り値:
        //     指定したインデックス位置にある要素。
        //
        // 例外:
        //   T:System.ArgumentOutOfRangeException:
        //     index が System.Collections.IList の有効なインデックスではありません。
        //
        //   T:System.NotSupportedException:
        //     このプロパティが設定されていますが、System.Collections.IList が読み取り専用です。
        public T this[int index]
        {
            get
            {
                return soundList[index];
            }
            set
            {
                soundList[index] = value;
            }
        }

        //
        // 概要:
        //     System.Collections.IList が読み取り専用かどうかを示す値を取得します。
        //
        // 戻り値:
        //     System.Collections.IList が読み取り専用である場合は true。それ以外の場合は false。
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        //
        // 概要:
        //     System.Collections.IList が固定サイズかどうかを示す値を取得します。
        //
        // 戻り値:
        //     true が固定サイズの場合は System.Collections.IList。それ以外の場合は false。
        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        private int maxSlot;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="maxSlot"></param>
        public SoundList(int maxSlot)
        {
            this.maxSlot = maxSlot;
            for (int i = 0; i < maxSlot; i++)
                soundQueue.Add(i);
        }


        //
        // 概要:
        //     System.Collections.IList に項目を追加します。
        //
        // パラメーター:
        //   value:
        //     System.Collections.IList に追加するオブジェクト。
        //
        // 戻り値:
        //     新しい要素が挿入された位置、または項目がコレクションに挿入されなかったことを示す -1。
        //
        // 例外:
        //   T:System.NotSupportedException:
        //     System.Collections.IList は読み取り専用です。 -または- System.Collections.IList のサイズが固定されています。
        public int Add(T value)
        {
            soundList.Add(value);
            soundQueue.Remove(value.Slot);
            soundQueue.Add(value.Slot);
            return soundList.Count - 1;
        }

        //
        // 概要:
        //     System.Collections.IList からすべての項目を削除します。
        //
        // 例外:
        //   T:System.NotSupportedException:
        //     System.Collections.IList は読み取り専用です。
        public void Clear()
        {
            soundList.Clear();
            soundQueue.Clear();
            for (int i = 0; i < maxSlot; i++)
                soundQueue.Add(i);
        }
        //
        // 概要:
        //     System.Collections.IList に特定の値が格納されているかどうかを判断します。
        //
        // パラメーター:
        //   value:
        //     System.Collections.IList 内で検索するオブジェクト。
        //
        // 戻り値:
        //     System.Object が System.Collections.IList に存在する場合は true。それ以外の場合は false。
        public bool Contains(T value)
        {
            return soundList.Contains(value);
        }
        //
        // 概要:
        //     System.Collections.IList 内の特定の項目のインデックスを確認します。
        //
        // パラメーター:
        //   value:
        //     System.Collections.IList 内で検索するオブジェクト。
        //
        // 戻り値:
        //     リストに存在する場合は value のインデックス。それ以外の場合は -1。
        public int IndexOf(T value)
        {
            return soundList.IndexOf(value);
        }
        //
        // 概要:
        //     指定したインデックスの System.Collections.IList に項目を挿入します。
        //
        // パラメーター:
        //   index:
        //     value を挿入する位置の、0 から始まるインデックス。
        //
        //   value:
        //     System.Collections.IList に挿入するオブジェクト。
        //
        // 例外:
        //   T:System.ArgumentOutOfRangeException:
        //     index が System.Collections.IList の有効なインデックスではありません。
        //
        //   T:System.NotSupportedException:
        //     System.Collections.IList は読み取り専用です。 -または- System.Collections.IList は固定サイズです。
        //
        //   T:System.NullReferenceException:
        //     value は System.Collections.IList の null 参照です。
        public void Insert(int index, T value)
        {
            //nothing
        }
        //
        // 概要:
        //     特定のオブジェクトで最初に出現したものを System.Collections.IList から削除します。
        //
        // パラメーター:
        //   value:
        //     System.Collections.IList から削除するオブジェクト。
        //
        // 例外:
        //   T:System.NotSupportedException:
        //     System.Collections.IList は読み取り専用です。 -または- System.Collections.IList のサイズが固定されています。
        public void Remove(T value)
        {
            soundList.Remove(value);
            soundQueue.Remove(value.Slot);
            soundQueue.Add(value.Slot);
        }
        //
        // 概要:
        //     指定したインデックスにある System.Collections.IList 項目を削除します。
        //
        // パラメーター:
        //   index:
        //     削除する項目の 0 から始まるインデックス。
        //
        // 例外:
        //   T:System.ArgumentOutOfRangeException:
        //     index が System.Collections.IList の有効なインデックスではありません。
        //
        //   T:System.NotSupportedException:
        //     System.Collections.IList は読み取り専用です。 -または- System.Collections.IList のサイズが固定されています。
        public void RemoveAt(int index)
        {
            var snd = soundList[index];
            soundList.RemoveAt(index);
            soundQueue.Remove(snd.Slot);
            soundQueue.Add(snd.Slot);
        }

        /// <summary>
        /// 未使用のスロットを取得する
        /// </summary>
        /// <returns>未使用のスロット</returns>
        public int GetEmptySlot(int maxSlot)
        {
            while (true)
            {
                var slot = soundQueue[0];
                if (slot > maxSlot - 1)
                {
                    soundQueue.RemoveAt(0);
                    soundQueue.Add(slot);
                }
                else
                {
                    return slot;
                }
            }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return soundList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }
    }
}
