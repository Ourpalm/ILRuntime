using System;

using Mono.Cecil;
using Mono.Cecil.Metadata;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class EventTests : BaseTestFixture {

		[Test]
		public void AbstractMethod ()
		{
			TestCSharp ("Events.cs", module => {
				var type = module.GetType ("Foo");

				Assert.IsTrue (type.HasEvents);

				var events = type.Events;

				Assert.AreEqual (1, events.Count);

				var @event = events [0];

				Assert.IsNotNull (@event);
				Assert.AreEqual ("Bar", @event.Name);
				Assert.IsNotNull (@event.EventType);
				Assert.AreEqual ("Pan", @event.EventType.FullName);

				Assert.IsNotNull (@event.AddMethod);
				Assert.AreEqual (MethodSemanticsAttributes.AddOn, @event.AddMethod.SemanticsAttributes);
				Assert.IsNotNull (@event.RemoveMethod);
				Assert.AreEqual (MethodSemanticsAttributes.RemoveOn, @event.RemoveMethod.SemanticsAttributes);
			});
		}

		[Test]
		public void OtherMethod ()
		{
			TestIL ("others.il", module => {
				var type = module.GetType ("Others");

				Assert.IsTrue (type.HasEvents);

				var events = type.Events;

				Assert.AreEqual (1, events.Count);

				var @event = events [0];

				Assert.IsNotNull (@event);
				Assert.AreEqual ("Handler", @event.Name);
				Assert.IsNotNull (@event.EventType);
				Assert.AreEqual ("System.EventHandler", @event.EventType.FullName);

				Assert.IsTrue (@event.HasOtherMethods);

				Assert.AreEqual (2, @event.OtherMethods.Count);

				var other = @event.OtherMethods [0];
				Assert.AreEqual ("dang_Handler", other.Name);

				other = @event.OtherMethods [1];
				Assert.AreEqual ("fang_Handler", other.Name);
			});
		}

		[Test]
		public void UnattachedEvent ()
		{
			var int32 = typeof (int).ToDefinition ();
			var evt = new EventDefinition ("Event", EventAttributes.None, int32);

			Assert.AreEqual (null, evt.AddMethod);
		}
	}
}
