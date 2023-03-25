# ASP.NET Core template

A Template for a ASP.NET Core API with the following features:
- CQRS
- AutoMapper
- Production ENV config loader
- Docker Support
- UnitTest

## Running Unit tests

Running the unit tests in a pipeline can be achieved by running the docker-compose file in the UnitTests solution.
It will return 0 when the unit tests succeed, but it will return -1 when one or multiple tests fail.