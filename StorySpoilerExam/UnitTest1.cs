using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoilerExam.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;


namespace StorySpoilerExam
{
    [TestFixture]
    public class StorySpoilerExamTests
    {
            private RestClient _client = default!;
            private static string _createdStoryId = string.Empty;
            private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

            private static readonly JsonSerializerOptions JsonOpts = new()
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

        [OneTimeSetUp]
        public void Setup()
        {
           
            string token = GetJwtToken("yoana15yoana15", "yoana15yoana15");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            _client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

       
        [Test, Order(1)]
            public void CreateStory_ShouldReturnCreated_AndStoryId()
            {
                var story = new StoryDTO
                {
                    Title = "New Spoiler Title",
                    Description = "A short spoiler description.",
                    Url = null
                };

                var request = new RestRequest("/api/Story/Create", Method.Post)
                    .AddJsonBody(story);

                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Expected 201 Created.");
                Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

                var payload = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!, JsonOpts);
                Assert.That(payload, Is.Not.Null, "Response payload should be parseable.");
                Assert.That(payload!.StoryId, Is.Not.Null.And.Not.Empty, "StoryId should be returned.");
                Assert.That(payload.Msg, Does.Contain("Successfully created!"));

                _createdStoryId = payload.StoryId!;
                TestContext.WriteLine($"Created StoryId: {_createdStoryId}");
            }

       
            [Test, Order(2)]
            public void EditStory_ShouldReturnOk_AndSuccessMessage()
            {
                Assert.That(_createdStoryId, Is.Not.Empty, "Previous test must create a story.");

                var updated = new StoryDTO
                {
                    Title = "Updated Spoiler Title",
                    Description = "Updated spoiler description.",
                    Url = ""
                };

                var request = new RestRequest($"/api/Story/Edit/{_createdStoryId}", Method.Put)
                    .AddJsonBody(updated);

                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected 200 OK.");
                Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

                var payload = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!, JsonOpts);
                Assert.That(payload, Is.Not.Null);
                Assert.That(payload!.Msg, Does.Contain("Successfully edited"));
            }

          
            [Test, Order(3)]
            public void GetAllStories_ShouldReturnOk_AndNonEmptyArray()
            {
                var request = new RestRequest("/api/Story/All", Method.Get);

                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

                
                var array = JsonSerializer.Deserialize<List<JsonElement>>(response.Content!, JsonOpts);
                Assert.That(array, Is.Not.Null);
                Assert.That(array!, Is.Not.Empty, "Expected non-empty stories array.");
            }

           
            [Test, Order(4)]
            public void DeleteStory_ShouldReturnOk_AndDeletedMessage()
            {
                Assert.That(_createdStoryId, Is.Not.Empty, "Previous tests must create a story.");

                var request = new RestRequest($"/api/Story/Delete/{_createdStoryId}", Method.Delete);
                var response = _client.Execute(request);

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

                var payload = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!, JsonOpts);
                Assert.That(payload, Is.Not.Null);
                Assert.That(payload!.Msg, Does.Contain("Deleted successfully!"));
            }

        
            [Test, Order(5)]
            public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
            {
                var invalid = new StoryDTO
                {
                    Title = "",
                    Description = "",
                    Url = null
                };

                var request = new RestRequest("/api/Story/Create", Method.Post)
                    .AddJsonBody(invalid);

                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            }


            [Test, Order(6)]
            public void EditNonExistingStory_ShouldReturnNotFound_WithNoSpoilersMessage()
            {
                var fakeId = "non-existing-id-123";

                var updated = new StoryDTO
                {
                    Title = "Does not matter",
                    Description = "Does not matter",
                    Url = null
                };

                var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put)
                    .AddJsonBody(updated);

                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

                var payload = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!, JsonOpts);
 
                Assert.That(payload?.Msg, Does.Contain("No spoilers"));
            }


            [Test, Order(7)]
            public void DeleteNonExistingStory_ShouldReturnBadRequest_WithUnableToDeleteMessage()
            {
                var fakeId = "non-existing-id-456";
                var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);

                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.Content, Is.Not.Null.And.Not.Empty);

                var payload = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content!, JsonOpts);

                Assert.That(payload?.Msg, Does.Contain("Unable to delete this story spoiler!"));
            }

            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                _client?.Dispose();
            }
        }
    }

