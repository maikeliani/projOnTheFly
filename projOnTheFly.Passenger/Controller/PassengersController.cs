﻿using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using projOnTheFly.Models;
using projOnTheFly.Passenger.DTO;
using projOnTheFly.Passenger.Service;
using projOnTheFly.Services;

namespace projOnTheFly.Passenger.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassengersController : ControllerBase
    {
        private readonly PassengerService _passengerService;
        public PassengersController(PassengerService passengerService)
        {
            _passengerService = passengerService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Models.Passenger>>> GetAll()
        {
            var containsPassenger = await  _passengerService.Get();

            if (containsPassenger.Count()==0)
            {
                return BadRequest("Não existem passageiros com status ativos");
            }
            return containsPassenger;
            
        }
        [HttpGet("{cpf}", Name = "Get CPF")]
        public async Task<ActionResult<Models.Passenger>> GetPassengerByCPF(string cpf)
        {
            var validateCpf = new ValidateCPF(cpf);

            if (!validateCpf.IsValid()) return BadRequest("CPF inválido");

             var containsPassenger =  await _passengerService.Get(cpf);

            if(containsPassenger is null)
            {
                return BadRequest("Cpf com status inativo ou inexistente");
            }
            return containsPassenger;

        }

        [HttpPost]
        public async Task<ActionResult<PassengerResponse>> Post(PassengerPostRequest passengerRequest)
        {
            var validateCpf = new ValidateCPF(passengerRequest.CPF);

            if (!validateCpf.IsValid()) return BadRequest("CPF inválido");
            
            if (passengerRequest == null) return UnprocessableEntity("Requisição de passageiro inválida");

            AddressDTO? postOffice = await PostOfficeService.GetAddressAsync(passengerRequest.Address.ZipCode!);

            if(postOffice == null)  return BadRequest("CEP inválido");

            char charToUpper = char.ToUpper(passengerRequest.Gender);

            if (!"FM".Contains(charToUpper)) 
                return BadRequest("Gênero inválido");

            Models.Passenger passenger = new()
            {
                CPF = passengerRequest.CPF,
                Name = passengerRequest.Name,
                Gender = charToUpper,
                DateBirth = passengerRequest.DateBirth,
                DtRegister = DateTime.Now,
                Status = passengerRequest.Status,
                Address = new Address
                {
                    City = postOffice.City,
                    ZipCode = postOffice.ZipCode,
                    Complement = passengerRequest.Address.Complement,
                    NeighborHood = postOffice.NeighborHood,
                    Number = passengerRequest.Address.Number,
                    State = postOffice.State,
                    Street = postOffice.Street
                },
            };

            passenger.Phone = passenger.RemovePhoneMask(passengerRequest.Phone);

            await _passengerService.Create(passenger);

            PassengerResponse passengerResponse = new()
            {
                Name = passenger.Name,
                DtRegister = passenger.DtRegister,
            };

            passengerResponse.Status = passengerResponse.StatusPassenger(passenger.Status);

            return CreatedAtAction("GetPassengerByCPF", new { cpf = passenger.CPF }, passengerResponse);
        }

        [HttpPut("{cpf}")]
        public async Task<ActionResult> Update(string cpf, PassengerPutRequest passengerRequest)
        {
            var validateCpf = new ValidateCPF(cpf);

            if (!validateCpf.IsValid()) return BadRequest("CPF inválido");

            if (passengerRequest == null) return UnprocessableEntity("Requisição de passageiro inválida");

            AddressDTO? postOffice = await PostOfficeService.GetAddressAsync(passengerRequest.Address.ZipCode!);

            if (postOffice == null) return BadRequest("CEP inválido");

            char charToUpper = char.ToUpper(passengerRequest.Gender);

            if (!"FM".Contains(charToUpper))
                return BadRequest("Gênero inválido");

            Models.Passenger passenger = new()
            {
                CPF = cpf,
                Name = passengerRequest.Name,
                Gender = charToUpper,
                Phone = passengerRequest.Phone,
                DateBirth = passengerRequest.DateBirth,
                Status = passengerRequest.Status,
                Address = new Address
                {
                    City = postOffice.City,
                    ZipCode = postOffice.ZipCode,
                    Complement = passengerRequest.Address.Complement,
                    NeighborHood = postOffice.NeighborHood,
                    Number = passengerRequest.Address.Number,
                    State = postOffice.State,
                    Street = postOffice.Street
                }
            };

            var passengerUpdate = _passengerService.Get(passenger.CPF);

            if (passengerUpdate == null) return NotFound();

            await _passengerService.Update(passenger);

            return NoContent();
        }

        [HttpDelete("{cpf}")]
        public async Task<ActionResult> Delete(string cpf)
        {
            var validateCpf = new ValidateCPF(cpf);

            if (!validateCpf.IsValid()) return BadRequest("CPF inválido");

            var passengerDelete = _passengerService.Get(cpf);
            
            if (passengerDelete== null) return NotFound();
            
            await _passengerService.Delete(cpf);
            
            return NoContent();
        }
    }
}
