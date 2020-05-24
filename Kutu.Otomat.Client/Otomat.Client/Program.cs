using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Otomat.Client.Models;

namespace Otomat.Client
{
    class Program
    {
         private static string IdpUri => "###";
         private static string ApiUri => "###";
            
        
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
                ClientId = "###",
                ClientSecret = "###",
                Scope = "###"
            });
            
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");
            
            
            // Get Catalog Items
            var catalogItems = await GetCatalogs(tokenResponse.AccessToken);

            if (catalogItems == null)
            {
                return;
            }

            foreach (var item in catalogItems)
            {
                Console.WriteLine($"{item.Name} - {item.Desc} - {item.Code} - {item.Image}");
            }
            
            
            // Pick up first catalog item
            if (catalogItems.FirstOrDefault() is CatalogItemForFlowModel  catalogItem)
            {
                var pickUpResult = await PickUp(catalogItem.Code, tokenResponse.AccessToken);

                if (pickUpResult == null)
                {
                    return;
                }
                
                Console.WriteLine(pickUpResult.OtpCode);
                Console.WriteLine(pickUpResult.OrderId);
                // Confirm pick up
                await Confirm(pickUpResult.OrderId, tokenResponse.AccessToken);
            } 
        }

        private static async Task Confirm(int orderId, string token)
        {
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(token);
            apiClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await apiClient.PutAsync($"{ApiUri}/api/otomat/pick-up/{orderId}/confirm", null);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
        }

        private static async Task<PickUpResult> PickUp(string catalogItemCode, string token)
        {
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(token);
            apiClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await apiClient.PostAsync($"{ApiUri}/api/otomat/{catalogItemCode}/pick-up", null);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var pickUpResult = JsonConvert.DeserializeObject<PickUpResult>(content);

            return pickUpResult;
        }

        private static async Task<IEnumerable<CatalogItemForFlowModel>> GetCatalogs(string token)
        {
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(token);
            apiClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await apiClient.GetAsync($"{ApiUri}/api/catalog");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
                return null;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var catalogItems = JsonConvert.DeserializeObject<IEnumerable<CatalogItemForFlowModel>>(content);

            return catalogItems;
        }

    }
}