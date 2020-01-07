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
            using (Craft game = new Craft(1600, 900, "CraftingGame"))
            {
                game.Run();
            }
        }
    }
}
