using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace craftinggame
{
    class Program
    {
        static void Main(string[] args)
        {
            using Craft game = new Craft(800, 600, "CraftingGame");
            game.Run();
        }
    }
}
