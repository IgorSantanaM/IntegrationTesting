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

namespace Customers.Api.Tets.Integration.CustomerController
{
    public class CreateCustomerControllerTests : IClassFixture<CustomerApiFactory>
    {
        private readonly CustomerApiFactory _apiFactory;
        private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.GitHubUsername, CustomerApiFactory.VALID_GITHUB_USER)
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);
        private readonly HttpClient _client;
        public CreateCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
            _client = _apiFactory.CreateClient();
        }

        [Fact]
        public async Task Create_CreatesUser_WhenDataIsValid()
        {
            // Arrange
            var customer = _customerGenerator.Generate();

            // Act
            var response = await _client.PostAsJsonAsync("customers", customer);

            // Assert
            var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            customerResponse.Should().BeEquivalentTo(customer);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().Be($"http://localhost/customers/{customerResponse.Id}");
        }
    }
}
