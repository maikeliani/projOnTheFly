﻿using MongoDB.Driver;
using projOnTheFly.Passenger.DTO;
using projOnTheFly.Passenger.Settings;

namespace projOnTheFly.Passenger.Service
{
    public class PassengerService
    {
        private readonly IMongoCollection<Models.Passenger> _collection;
        public PassengerService(IProjOnTheFlyPassengerSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<Models.Passenger>(settings.PassengerCollectionName);
        }
        public async Task<List<Models.Passenger>> GetAsync() => await _collection.Find(c => c.Status == true).ToListAsync();

        public Task<Models.Passenger> GetAsync(string cpf) => _collection.Find(c => c.CPF == cpf && c.Status == true).FirstOrDefaultAsync();

        public async Task<List<PassengerCheckResponse>> PostCheckAsync(List<string> cpfList)
        {  
             var passengers = await _collection.Find(c => cpfList.Contains(c.CPF)).ToListAsync();

            List <PassengerCheckResponse> passengerCheck = new(); 

            foreach (var passenger in passengers)
            {
                PassengerCheckResponse passengerCheckResponse = new()
                {
                    CPF = passenger.CPF,
                    Name = passenger.Name,
                    Status = passenger.Status,
                    Underage = passenger.IsUnderage()
                };

                passengerCheck.Add(passengerCheckResponse);
            }
            return passengerCheck;
        }
        public async Task<Models.Passenger> CreateAsync(Models.Passenger passenger)
        {
            await _collection.InsertOneAsync(passenger);
            return passenger;
        }
        public async Task<Models.Passenger> UpdateAsync(Models.Passenger passenger)
        {
            await _collection.ReplaceOneAsync(c => c.CPF == passenger.CPF, passenger);
            return passenger;
        }
        public Task DeleteAsync(string cpf) => _collection.DeleteOneAsync(c => c.CPF == cpf);

    }
}
