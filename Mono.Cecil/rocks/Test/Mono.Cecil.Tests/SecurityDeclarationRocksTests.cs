#if !NET_CORE

using System.Security.Permissions;

using NUnit.Framework;

using Mono.Cecil.Rocks;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class SecurityDeclarationRocksTests : BaseTestFixture {

		[Test]
		public void ToPermissionSetFromPermissionSetAttribute ()
		{
			TestModule ("decsec-xml.dll", module => {
				var type = module.GetType ("SubLibrary");

				Assert.IsTrue (type.HasSecurityDeclarations);
				Assert.AreEqual (1, type.SecurityDeclarations.Count);

				var declaration = type.SecurityDeclarations [0];

				var permission_set = declaration.ToPermissionSet ();

				Assert.IsNotNull (permission_set);

				string permission_set_value = "<PermissionSet class=\"System.Security.PermissionSe"
	+ "t\"\r\nversion=\"1\">\r\n<IPermission class=\"{0}\"\r\nversion=\"1\"\r\nFla"
	+ "gs=\"UnmanagedCode\"/>\r\n</PermissionSet>\r\n";

				permission_set_value = string.Format (permission_set_value, typeof (SecurityPermission).AssemblyQualifiedName);

				Assert.AreEqual (Normalize (permission_set_value), Normalize (permission_set.ToXml ().ToString ()));
			});
		}

		[Test]
		public void ToPermissionSetFromSecurityAttribute ()
		{
			TestModule ("decsec-att.dll", module => {
				var type = module.GetType ("SubLibrary");

				Assert.IsTrue (type.HasSecurityDeclarations);
				Assert.AreEqual (1, type.SecurityDeclarations.Count);

				var declaration = type.SecurityDeclarations [0];

				var permission_set = declaration.ToPermissionSet ();

				Assert.IsNotNull (permission_set);

				string permission_set_value = "<PermissionSet class=\"System.Security.PermissionSe"
	+ "t\"\r\nversion=\"1\">\r\n<IPermission class=\"{0}\"\r\nversion=\"1\"\r\nFla"
	+ "gs=\"UnmanagedCode\"/>\r\n</PermissionSet>\r\n";

				permission_set_value = string.Format (permission_set_value, typeof (SecurityPermission).AssemblyQualifiedName);

				Assert.AreEqual (Normalize (permission_set_value), Normalize (permission_set.ToXml ().ToString ()));
			});
		}
	}
}

#endif
