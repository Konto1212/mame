// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public interface IWsgEditorSbyteCapable
    {
        byte WsgBitWide
        {
            get;
        }

        sbyte[] WsgData
        {
            get;
            set;
        }

        string WsgDataSerializeData
        {
            get;
            set;
        }
    }
}
