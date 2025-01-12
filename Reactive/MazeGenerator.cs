using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reactive
{
    public class MazeGenerator
    {
        public static string StartPosition;
        public static string StopPosition;

        public static int[,] GetMatrix(int rows, int columns)
        {
            /*            int[,] maze = new int[rows, columns];
                        for (int i = 0; i < rows; i++)
                            for (int j = 0; j < columns; j++)
                                maze[i, j] = 1;

                        Random random = new Random();
                        Point start = new Point((int)(random.NextDouble() * rows), (int)(random.NextDouble() * columns), null);
                        maze[start.r, start.c] = 2;
                        StartPosition = Utils.Str(start.r, start.c);

                        SortedList<int, Point> initial = new SortedList<int, Point>();
                        SortedList<int, Point> frontier = GetFrontier(maze, start, initial);

                        Point last = null;

                        while (frontier.Count > 0)
                        {
                            var smallest = frontier.First();
                            frontier.RemoveAt(0); // Remove the cell with the smallest priority
                            Point current = smallest.Value;

                            Point opposite = current.opposite();
                            try
                            {
                                if (maze[current.r, current.c] == 1 && maze[opposite.r, opposite.c] == 1)
                                {
                                    maze[current.r, current.c] = 0;
                                    maze[opposite.r, opposite.c] = 0;
                                    last = opposite;

                                    // Get new frontier cells and add them to the existing frontier
                                    SortedList<int, Point> newFrontier = GetFrontier(maze, opposite, frontier);

                                    foreach (var newCell in newFrontier)
                                    {
                                        // Add new cells to the frontier (avoids duplicates)
                                        frontier.Add(newCell.Key, newCell.Value);
                                    }
                                }
                            }
                            catch (Exception) { }

                            if (frontier.Count == 0)
                            {
                                maze[last.r, last.c] = 3; // Mark the exit
                                StopPosition = Utils.Str(last.r, last.c);
                            }
                        }
                        return maze;*/

            //return maze;
            StartPosition = Utils.Str(4, 4);
            StopPosition = Utils.Str(2, 2);
            return  new int[8, 8]
{
    { 0, 0, 0, 0, 0, 1, 1, 1 },
    { 0, 1, 1, 1, 0, 1, 1, 1 },
    { 0, 0, 3, 1, 0, 0, 0, 1 },
    { 1, 1, 1, 1, 0, 1, 1, 1 },
    { 0, 0, 0, 0, 2, 0, 0, 1 },
    { 0, 1, 1, 1, 1, 1, 0, 1 },
    { 0, 0, 0, 0, 0, 1, 0, 1 },
    { 1, 1, 1, 1, 1, 1, 1, 1 }
};
        }

        /* private static SortedList<int, Point> GetFrontier(int[,] maze, Point start)
         {
             Random random = new Random();
             SortedList<int, Point> frontier = new SortedList<int, Point>();

             for (int x = -1; x <= 1; x++)
                 for (int y = -1; y <= 1; y++)
                 {
                     if ((x == 0 && y == 0) || (x != 0 && y != 0)) //not diagonal nor current cell
                         continue;
                     try
                     {
                         if (maze[start.r + x, start.c + y] == 0) continue; //ensures it is a wall
                     }
                     catch (IndexOutOfRangeException)
                     {
                         continue;
                     }
                     frontier.Add(random.Next(), new Point(start.r + x, start.c + y, start));//assigns a random value for frontier for simulating priority
                 }
             return frontier;
         }*/
        private static SortedList<int, Point> GetFrontier(int[,] maze, Point start, SortedList<int, Point> existingFrontier)
        {
            Random random = new Random();
            SortedList<int, Point> newFrontier = new SortedList<int, Point>();

            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) || (x != 0 && y != 0)) // Skip diagonal and current cell
                        continue;
                    try
                    {
                        int neighborRow = start.r + x;
                        int neighborCol = start.c + y;

                        // Skip cells that are not walls or already in the maze
                        if (maze[neighborRow, neighborCol] == 0) continue;

                        // Check if the cell is already in the existing frontier
                        if (existingFrontier.Values.Any(p => p.r == neighborRow && p.c == neighborCol)) continue;

                        // Add the valid neighbor to the new frontier
                        newFrontier.Add(random.Next(), new Point(neighborRow, neighborCol, start));
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // Skip out-of-bounds cells
                        continue;
                    }
                }

            return newFrontier;
        }


    }
}
