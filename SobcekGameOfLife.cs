using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GOLStartUpTemplate1
{
    public partial class SobcekGameOfLife : Form
    {
        static int xCellCount;
        static int yCellCount;

        bool showGrid = true;
        bool showNeighbors = false;
        bool showHUD = true;
        // The universe array
        bool[,] universe = new bool[5, 5];

        // Drawing colors
        Color gridColor;
        Color cellColor;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;
        int seed = 0;
        private int randomizeCellsDensity = 50;

        string filePath = "";

        public SobcekGameOfLife()
        {
            InitializeComponent();

           
            ReadSettings(); //reads all settings 


            universe = new bool[xCellCount, yCellCount]; //initializes the universe

            // Setup the timer
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer running


            UpdateBottomText();
        }

        
        private void UpdateBottomText() 
        {
            string generationText = "";
            generationText = "Generations = " + generations.ToString() + "     ";


            string modeText = "";
            if (GameRules.isToroidal)
            {
                modeText = "Mode: Toroidal     ";
            }
            else
            {
                modeText = "Mode: Finite     ";
            }

            string cellsText = "";
            int aliveCells = 0;
            for (int x = 0; x < universe.GetLength(0); x++)
            {
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    if (universe[x, y])
                    {
                        aliveCells++;
                    }
                }
            }
            cellsText = "Cells Alive: " + aliveCells + "     ";

            string timerText = "";
            timerText = "Interval: " + timer.Interval.ToString();


            toolStripStatusLabelGenerations.Text = generationText + modeText + cellsText + timerText;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e) //when you click, it updates the cells
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
                UpdateBottomText();
            }
        }
        private void NextGeneration()
        {

            List<CellPoint> cellsToToggle = new List<CellPoint>();
            GameRules.CalculateRules(ref universe, cellsToToggle);

            for (int i = 0; i < cellsToToggle.Count; i++)
            {
                universe[cellsToToggle[i].cellX, cellsToToggle[i].cellY] = cellsToToggle[i].cellState;
            }

            // Increment generation count
            generations++;

            UpdateBottomText();

            graphicsPanel1.Invalidate();
        }
        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

            for (int y = 0; y < universe.GetLength(1); y++) //loop all y
            {

                for (int x = 0; x < universe.GetLength(0); x++) //loop all x
                {
                    // A rectangle to represent each cell in pixels
                    Rectangle cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    } // Fill the cell with a brush if alive

                    if (showGrid)
                    {
                        // Outline the cell with a pen
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    } //toggles showing the gridlines

                    if (showHUD)
                    {
                        Brush brush = new SolidBrush(Color.PaleVioletRed);
                        PointF location = new PointF(1, graphicsPanel1.ClientSize.Height - 30);
                        string toroid = GameRules.isToroidal ? "Toroidal" : "Finite";
                        string[] HUDSentences = new string[]
                        {
                            $"Generations: {generations}",
                            $"Seed: {seed}",
                            $"Boundary mode: {toroid}",
                            $"Universe size: {universe.GetLength(0)} x {universe.GetLength(1)}"
                        };

                        for (int i = 0; i < HUDSentences.Length; i++)
                        {
                            e.Graphics.DrawString(HUDSentences[i], graphicsPanel1.Font, brush, location);

                            location.Y -= 15;
                        }
                    }//displays the heads up display

                    if (showNeighbors)
                    {
                        Brush redBrush = new SolidBrush(Color.Red);
                        Brush greenBrush = new SolidBrush(Color.Green);

                        int neighbors = 0;

                        if (GameRules.isToroidal)
                        {
                            neighbors = GameRules.CountNeighborsToroidal(ref universe, x, y);
                        }
                        else
                        {
                            neighbors = GameRules.CountNeighborsFinite(ref universe, x, y);
                        }

                        PointF location = new PointF(cellRect.X + cellRect.Width / 2.2f, cellRect.Y + cellRect.Height / 2.2f);
                        if (neighbors == 0)
                        {
                            continue;
                        }
                        else if (neighbors == 3 || (universe[x, y] && neighbors == 2))
                        {
                            e.Graphics.DrawString(neighbors.ToString(), graphicsPanel1.Font, greenBrush, location);
                        }
                        else
                        {
                            e.Graphics.DrawString(neighbors.ToString(), graphicsPanel1.Font, redBrush, location);
                        }

                        redBrush.Dispose();
                        greenBrush.Dispose();
                    }//shows the neighbor count
                }
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();


        }
        private void SaveFile(string _filePath)
        {
            StreamWriter sw = new StreamWriter(_filePath);
            filePath = _filePath;

            sw.WriteLine("//This is a cells file, this contains data for the universe");
            sw.WriteLine("//" + DateTime.Now.ToString());
            sw.WriteLine("//The size of the universe for this file is " + universe.GetLength(0) + " by " + universe.GetLength(1) + "\n");

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                string currentRow = "";

                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (universe[x, y])
                    {
                        currentRow += 'O';
                    }
                    else
                    {
                        currentRow += '.';
                    }
                }
                sw.WriteLine(currentRow);
            }
            sw.Close();
        } 
        private void SaveAsFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;
            dlg.DefaultExt = "cells";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                SaveFile(dlg.FileName);
            }
        } 

        private void newToolStripButton_Click(object sender, EventArgs e) //new file
        {
            for (int i = 0; i < universe.GetLength(0); i++)
            {
                for (int j = 0; j < universe.GetLength(1); j++)
                {
                    universe[i, j] = false;
                }
            }

            generations = 0;
            filePath = "";
            timer.Enabled = false;

            graphicsPanel1.Invalidate();
            UpdateBottomText();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(filePath)) 
            {
                SaveFile(filePath);
            }
            else 
            {
                SaveAsFile();
            }
        } 

        
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)  
        {
            SaveAsFile();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)  
        {
            OpenFileDialog dlg = new OpenFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(dlg.FileName);
                filePath = dlg.FileName;

                #region // resizing the array if its needed
                int rowCount = 0;
                int columnCount = 0;

                while (!sr.EndOfStream)  
                {
                    string currentRow = sr.ReadLine();
                    if (!string.IsNullOrEmpty(currentRow))  
                    {

                        if ((currentRow[0] == '.') || (currentRow[0] == 'O'))  
                        {
                            rowCount++;
                            columnCount = currentRow.Length;
                        }
                    }
                }

                if (universe.GetLength(0) < columnCount && universe.GetLength(1) > rowCount) 
                {
                    universe = new bool[columnCount, universe.GetLength(1)];
                    xCellCount = columnCount;
                }
                else if ((universe.GetLength(0) > columnCount && universe.GetLength(1) < rowCount)) 
                {
                    universe = new bool[universe.GetLength(0), rowCount];
                    yCellCount = rowCount;
                }
                else  
                {
                    universe = new bool[columnCount, rowCount];
                    xCellCount = columnCount;
                    yCellCount = rowCount;
                }
                #endregion

                sr.Close();

                sr = new StreamReader(dlg.FileName);

                int gCurrentRow = 0;
                while (!sr.EndOfStream)  
                {
                    string srCurrentRow = sr.ReadLine();
                    if (!string.IsNullOrEmpty(srCurrentRow))  
                    {

                        if ((srCurrentRow[0] != '/') && !(srCurrentRow[0] == (char)0))  
                        { 

                            for (int i = 0; i < srCurrentRow.Length; i++)  
                            {
                                if (srCurrentRow[i] == 'O')
                                {
                                    universe[i, gCurrentRow] = true;
                                }
                                else
                                {
                                    universe[i, gCurrentRow] = false;
                                }
                            }

                            gCurrentRow++;


                        }
                    }
                }
                sr.Close();
                graphicsPanel1.Invalidate();
            }
        }
        private void toggleGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showGrid = !showGrid;
            graphicsPanel1.Invalidate();

        }  
        private void toggleNeighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showNeighbors = !showNeighbors;
            graphicsPanel1.Invalidate();
        }  
        private void toggleHUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showHUD = !showHUD;
            graphicsPanel1.Invalidate();
        }  
        private void PauseButton_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }
        private void PlayButton_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
        }
        private void NextGenerationButton_Click(object sender, EventArgs e)

        {
            NextGeneration();
        }
        private void RandomizeCells()  
        {
            Random randy = new Random(seed);

            for (int i = 0; i < universe.GetLength(0); i++) //loops through all x
            {
                for (int j = 0; j < universe.GetLength(1); j++) //loops through all y
                {
                    if (randy.Next(0, 100) <= randomizeCellsDensity)
                    {
                        universe[i, j] = true;
                    }
                    else
                    {
                        universe[i, j] = false;
                    }
                }
            }
            graphicsPanel1.Invalidate();
            UpdateBottomText();
        }

        private void randomizeCellsToolStripMenuItem_Click(object sender, EventArgs e)  
        {
            RandomizeCells();
        }
        private void fromRandomSeedToolStripMenuItem_Click(object sender, EventArgs e)  
        {
            Random randy = new Random(seed);
            seed = randy.Next(0, 1073741824);

            RandomizeCells();

        }
        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            seed = DateTime.Now.Millisecond;

            RandomizeCells();
        }  
        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)  
        {
            RandomizeForm dlg = new RandomizeForm();

            dlg.SetSeed(seed);
            dlg.SetDensity(randomizeCellsDensity);

            if (DialogResult.OK == dlg.ShowDialog())
            {
                seed = dlg.GetSeed();
                randomizeCellsDensity = dlg.GetDensity();
                graphicsPanel1.Invalidate();
            }
        }
        private void randomizeSeedCellsToolStripMenuItem_Click(object sender, EventArgs e) 
        {
            Random randy = new Random((int)DateTime.Now.Ticks);

            seed = randy.Next(0, 1073741824);
            randomizeCellsDensity = randy.Next(0, 101);
            graphicsPanel1.Invalidate();
        }
        private void ReadSettings() 
        {
            timer.Interval = Properties.Settings.Default.TimerInterval;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            xCellCount = Properties.Settings.Default.XCells;
            yCellCount = Properties.Settings.Default.YCells;
            graphicsPanel1.BackColor = Properties.Settings.Default.PanelColor;
            universe = new bool[xCellCount, yCellCount];
            graphicsPanel1.Invalidate();
            UpdateBottomText();
        }
        private void WriteSettings() 
        {
            Properties.Settings.Default.TimerInterval = timer.Interval;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.XCells = xCellCount;
            Properties.Settings.Default.YCells = yCellCount;
            Properties.Settings.Default.PanelColor = graphicsPanel1.BackColor;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e) 
        {
            WriteSettings();

            Properties.Settings.Default.Save();
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e) 
        {
            ColorDialog dlg = new ColorDialog();

            dlg.Color = graphicsPanel1.BackColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                graphicsPanel1.BackColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }
        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            dlg.Color = cellColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }
        private void aliveCellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            dlg.Color = gridColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void toggleModeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GameRules.isToroidal = !GameRules.isToroidal;
            UpdateBottomText();
            string temp;
            if (GameRules.isToroidal)
            {
                temp = "toroidal ";
            }
            else
            {
                temp = "finite ";
            }
            graphicsPanel1.Invalidate();
            MessageBox.Show("The mode has been switched to " + temp + "mode.");

        }

        private void settingsToolStripMenuItem2_Click(object sender, EventArgs e) 
        {

            SettingsForm dlg = new SettingsForm();

            dlg.SetInterval(timer.Interval);
            dlg.SetXCells(xCellCount);
            dlg.SetYCells(yCellCount);

            if (dlg.ShowDialog() != DialogResult.Cancel)
            {
                DialogResult r = MessageBox.Show("This will delete the current universe. Is that ok?", "Warning", MessageBoxButtons.OKCancel);
                if (r == DialogResult.OK)
                {


                    timer.Interval = dlg.GetInterval();
                    xCellCount = dlg.GetXCells();
                    yCellCount = dlg.GetYCells();
                    universe = new bool[xCellCount, yCellCount];

                    graphicsPanel1.Invalidate();
                    UpdateBottomText();
                }
            }




        }
        private void resetToolStripMenuItem_Click(object sender, EventArgs e) 
        {
            DialogResult r = MessageBox.Show("This will reset all settings and delete the universe. Do you want to continue?", "Warning", MessageBoxButtons.YesNo);

            if (r == DialogResult.Yes)
            {
                Properties.Settings.Default.Reset();

                ReadSettings();
            }
        }
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e) 
        {
            DialogResult r = MessageBox.Show("This will reset all settings and delete the universe. Do you want to continue?", "Warning", MessageBoxButtons.YesNo);

            if (r == DialogResult.Yes)
            {
                Properties.Settings.Default.Reload();

                ReadSettings();
            }
        }
    }
   }
