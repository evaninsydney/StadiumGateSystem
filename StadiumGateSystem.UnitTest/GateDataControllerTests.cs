csharp StadiumGateSystem.UnitTests/GateDataControllerTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class GateDataControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public GateDataControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [TestMethod]
    public void ControllerType_IsResolvable()
    {
        // Try the app assembly first (Program is in the app project)
        var asm = typeof(Program).Assembly;
        var controllerType = asm.GetType("StadiumGateSystem.Controllers.GateDataController")
                             ?? AppDomain.CurrentDomain.GetAssemblies()
                                  .SelectMany(a => SafeGetTypes(a))
                                  .FirstOrDefault(t => t.Name == "GateDataController");

        Assert.IsNotNull(controllerType, "GateDataController type not found. Ensure namespace/class name is correct and the test project references the app project.");
    }

    [TestMethod]
    public async Task Get_ReturnsExpectedGateList()
    {
        var asm = typeof(Program).Assembly;
        var controllerType = asm.GetType("StadiumGateSystem.Controllers.GateDataController")
                             ?? AppDomain.CurrentDomain.GetAssemblies()
                                  .SelectMany(a => SafeGetTypes(a))
                                  .FirstOrDefault(t => t.Name == "GateDataController");

        Assert.IsNotNull(controllerType, "GateDataController type not found.");

        // Create controller via DI (will satisfy constructor parameters)
        using var scope = _factory.Services.CreateScope();
        var controllerInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, controllerType!);

        // Find a GET-like method (named "Get" or "GetAll"). Adjust if your controller uses other names.
        var getMethod = controllerType!.GetMethod("Get") ?? controllerType.GetMethod("GetAll");
        Assert.IsNotNull(getMethod, $"No suitable 'Get' method found on {controllerType.FullName}.");

        object? rawResult = getMethod.Invoke(controllerInstance, null);
        // handle Task returning methods
        if (rawResult is Task task)
        {
            await task.ConfigureAwait(false);
            var resultProp = rawResult.GetType().GetProperty("Result");
            rawResult = resultProp?.GetValue(rawResult);
        }

        var actual = ExtractStringEnumerable(rawResult);
        Assert.IsNotNull(actual, "Could not extract IEnumerable<string> from Get() result.");

        var actualList = actual!.ToList();
        // Example expected; replace with real expected values
        var expected = new List<string> { "Gate A", "Gate B", "Gate C" };
        CollectionAssert.AreEqual(expected, actualList);
    }

    private static IEnumerable<string>? ExtractStringEnumerable(object? raw)
    {
        if (raw == null) return null;

        // If it's already IEnumerable<string>
        if (raw is IEnumerable<string> se) return se;

        // If it's ActionResult<T> or ActionResult or ObjectResult-like, try to reflect Value property
        var valProp = raw.GetType().GetProperty("Value") ?? raw.GetType().GetProperty("Result");
        if (valProp != null)
        {
            var inner = valProp.GetValue(raw);
            if (inner is IEnumerable<string> innerSeq) return innerSeq;
            // If inner is IActionResult with Value
            var innerValProp = inner?.GetType().GetProperty("Value");
            if (innerValProp != null)
            {
                var v = innerValProp.GetValue(inner);
                if (v is IEnumerable<string> vseq) return vseq;
            }
        }

        // If it's IEnumerable of non-generic IEnumerable, try to cast elements to string
        if (raw is System.Collections.IEnumerable ie)
        {
            var list = new List<string>();
            foreach (var o in ie)
            {
                if (o is string s) list.Add(s);
                else if (o != null) list.Add(o.ToString() ?? "");
            }
            return list;
        }

        return null;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch { return Array.Empty<Type>(); }
    }

    public void Dispose()
    {
        _factory?.Dispose();
    }
}