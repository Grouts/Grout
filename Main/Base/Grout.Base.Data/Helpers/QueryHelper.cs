using Grout.Base.DataClasses;
using System;

namespace Grout.Base.Data
{
    public class QueryHelper
    {
        /// <summary>
        /// Checks the data for number, date and string and returns the formatted string/int value to be added in the query
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetData(object value)
        {
            object data;

            if (IsNumber(value))
            {
                data = value;
            }
            else if (value == DBNull.Value)
            {
                data = "NULL";
            }
            else if (value is DateTime)
            {
                data = "'" + DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "'";
            }
            else
            {
                data = "'" + value + "'";
            }

            return data;
        }

        /// <summary>
        /// Returns mathematical operator for the given condition
        /// </summary>
        /// <param name="condition">Conditions Enum</param>
        /// <returns>Mathematical operator as string</returns>
        public static string GetConditionOperator(Conditions condition)
        {
            switch (condition)
            {
                case Conditions.Equals:
                    return "=";

                case Conditions.GreaterThan:
                    return ">";

                case Conditions.GreaterThanOrEquals:
                    return ">=";

                case Conditions.LessThan:
                    return "<";

                case Conditions.LessThanOrEquals:
                    return "<=";

                case Conditions.NotEquals:
                    return "!=";

                case Conditions.IS:
                    return " IS ";

                case Conditions.IN:
                    return " IN ";

                case Conditions.LIKE:
                    return " LIKE ";

                case Conditions.NOTIN:
                    return " NOT IN ";

                default:
                    return String.Empty;
            }
        }

        /// <summary>
        ///     Checks if the value is a number type
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>True if the object is a number type</returns>
        public static bool IsNumber(object value)
        {
            if (value == null)
                return false;
            return (
                value is double ||
                value is int ||
                value is short ||
                value is long ||
                value is decimal
                );
        }
    }
}