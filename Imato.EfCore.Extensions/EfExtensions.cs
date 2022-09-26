using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text;

namespace Imato.EfCore.Extensions
{
    public static class EfExtensions
    {
        private static ConcurrentDictionary<string, string> _inserts = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, IEnumerable<FieldMapping>> _mappings = new ConcurrentDictionary<string, IEnumerable<FieldMapping>>();

        public static string GetTableOf<T>(this DbContext context)
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            var tableName = entityType?.GetSchemaQualifiedTableName();
            if (tableName == null)
            {
                throw new NotSupportedException($"Unknown table of type {typeof(T)}");
            }
            return tableName;
        }

        private static IEnumerable<FieldMapping> GenerateMappingsOf<T>(this DbContext context)
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
            {
                throw new NotSupportedException($"Type {typeof(T).FullName} not exists DbSets");
            }

            var columns = entityType.GetProperties()
                .Select(x => new FieldMapping
                {
                    ColumnName = x.GetColumnName(),
                    PropertyName = x.Name,
                })
                .ToArray();
            if (columns.Length == 0)
            {
                throw new NotSupportedException($"Not exists public properties in type {typeof(T)}");
            }
            return columns;
        }

        public static IEnumerable<FieldMapping> GetMappingsOf<T>(this DbContext context)
        {
            return _mappings.GetOrAdd(typeof(T).Name, (_) => context.GenerateMappingsOf<T>());
        }

        public static SqlBulkCopy CreateBulkCopy<T>(this DbContext context,
            string? tableName = null,
            int batchSize = 10000,
            int timeout = 30000)
        {
            var connStr = context.Database?.GetDbConnection()?.ConnectionString;

            if (!context.Database?.IsSqlServer() != true && !string.IsNullOrEmpty(connStr))
            {
                var bulkCopy = new SqlBulkCopy(connStr, SqlBulkCopyOptions.Default);
                bulkCopy.BatchSize = batchSize;
                bulkCopy.BulkCopyTimeout = timeout;
                bulkCopy.DestinationTableName = tableName ?? context.GetTableOf<T>();

                foreach (var mapping in context.GetMappingsOf<T>())
                {
                    bulkCopy.ColumnMappings.Add(mapping.PropertyName, mapping.ColumnName);
                }

                return bulkCopy;
            }

            throw new ApplicationException("Cannot use bulk insert on this database");
        }

        public static async Task BulkInsertAsync<T>(this DbContext context,
            IEnumerable<T> records,
            string? tableName = null,
            int batchSize = 1000,
            int timeout = 3000,
            CancellationToken cancellationToken = default)
        {
            using (var bulkCopy = CreateBulkCopy<T>(context, tableName, batchSize, timeout))
            using (var reader = ObjectReader.Create(records))
                await bulkCopy.WriteToServerAsync(reader, cancellationToken);
        }

        private static string GenerateInsert<T>(this DbContext context, string tableName)
        {
            return $"insert into {tableName} ([{string.Join("],[", context.GetMappingsOf<T>().Select(x => x.ColumnName))}]) values ";
        }

        private static string GetInsert<T>(this DbContext context, string tableName)
        {
            return _inserts.GetOrAdd(typeof(T).Name, (_) => context.GenerateInsert<T>(tableName));
        }

        private static StringBuilder Append<T>(this StringBuilder builder, T record, IEnumerable<FieldMapping> mappings)
        {
            builder.Append("(");
            var accessor = ObjectAccessor.Create(record);
            var first = true;
            foreach (var mapping in mappings)
            {
                var value = accessor[mapping.PropertyName];
                if (!first) builder.Append(",");
                builder.Append(value.ToSqlString());
                first = false;
            }
            builder.Append(")");
            return builder;
        }

        public static string GenerateInsert<T>(this DbContext context, T record, string? tableName = null)
        {
            var builder = new StringBuilder(context.GetInsert<T>(tableName ?? context.GetTableOf<T>()))
                .Append(record, context.GetMappingsOf<T>());
            return builder.ToString();
        }

        public static async Task InsertAsync<T>(this DbContext context,
            T record, string?
            tableName = null,
            CancellationToken cancellationToken = default)
        {
            await context.Database.ExecuteSqlRawAsync(GenerateInsert(context, record, tableName), cancellationToken);
        }

        public static string GenerateInserts<T>(this DbContext context,
            IEnumerable<T> records,
            string? tableName = null)
        {
            var columns = context.GetMappingsOf<T>();
            var builder = new StringBuilder(context.GetInsert<T>(tableName ?? context.GetTableOf<T>()));
            var first = true;
            foreach (var record in records)
            {
                if (!first) builder.Append(",");
                builder.Append(record, columns);
                first = false;
            }

            return builder.ToString();
        }

        public static async Task InsertAsync<T>(this DbContext context,
            IEnumerable<T> records,
            string? tableName = null,
            CancellationToken cancellationToken = default)
        {
            if (records == null || records.Count() == 0)
            {
                return;
            }

            if (records.Count() > 10)
            {
                await context.BulkInsertAsync(records);
                return;
            }

            await context.Database.ExecuteSqlRawAsync(GenerateInserts(context, records, tableName), cancellationToken);
        }
    }
}