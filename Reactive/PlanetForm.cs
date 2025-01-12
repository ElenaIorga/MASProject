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
                    // Draw different cells based on values in the matrix
                    if (Utils.Maze[i, j] == 1)
                    {
                        g.FillRectangle(Brushes.Black, 20 + j * cellSize, 20 + i * cellSize, cellSize, cellSize);
                    }
                    else if (Utils.Maze[i, j] == 2)
                    {
                        g.FillRectangle(Brushes.Green, 20 + j * cellSize, 20 + i * cellSize, cellSize, cellSize);
                    }
                    else if (Utils.Maze[i, j] == 3)
                    {
                        g.FillRectangle(Brushes.Red, 20 + j * cellSize, 20 + i * cellSize, cellSize, cellSize);
                    }
                }
            }

            // Draw agents
            if (_ownerAgent != null)
            {
                foreach (string explorer in _ownerAgent.ExplorerPositions.Keys)
                {
                    if (_ownerAgent.ExplorerStates[explorer] != PlanetAgent.ExplorerAgentState.Active)
                    {
                        continue;
                    }

                    string v = _ownerAgent.ExplorerPositions[explorer];
                    string[] t = v.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);

                    // Draw explorer as a blue ellipse
                    g.FillEllipse(Brushes.Blue, 20 + y * cellSize + 6, 20 + x * cellSize + 6, cellSize - 12, cellSize - 12);
                }
            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
        }
    }
}
    