// copyright-holders:K.Ito
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments.Vst
{
    [Editor(typeof(RefreshingCollectionEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(ExpandableCollectionConverter))]
    [RefreshProperties(RefreshProperties.All)]
    public class VSTPluginCollection : IList<VSTPlugin>, IList
    {
        private List<VSTPlugin> f_list = new List<VSTPlugin>();

        public VSTPluginCollection()
        {
        }

        public int IndexOf(VSTPlugin item)
        {
            return f_list.IndexOf(item);
        }

        public void Insert(int index, VSTPlugin item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.RemoveAt(index);
        }

        public VSTPlugin this[int index]
        {
            get
            {
                return f_list[index];
            }
            set
            {
                f_list[index] = value;
            }
        }

        public void Add(VSTPlugin item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Add(item);
        }

        public void Clear()
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Clear();
        }

        public bool Contains(VSTPlugin item)
        {
            return f_list.Contains(item);
        }

        public void CopyTo(VSTPlugin[] array, int arrayIndex)
        {
            f_list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return f_list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList)f_list).IsReadOnly;
            }
        }

        public bool Remove(VSTPlugin item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                return f_list.Remove(item);
        }

        public IEnumerator<VSTPlugin> GetEnumerator()
        {
            return f_list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IList.Add(object value)
        {
            int index = Count;
            Add((VSTPlugin)value);
            return index;
        }

        bool IList.Contains(object value)
        {
            return Contains((VSTPlugin)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((VSTPlugin)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (VSTPlugin)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return ((IList)f_list).IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return ((IList)f_list).IsReadOnly;
            }
        }

        void IList.Remove(object value)
        {
            Remove((VSTPlugin)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (VSTPlugin)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((VSTPlugin[])array, index);
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)f_list).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)f_list).SyncRoot;
            }
        }
    }

}
