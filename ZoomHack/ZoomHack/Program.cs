using System;

using LeagueSharp;
using LeagueSharp.Common;

namespace ZoomHack
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Hacks.ZoomHack = true;
        }
    }
}
