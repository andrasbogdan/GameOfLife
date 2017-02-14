using GameOfLife.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace GameOfLife.Models
{
    public class GameManager
    {
        private const int _boardSize = 32;
        private readonly static Lazy<GameManager> _instance = new Lazy<GameManager>(() => new GameManager(GlobalHost.ConnectionManager.GetHubContext<GameOfLifeHub>().Clients));

        private readonly List<List<Cell>> _board = new List<List<Cell>>();

        private readonly object _updateCellStateLock = new object();
        private volatile bool _updatingCellStates = false;

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(3000);
        private readonly Timer _timer;

        private IHubConnectionContext<dynamic> Clients { get; set; }

        public static GameManager Instance
        {
            get { return _instance.Value; }
        }

        private GameManager(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            _board.Clear();

            for (int i = 0; i < _boardSize; i++)
            {
                _board.Add(new List<Cell>());
                for (int j = 0; j < _boardSize; j++)
                {
                    _board[i].Add(new Cell { RowIndex = i, ColumnIndex = j, IsAlive = false });
                }
            }

            _timer = new Timer(UpdateBoardCells, null, _updateInterval, _updateInterval);
        }

        public void UpdateSingleCell(int row, int column, string connectionId)
        {
            if (IsWithinBorders(row, column))
            {
                if (!_updatingCellStates)
                {
                    _board[row][column].IsAlive = !_board[row][column].IsAlive;
                    BroadcastCell(_board[row][column]);
                }
            }
            else
            {
                string error = $"The selected cell ({row}, {column}) is out of the board";
                BroadcastError(connectionId, error);
            }
        }

        private bool IsWithinBorders(int row, int col)
        {
            if ((row >= 0 && row <= _boardSize) && (col >= 0 && col <= _boardSize))
                return true;

            return false;
        }

        private void UpdateBoardCells(object state)
        {
            lock (_updateCellStateLock)
            {
                if (!_updatingCellStates)
                {
                    _updatingCellStates = true;

                    List<List<Cell>> tempBoard = new List<List<Cell>>();
                    for (int i = 0; i < _board.Count; i++)
                    {
                        tempBoard.Add(new List<Cell>());
                        for (int j = 0; j < _board[i].Count; j++)
                        {
                            if (TryUpdateCellState(_board[i][j]))
                                tempBoard[i].Add(new Cell { RowIndex = i, ColumnIndex = j, IsAlive = true });
                            else
                                tempBoard[i].Add(new Cell { RowIndex = i, ColumnIndex = j, IsAlive = false });
                        }
                    }
                    BroadcastUpdatedCells(tempBoard);
                    for (int i = 0; i < _board.Count; i++)
                    {
                        for (int j = 0; j < _board.Count; j++)
                        {
                            _board[i][j].IsAlive = tempBoard[i][j].IsAlive;
                        }
                    }
                }
                _updatingCellStates = false;
            }
        }

        private bool TryUpdateCellState(Cell cell)
        {
            int minI = (cell.RowIndex - 1 >= 0) ? cell.RowIndex - 1 : 0;
            int maxI = (cell.RowIndex + 1 < _board.Count) ? cell.RowIndex + 1 : _board.Count - 1;
            int minJ = (cell.ColumnIndex - 1 >= 0) ? cell.ColumnIndex - 1 : 0;
            int maxJ = (cell.ColumnIndex + 1 < _board[cell.RowIndex].Count) ? cell.ColumnIndex + 1 : _board[cell.RowIndex].Count - 1;

            int counter = 0;

            for (int i = minI; i < maxI + 1; i++)
            {
                for (int j = minJ; j < maxJ + 1; j++)
                {
                    if (i != cell.RowIndex || j != cell.ColumnIndex)
                    {
                        if (_board[i][j].IsAlive)
                            counter++;
                    }
                }
            }

            if (!cell.IsAlive)
            {
                if (counter == 3)
                    return true;
                return false;
            }
            else
            {
                if (counter < 2 || counter > 3)
                    return false;

                return true;
            }
        }

        public List<List<Cell>> GetBoard()
        {
            return _board;
        }

        private void BroadcastError(string connectionId, string message)
        {
            Clients.Client(connectionId).updateError(message);
        }

        private void BroadcastUpdatedCells(List<List<Cell>> cells)
        {
            Clients.All.updateBoardCells(cells);
        }

        private void BroadcastCell(Cell cell)
        {
            Clients.All.updateBoardCell(cell);
        }
    }
}