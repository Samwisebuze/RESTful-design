using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> source,
            string fields)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // create a list to hold our shaped expando objects
            var expandoObjectList = new List<ExpandoObject>();

            // create a list to hold the propertyinfo for the TSource object
            // Reflection is very expensive so we only want to do it once.
            // Since we don't care about the instances, only the class TSource we
            // can afford to do it once.
            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public properties should be in the expandoObject
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.IgnoreCase | 
                        BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // Separate the comma separated felids
                var fieldsAfterSplit = fields.Split(',');

                foreach (var field in fieldsAfterSplit)
                {
                    // trim each field, as their might be leading 
                    // or trailing spaces. You can't modify the var in foreach
                    // so we'll make a new var
                    var propertyName = field.Trim();

                    // use refelection to get the property on the source object
                    // we need to include public and instance , b/c specifying a binding
                    // flag overwrites the already existing binding flags.
                    var propertyInfo = typeof(TSource)
                        .GetProperty(propertyName, BindingFlags.IgnoreCase |
                            BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo == null)
                        throw new Exception($"Property {propertyName} wasn't found on" +
                            $" {typeof(TSource)}");

                    // add propertyInfo to List
                    propertyInfoList.Add(propertyInfo);
                }

            }

            // Run through all source objects
            foreach (var sourceObject in source)
            {
                // create an ExpandoObject that will hold the
                // selected properties & values
                var dataShapedObject = new ExpandoObject();

                // Get  the value of each property we have to return
                // For that, we run through the list.
                foreach (var propertyInfo in propertyInfoList)
                {
                    // GetValue returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    // Add field to the ExpandoObject
                    ((IDictionary<string, object>)dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }
                // add the expandoObject to the list
                expandoObjectList.Add(dataShapedObject);
            }

            return expandoObjectList;
        }
    }
}