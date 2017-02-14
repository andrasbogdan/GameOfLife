using GameOfLife.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameOfLife.Hubs
{
    public class GameOfLifeHub : Hub
    {
        private readonly GameManager _manager;

        public GameOfLifeHub() : this(GameManager.Instance) { }

        public GameOfLifeHub(GameManager manager)
        {
            _manager = manager;
        }

        public List<List<Cell>> GetBoard()
        {
            return _manager.GetBoard();
        }

        public void CellClicked(int row, int column)
        {
            _manager.UpdateSingleCell(row, column, Context.ConnectionId);
        }
    }
}