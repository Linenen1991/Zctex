using System.ComponentModel;

namespace AsyncElastic;

public partial class Form1 : Form
{
    private readonly BindingList<FilterRule> _filterRules = new();

    private readonly FilterGridController _filterGridController;

    private readonly ContextMenuStrip _filterContextMenu = new();

    public Form1()
    {
        InitializeComponent();

        _filterGridController = new FilterGridController(filterGrid, _filterRules);

        _filterContextMenu.Items.Add("Remove selected", null, (_, _) => _filterGridController.RemoveSelectedRules());
        filterGrid.ContextMenuStrip = _filterContextMenu;

        Disposed += (_, _) =>
        {
            _filterGridController.Dispose();
            _filterContextMenu.Dispose();
        };
    }

    private void button1_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem l in listView1.Items)
        {
            foreach (var filter in _filterRules)
            {
                if(filter.ClassNamePattern == l.SubItems[0].Text)
                {
                    l.BackColor = filter.MatchColor;
                }
            }
        }
    }
}
