using ReadExcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadExcel
{
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    internal class ConfigEnumAttribute : Attribute
    {
        public  int exportDocumentType;
        internal ConfigEnumAttribute(ExportDocumentType exportDocumentType)
        {
            this.exportDocumentType = (int)exportDocumentType;
        }

        internal ConfigEnumAttribute(int Name)
        {
            this.exportDocumentType = Name;
        }

        public static implicit operator bool(ConfigEnumAttribute configEnum)
        {
            return configEnum != null;
        }
    }
}
