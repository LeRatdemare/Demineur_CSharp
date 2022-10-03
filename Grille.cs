using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace global
{
    class Grille
    {
        public const string FileName = "grille.csv";
        private const int BombValue = -1;

        private Random generator;
        private Logger logger;
        private int[,] grid;
        private int width;
        private int height;
        private double bombProportion;
        private List<int[]> casesRevelees;
        private List<int[]> casesMarquees;
        private List<int[]> casesQuestionnees;

        public enum Mode
        {
            Jeu, Drapeau, Question
        }

        // --------------------------------------- INITIALISATION ---------------------------------------

        public Grille(string path, int debugMode)
        {
            logger = new Logger(debugMode);

            // On commence par récupérer le contenu du fichier.
            List<string> lignesList = new List<string>();
            using (StreamReader sr = File.OpenText(path))
            {
                string ligne = "";
                sr.ReadLine(); // On passe la 1ère ligne qui contient le header
                while ((ligne = sr.ReadLine()) != null)
                {
                    logger.DebugRelou(ligne);
                    lignesList.Add(ligne);
                }
            }
            // On stocke ensuite l'ensemble des infos dans un tableau
            string[,] infos = new string[6, lignesList.Count]; // string[x,y] 
            for (int j = 0; j < lignesList.Count; j++)
            {
                string ligne = lignesList[j];
                string[] ligneSplit = ligne.Split(',');
                foreach (string s in ligneSplit)
                {
                    logger.DebugRelou("Grille.Grille(string,int) : ligneSplit -> " + s);
                }
                for (int i = 0; i < ligneSplit.Length; i++)
                {
                    infos[i, j] = ligneSplit[i];
                }
            }

            // Ensuite on récupère les variables depuis le tableau
            width = int.Parse(infos[0, lignesList.Count - 1]) + 1;
            height = int.Parse(infos[1, lignesList.Count - 1]) + 1;
            bombProportion = -1; // N'est pas utile dans ce constructeur
            generator = new Random();

            // On initialise les 3 listes ainsi que la grille
            casesRevelees = new List<int[]>();
            casesMarquees = new List<int[]>();
            casesQuestionnees = new List<int[]>();
            grid = new int[width, height];
            for (int j = 0; j < infos.GetLength(1); j++)
            {
                int x = int.Parse(infos[0, j]);
                int y = int.Parse(infos[1, j]);
                grid[x, y] = int.Parse(infos[2, j]);
                // Si la case est déjà révélée
                if (bool.Parse(infos[3, j])) casesRevelees.Add(new int[] { x, y });
                // Si la case est déjà marquée
                else if (bool.Parse(infos[4, j])) casesMarquees.Add(new int[] { x, y });
                // Si la case est déjà questionnée
                else if (bool.Parse(infos[5, j])) casesQuestionnees.Add(new int[] { x, y });
            }

            // Puis on joue
            Jouer();
        }

        /// <summary> Crée une grille de demineur de longueur <c>w</c>
        /// et de hauteur <c>h</c>. </summary>
        public Grille(int w, int h, double bombProp, int debugMode)
        {
            width = w;
            height = h;
            bombProportion = bombProp;
            casesRevelees = new List<int[]>();
            casesMarquees = new List<int[]>();
            casesQuestionnees = new List<int[]>();
            grid = new int[width, height];

            generator = new Random();
            logger = new Logger(debugMode);

            PremierCoup();
            Jouer();
        }

        private void PremierCoup()
        {
            Console.WriteLine(GetGrilleJoueur());
            logger.Debug(ToString());

            // Code du 1er coup
            Console.Write("Choisir une case à réveler au format 'A1' : ");
            string premierCoup = Console.ReadLine();
            premierCoup = premierCoup.ToLower();
            int i = (int)(premierCoup[0] - 'a');
            int j = int.Parse(premierCoup.Substring(1));
            do
            {
                InitialiserLaGrille();
            } while (grid[i, j] != 0);
            Reveler(i, j);

            sauvegarderLaGrille();
        }

        private void InitialiserLaGrille()
        {
            grid = new int[width, height];

            // On commence par placer les bombes
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    grid[i, j] = generator.NextDouble() < bombProportion ? BombValue : 0;
                }
            }

            // Ensuite on place les valeurs sur les cases restantes
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (grid[i, j] != -1) grid[i, j] = GetNbBombesAutour(i, j);
                }
            }
        }//end initGrid()

        private int GetNbBombesAutour(int x, int y)
        {
            int nbBombes = 0;
            for (int i = x - 1; i < x + 2; i++)
            {
                for (int j = y - 1; j < y + 2; j++)
                {
                    if (DansLaGrille(i, j) && grid[i, j] == BombValue) nbBombes++;
                }
            }
            return nbBombes;
        }

        // --------------------------------------- UTILE ---------------------------------------

        /// <summary> Retourne vrai si il existe une case de coordonnées (x,y) dans la grille. </summary>
        private bool DansLaGrille(int x, int y)
        {
            return x > -1 && x < width && y > -1 && y < height;
        }

        private bool DejaJouee(int x, int y)
        {
            foreach (int[] carre in casesRevelees)
            {
                if (carre[0] == x && carre[1] == y) return true;
            }
            return false;
        }
        private bool EstMarquee(int x, int y)
        {
            foreach (int[] carre in casesMarquees)
            {
                if (carre[0] == x && carre[1] == y) return true;
            }
            return false;
        }
        private bool EstQuestionnee(int x, int y)
        {
            foreach (int[] carre in casesQuestionnees)
            {
                if (carre[0] == x && carre[1] == y) return true;
            }
            return false;
        }

        // --------------------------------------- JEU ---------------------------------------

        private void Reveler(int x, int y)
        {
            if (!DansLaGrille(x, y) || DejaJouee(x, y))
            {
                logger.Debug($"Grille.Reveler(int, int) : x={x}, y={y}, DejaJouee:{DejaJouee(x, y)}");
                throw new Exception("La case n'est pas dans la grille ou a déjà été révélée.");
            }

            // Si c'est une bombe, le joueur a perdu
            if (grid[x, y] == BombValue)
                throw new OperationCanceledException("\n\n\n-_-_-_-_-_-_-_-GAME OVER-_-_-_-_-_-_-_-\n\n\n");

            // Sinon, on commence par reveler la case
            casesRevelees.Add(new int[] { x, y });

            // Et si c'est un 0 alors on révèle aussi ses alentours
            // du moment qu'ils n'ont pas déjà été révélés.
            if (grid[x, y] == 0)
            {
                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (DansLaGrille(i, j) && !DejaJouee(i, j))
                            Reveler(i, j);
                    }
                }
            }
        }

        private void Marquer(int x, int y)
        {
            if (!DansLaGrille(x, y) || DejaJouee(x, y))
            {
                logger.Debug($"Grille.Marquer(int, int) : x={x}, y={y}, EstMarquee:{EstMarquee(x, y)}, DejaJouee:{DejaJouee(x, y)}");
                throw new Exception("La case n'est pas dans la grille ou a déjà été révélée.");
            }

            if (EstQuestionnee(x, y)) retirerQuestion(x, y);
            if (EstMarquee(x, y)) retirerMarquage(x, y);
            else casesMarquees.Add(new int[] { x, y });
        }

        private void Questionner(int x, int y)
        {
            if (!DansLaGrille(x, y) || DejaJouee(x, y))
            {
                logger.Debug($"Grille.Questionner(int, int) : x={x}, y={y}, EstQuestionnee:{EstQuestionnee(x, y)}, DejaJouee:{DejaJouee(x, y)}");
                throw new Exception("La case n'est pas dans la grille ou a déjà été révélée.");
            }
            if (EstMarquee(x, y)) retirerMarquage(x, y);
            if (EstQuestionnee(x, y)) retirerQuestion(x, y);
            else casesQuestionnees.Add(new int[] { x, y });
        }

        private void retirerMarquage(int x, int y)
        {
            int i = 0;
            while (i < casesMarquees.Count && (casesMarquees[i][0] != x || casesMarquees[i][1] != y))
                i++;
            if (i < casesMarquees.Count)
                casesMarquees.RemoveAt(i);
        }

        private void retirerQuestion(int x, int y)
        {
            int i = 0;
            while (i < casesQuestionnees.Count && (casesQuestionnees[i][0] != x || casesQuestionnees[i][1] != y))
                i++;
            if (i < casesQuestionnees.Count)
                casesQuestionnees.RemoveAt(i);
        }

        private bool AGagne()
        {
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if ((EstMarquee(i, j) && grid[i, j] != BombValue) || (grid[i, j] == BombValue && !EstMarquee(i, j)))
                        return false;
                }
            }
            return true;
        }

        private void Jouer()
        {
            Mode mode = Mode.Jeu;
            bool victoire = false;

            // Debut de la boucle de jeu
            do
            {
                Console.WriteLine(GetGrilleJoueur());
                logger.Debug(ToString());

                string verbe = "découvrir";
                if (mode == Mode.Jeu) verbe = "découvrir";
                else if (mode == Mode.Drapeau) verbe = "marquer";
                else if (mode == Mode.Question) verbe = "questionner";

                Console.Write($"Choisir une case à {verbe} au format -A1- ou choisir parmi \n(j)ouer, (m)arquer, (q)uestion, (a)bandonner ou (f)inir l'exécution : ");
                string instruction = Console.ReadLine();
                instruction = instruction.ToLower();

                if (instruction == "j") mode = Mode.Jeu;
                else if (instruction == "m") mode = Mode.Drapeau;
                else if (instruction == "q") mode = Mode.Question;
                else if (instruction == "a") break;
                else if (instruction == "f") Environment.Exit(0);
                else
                {
                    try
                    {
                        logger.DebugRelou($"Grille.Jouer() : L'instruction {instruction} est de taille {instruction.Length}.");
                        if (instruction.Length > 1 + (height < 10 ? 1 : 2)) throw new Exception("Il ne peut pas y avoir autant de caractères -_-");
                        int x = (int)(instruction[0] - 'a');
                        int y = int.Parse(instruction.Substring(1)); ;

                        if (mode == Mode.Jeu) Reveler(x, y);
                        else if (mode == Mode.Drapeau) Marquer(x, y);
                        else if (mode == Mode.Question) Questionner(x, y);

                        sauvegarderLaGrille();
                    }
                    // Si le joueur a perdu on annule l'opération (un peu dégueux...)
                    catch (OperationCanceledException e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }
                    // Sinon on affiche, le message d'erreur et on attend un peu.
                    catch (Exception e)
                    {
                        Console.WriteLine("\n" + (e.Message == "" ? "...Instruction non valide..." : e.Message) + "\n");
                        Thread.Sleep(2500);
                    }
                }
                victoire = AGagne();
            } while (!victoire);

            if (victoire)
                Console.WriteLine("\n\n\n---___---___---___--- VICTOIRE ---___---___---___---\n\n\n");
        }

        // --------------------------------------- AFFICHAGE ---------------------------------------

        public override string ToString()
        {
            string chaine = "     " + (height < 11 ? "" : " ");
            for (int c = 0; c < width; c++)
            {
                chaine += (char)('A' + c) + " ";
            }
            chaine += "\n";

            for (int j = 0; j < height; j++)
            {
                chaine += ("" + j).PadLeft(height < 11 ? 1 : 2) + "    ";
                for (int i = 0; i < width; i++)
                {
                    if (grid[i, j] == BombValue) chaine += "*";
                    else chaine += grid[i, j];
                    chaine += " ";
                }
                chaine += "\n";
            }
            return chaine;
        }
        public string GetGrilleJoueur()
        {
            string chaine = "     " + (height < 11 ? "" : " ");
            for (int c = 0; c < width; c++)
            {
                chaine += (char)('A' + c) + " ";
            }
            chaine += "\n";

            for (int j = 0; j < height; j++)
            {
                chaine += ("" + j).PadLeft(height < 11 ? 1 : 2) + "    ";
                for (int i = 0; i < width; i++)
                {
                    if (DejaJouee(i, j))
                    {
                        if (grid[i, j] == BombValue) chaine += "*";
                        else if (grid[i, j] == 0) chaine += "°";
                        else chaine += grid[i, j];
                    }
                    else if (EstMarquee(i, j))
                    {
                        chaine += "!";
                    }
                    else if (EstQuestionnee(i, j))
                    {
                        chaine += "?";
                    }
                    else
                        chaine += ".";
                    chaine += " ";
                }
                chaine += "\n";
            }
            return chaine;
        }

        // --------------------------------------- AUTRE ---------------------------------------

        public void sauvegarderLaGrille()
        {
            // On crée le fichier. Si il existait déjà on l'écrase
            using (StreamWriter sw = File.CreateText(FileName))
            {
                sw.WriteLine("x,y,val,revelee,marquee,questionnee");
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        sw.WriteLine($"{i},{j},{grid[i, j]},{DejaJouee(i, j)},{EstMarquee(i, j)},{EstQuestionnee(i, j)}");
                    }
                }
            }
        }
    }
}