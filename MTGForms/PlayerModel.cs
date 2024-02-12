using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_MTG
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Card { get; set; }
        public bool Unveiled { get; set; }
        public string Password { get; set; } = "0000";
        public int PlayerCount { get; set; }
    }
}
