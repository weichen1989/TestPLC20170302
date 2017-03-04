using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIMI.RGWTB
{
    /// <summary>
    /// 自定义事件，事件传递数据
    /// </summary>
    public class PropertyChangedEventArgs : EventArgs
    {
        public string PropertyName { get; private set; }
        public object OldValue { get; private set; }
        public object NewValue { get; set; }
        public PropertyChangedEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
