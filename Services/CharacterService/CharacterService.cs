global using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotNetRPG.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private static List<Character> characters = new List<Character>()
        {
            new Character(),
            new Character { Id = 1, Name = "Sam" }
        };
        private readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            this._mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = _mapper.Map<Character>(newCharacter);
            character.Id = characters.Max(character => character.Id) + 1;
            characters.Add(character);
            ServiceResponse.Data = characters.Select(character => _mapper.Map<GetCharacterDto>(character)).ToList();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterDto>>();
            ServiceResponse.Data = characters.Select(character => _mapper.Map<GetCharacterDto>(character)).ToList();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterDto>();
            var character = characters.FirstOrDefault(character => character.Id == id);
            ServiceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterDto>();

            try
            {
                var character = characters.FirstOrDefault(character => character.Id == updatedCharacter.Id);

                if (character is null)
                {
                    throw new Exception($"Character with Id '{updatedCharacter.Id}' not found.");
                }

                _mapper.Map(updatedCharacter, character);

                //OLD STYLE UPDATE, we can use AutoMapper instead
                /*character.Name = updatedCharacter.Name;
                character.HipPoints = updatedCharacter.HipPoints;
                character.Strength = updatedCharacter.Strength;
                character.Defence = updatedCharacter.Defence;
                character.Inteligence = updatedCharacter.Inteligence;
                character.Class = updatedCharacter.Class;*/

                ServiceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception exception)
            {
                ServiceResponse.Success = false;
                ServiceResponse.Message = exception.Message;
            }

            return ServiceResponse;
        }
        
        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterDto>>();

            try
            {
                var character = characters.FirstOrDefault(character => character.Id == id);

                if (character is null)
                {
                    throw new Exception($"Character with Id '{id}' not found.");
                }

                characters.Remove(character);
                ServiceResponse.Data = characters.Select(character => _mapper.Map<GetCharacterDto>(character)).ToList();
            }
            catch (Exception exception)
            {
                ServiceResponse.Success = false;
                ServiceResponse.Message = exception.Message;
            }

            return ServiceResponse;
        }
    }
}