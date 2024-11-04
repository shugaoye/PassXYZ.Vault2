using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

using KPCLib;
using PassXYZLib;
using PassXYZ.Vault.Services;
using PassXYZ.Vault.Views;
using System.Diagnostics;

namespace PassXYZ.Vault.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public partial class ItemsViewModel : BaseViewModel
    {
        readonly IDataStore<Item> dataStore;
        ILogger<ItemsViewModel> logger;

        public ObservableCollection<Item> Items { get; }

        public ItemsViewModel(IDataStore<Item> dataStore, ILogger<ItemsViewModel> logger)
        {
            this.dataStore = dataStore;
            this.logger = logger;
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            IsBusy = false;
        }

        [ObservableProperty]
        private Item? selectedItem = default;

        [ObservableProperty]
        private string? title;

        [ObservableProperty]
        private bool isBusy;

        [RelayCommand]
        private async Task AddItem(object obj)
        {
            string[] templates = {
                Properties.Resources.item_subtype_group,
                Properties.Resources.item_subtype_entry,
                Properties.Resources.item_subtype_notes,
                Properties.Resources.item_subtype_pxentry
            };

            var template = await Shell.Current.DisplayActionSheet(Properties.Resources.pt_id_choosetemplate, Properties.Resources.action_id_cancel, null, templates);
            ItemSubType type;
            if (template == Properties.Resources.item_subtype_entry)
            {
                type = ItemSubType.Entry;
            }
            else if (template == Properties.Resources.item_subtype_pxentry)
            {
                type = ItemSubType.PxEntry;
            }
            else if (template == Properties.Resources.item_subtype_group)
            {
                type = ItemSubType.Group;
            }
            else if (template == Properties.Resources.item_subtype_notes)
            {
                type = ItemSubType.Notes;
            }
            else if (template == Properties.Resources.action_id_cancel)
            {
                type = ItemSubType.None;
            }
            else
            {
                type = ItemSubType.None;
            }

            if (type != ItemSubType.None)
            {
                var itemType = new Dictionary<string, object>
                {
                    { "Type", type }
                };
                await Shell.Current.GoToAsync(nameof(NewItemPage), itemType);
            }
        }

        private async Task GoToPage(Item item)
        {
            if (item == null)
            {
                logger.LogWarning("item is null.");
                return;
            }

            if (item.IsGroup)
            {
                await Shell.Current.GoToAsync($"{nameof(ItemsPage)}?{nameof(ItemsViewModel.ItemId)}={item.Id}");
            }
            else
            {
                if (item.IsNotes())
                {
                    await Shell.Current.GoToAsync($"{nameof(NotesPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
                }
            }
        }

        public override void OnSelection(object sender)
        {
            Item? item = sender as Item;
            SelectedItem = item;
        }

        public async void OnItemSelected(Item item)
        {
            if (item == null)
            {
                return;
            }

            logger.LogDebug($"OnItemSelected is called and the item is {item.Name}");
            //SelectedItem = item;
            await GoToPage(item);
        }

        /// <summary>
        /// Update the icon of an item. The item can be a group or an entry.
        /// </summary>
        /// <param name="item">an instance of Item</param>
        public async void UpdateIcon(Item item) 
        {
            if (item == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushAsync(new IconsPage(async (PxFontIcon icon) => {
                item.SetFontIcon(icon);
                await dataStore.UpdateItemAsync(item);
            }));
        }


        /// <summary>
        /// Update an item. The item can be a group or an entry.
        /// </summary>
        /// <param name="item">an instance of Item</param>
        public async void Update(Item item)
        {
            if (item == null)
            {
                return;
            }

            await Shell.Current.Navigation.PushAsync(new FieldEditPage(async (string k, string v, bool isProtected) => {
                item.Name = k;
                item.Notes = v;
                await dataStore.UpdateItemAsync(item);
            }, item.Name, item.Notes, true));
        }

        /// <summary>
        /// Delete an item.
        /// </summary>
        /// <param name="item">an instance of Item</param>
        public async Task Delete(Item item)
        {
            if (item == null)
            {
                return;
            }

            var question = Properties.Resources.action_id_delete + " " + item.Name + "!";
            var message = Properties.Resources.message_id_alert_deleting + " " + item.Name + "?";
            bool answer = await Shell.Current.DisplayAlert(question, message, Properties.Resources.alert_id_yes, Properties.Resources.alert_id_no);

            if (answer) 
            {
                if (Items.Remove(item))
                {
                    _ = await dataStore.DeleteItemAsync(item.Id);
                }
                else
                {
                    throw new NullReferenceException("Delete item error");
                }
            }
        }

        [RelayCommand]
        private async Task LoadItems()
        {
            try
            {
                Items.Clear();
                var items = await dataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
                logger.LogDebug($"IsBusy={IsBusy}, added {Items.Count()} items");
            }
            catch (Exception ex)
            {
                logger.LogError("{ex}", ex);
            }
            finally
            {
                IsBusy = false;
                logger.LogDebug("Set IsBusy to false");
            }
        }

        [RelayCommand]
        public async Task ExecuteSearch(string? strSearch)
        {
            try
            {
                Items.Clear();
                var items = await dataStore.SearchEntriesAsync(strSearch, null);
                foreach (Item entry in items)
                {
                    if (entry != null)
                    {
                        ImageSource imgSource = (ImageSource)entry.ImgSource;
                        if (entry.ImgSource == null)
                        {
                            entry.SetIcon();
                        }
                        Items.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ItemsViewModel: ExecuteSearch, {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoadSearchItems()
        {
            await ExecuteSearch(null);
        }

        [RelayCommand]
        private async Task Search()
        {
            await Shell.Current.GoToAsync($"{nameof(SearchPage)}");
            Debug.WriteLine("ItemsViewModel: SearchCommand clicked");
        }

        public string ItemId
        {
            get
            {
                return SelectedItem == null ? string.Empty : SelectedItem.Id;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    SelectedItem = null;
                }
                else
                {
                    var item = dataStore.GetItem(value);
                    if (item != null)
                    {
                        SelectedItem = item;
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(ItemId), "cannot find the selected item");
                    }
                }
            }
        }

        /// <summary>
        /// The logic of navigation is implemented here.
        /// The current group is set here according to the selected item.
        /// </summary>
        public void OnAppearing()
        {
            if (SelectedItem == null)
            {
                // If SelectedItem is null, this is the root group.
                Title = dataStore.SetCurrentGroup();
            }
            else
            {
                if (SelectedItem.IsGroup) 
                {
                    Title = dataStore.SetCurrentGroup(SelectedItem);
                }
            }
            // load items
            IsBusy = true;
            logger.LogDebug($"Loading and set IsBusy={IsBusy}");
        }
    }
}