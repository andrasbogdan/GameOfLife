using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameOfLife.Models
{
    public class Cell
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public bool IsAlive { get; set; }
    }
}