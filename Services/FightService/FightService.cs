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
        public FightService(DataContext context)
        {
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

                int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
                damage -= new Random().Next(opponent.Defence);

                if (damage > 0)
                {
                    opponent.HipPoints -= damage;
                }
                
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
    }
}