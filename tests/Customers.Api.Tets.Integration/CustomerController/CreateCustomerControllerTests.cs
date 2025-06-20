using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using Customers.Api.Tests.Integration;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg.Sig;

namespace Customers.Api.Tets.Integration.CustomerController
{
    public class CreateCustomerControllerTests : IClassFixture<CustomerApiFactory>
    {
        private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.VALID_GITHUB_USER)
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);
        private readonly HttpClient _client;
        public CreateCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _client = apiFactory.CreateClient();
        }

        [Fact]
        public async Task Create_CreatesUser_WhenDataIsValid()
        {
            // Arrange
            var customer =  _customerGenerator.Generate();

            // Act
            var response = await _client.PostAsJsonAsync("customers", customer);

            // Assert
            var customerResponse = response.Content.ReadFromJsonAsync<CustomerResponse>();

            customerResponse.Should().BeEquivalentTo(customer, options => options.ExcludingMissingMembers());
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location!.ToString().Should().Be($"http://localhost/customer/{customerResponse.Id!}");
        }

        [Fact]
        public async Task Create_ReturnsValidationError_WhenEmailIsInvalid()
        {
            // Arrange
            const string invalidEmail = "dasdadwdads";
            var customer = _customerGenerator.Clone()
                    .RuleFor(x => x.Email, invalidEmail).Generate();
            // Act
            var response = await _client.PostAsJsonAsync("customers", customer); 

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            error!.Status.Should().Be(400);
            error.Title.Should().Be("One or more validation errors occurred.");
            error.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
        }
    }
}