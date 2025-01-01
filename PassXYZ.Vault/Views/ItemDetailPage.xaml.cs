using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using HybridWebView;
using KPCLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views;

public partial class ItemDetailPage : ContentPage
{
    ItemDetailViewModel viewModel;
    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    void OnTap(object sender, ItemTappedEventArgs args)
    {
        var field = args.Item as Field;
        if (field == null)
        {
            return;
        }
        viewModel.OnFieldSelected(field);
    }
}