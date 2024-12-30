using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace ReflectSettings.EditableConfigs
{
    /// <summary>
    /// <br>泛型的集合编辑器</br>
    /// <br>通过命令添加和移除项，并支持数据绑定、值变化跟踪以及子编辑项管理</br>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TCollection"></typeparam>
    public class EditableCollection<TItem, TCollection> : EditableConfigBase<TCollection>, IEditableCollection,
        ICollection<TItem> where TCollection : class, ICollection<TItem>
    {
        private IEditableConfig _itemToAddEditable;
        private object _additionalData;

        /// <summary>
        /// 初始化
        /// 绑定集合的实例和相关的元数据
        /// </summary>
        /// <param name="forInstance">关联的对象实例</param>
        /// <param name="propertyInfo">集合属性的反射信息</param>
        /// <param name="factory">工厂类，用于创建可编辑配置</param>
        /// <param name="changeTrackingManager">管理值变化跟踪的对象</param>
        public EditableCollection(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory, ChangeTrackingManager changeTrackingManager) : base(
            forInstance, propertyInfo, factory, changeTrackingManager)
        {
            Value = Value;
            AddNewItemCommand = new DelegateCommand(AddNewItem);
            RemoveItemCommand = new DelegateCommand(RemoveItem);
            //准备一个新项的可编辑配置
            PrepareItemToAdd();
        }

        /// <summary>
        /// 当前选中的项
        /// </summary>
        public TItem SelectedItem { get; set; }

        public object SelectedValue
        {
            get => SelectedItem;
            set
            {
                if (value is TItem asItem)
                    SelectedItem = asItem;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 用于添加和移除项
        /// </summary>
        public ICommand AddNewItemCommand { get; }

        /// <summary>
        /// 用于添加和移除项
        /// </summary>
        public ICommand RemoveItemCommand { get; }

        /// <summary>
        /// 准备添加的新项的可编辑配置
        /// </summary>
        public IEditableConfig ItemToAddEditable
        {
            get => _itemToAddEditable;
            private set
            {
                _itemToAddEditable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 附加数据，用于传递到子编辑项中，并在它们发生变化时更新
        /// </summary>
        public new object AdditionalData
        {
            get => _additionalData;
            set
            {
                _additionalData = value;
                foreach (var config in SubEditables)
                {
                    config.AdditionalData = value;
                }

                if (ItemToAddEditable != null)
                    ItemToAddEditable.AdditionalData = AdditionalData;

                OnPropertyChanged();
            }
        }

        public Type SubItemType => typeof(TItem);

        public int ItemCount => AsCollection?.Count ?? 0;

        /// <summary>
        /// 将准备的新项添加到集合中
        /// </summary>
        private void AddNewItem()
        {
            var newItem = (TItem) ItemToAddEditable.Value;
            Add(newItem);
            PrepareItemToAdd();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<TItem> {newItem}));
        }

        /// <summary>
        /// 从集合中移除指定的项
        /// </summary>
        /// <param name="parameter"></param>
        private void RemoveItem(object parameter)
        {
            if (!(parameter is TItem asT))
                return;

            if (AsCollection.Contains(asT))
            {
                if (Remove(asT))
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<TItem> {asT}));
            }
        }
        /// <summary>
        /// 为新项创建一个初始实例，并生成其可编辑配置
        /// </summary>
        private void PrepareItemToAdd()
        {
            var instanceToAdd = InstantiateObject<TItem>();
            ItemToAddEditable = EditableConfigFor(instanceToAdd);
        }
        /// <summary>
        /// 解析并设置集合的值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override TCollection ParseValue(object value)
        {
            if (value is TCollection asT)
            {
                if (IsValueAllowed(asT))
                {
                    if (!ReferenceEquals(Value, asT) || !SubEditables.Any())
                        CreateSubEditables(asT);
                    return asT;
                }
                else if (Value is TCollection currentValue && IsValueAllowed(currentValue))
                {
                    return currentValue;
                }
            }

            // if null is allowed, return null
            if (PredefinedValues.Any(x => x == null))
                return null;

            // otherwise create a new instance
            var newInstance = InstantiateObject<TCollection>();
            CreateSubEditables(newInstance);
            return newInstance;
        }
        /// <summary>
        /// 为集合中的每个项创建可编辑配置
        /// </summary>
        /// <param name="collection"></param>
        private void CreateSubEditables(TCollection collection)
        {
            ClearSubEditables();
            foreach (var editable in collection.Select(EditableConfigFor))
            {
                editable.AdditionalData = AdditionalData;

                if (typeof(TItem).IsPrimitive || typeof(TItem) == typeof(string) || editable is IEditableKeyValuePair)
                    editable.ValueChanged += OnPrimitiveChildValueChanged;

                SubEditables.Add(editable);
            }
        }

        private ICollection<TItem> AsCollection => Value as ICollection<TItem> ?? new List<TItem>();

        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => AsCollection.GetEnumerator();

        public IEnumerator GetEnumerator() => AsCollection.GetEnumerator();
        /// <summary>
        /// 向集合中添加项，并生成该项的可编辑配置
        /// </summary>
        /// <param name="item"></param>
        public void Add(TItem item)
        {
            try
            {
                AsCollection.Add(item);
            }
            catch (Exception)
            {
                // ignored. Exception may occur for example, when an item with duplicate key gets added to a dictionary.
                return;
            }

            var editable = EditableConfigFor(item);
            if (typeof(TItem).IsPrimitive || typeof(TItem) == typeof(string) || editable is IEditableKeyValuePair)
                editable.ValueChanged += OnPrimitiveChildValueChanged;

            SubEditables.Add(editable);
            OnPropertyChanged(nameof(ItemCount));
            OnPropertyChanged(nameof(SubEditables));
        }

        private void OnPrimitiveChildValueChanged(object sender, EditableConfigValueChangedEventArgs e)
        {
            var oldValue = (TItem) e.OldValue;
            var newValue = (TItem) e.NewValue;

            var changedChild = (IEditableConfig) sender;
            var indexOfEditable = SubEditables.IndexOf(changedChild);

            if (Value is IList<TItem> asList)
                asList[indexOfEditable] = newValue;
            else
            {
                if (AsCollection.Contains(oldValue))
                {
                    AsCollection.Remove(oldValue);
                    AsCollection.Add(newValue);
                }
            }
        }
        /// <summary>
        /// 清除所有子编辑项的配置
        /// </summary>
        private void ClearSubEditables()
        {
            foreach (var editable in SubEditables)
            {
                editable.ValueChanged -= OnPrimitiveChildValueChanged;
                ChangeTrackingManager.Remove(editable);
            }

            SubEditables.Clear();
        }
        /// <summary>
        /// 清空集合并移除所有子编辑项
        /// </summary>
        public void Clear()
        {
            AsCollection.Clear();
            ClearSubEditables();
            OnPropertyChanged(nameof(SubEditables));
        }

        public bool Contains(TItem item) => AsCollection.Contains(item);

        public void CopyTo(TItem[] array, int arrayIndex) => AsCollection.CopyTo(array, arrayIndex);
        /// <summary>
        /// 移除指定的项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(TItem item)
        {
            if (AsCollection.Remove(item))
            {
                SubEditables.Remove(SubEditables.First(x => x.Value.Equals(item)));
                OnPropertyChanged(nameof(SubEditables));
                OnPropertyChanged(nameof(ItemCount));
                OnPropertyChanged(nameof(Value));
                return true;
            }
            else
                return false;
        }

        public int Count => AsCollection.Count;

        public bool IsReadOnly => AsCollection.IsReadOnly;

        /// <summary>
        /// 用于管理集合中每个项的子编辑配置
        /// </summary>
        public ObservableCollection<IEditableConfig> SubEditables { get; private set; } =
            new ObservableCollection<IEditableConfig>();
        /// <summary>
        /// 为指定的项生成可编辑配置
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private IEditableConfig EditableConfigFor(TItem item)
        {
            var config = Factory.Reflect(item, ChangeTrackingManager, true).First();
            config.CalculatedValues.InheritFrom(CalculatedValues);
            config.CalculatedValuesAsync.InheritFrom(CalculatedValuesAsync);
            config.CalculatedVisibility.InheritFrom(CalculatedVisibility);
            config.CalculatedTypes.InheritFrom(CalculatedTypes);
            config.UpdateCalculatedValues();
            config.AdditionalData = AdditionalData;
            return config;
        }

        public override void UpdateCalculatedValues()
        {
            base.UpdateCalculatedValues();
            foreach (var editable in SubEditables)
            {
                editable.CalculatedValues.InheritFrom(CalculatedValues);
                editable.CalculatedValuesAsync.InheritFrom(CalculatedValuesAsync);
                editable.CalculatedTypes.InheritFrom(CalculatedTypes);
                editable.CalculatedVisibility.InheritFrom(CalculatedVisibility);
            }

            if (ItemToAddEditable == null)
                return;

            ItemToAddEditable.CalculatedValues.InheritFrom(CalculatedValues);
            ItemToAddEditable.CalculatedValuesAsync.InheritFrom(CalculatedValuesAsync);
            ItemToAddEditable.CalculatedTypes.InheritFrom(CalculatedTypes);
            ItemToAddEditable.CalculatedVisibility.InheritFrom(CalculatedVisibility);
        }

        /// <summary>
        /// 通知外部监听者集合内容的变化
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}