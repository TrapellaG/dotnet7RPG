using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotNetRPG.Dtos.Fight;

namespace dotNetRPG.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public FightService(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {

            var response = new ServiceResponse<AttackResultDto>();

            try
            {
                var attacker = await _context.Characters
                    .Include(character => character.Weapon)
                    .FirstOrDefaultAsync(character => character.Id == request.AttackerId);

                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(character => character.Id == request.OpponentId);

                if (attacker == null || opponent == null || attacker.Weapon == null)
                {
                    throw new Exception("Attacker/oppenent or weapon not found.");
                }

                int damage = DoWeaponAttack(attacker, opponent);

                if (opponent.HipPoints <= 0)
                {
                    opponent.HipPoints = 0;
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HipPoints,
                    OpponentHP = opponent.HipPoints,
                    Damage = damage
                };

            }
            catch (Exception exception)
            {
                response.Success = false;
                response.Message = exception.Message;
            }

            return response;
        }

        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            if(attacker.Weapon == null)
            {
                throw new Exception("Attacker has no weapon.");
            }

            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(opponent.Defence);

            if (damage > 0)
            {
                opponent.HipPoints -= damage;
            }

            return damage;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();

            try
            {
                var attacker = await _context.Characters
                    .Include(character => character.Skills)
                    .FirstOrDefaultAsync(character => character.Id == request.AttackerId);

                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(character => character.Id == request.OpponentId);

                if (attacker == null || opponent == null || attacker.Skills == null)
                {
                    throw new Exception("Attacker/oppenent or skill not found.");
                }

                var skill = attacker.Skills.FirstOrDefault(skill => skill.Id == request.SkillId);

                if (skill == null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't know that skill!";
                    return response;
                }

                int damage = DoSkillAttack(attacker, opponent, skill);

                if (opponent.HipPoints <= 0)
                {
                    opponent.HipPoints = 0;
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HipPoints,
                    OpponentHP = opponent.HipPoints,
                    Damage = damage
                };

            }
            catch (Exception exception)
            {
                response.Success = false;
                response.Message = exception.Message;
            }

            return response;
        }

        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill)
        {
            int damage = skill.Damage + (new Random().Next(attacker.Inteligence));
            damage -= new Random().Next(opponent.Defence);

            if (damage > 0)
            {
                opponent.HipPoints -= damage;
            }

            return damage;
        }

        public async Task<ServiceResponse<FightResultsDto>> Fight(FightRequestDto request)
        {
            var response = new ServiceResponse<FightResultsDto>
            {
                Data = new FightResultsDto()
            };

            try
            {
                var characters = await _context.Characters
                    .Include(character => character.Weapon)
                    .Include(character => character.Skills)
                    .Where(character => request.CharactersId.Contains(character.Id))
                    .ToListAsync();

                bool defeated = false;

                while (!defeated)
                {
                    foreach (var attacker in characters)
                    {
                        var oppenents = characters.Where(character => character.Id != attacker.Id).ToList();
                        var opponent = oppenents[new Random().Next(oppenents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;

                        bool useWeapon = new Random().Next(2) == 0;

                        if (useWeapon && attacker.Weapon is not null)
                        {
                            // Weapon attack
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent);

                        }
                        else if (attacker.Skills != null && attacker.Skills.Count > 0)
                        {
                            // Skill attack
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker, opponent, skill);
                        }
                        else
                        {
                            response.Data.Log.Add($"{attacker.Name} wasn't able to attack!");
                            continue;
                        }

                        response.Data.Log.Add($"{attacker.Name} attacks {opponent.Name} using {attackUsed} and deals {(damage > 0 ? damage : 0)} damage.");

                        if (opponent.HipPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;
                            response.Data.Log.Add($"{opponent.Name} has been defeated!");
                            response.Data.Log.Add($"{attacker.Name} wins with {attacker.HipPoints} HP left.");
                            break;
                        }
                    }
                }

                characters.ForEach(character =>
                {
                    character.Fights++;
                    character.HipPoints = 100;
                });

                await _context.SaveChangesAsync();
            
            }
            catch (Exception exception)
            {
                response.Success = false;
                response.Message = exception.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<List<HighscoreDto>>> GetHighscore()
        {
            var characters = await _context.Characters
                .Where(character => character.Fights > 0)
                .OrderByDescending(character => character.Victories)
                .ThenBy(character => character.Defeats)
                .Select(character => new HighscoreDto
                {
                    Name = character.Name,
                    Fights = character.Fights,
                    Victories = character.Victories,
                    Defeats = character.Defeats
                })
                .ToListAsync();

            var response = new ServiceResponse<List<HighscoreDto>>
            {
                Data = characters.Select(character => _mapper.Map<HighscoreDto>(character)).ToList()
            };

           return response;
        }
    }
}