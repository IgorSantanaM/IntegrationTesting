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
        public CreateCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        [Fact]
        public async Task Test()
        {
            await Task.Delay(5000);
        }
    }
}
