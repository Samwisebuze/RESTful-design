using System;

namespace CourseLibrary.API.Helpers
{
    public static class DateTimeOffestExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffest, DateTimeOffset? dateOfDeath)
        {
            DateTime dateToCalculateTo = DateTime.UtcNow;

            if (dateOfDeath.HasValue)
            {
                dateToCalculateTo = dateOfDeath.Value.UtcDateTime;
            }

            int age = dateToCalculateTo.Year - dateTimeOffest.Year;

            if (dateToCalculateTo < dateTimeOffest.AddYears(age))
            {
                age--;
            }

            return age;
        }
    }
}