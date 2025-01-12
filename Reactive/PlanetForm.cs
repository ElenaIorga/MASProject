using System;
using System.Drawing;
using System.Windows.Forms;

namespace Reactive
{
    public partial class PlanetForm : Form
    {
        private PlanetAgent _ownerAgent;
        private Bitmap _doubleBufferImage;

        public PlanetForm()
        {
            InitializeComponent();
        }

        public void SetOwner(PlanetAgent a)
        {
            _ownerAgent = a;
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            DrawPlanet();
        }

        public void UpdatePlanetGUI()
        {
            DrawPlanet();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            DrawPlanet();
        }

        private void DrawPlanet()
        {
            int w = pictureBox.Width;
            int h = pictureBox.Height;

            if (_doubleBufferImage != null)
            {
                _doubleBufferImage.Dispose();
                GC.Collect(); // prevents memory leaks
            }

            _doubleBufferImage = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(_doubleBufferImage);
            g.Clear(Color.White);

            int minXY = Math.Min(w, h);
            int cellSize = (minXY - 40) / Utils.Size;

            for (int i = 0; i <= Utils.Size; i++)
            {
                g.DrawLine(Pens.DarkGray, 20, 20 + i * cellSize, 20 + Utils.Size * cellSize, 20 + i * cellSize);
                g.DrawLine(Pens.DarkGray, 20 + i * cellSize, 20, 20 + i * cellSize, 20 + Utils.Size * cellSize);
            }

            //g.FillEllipse(Brushes.Red, 20 + Utils.Size / 2 * cellSize + 4, 20 + Utils.Size / 2 * cellSize + 4, cellSize - 8, cellSize - 8); // the base
            // Draw the walls.
            for (int i = 0; i <= Utils.Size; i++)
            {
                g.DrawLine(Pens.DarkGray, 20, 20 + i * cellSize, 20 + Utils.Size * cellSize, 20 + i * cellSize);
                g.DrawLine(Pens.DarkGray, 20 + i * cellSize, 20, 20 + i * cellSize, 20 + Utils.Size * cellSize);
            }
            for (int i = 0; i < Utils.Size; i++)
            {
                for (int j = 0; j < Utils.Size; j++)
                {
                    // walls
                    if (Utils.Maze[i, j] == 1)
                    {
                        g.FillRectangle(Brushes.Black, 20 + i * cellSize, 20 + j * cellSize, cellSize, cellSize);
                    }
                    // start
                    else if (Utils.Maze[i, j] == 2)
                    {
                        g.FillRectangle(Brushes.Green, 20 + i * cellSize, 20 + j * cellSize, cellSize, cellSize);
                    }
                    // stop
                    else if (Utils.Maze[i, j] == 3)
                    {
                        g.FillRectangle(Brushes.Red, 20 + i * cellSize, 20 + j * cellSize, cellSize, cellSize);
                    }
                    else if (_ownerAgent?.Current != null)
                    {
                        PointF[] pointFs =
                        {
                            new PointF(20 + i * cellSize + cellSize/2, 20 + j * cellSize + cellSize/2),
                            new PointF(20 + i * cellSize + cellSize, 20 + j * cellSize),
                            new PointF(20 + i * cellSize + cellSize, 20 + j * cellSize + cellSize),
                            new PointF(20 + i * cellSize, 20 + j * cellSize + cellSize),
                            new PointF(20 + i * cellSize, 20 + j * cellSize)
                        };

                        int[,] pointsPositions =
                        {
                            {0, 1, 2},
                            {0, 2, 3},
                            {0, 3, 4},
                            {0, 4, 1}
                        };

                       /* // Split the square in 4 triangles and draw based on the weight.
                        for (int dir = 0; dir < 4; dir++)
                        {
                            int colorAlpha = (int)(255 - _ownerAgent.Current.Weights.Values[i, j, dir] * 255.0);
                            Color newColor = Color.FromArgb(colorAlpha, Color.Bisque);
                            PointF[] points = new PointF[3];
                            points[0] = pointFs[pointsPositions[dir, 0]];
                            points[1] = pointFs[pointsPositions[dir, 1]];
                            points[2] = pointFs[pointsPositions[dir, 2]];
                            g.FillPolygon(new SolidBrush(newColor), points);
                        }*/

                    }

                }
                if (_ownerAgent != null)
                {
                    foreach (string explorer in _ownerAgent.ExplorerPositions.Keys)
                    {
                        if (_ownerAgent.ExplorerStates[explorer] != PlanetAgent.ExplorerAgentState.Active) { continue; }

                        string v = _ownerAgent.ExplorerPositions[explorer];
                        string[] t = v.Split();
                        int x = Convert.ToInt32(t[0]);
                        int y = Convert.ToInt32(t[1]);

                        g.FillEllipse(Brushes.Blue, 20 + x * cellSize + 6, 20 + y * cellSize + 6, cellSize - 12, cellSize - 12);
                    }
                }

                /*foreach (string v in _ownerAgent.ResourcePositions.Values)
                {
                    string[] t = v.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);

                    g.FillRectangle(Brushes.LightGreen, 20 + x * cellSize + 10, 20 + y * cellSize + 10, cellSize - 20, cellSize - 20);
                }*/
            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
        }
    }
}
    