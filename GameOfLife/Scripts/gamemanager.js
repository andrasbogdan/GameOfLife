$(function () {
    var manager = $.connection.gameOfLifeHub,
        $board = $("#board"),
        $boardBody = $board.find('tbody');

    function init() {
        manager.server.getBoard().done(function (cells) {
            writeBoard(cells);
        });
    }

    function writeBoard(cells) {
        $boardBody.empty();
        for (i = 0; i < cells.length; i++) {
            var row = '<tr>';
            for (var item in cells[i]) {
                row += '<td></td>';
            }       
            row += '</tr>';
            $boardBody.append(row);
        }
        setCellStates(cells);
    }

    function setCellStates(cells) {
        for (i = 0; i < cells.length; i++) {
            for (var item in cells[i]) {
                var cell = $('#board tr').eq(i).find('td').eq(cells[i][item].ColumnIndex);
                cell.css('background-color', cells[i][item].IsAlive ? 'green' : 'black');
            }
        }
    }

    manager.client.updateBoardCells = function (cells) {
        setCellStates(cells);
    }

    manager.client.updateBoardCell = function (cell) {
        var cellToUpdate = $('#board tr').eq(cell.RowIndex).find('td').eq(cell.ColumnIndex);
        cellToUpdate.css('background-color', cell.IsAlive ? 'green' : 'black');
    }

    manager.client.updateError = function (message) {
        $('.modal-body').html(message);
        $('#error').modal('show');
    }

    $.connection.hub.start().done(function () {
        init();
    });

    $board.on('click', 'td', function () {
        var column = parseInt($(this).index());
        var row = parseInt($(this).parent().index());
        manager.server.cellClicked(row, column);
    });
});