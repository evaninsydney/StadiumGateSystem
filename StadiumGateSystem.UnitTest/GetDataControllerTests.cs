// PSEUDOCODE / PLAN (detailed):
// 1. Locate the type for `StadiumGateSystem.Controllers.GetDataController` using reflection.
// 2. Assert the type exists (fail fast with clear message if not).
// 3. Create an instance of the controller using Activator.CreateInstance (handle parameterless or non-public).
// 4. Locate a public method named `Get` on that type (fail if not found).
// 5. Invoke the `Get` method and capture the result.
// 6. Convert the result into `IEnumerable<string>` using several heuristics:
//    - If result is already IEnumerable<string>, use it.
//    - If result is an array of string, cast it.
//    - If result has a `Value` or `Content` property that contains IEnumerable<string>, extract it via reflection.
//    - Otherwise treat as unsupported and fail with a helpful message.
// 7. Compare the extracted sequence to an expected sequence using `CollectionAssert.AreEqual`.
// 8. Provide clear assertion messages for debugging when things go wrong.
//
// This test is defensive: it will produce understandable failures if the controller or method
// does not exist or returns an unexpected shape. It avoids compile-time dependencies on specific
// WebAPI/MVC types by using reflection.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StadiumGateSystem.UnitTest
{
    [TestClass]
    public class GetDataControllerTests
    {
        [TestMethod]
        public void Get_ReturnsExpectedGateList()
        {
            // Arrange
            var expected = new List<string> { "Gate A", "Gate B", "Gate C" };

            // Act & Assert via reflection to support multiple controller signatures/return shapes.
            var controllerType = Type.GetType("StadiumGateSystem.Controllers.GetDataController, StadiumGateSystem");
            Assert.IsNotNull(controllerType, "Type 'StadiumGateSystem.Controllers.GetDataController' not found. Ensure the controller exists and the assembly name is 'StadiumGateSystem'.");

            object controllerInstance;
            try
            {
                // Try public parameterless, then try non-public parameterless as fallback.
                controllerInstance = Activator.CreateInstance(controllerType);
            }
            catch
            {
                try
                {
                    controllerInstance = Activator.CreateInstance(controllerType, nonPublic: true);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to create an instance of '{controllerType.FullName}': {ex.Message}");
                    return; // unreachable, but keeps flow obvious
                }
            }

            var getMethod = controllerType.GetMethod("Get");
            Assert.IsNotNull(getMethod, "Public method 'Get' not found on GetDataController.");

            object rawResult;
            try
            {
                rawResult = getMethod.Invoke(controllerInstance, null);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Invoking 'Get' threw an exception: {ex.InnerException?.Message ?? ex.Message}");
                return;
            }

            var actual = ExtractStringEnumerable(rawResult);
            Assert.IsNotNull(actual, "Could not extract a sequence of strings from the result of 'Get'. The method may return an unsupported type.");

            var actualList = actual.ToList();
            CollectionAssert.AreEqual(expected, actualList, "The sequence returned by Get() did not match the expected values.");
        }

        private static IEnumerable<string> ExtractStringEnumerable(object result)
        {
            if (result == null)
            {
                return null;
            }

            // If it's already IEnumerable<string>
            if (result is IEnumerable<string> stringEnum)
            {
                return stringEnum;
            }

            // If it's a non-generic IEnumerable, try to cast elements to string
            if (result is IEnumerable nonGenericEnumerable)
            {
                try
                {
                    var cast = nonGenericEnumerable.Cast<object>().Select(o => o as string).ToList();
                    if (cast.All(s => s != null))
                    {
                        return cast;
                    }
                }
                catch
                {
                    // fall through
                }
            }

            var resultType = result.GetType();

            // Common wrapper: property named "Value" (e.g., ActionResult<T> or similar)
            var valueProp = resultType.GetProperty("Value");
            if (valueProp != null)
            {
                var value = valueProp.GetValue(result);
                if (value is IEnumerable<string> vs) return vs;
                if (value is IEnumerable nonGen) return nonGen.Cast<object>().Select(o => o as string);
            }

            // Another common wrapper: property named "Content"
            var contentProp = resultType.GetProperty("Content");
            if (contentProp != null)
            {
                var content = contentProp.GetValue(result);
                if (content is IEnumerable<string> cs) return cs;
                if (content is IEnumerable nonGenC) return nonGenC.Cast<object>().Select(o => o as string);
            }

            // If it's an array of strings
            if (resultType.IsArray && resultType.GetElementType() == typeof(string))
            {
                return ((string[])result);
            }

            // Unsupported shape
            return null;
        }
    }
}