namespace IntegrationTests
{
    [Collection("SUT")]
    public class TestCases
    {
        private readonly HttpClient _client;

        public TestCases(TestFixture testFixture)
        {
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
    }
}