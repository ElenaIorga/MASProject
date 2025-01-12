using System;
using System.Collections.Generic;
using System.Linq;

namespace Reactive
{
    public class Utils
    {
        public static int Size = 8;
        public static int NoExplorers = 2;
        public static int NoResources = 10;
        public static int[,] Maze = MazeGenerator.GetMatrix(Size, Size);
        public static decimal MinimumThreshold = 0.0000001m;
     
        public static int Delay = 400;
        public static int SpawnDelay = 2 * Delay;
        public static Random RandNoGen = new Random();
        public static decimal DecrementValue = 0.00001m;

        public static void ParseMessage(string content, out string action, out List<string> parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = new List<string>();
            for (int i = 1; i < t.Length; i++)
                parameters.Add(t[i]);
        }

        public static int[,] RotateAndFlipMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int[,] rotatedMatrix = new int[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotatedMatrix[j, rows - 1 - i] = matrix[i, j];
                }
            }

            int[,] flippedMatrix = new int[cols, rows];
            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    flippedMatrix[i, j] = rotatedMatrix[i, rows - 1 - j];
                }
            }

            return flippedMatrix;
        }


        public static void ParseMessage(string content, out string action, out string parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = "";

            if (t.Length > 1)
            {
                for (int i = 1; i < t.Length - 1; i++)
                    parameters += t[i] + " ";
                parameters += t[t.Length - 1];
            }
        }
        public static void ParseParameters(string content, out List<string> parameters)
        {
            parameters = content.Split().ToList<string>();
        }
        public static Position GetPreviousDirection(int startX, int startY, int endX, int endY)
        {
            int dX = endX - startX;
            int dY = endY - startY;

            if (dX == -1)
            {
                return Position.Up;
            }
            if (dX == 1)
            {
                return Position.Down;
            }
            if (dY == -1)
            {
                return Position.Left;
            }
            if (dY == 1)
            {
                return Position.Right;
            }

            return Position.Right;
        }
        public static void ParseIntParameters(string content, out List<int> parameters)
        {
            List<string> splited;
            ParseParameters(content, out splited);
            parameters = splited.Select(str => int.Parse(str)).ToList();
        }

        public static string Str(object p1, object p2)
        {
            return string.Format("{0} {1}", p1, p2);
        }

        public static string Str(object p1, object p2, object p3)
        {
            return string.Format("{0} {1} {2}", p1, p2, p3);
        }
        public static string Str(object p1, object p2, object p3, object p4)
        {
            return string.Format("{0} {1} {2} {3}", p1, p2, p3, p4);
        }


        public static List<Cell> CreateWeightedMaze()
        {
            //int[,] flippedMaze = RotateAndFlipMatrix(Maze);
            int[,] flippedMaze = Maze;
            List<Cell> cells = new List<Cell>();

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    decimal upDirection = 0;
                    decimal downDirection = 0;
                    decimal leftDirection = 0;
                    decimal rightDirection = 0;

                    if (flippedMaze[i, j] != 1)
                    {
                        if (i != 0)
                        {
                            if (flippedMaze[i - 1, j] != 1)
                            {
                                upDirection = 1;
                            }
                        }
                        if (i != Size - 1)
                        {
                            if (flippedMaze[i + 1, j] != 1)
                            {
                                downDirection = 1;
                            }
                        }
                        if (j != 0)
                        {
                            if (flippedMaze[i, j - 1] != 1)
                            {
                                leftDirection = 1;
                            }
                        }
                        if (j != Size - 1)
                        {
                            if (flippedMaze[i, j + 1] != 1)
                            {
                                rightDirection = 1;
                            }
                        }
                    }
                    cells.Add(new Cell()
                    {
                        X = i,
                        Y = j,
                        Up = upDirection,
                        Down = downDirection,
                        Left = leftDirection,
                        Right = rightDirection
                    });
                }
            }

            return cells;
        }

        public static decimal GetHeuristic(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

    }
}