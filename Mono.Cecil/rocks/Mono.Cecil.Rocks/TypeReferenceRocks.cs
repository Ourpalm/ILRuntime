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
using System.Linq;

namespace Mono.Cecil.Rocks {

#if INSIDE_ROCKS
	public
#endif
	static class TypeReferenceRocks {

		public static ArrayType MakeArrayType (this TypeReference self)
		{
			return new ArrayType (self);
		}

		public static ArrayType MakeArrayType (this TypeReference self, int rank)
		{
			if (rank == 0)
				throw new ArgumentOutOfRangeException ("rank");

			var array = new ArrayType (self);

			for (int i = 1; i < rank; i++)
				array.Dimensions.Add (new ArrayDimension ());

			return array;
		}

		public static PointerType MakePointerType (this TypeReference self)
		{
			return new PointerType (self);
		}

		public static ByReferenceType MakeByReferenceType (this TypeReference self)
		{
			return new ByReferenceType (self);
		}

		public static OptionalModifierType MakeOptionalModifierType (this TypeReference self, TypeReference modifierType)
		{
			return new OptionalModifierType (modifierType, self);
		}

		public static RequiredModifierType MakeRequiredModifierType (this TypeReference self, TypeReference modifierType)
		{
			return new RequiredModifierType (modifierType, self);
		}

		public static GenericInstanceType MakeGenericInstanceType (this TypeReference self, params TypeReference [] arguments)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (arguments == null)
				throw new ArgumentNullException ("arguments");
			if (arguments.Length == 0)
				throw new ArgumentException ();
			if (self.GenericParameters.Count != arguments.Length)
				throw new ArgumentException ();

			var instance = new GenericInstanceType (self);

			foreach (var argument in arguments)
				instance.GenericArguments.Add (argument);

			return instance;
		}

		public static PinnedType MakePinnedType (this TypeReference self)
		{
			return new PinnedType (self);
		}

		public static SentinelType MakeSentinelType (this TypeReference self)
		{
			return new SentinelType (self);
		}
	}
}
