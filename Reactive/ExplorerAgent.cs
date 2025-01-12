using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reactive
{
    public class ExplorerAgent : Agent
    {
        private int _currentX, _currentY;
        private State _state;
        private List<string> _lastPositions;
        private List<string> _pathToFinish;
        public IList<Cell> _agentMap;
        private int _exitX = -1, _exitY = -1;
        private List<Position> _nextDirections;
        private HashSet<string> _visitedPositions = new HashSet<string>();

        private enum State { Waiting, Started, Blocked, Finished };

        public override void Setup()
        {
            Console.WriteLine("Adding " + Name);

            _currentX = -1;
            _currentY = -1;
            _state = State.Waiting;
            _lastPositions = new List<string>();
            _pathToFinish = new List<string>();
            _agentMap = Utils.CreateWeightedMaze();
        }

        public override void Act(Message message)
        {
            Console.WriteLine($"\t[{message.Sender} -> {Name}]: {message.Content}");

            string action;
            List<string> parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "perform_based_on_state":
                    HandleAction();
                    break;
                case "perform_based_on_state_and_position":
                    HandleMove(parameters);
                    break;
                case "avoid_this_direction":
                    HandleAvoid(parameters);
                    break;
                case "decrement_explored_position":
                    DecrementExploredPosition(parameters);
                    break;
                case "got_stuck":
                    FixStuck(parameters);
                    break;
                case "what_should_i_do":
                    HandleWhatState(message.Sender, parameters);
                    break;
                case "state":
                    HandleState(parameters);
                    break;
                case "explorer_found_position":
                    IFoundTheExit(parameters);
                    break;
                case "another_explorer_already_found_exit":
                    AnotherAgentFoundExit(parameters);
                    break;
                case "follow_me_to_exit":
                    HandleCome(parameters);
                    break;
            }
        }

        private void HandleCome(List<string> parameters)
        {
            _exitX = int.Parse(parameters[0]);
            _exitY = int.Parse(parameters[1]);

            if (_state == State.Waiting) return;

            _state = State.Finished;
            CreatePathToExit(_exitX, _exitY);
        }

        private void AnotherAgentFoundExit(List<string> parameters)
        {
            _currentX = int.Parse(parameters[0]);
            _currentY = int.Parse(parameters[1]);

            Console.WriteLine($"{Name}: Another agent found exit");
            this.Stop();
        }

        private void IFoundTheExit(List<string> parameters)
        {
            _currentX = int.Parse(parameters[0]);
            _currentY = int.Parse(parameters[1]);

            Broadcast(Utils.Str("follow_me_to_exit", _currentX, _currentY), false, "explorers_channel");
            
            Console.WriteLine($"{Name}: Found the exit");
            this.Stop();
        }

        private void HandleWhatState(string sender, List<string> parameters)
        {
            Send(sender, Utils.Str("state", _state.ToString(), _currentX, _currentY));
        }

        private void HandleState(List<string> parameters)
        {
            if (_state == State.Blocked || _state == State.Finished)
            {
                Send(Name, "perform_based_on_state");
            }
            else if (_state == State.Started && parameters[0] == State.Blocked.ToString())
            {
                ResolveBlock(parameters);
            }
            else if (_state == State.Started && parameters[0] == State.Started.ToString())
            {
                Send(Name, "perform_based_on_state");
            }

        }

        private void ResolveBlock(List<string> parameters)
        {
            int otherX = int.Parse(parameters[1]);
            int otherY = int.Parse(parameters[2]);

            Position dir = Utils.GetPreviousDirection(_currentX, _currentY, otherX, otherY);
            Cell currentCell = _agentMap.FirstOrDefault(a => a.X == _currentX && a.Y == _currentY);

            if (dir == Position.Up) currentCell.Up = 0;
            else if (dir == Position.Down) currentCell.Down = 0;
            else if (dir == Position.Left) currentCell.Left = 0;
            else if (dir == Position.Right) currentCell.Right = 0;

            _state = State.Blocked;
            Send(Name, "perform_based_on_state");
        }

        private void FixStuck(List<string> parameters)
        {
            if (_state == State.Started)
            {
                if (_nextDirections.Count == 0 && parameters.Count > 0)
                {
                    Send(parameters[0], "what_should_i_do");
                }
                else
                {
                    ExecuteExploringStrategy();
                }
            }
            else if (_state == State.Blocked)
            {
                BacktrackOrRetry();
            }
            else if (_state == State.Finished)
            {
                FollowPathToFinish();
            }
        }

        private void BacktrackOrRetry()
        {
            if (_lastPositions.Count > 0)
            {
                Send("planet", Utils.Str("try_to_move", _lastPositions.Last()));
            }
            else
            {
                Console.WriteLine("No positions to backtrack. Stopping.");
                this.Stop();
            }
        }

        private void FollowPathToFinish()
        {
            if (_pathToFinish.Count > 0)
            {
                string nextPosition = _pathToFinish.First();
                _pathToFinish.RemoveAt(0);
                Send("planet", Utils.Str("try_to_move", nextPosition));
            }
            else
            {
                Console.WriteLine("Path to finish is empty. Stopping.");
                this.Stop();
            }
        }

        private void HandleAction()
        {
            Console.WriteLine($"Handling action: State = {_state}, Position = {_currentX}, {_currentY}");
            if (_state == State.Started)
            {
                ExecuteExploringStrategy();
            }
            else if (_state == State.Blocked)
            {
                ExecuteDeadEndStrategy();
            }
            else if (_state == State.Finished)
            {
                FollowPathToFinish();
            }
        }

        private void ExecuteDeadEndStrategy()
        {
            if (_lastPositions.Count > 0 && _lastPositions.Last() == Utils.Str(_currentX, _currentY))
            {
                _lastPositions.RemoveAt(_lastPositions.Count - 1);
            }

            List<string> exclude = new List<string>(_lastPositions);
            List<Position> availableDirections = GetNextDirectionsOrdered(exclude);

            if (_lastPositions.Count > 0 && availableDirections.Count == 0)
            {
                BacktrackOrRetry();
            }
            else
            {
                _state = State.Started;
                _nextDirections = availableDirections;
                Send(Name, "perform_based_on_state");
            }
        }

        private void ExecuteExploringStrategy()
        {
            if (_nextDirections.Count == 0)
            {
                _state = State.Blocked;
                Send(Name, "perform_based_on_state");
            }
            else
            {
                Position bestDir = _nextDirections[_nextDirections.Count - 1];
                int bestX = _currentX, bestY = _currentY;

                if (bestDir == Position.Up)
                {
                    bestX = _currentX - 1;
                  
                }
                else if (bestDir == Position.Down)
                {
                    bestX = _currentX + 1;
                    
                }
                else if (bestDir == Position.Left)
                {
                    bestY = _currentY - 1;
             
                }
                else if (bestDir == Position.Right)
                {
                    bestY = _currentY + 1;
                    
                }

                _nextDirections.RemoveAt(_nextDirections.Count - 1);
                Send("planet", Utils.Str("try_to_move", bestX, bestY));
            }
        }

        private List<Position> GetNextDirectionsOrdered(List<string> exclude = null)
        {
            if (exclude == null) exclude = new List<string>();

            List<Position> nextDirections = new List<Position>();
            Cell currentCell = _agentMap.FirstOrDefault(c => c.X == _currentX && c.Y == _currentY);
            Dictionary<decimal, Position> directionsWithWeights = new Dictionary<decimal, Position>();

            if (currentCell == null) return nextDirections;

            if (currentCell.Up > Utils.MinimumThreshold && !exclude.Contains(Utils.Str(_currentX - 1, _currentY)) && !_visitedPositions.Contains(Utils.Str(_currentX - 1, _currentY)))
                directionsWithWeights[currentCell.Up * 2] = Position.Up;
            if (currentCell.Down > Utils.MinimumThreshold && !exclude.Contains(Utils.Str(_currentX + 1, _currentY)) && !_visitedPositions.Contains(Utils.Str(_currentX + 1, _currentY)))
                directionsWithWeights[currentCell.Down * 2] = Position.Down;
            if (currentCell.Left > Utils.MinimumThreshold && !exclude.Contains(Utils.Str(_currentX, _currentY - 1)) && !_visitedPositions.Contains(Utils.Str(_currentX, _currentY - 1)))
                directionsWithWeights[currentCell.Left * 2] = Position.Left;
            if (currentCell.Right > Utils.MinimumThreshold && !exclude.Contains(Utils.Str(_currentX, _currentY + 1)) && !_visitedPositions.Contains(Utils.Str(_currentX, _currentY + 1)))
                directionsWithWeights[currentCell.Right * 2] = Position.Right;

            nextDirections = directionsWithWeights.OrderByDescending(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
            return nextDirections;
        }

        private void CreatePathToExit(int exitX, int exitY)
        {
            Queue<(int x, int y, List<string> path)> queue = new Queue<(int x, int y, List<string> path)>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue((_currentX, _currentY, new List<string>()));
            visited.Add(Utils.Str(_currentX, _currentY));

            while (queue.Count > 0)
            {
                var (currentX, currentY, path) = queue.Dequeue();

                if (currentX == exitX && currentY == exitY)
                {
                    _pathToFinish = new List<string>(path);
                    return;
                }

                foreach (var direction in GetNeighbors(currentX, currentY))
                {
                    int neighborX = direction.X;
                    int neighborY = direction.Y;
                    string neighborKey = Utils.Str(neighborX, neighborY);

                    if (!visited.Contains(neighborKey))
                    {
                        visited.Add(neighborKey);
                        var newPath = new List<string>(path) { neighborKey };
                        queue.Enqueue((neighborX, neighborY, newPath));
                    }
                }
            }
        }

        private IEnumerable<Cell> GetNeighbors(int x, int y)
        {
            var neighbors = new List<Cell>();
            Cell currentCell = _agentMap.FirstOrDefault(c => c.X == x && c.Y == y);
            if (currentCell == null) return neighbors;

            if (currentCell.Up > Utils.MinimumThreshold && IsValidCell(x - 1, y))
                neighbors.Add(new Cell { X = x - 1, Y = y });
            if (currentCell.Down > Utils.MinimumThreshold && IsValidCell(x + 1, y))
                neighbors.Add(new Cell { X = x + 1, Y = y });
            if (currentCell.Left > Utils.MinimumThreshold && IsValidCell(x, y - 1))
                neighbors.Add(new Cell { X = x, Y = y - 1 });
            if (currentCell.Right > Utils.MinimumThreshold && IsValidCell(x, y + 1))
                neighbors.Add(new Cell { X = x, Y = y + 1 });

            return neighbors;
        }

        private bool IsValidCell(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Utils.Size && y < Utils.Size &&
                   _agentMap.Any(c => c.X == x && c.Y == y);
        }

        private void HandleMove(List<string> parameters)
        {
            int oldX = _currentX;
            int oldY = _currentY;
            _currentX = int.Parse(parameters[0]);
            _currentY = int.Parse(parameters[1]);

            if (_state == State.Waiting)
            {
                if (_exitX != -1 && _exitY != -1)
                {
                    _state = State.Finished;
                    CreatePathToExit(_exitX, _exitY);
                }
                else
                {
                    _state = State.Started;
                    _nextDirections = GetNextDirectionsOrdered();
                }
                Send(Name, "perform_based_on_state");
            }
            else if (_state == State.Started)
            {
                _lastPositions.Add(Utils.Str(oldX, oldY));
                Position dir = Utils.GetPreviousDirection(oldX, oldY, _currentX, _currentY);
                Broadcast(Utils.Str("decrement_explored_position", oldX, oldY, dir), false, "explorers_channel");
                DecrementExploredPosition(new List<string>() { oldX.ToString(), oldY.ToString(), dir.ToString() });
                _nextDirections = GetNextDirectionsOrdered(_lastPositions);
                ExecuteExploringStrategy();
            }
            else if (_state == State.Blocked)
            {
                Position dir = Utils.GetPreviousDirection(_currentX, _currentY, oldX, oldY);
                Cell cell = _agentMap.FirstOrDefault(c => c.X == _currentX && c.Y == _currentY);
                if (dir == Position.Up) cell.Up = 0;
                else if (dir == Position.Down) cell.Down = 0;
                else if (dir == Position.Left) cell.Left = 0;
                else if (dir == Position.Right) cell.Right = 0;
                Broadcast(Utils.Str("avoid_this_direction", _currentX, _currentY, dir), false, "explorers_channel");
                ExecuteDeadEndStrategy();
            }
            else if (_state == State.Finished)
            {
                _pathToFinish.RemoveAt(0);
                string nextPosition = _pathToFinish.First();
                Send("planet", Utils.Str("try_to_move", nextPosition));
            }
        }

        private void HandleAvoid(List<string> parameters)
        {
            if (_state == State.Finished) return;

            int avoidX = int.Parse(parameters[0]);
            int avoidY = int.Parse(parameters[1]);
            Position direction = (Position)Enum.Parse(typeof(Position), parameters[2], true);

            Cell cellToAvoidDirection = _agentMap.FirstOrDefault(f => f.X == _currentX && f.Y == _currentY);

            if (cellToAvoidDirection != null)
            {
                if (direction == Position.Up) cellToAvoidDirection.Up = 0;
                else if (direction == Position.Down) cellToAvoidDirection.Down = 0;
                else if (direction == Position.Left) cellToAvoidDirection.Left = 0;
                else if (direction == Position.Right) cellToAvoidDirection.Right = 0;
            }
        }

        private void DecrementExploredPosition(List<string> parameters)
        {
            if (_state == State.Finished) return;

            int x = int.Parse(parameters[0]);
            int y = int.Parse(parameters[1]);
            Position direction = (Position)Enum.Parse(typeof(Position), parameters[2], true);

            Cell cell = _agentMap.FirstOrDefault(a => a.X == x && a.Y == y);
            if (cell != null)
            {
                if (direction == Position.Up && cell.Left - Utils.DecrementValue >= Utils.MinimumThreshold) cell.Left -= Utils.DecrementValue;
                else if (direction == Position.Down && cell.Right - Utils.DecrementValue >= Utils.MinimumThreshold) cell.Right -= Utils.DecrementValue;
                else if (direction == Position.Left && cell.Down - Utils.DecrementValue >= Utils.MinimumThreshold) cell.Down -= Utils.DecrementValue;
                else if (direction == Position.Right && cell.Up - Utils.DecrementValue >= Utils.MinimumThreshold) cell.Up -= Utils.DecrementValue;
            }
        }
    }
}
