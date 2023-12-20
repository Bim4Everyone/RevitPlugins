using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;

using RevitToMongoDB.Model;
using RevitToMongoDB.ViewModels.Interfaces;

namespace RevitToMongoDB.Connections {
    internal sealed class MongoDBConnection : IElementRepository {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoClientDataBase;
        private readonly IMongoCollection<ElementDto> _mongoClientCollection;

        public MongoDBConnection(string connectionString, MongoDBSettings settings) {
            _mongoClient = new MongoClient(connectionString);
            _mongoClientDataBase = _mongoClient.GetDatabase(settings.ProjectName);
            _mongoClientCollection = _mongoClientDataBase.GetCollection<ElementDto>(settings.VersionName);
        }

        public async Task<List<ElementDto>> GetElementsAsync() {
            return await (await _mongoClientCollection.FindAsync(f => true)).ToListAsync();
        }

        public void Insert(ElementDto element) {
            _mongoClientCollection.InsertOne(element);
        }

        public async Task InsertAsync(ElementDto element) {
            await _mongoClientCollection.InsertOneAsync(element);
        }
    }
}
