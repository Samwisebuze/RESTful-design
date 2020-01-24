using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParamaters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseLibrary.API.Controllers
{
    /// <summary>
    ///
    /// /// </summary>
    [ApiController]
    [Route("api/authors/")]
    public class AuthorsController : ControllerBase // We can also inherit Controller, but that adds view support which is not necessary 
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IMapper _mapper;

        public AuthorsController(
            ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));

        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public ActionResult<IEnumerable<Author>> GetAuthors(
            [FromQuery]AuthorsResourceParameters authorsResourceParamaters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Entities.Author>(authorsResourceParamaters.OrderBy))
                return BadRequest();

            PagedList<Author> authorsFromRepo = _courseLibraryRepository.GetAuthors(authorsResourceParamaters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorsResourceParamaters, 
                ResourceUriType.PreviousPage) : null;
            var nextPageLink = authorsFromRepo.HasNext ?
                CreateAuthorsResourceUri(authorsResourceParamaters, 
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new 
            {
                totalCount = authorsFromRepo.TotalCount,
                totalPages = authorsFromRepo.TotalPages,
                pageNumber = authorsFromRepo.CurrentPage,
                pageSize = authorsFromRepo.PageSize,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));
                
            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
        }

        [HttpGet("{authorId:guid}", Name = "GetAuthor")]
        [HttpHead]
        public ActionResult<AuthorDto> GetAuthor(
            [FromRoute]Guid authorId)
        {
            Author authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor([FromBody]AuthorForCreationDto author)
        {
            var authorEntity = _mapper.Map<Entities.Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute(
                routeName: "GetAuthor",
                routeValues: new { authorId = authorToReturn.Id },
                value: authorToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow",
                string.Join(',', new[] { HttpMethods.Get, HttpMethods.Options, HttpMethods.Post }));
            return Ok();
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(
            [FromRoute]Guid authorId)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
                return NotFound();

            _courseLibraryRepository.DeleteAuthor(authorFromRepo);
            _courseLibraryRepository.Save();

            return NoContent();
        }

        private string CreateAuthorsResourceUri(
            AuthorsResourceParameters parameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            orderBy = parameters.OrderBy,
                            pageNumber = parameters.PageNumber + 1,
                            pageSize = parameters.PageSize,
                            mainCategory = parameters.MainCategory,
                            searchQuery = parameters.SearchQuery
                        });
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                        new
                        {
                            orderBy = parameters.OrderBy,
                            pageNumber = parameters.PageNumber - 1,
                            pageSize = parameters.PageSize,
                            mainCategory = parameters.MainCategory,
                            searchQuery = parameters.SearchQuery
                        });
                default:
                    return Url.Link("GetAuthors",
                        new
                        {
                            orderBy = parameters.OrderBy,
                            pageNumber = parameters.PageNumber,
                            pageSize = parameters.PageSize,
                            mainCategory = parameters.MainCategory,
                            searchQuery = parameters.SearchQuery
                        });
            }
        }

    }
}