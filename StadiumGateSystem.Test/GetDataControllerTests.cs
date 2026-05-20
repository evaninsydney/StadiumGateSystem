using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StadiumGateSystem.Controllers;
using StadiumGateSystem.Data;
using StadiumGateSystem.Models;

namespace StadiumGateSystem.UnitTest
{
    [TestClass]
    public class GetDataControllerTests
    {
        private StadiumGateSystemDbContext context;

        [TestInitialize]
        public async Task TestInitialize()
        {
            context = await GetInMemoryDbContext();
        }

        private async Task<StadiumGateSystemDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<StadiumGateSystemDbContext>()
                .UseInMemoryDatabase(databaseName: "StadiumGateSystemTestDb")
                .Options;

            context = new StadiumGateSystemDbContext(options);
            if (!await context.Gates.AnyAsync())
            {
                // Seed test data
                context.Gates.Add(new GateData { Id = 1, Timestamp = DateTime.Now, Gate = "Gate A", Type = "enter", NumberOfPeople = 100 });
                context.Gates.Add(new GateData { Id = 2, Timestamp = DateTime.Now.AddDays(-1), Gate = "Gate A", Type = "exit", NumberOfPeople = 13 });
                context.Gates.Add(new GateData { Id = 3, Timestamp = DateTime.Now.AddHours(-12), Gate = "Gate B", Type = "enter", NumberOfPeople = 29 });
                context.Gates.Add(new GateData { Id = 4, Timestamp = DateTime.Now.AddMonths(-1), Gate = "Gate B", Type = "exit", NumberOfPeople = 21 });
                context.Gates.Add(new GateData { Id = 5, Timestamp = DateTime.Now.AddDays(-7), Gate = "Gate C", Type = "enter", NumberOfPeople = 8 });
                context.Gates.Add(new GateData { Id = 6, Timestamp = DateTime.Now.AddHours(-4), Gate = "Gate C", Type = "exit", NumberOfPeople = 4 });
                context.Gates.Add(new GateData { Id = 7, Timestamp = DateTime.Now.AddMonths(-2), Gate = "Gate C", Type = "enter", NumberOfPeople = 76 });
                context.Gates.Add(new GateData { Id = 8, Timestamp = DateTime.Now.AddDays(-4), Gate = "Gate A", Type = "enter", NumberOfPeople = 29 });
                context.Gates.Add(new GateData { Id = 9, Timestamp = DateTime.Now.AddDays(-9), Gate = "Gate A", Type = "exit", NumberOfPeople = 13 });
                context.Gates.Add(new GateData { Id = 10, Timestamp = DateTime.Now.AddHours(-19), Gate = "Gate B", Type = "enter", NumberOfPeople = 2 });
                context.Gates.Add(new GateData { Id = 11, Timestamp = DateTime.Now.AddMonths(-2), Gate = "Gate B", Type = "exit", NumberOfPeople = 71 });
                context.Gates.Add(new GateData { Id = 12, Timestamp = DateTime.Now.AddDays(-23), Gate = "Gate C", Type = "enter", NumberOfPeople = 43 });
                context.Gates.Add(new GateData { Id = 13, Timestamp = DateTime.Now.AddHours(-13), Gate = "Gate C", Type = "exit", NumberOfPeople = 49 });
                context.Gates.Add(new GateData { Id = 14, Timestamp = DateTime.Now.AddMonths(-1), Gate = "Gate C", Type = "enter", NumberOfPeople = 36 });

                context.SaveChanges();
            }
            return context;
        }

        [TestMethod]
        public async Task DbContext_GateData_HasRecords()
        {
            bool hasRecords;
            try
            {
                hasRecords = await context.Gates.AnyAsync();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Querying GateData failed: {ex.Message}");
                return;
            }

            Assert.IsTrue(hasRecords, "Expected at least one GateData record in the in-memory database after seeding using GetInMemoryDbContext().");
            Assert.IsTrue(context.Gates.Count() >= 14, $"Expected at least 14 records in the GateData table, but found {context.Gates.Count()}. This may indicate an issue with the seeding logic in GetInMemoryDbContext().");
            Assert.IsTrue(context.Gates.Any(g => g.Gate == "Gate A" && g.Type == "enter" && g.NumberOfPeople == 100), "Expected to find a record for Gate A with type 'enter' and 100 people. This may indicate an issue with the seeding logic in GetInMemoryDbContext().");
        }

        [TestMethod]
        public async Task DbContext_GateData_SearchRecordsA()
        {
            var controller = new GateDataController(context);

            ActionResult<IEnumerable<SearchResult>> actionResult = await controller.Search("Gate A", "enter", null, null);

            var actionResultResult = actionResult.Result as ObjectResult;

            Assert.IsNotNull(actionResultResult);
            var actionResultValue = actionResultResult?.Value;
            Assert.IsNotNull(actionResultValue);

            var searchResultList = actionResultValue as IEnumerable<SearchResult>;
            Assert.IsNotNull(searchResultList);
            Assert.IsTrue(searchResultList.Any(), "Expected to find at least one search result for Gate A with type 'enter'. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");
            Assert.IsTrue(searchResultList?.Count() == 1, "Expect there to be one record returned for Gate A with type 'enter'");
            Assert.IsTrue(searchResultList?.FirstOrDefault()?.NumberOfPeople == 129, "Expected to find a search result for Gate A with type 'enter' and 100 people. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");
        }


        [TestMethod]
        public async Task DbContext_GateData_SearchRecordsB()
        {
            var controller = new GateDataController(context);

            var searchGateB = await controller.Search("Gate B", string.Empty, DateTime.Now.AddDays(-40), DateTime.Now);
            Assert.IsNotNull(searchGateB, "Search result for Gate B with start and end date should not be null. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");

            var actionResultResult = searchGateB.Result as ObjectResult;
            Assert.IsNotNull(actionResultResult, "Expected the result of the Search method to be an ObjectResult. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");
            var actionResultValue = actionResultResult?.Value;
            Assert.IsNotNull(actionResultValue, "Expected the Value property of the ObjectResult to be non-null. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");

            var searchResultList = actionResultValue as IEnumerable<SearchResult>;
            Assert.IsNotNull(searchResultList);
            Assert.IsTrue(searchResultList.Any(), "Expected to find at least one search result for Gate B with the specified date range. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");
            Assert.IsTrue(searchResultList?.Count() == 2, $"Expected to find exactly 2 search results for Gate B with the specified date range, but found {searchResultList?.Count()}. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");
            Assert.IsTrue(searchResultList?.FirstOrDefault()?.NumberOfPeople == 31, "Expected to find a search result for Gate B with type 'enter' and 31 people. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");
            Assert.IsTrue(searchResultList?.ElementAtOrDefault(1)?.NumberOfPeople == 21, "Expected to find a search result for Gate B with type 'exit' and 21 people. This may indicate an issue with the Search method implementation or the seeding logic in GetInMemoryDbContext().");
        }
    }
}