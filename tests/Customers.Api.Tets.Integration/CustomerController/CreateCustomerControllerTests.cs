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

namespace Customers.Api.Tets.Integration.CustomerController
{
    [Collection("CustomerApi Collection")]
    public class CreateCustomerControllerTests :  IAsyncLifetime //, IClassFixture<WebApplicationFactory<IApiMarker>>
    {
        private readonly HttpClient _httpClient;
        private readonly Faker<CustomerRequest> _customerRequest = new Faker<CustomerRequest>()
            .RuleFor(x => x.FullName, faker => faker.Person.FullName)
            .RuleFor(x => x.Email, faker => faker.Person.Email)
            .RuleFor(x => x.GitHubUsername, "nickchapsas")
            .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        private readonly List<Guid> _createdIds = new();

        public CreateCustomerControllerTests(WebApplicationFactory<IApiMarker> appFactory)
        {
            _httpClient = appFactory.CreateClient();
        }

        [Fact]
        public async Task Create_ReturnsCreated_WhenCustomerIsCreated()
        {
            // Arrange
            var customer = _customerRequest.Generate();

            // Act
            var response = await _httpClient.PostAsJsonAsync("customers", customer);

            // Assert
            var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            customerResponse.Should().BeEquivalentTo(response);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            _createdIds.Add(customerResponse!.Id);
        }
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach (var createdId in _createdIds)
            {
                await _httpClient.DeleteAsync($"customers/{createdId}");
            }
        }
    }
}
