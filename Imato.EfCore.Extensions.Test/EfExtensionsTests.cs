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
        private Customer[] customers = new Customer[3]
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
            },
             new Customer
            {
                Id = 3,
                Name = "Test 3",
                Created = DateTime.Parse("2022-01-04 00:12:00"),
                IsActive = false,
                Closed = DateTime.Parse("2022-06-10 00:00:00"),
                Contacts = 11
            }
        };

        private TestContext context = new TestContext();

        [Test]
        public void GetTableOfTest()
        {
            var result = context.GetTableOf<Customer>();
            Assert.That("Customers", Is.EqualTo(result));
        }

        [Test]
        public void GetColumnsOfTest()
        {
            var result = string.Join(",", context.GetMappingsOf<Customer>().Select(x => x.ColumnName));
            Assert.That("Id,Amount,Closed,Contacts,Created,Description,active,Name", Is.EqualTo(result));
        }

        [Test]
        public void GenerateInsertTest()
        {
            var result = context.GenerateInsert(customers.First());
            Assert.That("insert into Customers ([Id],[Amount],[Closed],[Contacts],[Created],[Description],[active],[Name]) values (1,1210.34,null,null,'2022-01-02 12:34:00.000',null,1,'Test 1')",
                Is.EqualTo(result));
        }

        [Test]
        public void GenerateInsertsTest()
        {
            var result = context.GenerateInserts(customers.Take(2));
            Assert.That("insert into Customers ([Id],[Amount],[Closed],[Contacts],[Created],[Description],[active],[Name]) values (1,1210.34,null,null,'2022-01-02 12:34:00.000',null,1,'Test 1'),(2,0.323,null,null,'2022-01-04 00:12:00.000','Same test',0,'Test 2')",
                Is.EqualTo(result));
        }

        [Test]
        public void CreateBulkCopyTest()
        {
            var resutl = context.CreateBulkCopy<Customer>();
            Assert.That("active", Is.EqualTo(resutl.ColumnMappings[6].DestinationColumn));
        }
    }
}