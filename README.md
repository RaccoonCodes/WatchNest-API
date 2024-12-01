# WatchNest
This is a revamped and refactored version of the WatchList Website, this will be the final version of this project and will continue on the front end development. For now, this project contains an API that handles CRUD operations for both users and Administrators. 

## What's new
- Changed Seed file into a controller that is only usable for Administrator instead of being called in Middleware.
- Added cookies that will hold JWT authentication.
- Added proper use of caching.
- Added CORS, more info below.
- Added a filter for endpoints that required Authorization for Swagger, more info below.
- Added use of Global No Cache on enpoints.
- Added Pagination Helper class, more info below.
- Seperated Interfaces and Implementation in Models folder

**NOTE: When using Swagger, you may use either cookies or input token to access endpoints that require a login. If you are using cookies, just simply login in the login enpoint and you can use the restricted endpoint that are protected with Authorization attribute. You can also input the Bearer Token produced in login input when credentials are verified**

# Overview
This Project is developed in .NET 6 and its main purpose is to manage and store user's series, movies, and videos that they have watched and filter their request by genre, Title, and/or provider. The API follows the RESTful convention and relies on Distributed SQL for storage and Caching, JWT for authentication and authorization, and cookies for storing JWT Bearer token. 

I have set up 3 different host to be tested on:

`http://localhost:5000`

`http://localhost:5001`

`https://localhost:44350`

Ensure that whichever host you choose, add `/swagger/index.html` at the end of the URL to access Swagger.

## Packages Used 
Please, when using this project, download the necessary packages that was used for this project:


- Entity Framework 7.0.14
- Entity Framwork SQL 7.0.14
- Caching SqlServer 8.0.6
- Identity Framework 6.0.25
- JWT Authentication 6.0.29
- Authentication Cookies 2.1.2
- Swashbuckle 6.9.0
- Swashbuckle Annotation 6.4.0
- Linq.Dynamic.Core 1.4.5

## CORS
When running the application:
calling `http://localhost:5000` will redirect to `https://localhost:44350`, if you use any of the two mentioned host for swagger testing , you will be able to access the available endpoint. 

The third host, `http://localhost:5001` is not mentioned or part of the policy made by CORS in this project. Hence, when trying to access their endpoint in swagger, you will not be able to do so due to CORS not allowing the any outside domain or third party calls to access them becuase they are not part of their allowed acess. 

You can add or change the allowed orgins or host in appsettings.json 

**The purpose of CORS is to allow browsers to access resources by using HTTP requests initiated from scripts when those resources are located in their domains other than the one hosting the script. This helps with protecting the site with Cross-Site Request Forgery (CSRF) attacks**

## Test Login 
You can use the following credentials for testing:
- **User:**
  - `userName`: `TestUser`
  - `password`: `MyVeryOwnTestPassword123$`

- **Admin:**
  - `userName`: `TestAdministrator`
  - `password`: `MyVeryOwnTestPassword123$`

After a successful login, a JWT token will be returned

## Swagger Documentation Filter

There is an AuthRequirementFilter class that inherits `IOperationFilter`. The purpose of this class is add security requirement to endpoint that use the Authorize attribute along with status response code for unauthorized (401) and forbidden (403) if they are not already defined. This ensures that endpoints not marked with the Authorize attribute are excluded from these security requirement.

```csharp
 public void Apply(OpenApiOperation operation, OperationFilterContext context)
 {
     if (!context.ApiDescription.ActionDescriptor.EndpointMetadata
         .OfType<AuthorizeAttribute>().Any())
     {
         return;
     }
     operation.Security = new List<OpenApiSecurityRequirement>
     {
         new OpenApiSecurityRequirement
         {
             {
                 new OpenApiSecurityScheme
                 {
                     Name = "Bearer",
                     In = ParameterLocation.Header,
                     Reference = new OpenApiReference
                     {
                         Type= ReferenceType.SecurityScheme,
                         Id = "Bearer"
                     }
                 },
                 Array.Empty<string>()
             }
         }
     };

     if (!operation.Responses.ContainsKey("401"))
     {
         operation.Responses.Add("401", new OpenApiResponse
         {
             Description = "Unauthorized - Authentication is required and failed or was not provided."
         });
     }

     if (!operation.Responses.ContainsKey("403"))
     {
         operation.Responses.Add("403", new OpenApiResponse
         {
             Description = "Forbidden - You do not have permission to access this resource."
         });
     }


 }
```

## JWT
The information provided needed for creating the JWT is stored in User secrets for privacy and security resons. You will need to create your own JWT Payload. This typically will contain the following

```
"JWT": {
   "Issuer": "YourIssuerHere",
   "Audience": "YourAudienceHere",
   "SigningKey": "Create_A_Secure_Key_Here"
 }
```

The JWT Bearer is added to Authentication Middleware in `Program.cs`.

There are many resons for using JWT, for this case, I used it for security and performance improvement as JWT will contain encrypted information about user info and their roles. 
 
## Cookies
As mentioned before, I have added cookies in this projects. The cookies contain the Bearer Token produced by JWT. This way, when stored in cookies, JWTs are automatically sent with every request to the server since JWT itself are stateless. The Cookies information on name, security and expiration are on Program.cs

**The section below will be focusing on the Time Complexity on the Algorithms made for the operation made in controllers and methods.**

## Time Complexities

The Time complexity are focused on Model classes that are used by the controller since the business logic and action response are following the Seperation of Concerns. Before getting into each class and method, I wll mention the database set up.

### ApplicationDbContext
The databse has two tables, `SeriesModel` and `ApiUsers`. `SeriesModel` contains properies model for series that will hold SeriesID, UserID, Title, etc. `ApiUsers` inherits IdentityUser class that contains property for Users such as username, password, etc. It also contains a collection of `SeriesModel` object that will hold a one to many relationship between user and collection of their series. 

In the `ApplicationDbContext.cs`, I use Fluent API to model the tables for the database. I Added Index for performance and Time complexity improvement from WatchList V2. For `ApiUser`, I've added Indexing on `Id` and `UserName`. On `SeriesModel`, there is Indexing on UserID, Title, Genre, and SeriesID. 

### UserService Class
UserService class inherits IUserServices that contains `RegisterAsync` and `LoginAsync` method.

**RegisterAsync method**

This Method Registers users that will contain their UserName, Email, and Password. This inserts, after validation and assigning their role as a User, into the database. This Method has a **`Time Complexity of O(1)`**

**LoginAsync method**

This method searches and validate credentials. If successful, it generates a JWT Bearer that contains username, UserID, and their Role. it returns and encrypted JWT string or null when their is an invalid attempt. Becuase of Indexing, **`Time Complexity is O(lg n)`** where n is the number of records in the table.

### SeriesService Class
This class handles CRUD operations for series in each users. 

**CreateSeriesAsync method**

This method creates a series and insert it into the database based on UserID and the series info provided. Since this is a simple insertion with table index appropiately,**`Time Complexity is O(1)`**

**GetSeriesAsync method**

**This README is a Work in Progress**
