using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Timers.Timer;
using System.Timers;

namespace Reactive
{
    public class PlanetAgent : Agent
    {
        private PlanetForm _formGui;
        public Dictionary<string, string> ExplorerPositions { get; set; }
        public List<ExplorerAgent> ExplorerAgents { get; set; }
        private string _basePosition;
        public enum ExplorerAgentState { Dormant, Active, Dead };
        public ExplorerAgent Current;
        private bool solutionFound = false;
        private Timer _spawnTimer;

        public Dictionary<string, ExplorerAgentState> ExplorerStates { get; set; }

        public PlanetAgent()
        {
            ExplorerPositions = new Dictionary<string, string>();
            ExplorerStates = new Dictionary<string, ExplorerAgentState>();
            ExplorerAgents = new List<ExplorerAgent>();

            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();

            _spawnTimer = new System.Timers.Timer();
            _spawnTimer.Elapsed += SpawnTimeOut;
            _spawnTimer.Interval = Utils.SpawnDelay;
        }

        private void GUIThread()
        {
            _formGui = new PlanetForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            Application.Run();
        }

        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);

            foreach (ExplorerAgent explorer in ExplorerAgents)
            {
                ExplorerStates[explorer.Name] = ExplorerAgentState.Dormant;
            }
            _spawnTimer.Start();
        }

        public void SpawnTimeOut(object sender, ElapsedEventArgs e)
        {
            Send(Name, "[StartNewExplorer]");
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "[StartNewExplorer]":
                    HandleSpawn();
                    break;
                case "try_to_move":
                    HandleMove(message.Sender, parameters);
                    break;

                default:
                    break;
            }
            _formGui.UpdatePlanetGUI();
        }

        private void HandlePosition(string sender, string position)
        {
            ExplorerPositions.Add(sender, position);
            Send(sender, "perform_based_on_state_and_position");
        }

        private void HandleSpawn()
        {
            _spawnTimer.Stop();

            int numberOfAvailable = 0;
            foreach (string explorer in ExplorerStates.Keys)
            {
                if (ExplorerStates[explorer] == ExplorerAgentState.Dormant)
                {
                    numberOfAvailable++;
                }
            }

            Console.WriteLine("{0}: Left to spawn: {1}", Name, numberOfAvailable);

            if (numberOfAvailable > 0)
            {
                string nextExplorer = null;
                bool isStartFree = true;
                foreach (ExplorerAgent explorer in ExplorerAgents)
                {
                    if (ExplorerStates[explorer.Name] == ExplorerAgentState.Dormant)
                    {
                        nextExplorer = explorer.Name;
                        break;
                    }
                    if (ExplorerPositions.ContainsKey(explorer.Name) && ExplorerPositions[explorer.Name].Equals(MazeGenerator.StartPosition))
                    {
                        isStartFree = false;
                    }
                }

                if (nextExplorer != null && isStartFree)
                {
                    if (Current == null)
                    {
                        foreach (ExplorerAgent explorer in ExplorerAgents)
                        {
                            if (explorer.Name == nextExplorer)
                            {
                                Current = explorer;
                                break;
                            }
                        }
                    }
                    numberOfAvailable--;
                    ExplorerStates[nextExplorer] = ExplorerAgentState.Active;
                    ExplorerPositions[nextExplorer] = MazeGenerator.StartPosition;
                    Send(nextExplorer, Utils.Str("perform_based_on_state_and_position", MazeGenerator.StartPosition));
                }
            }

            if (numberOfAvailable > 0)
            {
                _spawnTimer.Start();
            }
        }

        private void HandleMove(string sender, string position)
        {
            List<int> point;
            Utils.ParseIntParameters(position, out point);
            if (Utils.Maze[point[0], point[1]] == 1)
            {
                Send(sender, "got_stuck");
                return;
            }

            foreach (string k in ExplorerPositions.Keys)
            {
                if (k == sender)
                    continue;
                if (ExplorerPositions[k] == position)
                {
                    Send(sender, Utils.Str("got_stuck", k));
                    return;
                }
            }

            ExplorerPositions[sender] = position;

            if (position == MazeGenerator.StopPosition)
            {
                if (solutionFound)
                {
                    Send(sender, Utils.Str("another_explorer_already_found_exit", position));
                }
                else
                {
                    Send(sender, Utils.Str("explorer_found_position", position));
                    solutionFound = true;
                }

                ExplorerStates[sender] = ExplorerAgentState.Dead;
                ExplorerPositions.Remove(sender);

                Console.WriteLine("Remaining Explorers: {0}", ExplorerPositions.Count);
                if (ExplorerPositions.Count == 0)
                {
                    Console.WriteLine("{0}: Stopped", Name);
                    foreach (string agent in Environment.AllAgents())
                    {
                        Console.WriteLine("Remaining agent: {0}", agent);
                    }
                    this.Stop();
                }
                return;
            }

            Send(sender, Utils.Str("perform_based_on_state_and_position", position));
        }
    }
}
