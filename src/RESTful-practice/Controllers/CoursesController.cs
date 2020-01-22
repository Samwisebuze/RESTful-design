using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.API.Controllers
{
    /// <summary>
    ///
    /// </summary>
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase // We can also inherit Controller, but that adds view support which is not necessary 
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            IEnumerable<Course> coursesFromRepo = _courseLibraryRepository.GetCourses(authorId);

            return Ok(_mapper.Map<IEnumerable<Course>>(coursesFromRepo));
        }


        [HttpGet("{courseId:guid}", Name = "GetCourseForAuthor")]
        public ActionResult<CourseDto> GetCourseForAuthor(
            [FromRoute]Guid authorId,
            [FromRoute]Guid courseId)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
                return NotFound();

            var courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseFromRepo == null)
                return NotFound();

            return Ok(_mapper.Map<CourseDto>(courseFromRepo));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(
            [FromRoute]Guid authorId,
            [FromBody]CourseForCreationDto courseForCreationDto)
        {
            if (!AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Course>(courseForCreationDto);
            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            _courseLibraryRepository.Save();

            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);

            return CreatedAtRoute(
                routeName: "GetCourseForAuthor",
                routeValues: new { authorId = courseToReturn.AuthorId, courseId = courseToReturn.Id },
                value: courseToReturn);
        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(
            [FromRoute]Guid authorId,
            [FromRoute]Guid courseId,
            [FromBody]CourseForUpdateDto courseForUpdate)
        {
            if (!AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseFromRepo == null)
            {
                Course courseToAdd = _mapper.Map<Course>(courseForUpdate);
                courseToAdd.Id = courseId;

                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();

                CourseDto courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute(
                    routeName: "GetCourseForAuthor",
                    routeValues: new { authorId = authorId, courseId = courseToReturn.Id },
                    value: courseToReturn
                );
            }

            // Maps the values from the source to destination
            // This allows us to update the values with out doing it piecewise manually
            _mapper.Map(source: courseForUpdate, destination: courseFromRepo);

            _courseLibraryRepository.UpdateCourse(courseFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();

        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(
            [FromRoute]Guid authorId,
            [FromRoute]Guid courseId,
            [FromBody] JsonPatchDocument<CourseForUpdateDto> patchDocument
        )
        {
            if (!AuthorExists(authorId))
                return NotFound();

            Course courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            // Upsert course
            if (courseFromRepo == null)
            {
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto))
                {
                    return ValidationProblem(ModelState);
                }

                var courseToAdd = _mapper.Map<Course>(courseDto);
                courseToAdd.Id = courseId;

                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute(
                    routeName: "GetCourseForAuthor",
                    routeValues: new { authorId = authorId, courseId = courseToReturn.Id },
                    value: courseToReturn
                );
            }
            CourseForUpdateDto courseToPatch = _mapper.Map<CourseForUpdateDto>(courseFromRepo);
            // add validation
            patchDocument.ApplyTo(courseToPatch, ModelState);

            if (!TryValidateModel(courseToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(source: courseToPatch, destination: courseFromRepo);

            _courseLibraryRepository.UpdateCourse(courseFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();
        }

        [HttpDelete("{courseId}")]
        public ActionResult DeleteCourseForAuthor(
            [FromRoute]Guid authorId,
            [FromRoute]Guid courseId)
        {
            if (!AuthorExists(authorId))
                return NotFound();

            Course courseFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseFromRepo == null)
                return NotFound();

            _courseLibraryRepository.DeleteCourse(courseFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();
        }

        /// <summary>
        /// Overrides the ValidationProblem implimentation in ControllerBase
        /// And uses our InvalidModeStatResponseFactory as defined in the Startup class.
        /// </summary>
        /// <param name="modelStateDictionary"></param>
        /// <returns> Standard compliant application/problem+json result</returns>
        public override ActionResult ValidationProblem(
            [ActionResultObjectValue]ModelStateDictionary modelStateDictionary)
        {
            IOptions<ApiBehaviorOptions> options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();

            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }

        private bool AuthorExists(Guid authorId) => _courseLibraryRepository.AuthorExists(authorId);
    }
}