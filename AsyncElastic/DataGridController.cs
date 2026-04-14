using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AsyncElastic;

public sealed class DataGridController : IDisposable
{
    private readonly DataGridView _grid;
    private readonly BindingList<DataItem> _data;
    private readonly BindingSource _bindingSource;
    private readonly Func<IEnumerable<FilterRule>> _getRules;

    public DataGridController(DataGridView grid, BindingList<DataItem> data, Func<IEnumerable<FilterRule>> getRules)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _data = data ?? throw new ArgumentNullException(nameof(data));
        _getRules = getRules ?? throw new ArgumentNullException(nameof(getRules));

        _bindingSource = new BindingSource { DataSource = _data };

        ConfigureGrid(_grid);
        _grid.DataSource = _bindingSource;

        _grid.RowPrePaint += OnRowPrePaint;
        _grid.DataError += OnDataError;
    }

    public void Dispose()
    {
        _grid.RowPrePaint -= OnRowPrePaint;
        _grid.DataError -= OnDataError;
        _bindingSource.Dispose();
    }

    public void AddItem(DataItem? item = null)
    {
        _data.Add(item ?? new DataItem());
        _grid.Invalidate();
    }

    public void Clear()
    {
        _data.Clear();
        _grid.Invalidate();
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = true;
        grid.AllowUserToDeleteRows = true;
        grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        grid.MultiSelect = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.RowHeadersVisible = false;
        grid.Dock = DockStyle.Fill;

        grid.Columns.Clear();

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(DataItem.ServiceName),
            HeaderText = "ServiceName",
            DataPropertyName = nameof(DataItem.ServiceName),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(DataItem.ClassName),
            HeaderText = "ClassName",
            DataPropertyName = nameof(DataItem.ClassName),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(DataItem.MethodName),
            HeaderText = "MethodName",
            DataPropertyName = nameof(DataItem.MethodName),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(DataItem.Message),
            HeaderText = "Message",
            DataPropertyName = nameof(DataItem.Message),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
    }

    private void OnDataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
        e.ThrowException = false;
    }

    private void OnRowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var row = _grid.Rows[e.RowIndex];
        if (row.IsNewRow || row.DataBoundItem is not DataItem item)
        {
            return;
        }

        if (FilterEngine.TryGetMatchColor(item, _getRules(), out var color))
        {
            row.DefaultCellStyle.BackColor = color;
            row.DefaultCellStyle.ForeColor = UiColorUtil.GetReadableTextColor(color);
            row.DefaultCellStyle.SelectionBackColor = color;
            row.DefaultCellStyle.SelectionForeColor = UiColorUtil.GetReadableTextColor(color);
        }
        else
        {
            row.DefaultCellStyle.BackColor = Color.Empty;
            row.DefaultCellStyle.ForeColor = Color.Empty;
            row.DefaultCellStyle.SelectionBackColor = Color.Empty;
            row.DefaultCellStyle.SelectionForeColor = Color.Empty;
        }
    }
}
