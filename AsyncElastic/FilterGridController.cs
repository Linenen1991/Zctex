using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AsyncElastic;

public sealed class FilterGridController : IDisposable
{
    private readonly DataGridView _grid;
    private readonly BindingList<FilterRule> _rules;
    private readonly BindingSource _bindingSource;
    private readonly ColorDialog _colorDialog;

    private readonly int _removeColumnIndex;
    private readonly int _colorColumnIndex;

    public event EventHandler? RulesChanged;

    public FilterGridController(DataGridView grid, BindingList<FilterRule> rules)
    {
        _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));

        _bindingSource = new BindingSource { DataSource = _rules };
        _colorDialog = new ColorDialog { FullOpen = true };

        ConfigureGrid(_grid);
        _grid.DataSource = _bindingSource;

        _removeColumnIndex = _grid.Columns["Remove"]!.Index;
        _colorColumnIndex = _grid.Columns["MatchColor"]!.Index;

        WireEvents(_grid);
    }

    public void Dispose()
    {
        _grid.CellClick -= OnCellClick;
        _grid.CellFormatting -= OnCellFormatting;
        _grid.CellEndEdit -= OnCellEndEdit;
        _grid.KeyDown -= OnKeyDown;
        _grid.DataError -= OnDataError;

        _bindingSource.Dispose();
        _colorDialog.Dispose();
    }

    public void AddRule(FilterRule? rule = null)
    {
        _rules.Add(rule ?? new FilterRule());
        RulesChanged?.Invoke(this, EventArgs.Empty);
    }

    public int RemoveSelectedRules()
    {
        var items = _grid.SelectedRows
            .Cast<DataGridViewRow>()
            .Where(r => !r.IsNewRow)
            .Select(r => r.DataBoundItem)
            .OfType<FilterRule>()
            .Distinct()
            .ToList();

        foreach (var item in items)
        {
            _rules.Remove(item);
        }

        if (items.Count > 0)
        {
            RulesChanged?.Invoke(this, EventArgs.Empty);
        }

        return items.Count;
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = true;
        grid.AllowUserToDeleteRows = false; // we control deletes to support multi-select reliably
        grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        grid.MultiSelect = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.RowHeadersVisible = false;
        grid.Dock = DockStyle.Fill;

        grid.Columns.Clear();

        var removeColumn = new DataGridViewButtonColumn
        {
            Name = "Remove",
            HeaderText = "",
            Text = "Del",
            UseColumnTextForButtonValue = true,
            Width = 44,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        grid.Columns.Add(removeColumn);

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(FilterRule.ServiceNamePattern),
            HeaderText = "ServiceName",
            DataPropertyName = nameof(FilterRule.ServiceNamePattern),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(FilterRule.ClassNamePattern),
            HeaderText = "ClassName",
            DataPropertyName = nameof(FilterRule.ClassNamePattern),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(FilterRule.MethodNamePattern),
            HeaderText = "MethodName",
            DataPropertyName = nameof(FilterRule.MethodNamePattern),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(FilterRule.MessagePattern),
            HeaderText = "Message",
            DataPropertyName = nameof(FilterRule.MessagePattern),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "MatchColor",
            HeaderText = "Color",
            ReadOnly = true,
            Width = 84,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private void WireEvents(DataGridView grid)
    {
        grid.CellClick += OnCellClick;
        grid.CellFormatting += OnCellFormatting;
        grid.CellEndEdit += OnCellEndEdit;
        grid.KeyDown += OnKeyDown;
        grid.DataError += OnDataError;
    }

    private void OnDataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
        // Ignore binding/formatting glitches; user will correct input.
        e.ThrowException = false;
    }

    private void OnCellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        RulesChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            var removed = RemoveSelectedRules();
            if (removed > 0)
            {
                e.Handled = true;
            }
        }
    }

    private void OnCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != _colorColumnIndex)
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].DataBoundItem is not FilterRule rule)
        {
            return;
        }

        var color = rule.MatchColor;
        e.Value = UiColorUtil.ToRgbHex(color);
        var style = e.CellStyle;
        if (style is not null)
        {
            style.BackColor = color;
            style.ForeColor = UiColorUtil.GetReadableTextColor(color);
            style.SelectionBackColor = color;
            style.SelectionForeColor = UiColorUtil.GetReadableTextColor(color);
        }
        e.FormattingApplied = true;
    }

    private void OnCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].IsNewRow)
        {
            return;
        }

        if (_grid.Rows[e.RowIndex].DataBoundItem is not FilterRule rule)
        {
            return;
        }

        if (e.ColumnIndex == _removeColumnIndex)
        {
            _rules.Remove(rule);
            RulesChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (e.ColumnIndex == _colorColumnIndex)
        {
            _colorDialog.Color = rule.MatchColor;
            var owner = _grid.FindForm();
            var result = owner is null ? _colorDialog.ShowDialog() : _colorDialog.ShowDialog(owner);
            if (result == DialogResult.OK)
            {
                rule.MatchColor = _colorDialog.Color;
                _grid.InvalidateRow(e.RowIndex);
                RulesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
