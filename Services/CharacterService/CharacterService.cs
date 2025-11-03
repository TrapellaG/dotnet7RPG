global using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotNetRPG.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public CharacterService(IMapper mapper, DataContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = _mapper.Map<Character>(newCharacter);

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            ServiceResponse.Data = await _context.Characters.Select(character => _mapper.Map<GetCharacterDto>(character)).ToListAsync();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters(int userId)
        {
            var ServiceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _context.Characters.Where(character => character.User!.Id == userId).ToListAsync();
            ServiceResponse.Data = dbCharacters.Select(character => _mapper.Map<GetCharacterDto>(character)).ToList();
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacters = await _context.Characters.FirstOrDefaultAsync(character => character.Id == id);
            ServiceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacters);
            return ServiceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var ServiceResponse = new ServiceResponse<GetCharacterDto>();

            try
            {
                var character = await _context.Characters.FirstOrDefaultAsync(character => character.Id == updatedCharacter.Id);

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

                await _context.SaveChangesAsync();
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
                var character = await _context.Characters.FirstOrDefaultAsync(character => character.Id == id);

                if (character is null)
                {
                    throw new Exception($"Character with Id '{id}' not found.");
                }

                _context.Characters.Remove(character);

                await _context.SaveChangesAsync();
                
                ServiceResponse.Data = await _context.Characters.Select(character => _mapper.Map<GetCharacterDto>(character)).ToListAsync();
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