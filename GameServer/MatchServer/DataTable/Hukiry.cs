namespace Hukiry.Table
{
    public class Hukiry<T1, T2>
	{
		/// <summary>
		/// 物品id
		/// </summary>
		public T1 key;
		/// <summary>
		/// 物品数量
		/// </summary>
		public T2 num;
		public Hukiry(T1 t1, T2 t2)
		{
			this.key = t1;
			this.num = t2;
		}
	}

	public class Hukiry<T1, T2, T3>
	{
		public T1 value1;
		public T2 value2;
		public T3 value3;
		public Hukiry(T1 t1, T2 t2, T3 t3)
		{
			this.value1 = t1;
			this.value2 = t2;
			this.value3 = t3;
		}

		public static implicit operator Hukiry<T1, T2>(Hukiry<T1, T2, T3> hukiry)
		{
			return new Hukiry<T1, T2>(hukiry.value1, hukiry.value2);
		}
	}

	public class Hukiry<T1, T2, T3, T4>
	{
		public T1 value1;
		public T2 value2;
		public T3 value3;
		public T4 value4;
		public Hukiry(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			this.value1 = t1;
			this.value2 = t2;
			this.value3 = t3;
			this.value4 = t4;
		}

		public static implicit operator Hukiry<T1, T2>(Hukiry<T1, T2, T3, T4> hukiry)
		{
			return new Hukiry<T1, T2>(hukiry.value1, hukiry.value2);
		}

		public static implicit operator Hukiry<T1, T2, T3>(Hukiry<T1, T2, T3, T4> hukiry)
		{
			return new Hukiry<T1, T2, T3>(hukiry.value1, hukiry.value2, hukiry.value3);
		}
	}

	public class Hukiry<T1, T2, T3, T4,T5>
	{
		public T1 value1;
		public T2 value2;
		public T3 value3;
		public T4 value4;
		public T5 value5;
		public Hukiry(T1 t1, T2 t2, T3 t3, T4 t4,T5 t5)
		{
			this.value1 = t1;
			this.value2 = t2;
			this.value3 = t3;
			this.value4 = t4;
			this.value5 = t5;
		}

		public static implicit operator Hukiry<T1, T2, T3, T4>(Hukiry<T1, T2, T3, T4, T5> hukiry)
		{
			return new Hukiry<T1, T2, T3,T4>(hukiry.value1, hukiry.value2, hukiry.value3,hukiry.value4);
		}

		public static implicit operator Hukiry<T1, T2, T3>(Hukiry<T1, T2, T3, T4, T5> hukiry)
		{
			return new Hukiry<T1, T2, T3>(hukiry.value1, hukiry.value2,hukiry.value3);
		}

		public static implicit operator Hukiry<T1, T2>(Hukiry<T1, T2, T3, T4,T5> hukiry)
		{
			return new Hukiry<T1, T2>(hukiry.value1, hukiry.value2);
		}
	}
}
