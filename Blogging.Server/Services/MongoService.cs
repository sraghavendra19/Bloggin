using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Blogging.Shared.Models;

namespace Blogging.Server.Services
{
    public class MongoService
    {
        private readonly IMongoDatabase _db;
        public IMongoCollection<User> Users => _db.GetCollection<User>("users");
        public IMongoCollection<Post> Posts => _db.GetCollection<Post>("posts");

        public MongoService(IOptions<MongoSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            _db = client.GetDatabase(settings.DatabaseName);
        }
    }
}
