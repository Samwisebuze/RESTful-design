using System.Collections.Generic;
using CourseLibrary.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class RootController : ControllerBase 
    {
        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot(){
            var links = new List<LinkDto>();

            links.Add(
                new LinkDto(Url.Link("GetRoot", new {}),
                "self",
                HttpMethods.Get));

            links.Add(
                new LinkDto(Url.Link("GetAuthors", new {}), 
                "authors", 
                HttpMethods.Get));
            
            links.Add(
                new LinkDto(Url.Link("CreateAuthor", new {}), 
                "create_author", 
                HttpMethods.Post));

            return Ok(links); 

        }
    }
}