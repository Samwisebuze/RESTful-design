using System;
using System.Collections.Generic;
using System.Linq;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string>(){ "Id"} )},
                {"MainCategory", new PropertyMappingValue(new List<string>(){ "MainCategory"} )},
                {"Age", new PropertyMappingValue(new List<string>(){ "DateOfBirth"} )},
                {"Name", new PropertyMappingValue(new List<string>(){ "FirstName", "LastName"} )},
            };

        private Dictionary<string, PropertyMappingValue> _coursePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string>(){ "Id"} )},
                {"Title", new PropertyMappingValue(new List<string>(){ "Title"} )},
                {"Description", new PropertyMappingValue(new List<string>(){ "Description"} )},
                {"AuthorId", new PropertyMappingValue(new List<string>(){ "AuthorId" } )},
            };


        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
             _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
             _propertyMappings.Add(new PropertyMapping<CourseDto, Course>(_coursePropertyMapping));
        }
        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMappings = _propertyMappings
                .OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMappings.Count() == 1)
            {
                return matchingMappings.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance " +
                $"for <{typeof(TSource)},{typeof(TDestination)}>");
            

        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
                return true;

            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();

                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                if (!propertyMapping.ContainsKey(propertyName))
                    return false;

            }
            return true;
        }

    }
}