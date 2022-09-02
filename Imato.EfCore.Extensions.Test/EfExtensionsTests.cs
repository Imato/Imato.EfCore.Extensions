using NUnit.Framework;
using System;
using System.Collections.Generic;
using Imato.EfCore.Extensions;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace Imato.EfCore.Extensions.Test
{
    public class EfExtensionsTests
    {
        private Customer[] customers = new Customer[2]
        {
            new Customer
            {
                Id = 1,
                Name = "Test 1",
                Amount = 1210.34,
                Created = DateTime.Parse("2022-01-02 12:34:00"),
                IsActive = true
            },
            new Customer
            {
                Id = 2,
                Name = "Test 2",
                Amount = 0.323,
                Created = DateTime.Parse("2022-01-04 00:12:00"),
                IsActive = false,
                Description = "Same test"
            }
        };

        private TestContext context = new TestContext();

        [Test]
        public void GetTableOfTest()
        {
            var result = context.GetTableOf<Customer>();
            Assert.AreEqual("Customers", result);
        }

        [Test]
        public void GetColumnsOfTest()
        {
            var result = string.Join(",", context.GetMappingsOf<Customer>().Select(x => x.ColumnName));
            Assert.AreEqual("Id,Amount,Created,Description,active,Name", result);
        }

        [Test]
        public void GenerateInsertTest()
        {
            var result = context.GenerateInsert(customers.First());
            Assert.AreEqual("insert into Customers (Id,Amount,Created,Description,active,Name) values (1,1210,34,'2022-01-02 12:34:00.0000',null,1,'Test 1')",
                result);
        }

        [Test]
        public void GenerateInsertsTest()
        {
            var result = context.GenerateInserts(customers);
            Assert.AreEqual("insert into Customers (Id,Amount,Created,Description,active,Name) values (1,1210,34,'2022-01-02 12:34:00.0000',null,1,'Test 1'),(2,0,323,'2022-01-04 00:12:00.0000','Same test',0,'Test 2')",
                result);
        }

        [Test]
        public void CreateBulkCopyTest()
        {
            var resutl = context.CreateBulkCopy<Customer>();
            Assert.AreEqual("active", resutl.ColumnMappings[4].DestinationColumn);
        }
    }
}