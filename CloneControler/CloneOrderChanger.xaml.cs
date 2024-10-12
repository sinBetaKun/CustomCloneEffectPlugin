using System.Windows;
using YukkuriMovieMaker.Commons;
using UserControl = System.Windows.Controls.UserControl;

namespace CustumCloneEffectPlugin.CloneControler
{
    /// <summary>
    /// CloneOrderChenger.xaml の相互作用ロジック
    /// </summary>
    public partial class CloneOrderChanger : UserControl, IPropertyEditorControl
    {
        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public CloneOrderChanger()
        {
            InitializeComponent();
            DataContextChanged += PointsEditor_DataContextChanged;
        }

        private void PointsEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CloneOrderChangerViewModel oldVm)
            {
                oldVm.BeginEdit -= PropertiesEditor_BeginEdit;
                oldVm.EndEdit -= PropertiesEditor_EndEdit;
            }
            if (e.NewValue is CloneOrderChangerViewModel newVm)
            {
                newVm.BeginEdit += PropertiesEditor_BeginEdit;
                newVm.EndEdit += PropertiesEditor_EndEdit;
            }
        }

        private void PropertiesEditor_BeginEdit(object? sender, EventArgs e)
        {
            BeginEdit?.Invoke(this, e);
        }

        private void PropertiesEditor_EndEdit(object? sender, EventArgs e)
        {
            //Point内のAnimationを変更した際にClonesを更新する
            //複数のアイテムを選択している場合にすべてのアイテムを更新するために必要
            var vm = DataContext as CloneOrderChangerViewModel;
            vm?.CopyToOtherItems();
            EndEdit?.Invoke(this, e);
        }   
    }
}
