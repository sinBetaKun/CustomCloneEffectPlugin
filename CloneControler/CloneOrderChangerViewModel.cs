using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YukkuriMovieMaker.Commons;

namespace CustumCloneEffectPlugin.CloneControler
{
    internal class CloneOrderChangerViewModel : Bindable, IPropertyEditorControl, IDisposable
    {
        readonly INotifyPropertyChanged item;
        readonly ItemProperty[] properties;

        List<SingleCloneInfo> clones = [];

        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public List<SingleCloneInfo> Clones { get => clones; set => Set(ref clones, value); }
        public int SelectedIndex {get => selectedIndex; set => Set(ref selectedIndex, value); }
        int selectedIndex = 0;

        public ActionCommand AddCommand { get; }
        public ActionCommand RemoveCommand { get; }
        public ActionCommand DuplicationCommand { get; }
        public ActionCommand MoveUpCommand { get; }
        public ActionCommand MoveDownCommand { get; }

        public CloneOrderChangerViewModel(ItemProperty[] properties)
        {
            this.properties = properties;

            item = (INotifyPropertyChanged)properties[0].PropertyOwner;
            item.PropertyChanged += Item_PropertyChanged;

            AddCommand = new ActionCommand(
                _ => true,
                _ =>
                {
                    var tmpSelectedIndex = SelectedIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    var clones = Clones.ToList();
                    clones.Insert(tmpSelectedIndex + 1, new SingleCloneInfo());
                    foreach (var property in properties)
                        property.SetValue(clones.Select(x => new SingleCloneInfo(x)).ToImmutableList());
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedIndex = tmpSelectedIndex + 1;
                });

            RemoveCommand = new ActionCommand(
                _ => clones.Count > 1 && SelectedIndex > -1,
                _ =>
                {
                    var tmpSelectedIndex = SelectedIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    var clones = Clones.ToList();
                    clones.RemoveAt(tmpSelectedIndex);
                    foreach (var property in properties)
                        property.SetValue(clones.Select(x => new SingleCloneInfo(x)).ToImmutableList());
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedIndex = Math.Min(tmpSelectedIndex, clones.Count - 1);
                });

            DuplicationCommand = new ActionCommand(
                _ => SelectedIndex > -1,
                _ =>
                {
                    var tmpSelectedIndex = SelectedIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    var clones = Clones.ToList();
                    var copied = new SingleCloneInfo(Clones[SelectedIndex]);
                    clones.Insert(tmpSelectedIndex + 1, copied);
                    foreach (var property in properties)
                        property.SetValue(clones.Select(x => new SingleCloneInfo(x)).ToImmutableList());
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedIndex = tmpSelectedIndex + 1;
                });

            MoveUpCommand = new ActionCommand(
                _ => SelectedIndex > 0,
                _ =>
                {
                    var tmpSelectedIndex = SelectedIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    var clones = Clones.ToList();
                    var clone = clones[SelectedIndex];
                    clones.RemoveAt(tmpSelectedIndex);
                    clones.Insert(tmpSelectedIndex - 1, clone);
                    foreach (var property in properties)
                        property.SetValue(clones.Select(x => new SingleCloneInfo(x)).ToImmutableList());
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedIndex = tmpSelectedIndex - 1;
                });

            MoveDownCommand = new ActionCommand(
                _ => SelectedIndex < clones.Count - 1 && SelectedIndex > -1,
                _ =>
                {
                    var tmpSelectedIndex = SelectedIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    var clones = Clones.ToList();
                    var clone = clones[SelectedIndex];
                    clones.RemoveAt(tmpSelectedIndex);
                    clones.Insert(tmpSelectedIndex + 1, clone);
                    foreach (var property in properties)
                        property.SetValue(clones.Select(x => new SingleCloneInfo(x)).ToImmutableList());
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedIndex = tmpSelectedIndex + 1;
                });

            UpdateClones();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == properties[0].PropertyInfo.Name)
                UpdateClones();
        }

        void UpdateClones()
        {
            var values = properties[0].GetValue<ImmutableList<SingleCloneInfo>>() ?? [];
            if (!Clones.SequenceEqual(values))
            {
                Clones = [.. values];
            }

            var commands = new[] { AddCommand, RemoveCommand, DuplicationCommand, MoveUpCommand, MoveDownCommand };
            foreach (var command in commands)
                command.RaiseCanExecuteChanged();
        }

        public void CopyToOtherItems()
        {
            //現在のアイテムの内容を他のアイテムにコピーする
            var otherProperties = properties.Skip(1);
            foreach (var property in otherProperties)
                property.SetValue(Clones.Select(x => new SingleCloneInfo(x)).ToImmutableList());
        }

        public void Dispose()
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
