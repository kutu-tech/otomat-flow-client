using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Otomat.Client
{
    class Program
    {
         private static string IdpUri => "IDP_URL_#####";
            
        
        private static async Task Main()
        {
            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(IdpUri); // IDP
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "ClientId_#####",
                ClientSecret = "ClientSecret_######",
                Scope = "Scope_######"
            });
            
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            
        }

    }
}