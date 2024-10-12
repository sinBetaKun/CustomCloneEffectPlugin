using System.Windows;
using YukkuriMovieMaker.Commons;

namespace CustumCloneEffectPlugin.CloneControler
{
    internal class CloneOrderChangerAttribute : PropertyEditorAttribute2
    {
        public override FrameworkElement Create()
        {
            return new CloneOrderChanger();
        }

        public override void SetBindings(FrameworkElement control, ItemProperty[] itemProperties)
        {
            if (control is not CloneOrderChanger editor)
                return;
            editor.DataContext = new CloneOrderChangerViewModel(itemProperties);
        }

        public override void ClearBindings(FrameworkElement control)
        {
            if (control is not CloneOrderChanger editor)
                return;
            var vm = editor.DataContext as CloneOrderChangerViewModel;
            vm?.Dispose();
            editor.DataContext = null;
        }
    }
}
