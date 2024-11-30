# WatchNest
This is a revamped and refactored version of the WatchList Website, this will be the final version of this project and will continue on the front end development. For now, this project contains an API that handles CRUD operations for both users and Administrators. 

## Whats new
- Changed Seed file into a controller that is only usable for Administrator instead of being called in Middleware.
- Added cookies that will hold JWT authentication.
- Added proper use of caching.
- Added CORS, more info below.
- Added a filter for endpoints that required Authorization for Swagger, more info below.
- Added use of Global No Cache on enpoints.
- Added Pagination Helper class, more info below.
- Seperated Interfaces and Implementation in Models folder

# Overview
This Project is developed in .NET 6 and its main purpose is to manage and store user's series, movies, and videos that they have watched and filter their request by genre, Title, and/or provider. The API follows the RESTful convention and relies on Distributed SQL for storage and Caching, JWT for authentication and authorization, and cookies for storing JWT Bearer token. 

**This file is a Work in Progress**
