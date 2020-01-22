using System;

namespace CourseLibrary.API.Helpers
{
    public static class DateTimeOffestExtensions
    {
        public static int GetCurrentAge(this System.DateTimeOffset dateTimeOffest)
        {
            var currentDate = DateTime.UtcNow;
            int age = currentDate.Year - dateTimeOffest.Year;
            
            if (currentDate < dateTimeOffest.AddYears(age)){
                age--;
            }

            return age;
        }
    }
}