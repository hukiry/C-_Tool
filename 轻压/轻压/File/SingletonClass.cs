
namespace Filejieyasuo
{
    /// <summary>
    /// 单列设计模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
   public  class SingletonClass<T> where T:class,new()
    {
        private static T instance = null ;
        public static T Instance
        {
            get {
                if(instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
    }
}
