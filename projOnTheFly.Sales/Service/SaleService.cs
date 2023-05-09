﻿using MongoDB.Driver;
using projOnTheFly.Models;
using projOnTheFly.Sales.Config;

namespace projOnTheFly.Sales.Service
{
    public class SaleService
    {
        private readonly IMongoCollection<Sale> _collection;
        public SaleService(IProjOnTheFlySaleSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<Sale>(settings.SaleCollectionName);
        }
        public async Task<List<Sale>> Get() => await _collection.Find(c => c.Sold == true).ToListAsync();

        public async Task<Sale> GetSaleByPassenger(string cpf)
        {
           return  await _collection.Find(cpf).FirstOrDefaultAsync();
        }
        public async Task<Sale> Create(Sale sale)
        {
            await _collection.InsertOneAsync(sale);
            return sale;
        }
        public async Task<Sale> Update(Sale sale)
        {
            await _collection.ReplaceOneAsync(c => c.Passenger == sale.Passenger, sale);
            return sale;
        }
       //public Task Delete(string cpf) => _collection.DeleteOneAsync(c => c.CPF == cpf);
    }
}
