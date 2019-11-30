using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    public class InstancePropertyInfo
    {
        public Object Owner
        {
            get;
        }

        public PropertyInfo Property
        {
            get;
        }

        public InstancePropertyInfo(object ownerObject, PropertyInfo propertyInfo)
        {
            Owner = ownerObject;
            Property = propertyInfo;
        }
    }

}
