using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.Api.Tets.Integration
{
    [CollectionDefinition("CustomerApi Collection")]
    public class TestCollection : ICollectionFixture<WebApplicationFactory<IApiMarker>>
    {
    }
}
