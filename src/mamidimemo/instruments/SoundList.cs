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
    public class SoundList<T> : List<T> where T : SoundBase
    {
        private int maxSlot;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="maxSlot"></param>
        public SoundList(int maxSlot)
        {
            this.maxSlot = maxSlot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public new void Remove(T item)
        {
            item.Disposed -= Item_Disposed;
            base.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public new void Add(T item)
        {
            item.Disposed += Item_Disposed;
            base.Add(item);
        }

        private void Item_Disposed(object sender, EventArgs e)
        {
            Remove((T)sender);
        }
    }
}
