using Farmundo.Demo.Api.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Farmundo.Demo.Api.Configurations
{
    public class JwtBearerConfigOptions(IConfiguration config) : IConfigureNamedOptions<JwtBearerOptions>
    {
        public void Configure(JwtBearerOptions options)
        {
            config.GetSection(ApiConstants.JwtConfigurationSectionName).Bind(options);
        }

        public void Configure(string? name, JwtBearerOptions options)
        {
            Configure(options);
        }
    }
}
