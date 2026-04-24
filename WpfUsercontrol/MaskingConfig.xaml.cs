using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WpfUsercontrol.ViewModels;

namespace WpfUsercontrol
{
    public partial class MaskingConfig : Window
    {
        public MaskingConfig()
        {
            InitializeComponent();
            DataContext = new MaskingConfigViewModel();
        }

        private void SearchRuleTable_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
            {
                return;
            }

            var rule = e.Row?.Item as SearchRuleViewModel;
            if (rule == null)
            {
                return;
            }

            if (!rule.IsEmpty)
            {
                return;
            }

            var viewModel = DataContext as MaskingConfigViewModel;
            if (viewModel == null)
            {
                return;
            }

            var editableView = CollectionViewSource.GetDefaultView(viewModel.SearchRules) as IEditableCollectionView;
            var wasAddingNew = editableView != null &&
                               editableView.IsAddingNew &&
                               ReferenceEquals(editableView.CurrentAddItem, rule);

            if (!wasAddingNew)
            {
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (rule.IsEmpty && viewModel.SearchRules.Contains(rule))
                {
                    viewModel.SearchRules.Remove(rule);
                }
            }));
        }
    }
}
