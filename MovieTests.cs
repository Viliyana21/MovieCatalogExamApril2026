using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using MovieCatalogExamApril.Models;


namespace MovieCatalogExamApril
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIwZTA2YzA1Ni0zZGY3LTQzYzUtYjFjZC0zYzAzNzUwNTk3YWIiLCJpYXQiOiIwNC8xOC8yMDI2IDA3OjUxOjMzIiwiVXNlcklkIjoiOTFiMDNiNjQtZTc2NC00OGRlLTYyYWQtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJnb29kam9iQHNvZnR1bmkuY29tIiwiVXNlck5hbWUiOiJWUEsyMDI2ZmluYWwiLCJleHAiOjE3NzY1MjAyOTMsImlzcyI6Ik1vdmllQ2F0YWxvZ19BcHBfU29mdFVuaSIsImF1ZCI6Ik1vdmllQ2F0YWxvZ19XZWJBUElfU29mdFVuaSJ9.ixeL_CW_wqYg0MDpkFM6sMq37jDY1eu-fpY7e95ope0"; //след като е генериран от постман се поставя тук или се слагат само кавички защото иначе гърми

        private const string LoginEmail = "goodjob@softuni.com"; // да си сложа мои автентични тестови данни, vuzmojno e da iska username, ne smenqm imeto na stringa loginEmail, samo infoto v kavichkite
        private const string LoginPassword = "12345678";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Register", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");

            }
        }


        [Test, Order(1)]
        public void CreateMovie_WithRequiredFields_ShouldReturnSuccess()
        {
            // Arrange
            MovieDTO movieData = new MovieDTO
            {
                Title = "Test Movie",
                Description = "This is a test movie description."
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            // Act
            RestResponse response = this.client.Execute(request);

            TestContext.WriteLine($"Status code: {response.StatusCode}");
            TestContext.WriteLine($"Response content: {response.Content}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty.");

            ApiResponseDTO? responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData, Is.Not.Null, "Response should be deserialized.");
            Assert.That(responseData.Movie, Is.Not.Null, "A Movie object should be returned in the response.");
            Assert.That(responseData.Movie.Id, Is.Not.Null.And.Not.Empty, "Movie.Id should not be null or empty.");
            Assert.That(responseData.Msg, Is.EqualTo("Movie created successfully!"), "Expected success message.");

            lastCreatedMovieId = responseData.Movie.Id;
        }

        [Test, Order(2)]
        public void EditMovie_WithCreatedMovieId_ShouldReturnSuccess()
        {
            // Arrange
            MovieDTO editedMovie = new MovieDTO
            {
                Id = lastCreatedMovieId,
                Title = "Edited Test Movie",
                Description = "Edited test movie description."
            };

            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", lastCreatedMovieId);
            request.AddJsonBody(editedMovie);

            // Act
            RestResponse response = this.client.Execute(request);

            TestContext.WriteLine($"Status code: {response.StatusCode}");
            TestContext.WriteLine($"Response content: {response.Content}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty.");

            ApiResponseDTO? responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData, Is.Not.Null, "Response should be deserialized.");
            Assert.That(responseData.Msg, Is.EqualTo("Movie edited successfully!"), "Expected success message.");
        }
        [Test, Order(3)]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Empty);
            Assert.That(responseItems, Is.Not.Null); 

        }
        [Test, Order(4)]
        public void DeleteMovie_WithCreatedMovieId_ShouldReturnSuccess()
        { 
         //Arrange
         RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
         request.AddQueryParameter("movieId", lastCreatedMovieId);

         //Act
           RestResponse response = this.client.Execute(request);

           TestContext.WriteLine($"Status code: {response.StatusCode}");
          TestContext.WriteLine($"Response content: {response.Content}");

        // Assert
         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
         Assert.That(response.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty.");

         ApiResponseDTO? responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
        
        Assert.That(responseData, Is.Not.Null, "Response should be deserialized.");
           Assert.That(responseData.Msg, Is.EqualTo("Movie deleted successfully!"));
        }


        [Order(5)]
        [Test]
        public void CreateMovie_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var MovieDTO = new MovieDTO
            {
                Title = "",
                Description = "This is a test  description.",
                Id = ""
            };
            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(MovieDTO);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }
        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            // Arrange
            string nonExistingMovieId = "999999";

            MovieDTO editedMovie = new MovieDTO
            {
                Id = nonExistingMovieId,
                Title = "Edited Non-existing Movie",
                Description = "Edited description"
            };

            RestRequest request = new RestRequest($"/api/Movie/Edit?movieId={nonExistingMovieId}", Method.Put);
            request.AddJsonBody(editedMovie);

            // Act
            RestResponse response = this.client.Execute(request);

            TestContext.WriteLine($"Status code: {response.StatusCode}");
            TestContext.WriteLine($"Response content: {response.Content}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 BadRequest.");
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty.");

            ApiResponseDTO? responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData, Is.Not.Null, "Response should be deserialized.");
            Assert.That(responseData.Msg,
                Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"),
                "Expected error message.");
        }
        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            // Arrange
            string nonExistingMovieId = "999999";

            RestRequest request = new RestRequest($"/api/Movie/Delete?movieId={nonExistingMovieId}", Method.Delete);

            // Act
            RestResponse response = this.client.Execute(request);

            TestContext.WriteLine($"Status code: {response.StatusCode}");
            TestContext.WriteLine($"Response content: {response.Content}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 BadRequest.");
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty, "Response content should not be empty.");

            ApiResponseDTO? responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData, Is.Not.Null, "Response should be deserialized.");
            Assert.That(responseData.Msg,
                Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"),
                "Expected error message.");
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
