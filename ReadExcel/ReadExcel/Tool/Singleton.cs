using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadExcel
{
    public class Singleton<T> where T : new()
    {
        protected static T m_sInstance = new T();

        public static T instance
        {
            get
            {
                if (m_sInstance == null)
                {
                    m_sInstance = new T();
                }
                return Singleton<T>.m_sInstance;
            }
        }
    }
}
