# RESTful-design
This repo tracks work done while following Kevin Dockx's courses on Pluralsight. These courses explore what it means for an API to be RESTful and how to properly implment a RESTful API using ASP.NET Core 3. The first course led me through level 0-2 of the Richardson Maturity Model for an API. Along the way I learned more about the .NET Core Framework and the differne infrastructure it provides to create well-crafter RESTful APIs.

The second course covers more complex topics such as HATEOAS(Hypermedia as the Engine of Application State), Concurrency, Caching, and more. You can see examples of these concepts and more in the src folder of this repository. 

# Notes

> REST is an architectural framework implmented through a set of constraints

### REST Constraints

#### 1. Uniform Interface

The API and consumers must share a consistent contract, comprised of:

- URI (Uniform Resource Identifier)
- HTTP Verbs (GET, POST, PUT, PATCH, DELETE, OPTIONS, HEAD)
- Media Types (application/xml, application/json, application/problem+json, etc.)

The tighter this contract is  the more reliable and evolvable the API will become. The contract is comprise of three parts: the URI, the HTTP method, and the payload(requeast AND response).

There are also four sub-constraints of the Uniform Interface constraint:

1. Individual resources should be identified in request using URIs. Resouces are conceptually seperate from their representation. (i.e the response format shouldn't depend on the resource)
2. When a client holds a representation of a resource, it must have enough information to modify or delete the resource on the server. This includes metadata sent with responses and permissions.
3. Each message must include enough information to describe how to process the message. This includes both request and response messages, by having proper headers and body formats.
4. Allow for a self-documenting API. This is HATEOAS
   
#### 2. Client-Server
#### 3. Statelessness
#### 4. Layered System
#### 5. Cacheable

> Each response must explicitly state if it can be chaced or not.

The goal of chaching is to save on repeated client-server interaction and (more importantly) prevent clients from operating on out of date data.

### Richardson Maturity Model

### Disclaimer
This is not an original project and I do not claim any of this code as my own. I was merely following along with the courses and wanted to save my own documented version for future reference. If you would like to see the original code as writtin by Kevin Dockx you can find it at [this]() link.

## Reference

[RESTful API w/ASP.NET Core 3](https://app.pluralsight.com/library/courses/asp-dot-net-core-3-restful-api-building/description)

[Advanced RSETful API Concerns Course](https://app.pluralsight.com/library/courses/asp-dot-net-core-3-advanced-restful-concerns/description)
