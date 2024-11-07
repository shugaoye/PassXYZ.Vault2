using PassXYZLib;
using PassXYZLib.Resources;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PassXYZ.Vault.Views;

public partial class IconsPage : ContentPage
{
    Dictionary<string, string> glyphs;
    private Action<PxFontIcon> updateIcon;
    Type selectedFontFamilyType = FontData.FontFamily[nameof(FontType.FontAwesomeBrands)];
    PxFontIcon selectedFontIcon = new PxFontIcon { FontFamily = nameof(FontType.FontAwesomeBrands), Glyph = FontAwesomeBrands.FontAwesomeAlt };
    FontImageSource? selectedFontImageSource = default;

    public IconsPage()
	{
		InitializeComponent();
        LoadIcons();
        Debug.WriteLine("Loading icons ...");
        this.BindingContext = this;
    }

    public IconsPage(Action<PxFontIcon> callback) : this()
    {
        this.updateIcon = callback;
    }

    List<string> fontFamilyNames = FontData.FontFamily.Keys.ToList();
    public List<string> FontFamilyNames { get => fontFamilyNames; }

    string selectedFontFamilyName = nameof(FontType.FontAwesomeBrands);
    public string SelectedFontFamilyName
    {
        get => selectedFontFamilyName;
        set
        {
            if (selectedFontFamilyName != value)
            {
                selectedFontFamilyName = value;
                selectedFontFamilyType = FontData.FontFamily[value];
                //glyphs = FontData.GetGlyphs(selectedFontFamilyType);
                flexLayout.Children.Clear();
                LoadIcons();
                OnPropertyChanged();
            }
        }
    }

    void OnImageButtonClicked(object? sender, EventArgs e)
    {
        if (sender is ImageButton imageButton && imageButton.Source is FontImageSource fontImageSource)
        {
            var glyph = glyphs.FirstOrDefault(x => x.Value == fontImageSource.Glyph);
            Debug.WriteLine($"ImageButton clicked with Glyph: {glyph.Key}");
            selectedIcon.Text = $"{Properties.Resources.message_id_selected_icon}{glyph.Key}.";
            // searchBar.Text = glyph.Key;
            selectedFontIcon = new PxFontIcon { FontFamily = selectedFontFamilyName, Glyph = glyph.Value };
            if(selectedFontImageSource != null) 
            {
                selectedFontImageSource.Color = Microsoft.Maui.Graphics.Colors.Black;
            }
            selectedFontImageSource = fontImageSource;
            fontImageSource.Color = (Color)Application.Current.Resources["Primary"];
        }
        else
        {
            Debug.WriteLine("ImageButton clicked");
        }
    }

    async Task LoadIcons(string keyword = "")
    {
        glyphs = FontData.GetGlyphs(selectedFontFamilyType);
        if (!string.IsNullOrEmpty(keyword))
        {
            glyphs = glyphs.Where(x => x.Key.Contains(keyword)).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        await Task.Run(async () =>
        {
            foreach (var glyph in glyphs)
            {
                StackLayout stackLayout = new StackLayout { Margin = new Thickness(10) };
                ImageButton image = new ImageButton
                {
                    Source = new FontImageSource
                    {
                        FontFamily = selectedFontFamilyName,
                        Glyph = glyph.Value,
                        Size = 32,
                        Color = Microsoft.Maui.Graphics.Colors.Black
                    },

                };
                image.Clicked += OnImageButtonClicked;
                stackLayout.Children.Add(image);
                Label label = new Label
                {
                    Text = glyph.Key,
                    HorizontalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    WidthRequest = 64
                };
                stackLayout.Children.Add(label);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Code to run on the main thread
                    flexLayout.Children.Add(stackLayout);
                });
            }
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        _ = await Shell.Current.Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        updateIcon?.Invoke(selectedFontIcon);
        _ = await Shell.Current.Navigation.PopAsync();
    }

    void OnSearchButtonPressed(object sender, EventArgs e)
    {
        SearchBar searchBar = (SearchBar)sender;
        flexLayout.Children.Clear();
        LoadIcons(searchBar.Text);
    }
}