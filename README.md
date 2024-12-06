```
██████╗░░█████╗░░██╗░░░░░░░██╗██╗███╗░░██╗
██╔══██╗██╔══██╗░██║░░██╗░░██║██║████╗░██║
██║░░██║███████║░╚██╗████╗██╔╝██║██╔██╗██║
██║░░██║██╔══██║░░████╔═████║░██║██║╚████║
██████╔╝██║░░██║░░╚██╔╝░╚██╔╝░██║██║░╚███║
╚═════╝░╚═╝░░╚═╝░░░╚═╝░░░╚═╝░░╚═╝╚═╝░░╚══╝

░█████╗░██╗░░░██╗████████╗██╗░░██╗░█████╗░██████╗░██╗███████╗░█████╗░████████╗██╗░█████╗░███╗░░██╗
██╔══██╗██║░░░██║╚══██╔══╝██║░░██║██╔══██╗██╔══██╗██║╚════██║██╔══██╗╚══██╔══╝██║██╔══██╗████╗░██║
███████║██║░░░██║░░░██║░░░███████║██║░░██║██████╔╝██║░░███╔═╝███████║░░░██║░░░██║██║░░██║██╔██╗██║
██╔══██║██║░░░██║░░░██║░░░██╔══██║██║░░██║██╔══██╗██║██╔══╝░░██╔══██║░░░██║░░░██║██║░░██║██║╚████║
██║░░██║╚██████╔╝░░░██║░░░██║░░██║╚█████╔╝██║░░██║██║███████╗██║░░██║░░░██║░░░██║╚█████╔╝██║░╚███║
╚═╝░░╚═╝░╚═════╝░░░░╚═╝░░░╚═╝░░╚═╝░╚════╝░╚═╝░░╚═╝╚═╝╚══════╝╚═╝░░╚═╝░░░╚═╝░░░╚═╝░╚════╝░╚═╝░░╚══╝

███╗░░░███╗██╗██████╗░██████╗░██╗░░░░░███████╗░██╗░░░░░░░██╗░█████╗░██████╗░███████╗
████╗░████║██║██╔══██╗██╔══██╗██║░░░░░██╔════╝░██║░░██╗░░██║██╔══██╗██╔══██╗██╔════╝
██╔████╔██║██║██║░░██║██║░░██║██║░░░░░█████╗░░░╚██╗████╗██╔╝███████║██████╔╝█████╗░░
██║╚██╔╝██║██║██║░░██║██║░░██║██║░░░░░██╔══╝░░░░████╔═████║░██╔══██║██╔══██╗██╔══╝░░
██║░╚═╝░██║██║██████╔╝██████╔╝███████╗███████╗░░╚██╔╝░╚██╔╝░██║░░██║██║░░██║███████╗
╚═╝░░░░░╚═╝╚═╝╚═════╝░╚═════╝░╚══════╝╚══════╝░░░╚═╝░░░╚═╝░░╚═╝░░╚═╝╚═╝░░╚═╝╚══════╝
```

### What is Darwin Authorization Middleware?

Darwin Authorization Middleware is a package made in C# to be used in .Net applications that validates the authorization models (Bearer Token"JWT" and API Key) for resource use and can also send the request to the OPA Service to validate the necessary rules.

## Required
* .Net 6 -> https://dotnet.microsoft.com/en-us/download/dotnet/6.0

