using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Transport;
using Indexer.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;
using static System.Net.Mime.MediaTypeNames;

namespace Indexer.Elasticsearch
{
    public class ElasticsearchRepository
    {
        private static ElasticsearchClient _client { get; set; }
        private static ElasticsearchClientSettings _settings;

        private static string _productsIndexName = "products";

        public ElasticsearchRepository()
        {

            var elasticSearchContainerUri = Environment.GetEnvironmentVariable("ELASTICSEARCH_URI") ?? "";
            var user = Environment.GetEnvironmentVariable("ELASTICSEARCH_USER") ?? "";
            var password = Environment.GetEnvironmentVariable("ELASTICSEARCH_PASSWORD") ?? "";

            var uri = new Uri(elasticSearchContainerUri);

            _settings = new ElasticsearchClientSettings(uri)
                .CertificateFingerprint("")
                .Authentication(new BasicAuthentication(user, password))
                .DefaultMappingFor<Product>(m => m.IndexName(_productsIndexName));

            InitElasticSearchRepository();


        }


        public void InitElasticSearchRepository()
        {
            if (_client == null)
            {
                _client = new ElasticsearchClient(_settings);
            }
        }

        public Product? GetProduct(string id)
        {
            var response = _client.GetAsync<Product>(id).Result;

            if (response.IsValidResponse)
            {
                //Console.WriteLine("Document indexed successfully!\n\nObject:   " + response.Source);
                return response.Source;
            }

            Console.WriteLine($"Failed to index document: {response.DebugInformation}");
            return null;

        }

        public IEnumerable<Product> GetAllProducts()
        {
            var response = _client.SearchAsync<Product>(s => s
                .Index(_productsIndexName)
                .From(0)
                .Size(10)
            ).Result;

            if (response.IsValidResponse)
            {
                Console.WriteLine("Documents gotten succesfully!\n\nObject:   " + response.Documents.Count);
                Console.WriteLine("First Document name: " + response.Documents.ToList()[0].Name);
                return response.Documents;
            }
            else
            {
                Console.WriteLine($"Failed to index document: {response.DebugInformation}");
            }

            return new List<Product>();
        }

        public async Task<bool> IndexProduct(Product product)
        {
            await Console.Out.WriteLineAsync(product.ToString());

            var response = await _client.IndexAsync(product);

            if (response.IsValidResponse)
            {
                Console.WriteLine($"Index document with ID {response.Id} succeeded.");
            }
            else
            {
                Console.WriteLine("Failed to index product " + response.ElasticsearchServerError.ToString());
            }
            return response.IsSuccess();
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            var existingProduct = GetProduct(product.Id.ToString());

            if (existingProduct != null)
            {
                var isUpdateNewer = product.CreatedAt > existingProduct.CreatedAt;
                if (!isUpdateNewer)
                {
                    Console.WriteLine("Existing product is newer");
                    return false;
                }

                await _client.UpdateAsync<Product, Product>(_productsIndexName, product.Id, u => u.Doc(product));

                await IndexProduct(product);
                return true;
            }

            Console.WriteLine("Product does not exist");
            return false;
        }

        public async Task<bool> DeleteProduct(string id)
        {
            var response = await _client.DeleteAsync<Product>(id);

            await Console.Out.WriteLineAsync("Delete success " + response.IsSuccess());

            return response.IsSuccess();
        }

    }
}
