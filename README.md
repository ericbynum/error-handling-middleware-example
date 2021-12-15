# Error Handling Middleware Example
.NET 6 Web API example to demonstrate how to create middleware that can handle
errors at a global level.

## Prerequisites
* .NET 6
* Visual Studio 2022, VS Code, or Jetbrains Rider

## Getting Started
1. Clone this repo
2. Run the app to view swagger docs
3. Execute the swagger endpoints to demonstrate how exceptions can be caught by
middleware and respond with ProblemDetails, a standard for relaying
errors in http apis. Read more: https://datatracker.ietf.org/doc/html/rfc7807