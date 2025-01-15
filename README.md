# WatchNest
This is a revamped and refactored version of the WatchList Website, this will be the final version of this project and will continue on the front end development. For now, this project contains an API that handles CRUD operations for both users and Administrators. 

# Update 1/13/2025
Some major updates were made since the completion of front-end development:

- Removed caching to certain actions including profile caching in the API. They have been moved to the main website.

- Changed two action, GetAllUsers and GetAllSeries in Admin Controller, return types from generic to strongly type for type safety and clarity in knowing what type of return is expected for each action methods.

- Added New Models for the two actions previously mentioned.
- Added more methods in CORS to allow Cookies to be used as credentials in main app.
- Expiration for cookies has been extended to 30 min
- SortColumnValidation Attribute as been modified to check other properties
- Corrected and added proper JWT and Cookies validation to expire at the same time.

**Theses changes made little to no impact to performance so Time Complexity is still the same after these changes were made**

**The Main Application is Complete. It's on its own Repository called WatchNest-MainApp**


## Table of Contents
1. [What's New](#Whats-New)
2. [Overview](#Overview)
3. [Packages Used](#Packages-Used)
4. [CORS](#CORS)
5. [Test Login](#Test-Login)
6. [Swagger Documentation Filter](#Swagger-Documentation-Filter)
7. [JWT](#JWT)
8. [Cookies](#Cookies)
9. [Time Complexities](#Time-Complexities)
    - [ApplicationDbContext](#ApplicationDbContext)
    - [UserService Class](#UserService-Class)
    - [SeriesService Class](#SeriesService-Class)
    - [AdminService Class](#AdminService-Class)
10. [Controller Improvements](#Controller-Improvements)
11. [Pagination Helper](#PaginationHelper)
12. [DistributedCacheExtensions](#DistributedCacheExtensions)
13. [Attributes](#Attributes)
14. [Summary](#Summary)
    - [Future Updates](#Future-Updates)

## What's New
- Changed Seed file into a controller that is only usable for Administrator instead of being called in Middleware.
- Added cookies that will hold JWT authentication.
- Added proper use of caching.
- Added CORS, more info below.
- Added a filter for endpoints that required Authorization for Swagger, more info below.
- Added use of Global No Cache on endpoints.
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

After a successful login, the endpoint produces a string of JWT Bearer

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

This was, then, added to a middleware for AddSwaggerGen
```csharp
builder.Services.AddSwaggerGen(opts =>
{
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description ="Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme ="bearer"
    });
    opts.OperationFilter<AuthRequirementFilter>();
    opts.EnableAnnotations();
});
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

The Time complexity are focused on Model classes that are used by the controller since the business logic and action response are following the Seperation of Concerns. Thanks to indexing the database properly, the performance was improved from previous version which had no indexing. Before getting into the classes and methods, I wll mention the database set up.

### ApplicationDbContext
The databse has two tables, `SeriesModel` and `ApiUsers`. `SeriesModel` contains properies model for series that will hold SeriesID, UserID, Title, etc. `ApiUsers` inherits `IdentityUser` class that contains property for Users such as username, password, etc. It also contains a collection of `SeriesModel` object that will hold a one to many relationship between user and collection of their series. 

In the `ApplicationDbContext.cs`, I use Fluent API to model the tables for the database. I Added Index for performance and Time complexity improvement from WatchList V2. For `ApiUser`, I've added Indexing on `Id` and `UserName`. On `SeriesModel`, there is Indexing on UserID, Title, Genre, and SeriesID. 

### UserService Class
UserService class inherits IUserServices that contains `RegisterAsync` and `LoginAsync` method.

**RegisterAsync method**

This Method Registers users that will contain their UserName, Email, and Password. This inserts, after validation and assigning their role as a User, into the database. This Method has a **`Time Complexity of O(1)`**

**LoginAsync method**

This method searches and validate credentials. If successful, it generates a JWT Bearer that contains username, UserID, and their Role. it returns and encrypted JWT string or null when their is an invalid attempt. Becuase of Indexing, **`Time Complexity is O(lg n)`** where n is the number of records in the table.

### SeriesService Class
This class handles CRUD operations for series in each users. This has 4 impotant methods:
`CreateSeriesAsync`, `GetSeriesAsync`,`UpdateSeriesAsync` and `DeleteSeriesAsync`.

**CreateSeriesAsync method**

This method creates a series and insert it into the database based on UserID and the series info provided. Since this is a simple insertion with table index appropiately,**`Time Complexity is O(1)`**

**GetSeriesAsync method**

This method retrieves a collection of user's series list and turn them into paginated result where links are created based on page index, pagesize, and if there is filtering and sorting. There is caching in this method which can help performance along with using Iqueryable function which does lazy loading where it execute a query when it calls and store into a collection, in my case. 

The Time Complexity depends on the situation and the state of the method. If it is the first time using this method or the Cache is a miss, The **`Time Complexity is O(lg n + r )`** where r is the number of records matching the filter and n is number of series. If the cache is a Hit, then the **`Time Complexity is O(lg n)`** 

**UpdateSeriesAsync method**

This Method Updates existing series based on userID and SeriesID. There is Concurrency check in case ther was a change in the database before the user can do more changes. 
Overall **`Time Complexity is O(lg n)`**

**DeleteSeriesAsync method**

The DeleteSeriesAsync method deletes a series from the database using the provided series ID. Its **`Time complexity is O(lg n)`**, where n is the total number of series in the database. This is because of proper indexing in the database.

## AdminService Class

This class is for administration where it provides more action such as retriving all series and users in the database along with deleting a specific user. This class contains three methods: `DeleteUserAsync`, `GetAllUsersAsync`, and `GetAllSeriesAsync`.

**DeleteUserAsync method**

The DeleteUserAsync method removes a user from the identity database by their user ID. The **`Time complexity is O(lg n)`** where n is total number of users in the database. 

**GetAllUsersAsync method**

This method retrieves all the users in the database with pagination and caching, if cache is a miss, the **`Time complexity is O(lg n)`**. If cache is a hit, then the  **`Time complexity is O(1)`**

**GetAllSeriesAsync method**

This method is similar to GetAllUsersAsync method but only with series. It retrieves paginated unique series titles from the database with filtering and sorting. If caching is a miss, **`Time complexity is O(n lg n)`** where n is the total number of series records before applying pagination. If cache is a hit, then the **`Time complexity is O(1)`**

## Controller Improvements
Along side with improved performance in algorithms, I've also added Cache profiles that also improve performance in controllers and action. I have added the following Middleware in Program.cs

```csharp
builder.Services.AddControllers(opts =>
{
    opts.CacheProfiles.Add("NoCache", new CacheProfile()
    {
        Location = ResponseCacheLocation.None,
        NoStore = true
    });

    opts.CacheProfiles.Add("Any-60", new CacheProfile()
    {
        Location = ResponseCacheLocation.Any,
        Duration = 60
    });

});
```

The profiles I use are `Nocache` (which i do not use however left it there to show that it is possible to do so) and `Any-60`. `Any-60` stores the responses made by the action method in controllers for 60 seconds or 1 minute. This way it helps with performance by reducing repeated database queries, speeding up response times, and minimizing expensive operations. 

As mentioned before, I did not use `Nocache`. Instead, I used:

```csharp
app.Use((context, next) =>
{
    context.Response.Headers["cache-control"] = "no-cache, no-store";
    return next.Invoke();
});
```

This sets all endpoints or, in this case, action method to no-cache, no-store. It will not store any cache responses produced by these action. So the only way to store an action response is to explicitly use the attribute for the action method: 
`[ResponseCache(CacheProfileName = "Any-60")]`

which are appropiately seen in controllers respectively. 

Along side with using Response Caching in controllers, I have also added Pagination which will be the next improvement that I will mention in this project

## PaginationHelper

```csharp
 public static List<LinkDTO> GeneratePaginationLinks(string baseUrl, string rel, string action, int pageIndex,
     int pageSize, int totalPages,Dictionary<string,string>? additionalParams = null)
```

This static class generates paginated links for HATEOAS (Hypermedia as the Engine of Application State). The paginated links offered in this class provides previous, current, and next for paginated results provided in Model classes. As the name suggest, this is a helper classes that provides a way to use for all data types which makes it reusable for model classes and reduces redundancy. 

Its parameters provide the following:

`string baseUrl`: which is the base API URL endpoint

`string rel`: Describes the relationship of the link (Self, next, previous)

`string action`: Describes the action of the link (GET, POST, DELETE, etc) 

`int pageIndex`: Current index or page

`int pageSize`: Number of entity per page

`int totalPages`: Number of total pages based on page size

`Dictionary<string,string>? additionalParams`: Optional dictionary of additional query  to include in the links.

When used in model and returned in controllers, it provides a list of URL of links that provide  easy-to-follow navigation links. Also since it provides custom query parameters, if the users provides more info for pagination such as sorting and Filter, this methods handles it without needing to create a new method, allowing flexible pagination for complex use cases.

## DistributedCacheExtensions
This class provides helper methods for caching operations such as retrieving, sorting and generating cache keys. This was made for reusability.

```csharp
public static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
```

This method attempts to retrieve a cached value already stored and deserialize if it exist. It returns true if the key exist and deserialization succeeds, otherise, it returns false. 

```csharp
public static void Set<T>(this IDistributedCache cache, string key, T value, TimeSpan timeSpan)
```

This method stores the value in cache after serializing it to a JSON and setting an expiration time to the cache to be valid. 

```csharp
public static string GenerateCacheKey(this object input)
```

This method creates cache keys by serializing the object and hasing it using MD5 encryption. It returns a string of hashed string representing the serialized object.

This way, this class simplifies caching, which reduces boilerplate code or redundency, it also ensures type-safe retrieval of cached data with deserialization into the desired data type, improved cache efficiency from previous version (WatchList V2) by creating  cache key that are unique, reducing cache key collisions. 

## Attributes
In this Folder, I added two important classes, `SortColumnValidatorAttribute` and `SortOrderValidatorAttribute`.

**SortColumnValidatorAttribute**

This class is a custom validator for sorting specific data types. This ensures that services provide valid column names when sorting data. 

```csharp
 public SortColumnValidatorAttribute(Type entityType) : base("Value must match an existing column.")
     => EntityType = entityType;
```

This constructor accepts Type which will represents the entity type that will be validate against. It also sets an error message by default. 

```csharp
 protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
```

In this validation override, it checks if the value that the user inputs is not null or empty and if the entity type has propertgy matching the value. It returns the results of the validation. 

This Attribute gets used mostly in my DTO classes. For example, in my `RequestDTO`, I used in:

```csharp
 [DefaultValue("TitleWatched")]
 [SortColumnValidator(typeof(SeriesDTO))]
 public string? SortColumn { get; set; } = "TitleWatched";
```

This DTO is used to contain User's request for Get action method. This sorts user's series based on valid properties that has data type 'string' in SeriesDTO which contains
are:

`public string UserID `

`public string TitleWatched`

`public string ProviderWatched`

`public string Genre`

This way it encapsulates validation rules by using attributes, reduces the need to manually validate multiple places, prevents invalid sorting, and easly used for any model property by specifying the target entity type. 

**SortOrderValidatorAttribute**

This class is also a custom validator but for sorting order which are given two predefined sort order "ASC" and "DESC". 

```csharp
 public SortOrderValidatorAttribute() : base("Values must be one of the following {0}") { }
```

The constructor initializes the attribute with the default error message which will display the two predefined allowed string values. 

```csharp
 protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
```

The same as `SortColumnValidatorAttribute` ovveride, but instead of checking for entity property, it checks if the user's sorting value is valid.

This attribute is used in DTO classes for example, again, in my `RequestDTO`, 

```csharp
[SortOrderValidator]
public string? SortOrder { get; set; } = "ASC";
```

The attributes sets the propeties to validate against the two predefined allowed order. Which validate logic in one place. It is also possible to extend additional values, for example:

```csharp
[SortOrderValidator(AllowedValues = new[] { "ASC", "DESC", "RANDOM" })]
public string? SortOrder { get; set; } = "ASC";
```

which makes the 3 defined sorting strings the only valid values.

## Summary
In this project, I focused on performance and refactored the code from my provious work (which was WatchListV2_RESTAPI). I also included the Time Complexity of all important algorithms that I have created. Below is a quick list and recap of major changes and improvement made in this project:

- Altered Seed file and made into a controller that is only usable for Administrator
- Added cookies that will hold JWT authentication.
- Added proper use of Distributed Caching and Profile Caching.
- Added CORS for Cross-Site Request Forgery attacks.
- Added filter for endpoints that required Authorization for Swagger testing.
- Added Pagination Helper class.
- Indexed necessary propeties for performance Improvements
- Seperated Interfaces and Implementation in Models folder
- Added Time Complexity to show improvements made for action methods.

## Future Updates
As mentioned before, this will be the last version of this project I will make. Therefore, back-end work is finished (unless I come across a bug or need to update certain functionalities). Moving forward, I will start developing front-end work for this project that it desperately needs.

Once the front end is finished I will update this project and add a seperate README specifically for front-end development

Have a nice day!
