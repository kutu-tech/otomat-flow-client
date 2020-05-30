using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using IdentityModel.Client;
using Kutu.Otomat.Web.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kutu.Otomat.Web.Client.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public IEnumerable<CatalogItemForFlowModel> Model { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5051"); // IDP
            if (disco.IsError)
            {
                return StatusCode(401);
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "koctas-1",
                ClientSecret = "secret",
                Scope = "product-api"
            });
            
            if (tokenResponse.IsError)
            {
                return StatusCode(403);
            }
            
            var apiClient = new HttpClient {BaseAddress = new Uri("https://localhost:5005")};
            apiClient.SetBearerToken(tokenResponse.AccessToken);
            apiClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await apiClient.GetAsync($"/api/catalog");
            
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            
            
            var content = await response.Content.ReadAsStringAsync();
            Model = JsonConvert.DeserializeObject<IEnumerable<CatalogItemForFlowModel>>(content);


            return Page();


        }
    }
}