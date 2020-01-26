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
using System.Linq;
using Microsoft.Net.Http.Headers;

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
        private readonly IPropertyCheckerService _propertyCheckerService;
        private readonly IMapper _mapper;

        public AuthorsController(
            ICourseLibraryRepository courseLibraryRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ??
                throw new ArgumentNullException(nameof(propertyCheckerService));

        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public IActionResult GetAuthors(
            [FromQuery]AuthorsResourceParameters authorsResourceParamaters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Entities.Author>(
                authorsResourceParamaters.OrderBy))
                return BadRequest();

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(authorsResourceParamaters.Fields))
                return BadRequest();

            PagedList<Author> authorsFromRepo = _courseLibraryRepository.GetAuthors(authorsResourceParamaters);

            var paginationMetadata = new
            {
                totalCount = authorsFromRepo.TotalCount,
                totalPages = authorsFromRepo.TotalPages,
                pageNumber = authorsFromRepo.CurrentPage,
                pageSize = authorsFromRepo.PageSize,
            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForAuthors(
                authorsResourceParamaters,
                authorsFromRepo.HasNext,
                authorsFromRepo.HasPrevious);

            var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                .ShapeData(authorsResourceParamaters.Fields);

            var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
            {
                var authorAsDictionary = author as IDictionary<string, object>;
                var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], null);
                authorAsDictionary.Add("links", links);
                return authorAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAuthorsWithLinks,
                links
            };

            return Ok(linkedCollectionResource);
        }

        [HttpGet("{authorId:guid}", Name = "GetAuthor")]
        [HttpHead]
        [Produces("application/json",
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.author.full+json",
            "application/vnd.marvin.author.full.hateoas+json",
            "application/vnd.marvin.author.friendly+json",
            "application/vnd.marvin.author.friendly.hateoas+json")]
        public IActionResult GetAuthor(
            [FromRoute]Guid authorId,
            [FromQuery]string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!IsSupportedMediaType(mediaType, out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
                return BadRequest();

            Author authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            bool includeLinks = parsedMediaType.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);


            IEnumerable<LinkDto> links = new List<LinkDto>();

            if (includeLinks)
            {
                links = CreateLinksForAuthor(authorId, fields);
            }

            var primaryMediaType = includeLinks ?
                parsedMediaType.SubTypeWithoutSuffix
                .Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - ".hateoas".Length)
                : parsedMediaType.SubTypeWithoutSuffix;

            // Full Author
            if (primaryMediaType == "vnd.marvin.author.full")
            {
                var fullResource = _mapper.Map<AuthorFullDto>(authorFromRepo)
                    .ShapeData(fields) as IDictionary<string, object>;

                if (includeLinks)
                    fullResource.Add("links", links);

                return Ok(fullResource);
            }

            var friendlyResource = _mapper.Map<AuthorDto>(authorFromRepo)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
                friendlyResource.Add("links", links);


            return Ok(friendlyResource);

        }

        [HttpPost(Name = "CreateAuthor")]
        public ActionResult<AuthorDto> CreateAuthor(
            [FromBody]AuthorForCreationDto author,
            [FromHeader(Name = "Accept")]string mediaType)
        {
            if (!IsSupportedMediaType(mediaType, out MediaTypeHeaderValue parsedMediaType))
                return BadRequest();


            var authorEntity = _mapper.Map<Entities.Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            if (parsedMediaType.MediaType == "application/vnd.marvin.hateoas+json")
            {
                var links = CreateLinksForAuthor(authorToReturn.Id, string.Empty);

                var linkedResourceToReturn = authorToReturn.ShapeData(string.Empty)
                    as IDictionary<string, object>;

                linkedResourceToReturn.Add("links", links);

                return CreatedAtRoute(
                    routeName: "GetAuthor",
                    routeValues: new { authorId = linkedResourceToReturn["Id"] },
                    value: linkedResourceToReturn);
            }

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

        [HttpDelete("{authorId}", Name = "DeleteAuthor")]
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
                            fields = parameters.Fields,
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
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            pageNumber = parameters.PageNumber - 1,
                            pageSize = parameters.PageSize,
                            mainCategory = parameters.MainCategory,
                            searchQuery = parameters.SearchQuery
                        });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetAuthors",
                        new
                        {
                            fields = parameters.Fields,
                            orderBy = parameters.OrderBy,
                            pageNumber = parameters.PageNumber,
                            pageSize = parameters.PageSize,
                            mainCategory = parameters.MainCategory,
                            searchQuery = parameters.SearchQuery
                        });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForAuthor(
            Guid authorId,
            string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDto(
                        Url.Link(
                            "GetAuthor",
                            new { authorId }),
                        "self",
                        HttpMethods.Get));
            }
            else
            {
                links.Add(
                    new LinkDto(
                        Url.Link(
                            "GetAuthor",
                            new { authorId, fields }),
                        "self",
                        HttpMethods.Get));
            }

            links.Add(
                new LinkDto(
                    Url.Link("DeleteAuthor", new { authorId }),
                    "delete_author",
                    HttpMethods.Delete));

            links.Add(
                new LinkDto(
                    Url.Link("CreateCOurseForAuthor", new { authorId }),
                    "create_course_for_author",
                    HttpMethods.Post));

            links.Add(
                new LinkDto(
                    Url.Link("GetCoursesForAuthor", new { authorId }),
                    "courses",
                    HttpMethods.Get
                )
            );

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(
            AuthorsResourceParameters parameters,
            bool hasNext, bool HasPrevious
        )
        {
            var links = new List<LinkDto>();

            // self
            links.Add(
                new LinkDto(CreateAuthorsResourceUri(
                    parameters, ResourceUriType.Current
                ), "self", HttpMethods.Get));

            if (hasNext)
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(parameters, ResourceUriType.NextPage),
                    "nextPage", HttpMethods.Get));

            if (HasPrevious)
                links.Add(
                    new LinkDto(CreateAuthorsResourceUri(parameters, ResourceUriType.PreviousPage),
                    "previousPage", HttpMethods.Get));



            return links;
        }

        private bool IsSupportedMediaType(string mediaType, out MediaTypeHeaderValue parsedMediaType) =>
            MediaTypeHeaderValue.TryParse(mediaType, out parsedMediaType);
    }
}