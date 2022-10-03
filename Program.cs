// See https://aka.ms/new-console-template for more information
using System;

namespace global
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            int width = 12;
            int height = 8;
            double bombProportion = 0.12;
            int debugMode = 0;

            // Prise en compte des arguments du dotnet run
            switch (args.Length)
            {
                case 1:
                    width = int.Parse(args[0]);
                    height = width;
                    break;
                case 2:
                    width = int.Parse(args[0]);
                    height = int.Parse(args[1]);
                    break;
                case 3:
                    width = int.Parse(args[0]);
                    height = int.Parse(args[1]);
                    bombProportion = double.Parse(args[2]);
                    break;
                case 4:
                    width = int.Parse(args[0]);
                    height = int.Parse(args[1]);
                    bombProportion = double.Parse(args[2]);
                    debugMode = int.Parse(args[3]);
                    break;
            }

            // Lancement du jeu
            bool premierePartie = true;
            Grille grid;
            do
            {
                if (premierePartie)
                {
                    Console.Write("Voulez-vous reprendre la partie précédente ? (Taper 'o'/'n' pour oui/non) : ");
                    try
                    {
                        if (Console.ReadLine().ToLower() == "o")
                            grid = new Grille(Grille.FileName, debugMode);
                        else
                            grid = new Grille(width, height, bombProportion, debugMode);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Il y a eu une erreur, début d'une nouvelle partie : " + e.Message + "\n" + e.StackTrace);
                        grid = new Grille(width, height, bombProportion, debugMode);
                    }
                }

                // A partir de la 2eme partie consécutive on ne propose pas de reprendre
                else
                    grid = new Grille(width, height, bombProportion, debugMode);
                Console.Write("Recommencer une partie ?? (Taper 'o' si oui) : ");
            } while (Console.ReadLine().ToLower() == "o");
        }
    }
}