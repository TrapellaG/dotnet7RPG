using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotNetRPG.Dtos.Weapon;

namespace dotNetRPG.Services.WeaponService
{
    public interface IWeaponService
    {
        Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWweapon);
    }
}