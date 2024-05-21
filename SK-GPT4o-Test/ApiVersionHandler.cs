using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SK_GPT4o_Test
{
    // based on the following document
    // https://github.com/microsoft/semantic-kernel/issues/2833

    public class ApiVersionHandler : DelegatingHandler
    {
        private const string ApiVersionKey = "api-version";
        private const string NewApiVersion = "2024-04-01-preview";

        public ApiVersionHandler() : base(new HttpClientHandler())
        {

        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var uriBuilder = new UriBuilder(request.RequestUri!);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if (query[ApiVersionKey] == null) return await base.SendAsync(request, cancellationToken);

            query[ApiVersionKey] = NewApiVersion;
            uriBuilder.Query = query.ToString();
            request.RequestUri = uriBuilder.Uri;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
