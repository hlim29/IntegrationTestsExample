using Azure.Storage.Blobs;
using System.Runtime.InteropServices;
using Testcontainers.Azurite;

namespace IntegrationTests
{
    [Collection("SUT")]
    public class TestCases : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;
        private readonly HttpClient _client;

        public TestCases(TestFixture testFixture)
        {
            _testFixture = testFixture;
            _client = testFixture.CreateSutClient();
        }

        [Fact]
        public async Task Success()
        {
            var res = await _client.GetAsync("json");
            res.EnsureSuccessStatusCode();
            Assert.True(res.StatusCode == System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Fail()
        {
            var res = await _client.GetAsync("json1");
            Assert.True(res.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task BlobStorage_tests()
        {
            var blobService = _testFixture.AzureStorage.BlobServiceClient;
            var container = await blobService.CreateBlobContainerAsync("teststorage");

            var blob = container.Value.GetBlobClient("test.txt");
            await blob.UploadAsync(BinaryData.FromString($"Hello from .NET on {RuntimeInformation.OSDescription}"), overwrite: true);

            Assert.True(await blob.ExistsAsync());
        }
    }
}