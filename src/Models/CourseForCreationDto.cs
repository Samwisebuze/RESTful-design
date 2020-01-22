
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CourseLibrary.ValidationAttributes;

namespace CourseLibrary.API.Models
{
    public class CourseForCreationDto : CourseForManipulationDto //: IValidatableObject  
    {
        // public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        // {
        //     if (Title == Description)
        //     {
        //         yield return new ValidationResult(
        //             "The provided course description should be different from the title.",
        //             new[] { nameof(CourseForCreationDto) });
        //     }
        // }
    }
} 