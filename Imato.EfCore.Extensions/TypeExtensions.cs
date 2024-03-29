﻿using System.Text.Json;

namespace Imato.EfCore.Extensions
{
    public static class TypeExtensions
    {
        public static string SqlDateFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private static JsonSerializerOptions jOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static string ToSqlString(this string str)
        {
            return $"'{str.Replace("'", "''")}'";
        }

        public static string ToSqlString(this DateTime date)
        {
            return date.ToString(SqlDateFormat).ToSqlString();
        }

        public static string ToSqlString(this object? field)
        {
            if (field == null)
            {
                return "null";
            }

            if (field != null)
            {
            }

            if (field.GetType() == typeof(string))
            {
                return field.ToString().ToSqlString();
            }

            if (field.GetType() == typeof(DateTime))
            {
                return DateTime.Parse(field.ToString()).ToSqlString();
            }

            if (field.GetType() == typeof(bool))
            {
                return bool.Parse(field.ToString()) ? "1" : "0";
            }

            if (field.GetType().IsValueType)
            {
                return JsonSerializer.Serialize(field, jOptions);
            }

            return $"'{JsonSerializer.Serialize(field, jOptions).ToSqlString()}'";
        }
    }
}