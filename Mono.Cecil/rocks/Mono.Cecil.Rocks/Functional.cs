//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

using System;
using System.Collections.Generic;

namespace Mono.Cecil.Rocks {

	static class Functional {

		public static System.Func<A, R> Y<A, R> (System.Func<System.Func<A, R>, System.Func<A, R>> f)
		{
			System.Func<A, R> g = null;
			g = f (a => g (a));
			return g;
		}

		public static IEnumerable<TSource> Prepend<TSource> (this IEnumerable<TSource> source, TSource element)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return PrependIterator (source, element);
		}

		static IEnumerable<TSource> PrependIterator<TSource> (IEnumerable<TSource> source, TSource element)
		{
			yield return element;

			foreach (var item in source)
				yield return item;
		}
	}
}
