using System.Collections.Generic;
using System.Linq;
using CourseLibrary.API.Services;
using System.Linq.Dynamic.Core;
using System;

namespace CourseLibrary.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> source,
            string orderby,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (mappingDictionary == null)
                throw new ArgumentNullException(nameof(mappingDictionary));

            if (string.IsNullOrWhiteSpace(orderby))
                return source;

            // OrderBy is seperated by a comma for each term
            var orderByAfterSplit = orderby.Split(',');

            // Apply the orderBy clauses in reverse order
            // otherwise the IQueryable will be ordered wrong.
            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                // Trim each clause, as it might contain leading
                // or trailing spaces. Can't modify the var in foreach.
                var trimmedOrderByClause = orderByClause.Trim();

                //if the order clause ends "desc" then,
                // we order descending, otherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                //remove the "asc" or "desc" from the clause, so we
                // can isolate the property name to order with
                var idxOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = idxOfFirstSpace == -1 ?
                    trimmedOrderByClause : trimmedOrderByClause.Remove(idxOfFirstSpace);

                // find the property mapping
                if (!mappingDictionary.ContainsKey(propertyName))
                    throw new ArgumentException($"Key mapping for {propertyName} is missing.");

                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue == null)
                    throw new ArgumentException(nameof(propertyMappingValue));

                foreach (var destinationProperty in
                    propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                        orderDescending = !orderDescending;

                    source = source.OrderBy(destinationProperty +
                        (orderDescending ? " descending" : " ascending"));
                }
            }

            return source;
        }
    }
}