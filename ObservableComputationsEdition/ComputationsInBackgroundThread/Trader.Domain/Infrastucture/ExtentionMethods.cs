using System.Collections.Generic;
using System.Linq;

namespace Trader.Domain.Infrastucture
{
	public static class ExtentionMethods
	{
		public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> items, int size)
		{
			int count = items.Count();
			for (int i = 0; i < count; i += size) 
			{ 
				yield return items.Skip(i).Take(size); 
			}
		}
	}
}
