using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace ClearentPracticalExercise
{
    public static class Utilities
    {
        private static string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public static string RandomString (int length)
        {
            string result = string.Empty;
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                result += chars[random.Next(chars.Length)];
            }
            return result;
        }
    }
}
