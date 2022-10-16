using Vanadium.Common.Mathematics;

namespace Vanadium.Test
{
	[TestFixture]
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
			Assert.Pass();
		}

		[Test]
		public void Test2()
		{
			Assert.Fail();
		}
	}
}