### Release Version
| Version | Release Notes | Status |
|---------|---------------|--------|
| 1.0.8   | Updated correct flow. | Build Success [![Build Status](https://jenkins.crossknowledge.com/buildStatus/icon?job=WLS%2Fk8s%2Fdarwin-authorization-middleware)](https://jenkins.crossknowledge.com/job/WLS/job/k8s/job/darwin-authorization-middleware/) |

### How to install middleware?
Must be connected to Wiley's network/VPN
#### Visual Studio
In case you are using Visual Studio go to confluence to see step-by-step to get from Package Management: https://confluence.wiley.com/display/PL/%5BDotNet%5D+DarwinAuthorization+Middleware

#### VS Code
For those using VSCode you can add the package via dotnet cli using the following command line:

```shell
dotnet add package DarwinAuthorization --version 1.0.0
```

### How to use middleware

#### Startup.cs
After importing the package you need to configure two extensions in your Startup.cs

```csharp
// Add the using of DarwinAuthorization Package
using DarwinAuthorization; 

// Add Authentication Schemes for Users API v4 Jwt and ApiKey
// Add Authorization Policy using OPA
services.AddDarwinAuthzConfiguration(); 

// Injects AuthenticationContextMiddleware: This middleware will have the information about authentication context, such as hasJwt, hasApiKey, userId
app.UseDarwinAuthenticationContext();
```
** Make sure the UseDarwinAuthenticationContext() call needs to be in the following order in the API configuration steps.
```csharp
app.UseRouting();
app.UseDarwinAuthenticationContext();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
```
#### AuthenticationContext
You can simply inject the AuthenticationContext in you Controller, with this you'll be able to access some authentication context information.
```csharp
public class ExampleController: ControllerBase
{
    private readonly DarwinAuthorizationContext _darwinAuthorizationContext;

    public ExampleController(DarwinAuthorizationContext darwinAuthorizationContext) {
        _darwinAuthorizationContext = darwinAuthorizationContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get(){
        _darwinAuthorizationContext.HasJwt;
        _darwinAuthorizationContext.HasApiKey;
        _darwinAuthorizationContext.UserId;
    }
}

```

#### All Controllers routes
Add the [Authorize] annotation in the methods that you want to protect with authentication/authorization.
```csharp
//Add Authorize to each controller method.
[HttpGet("get")]
[Authorize]
public IActionResult Get()
{
    return Ok(_authorizationContext.UserId);
}
```
#### Env-Vars
Environment variables that need to exist in the application to use the middleware.

```json
//Required
"KEYCLOAK_BASE_URL": "https://keycloak.nonprod.darwin.wiley.host", 
//Required
"KEYCLOAK_REALM": "darwin",
//Required
"KEYCLOAK_AUDIENCE": "account",
// Only if this application use API Key
"API_KEY": "<YOUR API KEY>",
//Required
"OPA_BASE_URL": "https://opa.{env}.darwin.wiley.host" 
```
### OPA Service - Create new policies for new endpoints.
When we create new controllers/endpoints in an API we need to develop new policies to be validated within the OPA Service.
To create a new policy, it is necessary to add it and make a new implementation of the following repository https://github.com/wiley/darwin_opa.
This policy can only be a policy that allows full access.

As in the example below:
```go
package darwin.resources.organization_roles

import data.darwin.authz.dinput # always use dinput instead of input
import data.darwin.token.get_jwt_payload
import data.darwin.token.jwt_is_valid

default allow = false

allow {
	dinput.path[1] == "organization-roles"
}
```
**Note: Once you add this project to a new API you will need to have the rules configured, and you probably won't have OPA already configured and you won't be able to test locally.
As mentioned above you can create the rules in this repository and deploy them in the development environment.

QE: https://opa.qe01.darwin.wiley.host/

NonProd: https://opa.nonprod.darwin.wiley.host/

Prod: https://opa.prod.darwin.wiley.host/



** It is being worked on a way to use this project locally in a simpler way.

### Requests for the API that uses the middleware.
After downloading the package in your application, your requests should be as follows when using Api Key or JWT

#### Request with Token Bearer Returns 200

```shell
GET https://localhost:7027/api/v4/Resource

Authorization Bearer <JWT Token>
```

#### Request with ApiKey Returns 200
```shell
GET https://localhost:7027/api/v4/Resource
X-Api-Key <API_KEY>
X-User-Id <User ID>
X-Authz-Data <> 1 / null / not passed
```

#### Request with ApiKey Returns 200 without call OPA
```shell
GET https://localhost:7027/api/v4/Resource
X-Api-Key <API_KEY>
X-User-Id <User ID>
X-Authz-Data 1
```

#### Request without ApiKey or Token Bearer Returns 401
```shell
GET https://localhost:7027/api/v4/Resource
```

#### Request with ApiKey invalid Returns 401
```shell
GET https://localhost:7027/api/v4/Resource
X-Api-Key <INVALID API_KEY>
X-User-Id <User ID>
```

### Not Authorized by Policies in OPA
You're going to receive a 403 response if your resource is not authorized by OPA (check the policies)

### Tests (Unit/API)
This project contains unit tests that test the methods and services that surround the middleware

#### Run Unit Test
```shell
dotnet test .\DarwinAuthorization.UnitTests\
```

### For more information about flow and documentation, visit:
https://confluence.wiley.com/display/PL/%5BDotNet%5D+DarwinAuthorization+Middleware
