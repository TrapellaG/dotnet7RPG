using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotNetRPG.Dtos.Fight
{
    public class FightRequestDto
    {
        public List<int> CharactersId { get; set; } = new List<int>();
    }
}