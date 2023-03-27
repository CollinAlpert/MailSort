using System;
using System.Collections.Generic;

namespace MailSort;

public static class Extensions
{
	public static string? Capitalize(this string? s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return s;
		}

		var charArray = s.ToCharArray();
		if (!char.IsUpper(charArray[0]))
		{
			charArray[0] = char.ToUpper(charArray[0]);
		}

		return new string(charArray);
	}

	public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
	{
		foreach (var item in items)
		{
			action(item);
		}
	}
}