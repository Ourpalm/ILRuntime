//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//

#if !NET_CORE

using System;
using System.Security;
using SSP = System.Security.Permissions;

namespace Mono.Cecil.Rocks {

#if INSIDE_ROCKS
	public
#endif
	static class SecurityDeclarationRocks {

		public static PermissionSet ToPermissionSet (this SecurityDeclaration self)
		{
			if (self == null)
				throw new ArgumentNullException ("self");

			PermissionSet set;
			if (TryProcessPermissionSetAttribute (self, out set))
				return set;

			return CreatePermissionSet (self);
		}

		static bool TryProcessPermissionSetAttribute (SecurityDeclaration declaration, out PermissionSet set)
		{
			set = null;

			if (!declaration.HasSecurityAttributes && declaration.SecurityAttributes.Count != 1)
				return false;

			var security_attribute = declaration.SecurityAttributes [0];
			if (!security_attribute.AttributeType.IsTypeOf ("System.Security.Permissions", "PermissionSetAttribute"))
				return false;

			var attribute = new SSP.PermissionSetAttribute ((SSP.SecurityAction) declaration.Action);

			var named_argument = security_attribute.Properties [0];
			string value = (string) named_argument.Argument.Value;
			switch (named_argument.Name) {
			case "XML":
				attribute.XML = value;
				break;
			case "Name":
				attribute.Name = value;
				break;
			default:
				throw new NotImplementedException (named_argument.Name);
			}

			set = attribute.CreatePermissionSet ();
			return true;
		}

		static PermissionSet CreatePermissionSet (SecurityDeclaration declaration)
		{
			var set = new PermissionSet (SSP.PermissionState.None);

			foreach (var attribute in declaration.SecurityAttributes) {
				var permission = CreatePermission (declaration, attribute);
				set.AddPermission (permission);
			}

			return set;
		}

		static IPermission CreatePermission (SecurityDeclaration declaration, SecurityAttribute attribute)
		{
			var attribute_type = Type.GetType (attribute.AttributeType.FullName);
			if (attribute_type == null)
				throw new ArgumentException ("attribute");

			var security_attribute = CreateSecurityAttribute (attribute_type, declaration);
			if (security_attribute == null)
				throw new InvalidOperationException ();

			CompleteSecurityAttribute (security_attribute, attribute);

			return security_attribute.CreatePermission ();
		}

		static void CompleteSecurityAttribute (SSP.SecurityAttribute security_attribute, SecurityAttribute attribute)
		{
			if (attribute.HasFields)
				CompleteSecurityAttributeFields (security_attribute, attribute);

			if (attribute.HasProperties)
				CompleteSecurityAttributeProperties (security_attribute, attribute);
		}

		static void CompleteSecurityAttributeFields (SSP.SecurityAttribute security_attribute, SecurityAttribute attribute)
		{
			var type = security_attribute.GetType ();

			foreach (var named_argument in attribute.Fields)
				type.GetField (named_argument.Name).SetValue (security_attribute, named_argument.Argument.Value);
		}

		static void CompleteSecurityAttributeProperties (SSP.SecurityAttribute security_attribute, SecurityAttribute attribute)
		{
			var type = security_attribute.GetType ();

			foreach (var named_argument in attribute.Properties)
				type.GetProperty (named_argument.Name).SetValue (security_attribute, named_argument.Argument.Value, null);
		}

		static SSP.SecurityAttribute CreateSecurityAttribute (Type attribute_type, SecurityDeclaration declaration)
		{
			SSP.SecurityAttribute security_attribute;
			try {
				security_attribute = (SSP.SecurityAttribute) Activator.CreateInstance (
					attribute_type, new object [] { (SSP.SecurityAction) declaration.Action });
			} catch (MissingMethodException) {
				security_attribute = (SSP.SecurityAttribute) Activator.CreateInstance (attribute_type, new object [0]);
			}

			return security_attribute;
		}

		public static SecurityDeclaration ToSecurityDeclaration (this PermissionSet self, SecurityAction action, ModuleDefinition module)
		{
			if (self == null)
				throw new ArgumentNullException ("self");
			if (module == null)
				throw new ArgumentNullException ("module");

			var declaration = new SecurityDeclaration (action);

			var attribute = new SecurityAttribute (
				module.TypeSystem.LookupType ("System.Security.Permissions", "PermissionSetAttribute"));

			attribute.Properties.Add (
				new CustomAttributeNamedArgument (
					"XML",
					new CustomAttributeArgument (
						module.TypeSystem.String, self.ToXml ().ToString ())));

			declaration.SecurityAttributes.Add (attribute);

			return declaration;
		}
	}
}

#endif
