using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace StadiumGateSystem.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var expected = new List<string> { "Gate A", "Gate B", "Gate C" };
            // Act
            var actual = new List<string> { "Gate A", "Gate B", "Gate C" };
            // Assert
            CollectionAssert.AreEqual(expected, actual);


        }
    }
}
