using System.ComponentModel;

namespace AsyncElastic;

public partial class Form1 : Form
{
    private readonly BindingList<FilterRule> _filterRules = new();
    private readonly BindingList<DataItem> _dataItems = new();

    private readonly FilterGridController _filterGridController;
    private readonly DataGridController _dataGridController;

    private readonly ContextMenuStrip _filterContextMenu = new();

    public Form1()
    {
        InitializeComponent();

        _filterGridController = new FilterGridController(filterGrid, _filterRules);
        _dataGridController = new DataGridController(dataGrid, _dataItems, () => _filterRules);

        _filterContextMenu.Items.Add("Remove selected", null, (_, _) => _filterGridController.RemoveSelectedRules());
        filterGrid.ContextMenuStrip = _filterContextMenu;

        _filterGridController.RulesChanged += (_, _) => dataGrid.Invalidate();

        Disposed += (_, _) =>
        {
            _filterGridController.Dispose();
            _dataGridController.Dispose();
            _filterContextMenu.Dispose();
        };
    }
}
