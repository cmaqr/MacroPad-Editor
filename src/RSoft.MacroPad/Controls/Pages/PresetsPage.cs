using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Todas as macros prontas, com busca e filtro por categoria.
    /// </summary>
    internal class PresetsPage : Panel
    {
        private const string AllCategories = "Todos";

        private readonly RoundedPanel _searchBox;
        private readonly TextBox _searchInput;
        private readonly FlowLayoutPanel _chips;
        private readonly FlowLayoutPanel _tiles;
        private readonly Label _emptyLabel;
        private readonly List<MacroTile> _allTiles = new List<MacroTile>();
        private readonly IReadOnlyList<string> _categories;
        private string _category = AllCategories;

        public event EventHandler<Macro> MacroChosen;

        public PresetsPage(IReadOnlyList<Macro> macros, IReadOnlyList<string> categories)
        {
            _categories = categories;
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);
            DoubleBuffered = true;

            _searchBox = new RoundedPanel { Surface = SurfaceRole.Background, Radius = 10 };
            var searchIcon = Theme.Register(new Label
            {
                Text = "\uE721",
                Font = Theme.IconSmall,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Left,
                Width = Theme.Scale(this, 34),
            }, TextRole.Secondary, SurfaceRole.Background);
            _searchInput = Theme.Register(new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = Theme.Body,
                PlaceholderText = "Buscar atalho, ex.: copiar, volume, aba…",
            }, TextRole.Primary, SurfaceRole.Background);
            _searchInput.TextChanged += (s, e) => ApplyFilter();
            _searchBox.Controls.Add(_searchInput);
            _searchBox.Controls.Add(searchIcon);
            _searchBox.Resize += (s, e) => PlaceSearchInput();

            _chips = Theme.Register(new FlowLayoutPanel { WrapContents = true, Margin = Padding.Empty }, TextRole.Primary, SurfaceRole.Card);
            foreach (var category in new[] { AllCategories }.Concat(_categories))
            {
                var chip = new PillButton
                {
                    Text = category,
                    Style = PillStyle.Chip,
                    Selected = category == AllCategories,
                    Font = Theme.Callout,
                    Height = Theme.Scale(this, 30),
                    Margin = new Padding(0, 0, Theme.Scale(this, 6), Theme.Scale(this, 6)),
                };
                chip.FitWidthToText();
                chip.Click += (s, e) => SelectCategory(category);
                _chips.Controls.Add(chip);
            }

            _tiles = Theme.Register(new FlowLayoutPanel { AutoScroll = true, WrapContents = true, Margin = Padding.Empty }, TextRole.Primary, SurfaceRole.Card);
            foreach (var macro in macros)
            {
                var colorIndex = Math.Max(0, _categories.ToList().IndexOf(macro.Category)) % Theme.CategoryColors.Length;
                var tile = new MacroTile(macro, Theme.CategoryColors[colorIndex]);
                tile.Click += (s, e) =>
                {
                    if (tile.Available)
                        MacroChosen?.Invoke(this, tile.Macro);
                };
                _allTiles.Add(tile);
                _tiles.Controls.Add(tile);
            }
            _tiles.Resize += (s, e) => ResizeTiles();

            _emptyLabel = Theme.Register(new Label
            {
                Text = "Nenhum atalho encontrado. Tente outra palavra ou grave o seu na aba Gravar.",
                Font = Theme.Body,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
            }, TextRole.Secondary, SurfaceRole.Card);

            Controls.Add(_emptyLabel);
            Controls.Add(_tiles);
            Controls.Add(_chips);
            Controls.Add(_searchBox);
        }

        /// <summary>Destaca o bloco da macro escolhida (ou nenhum, se for null ou uma macro que não está na lista).</summary>
        public void ShowSelected(Macro macro)
        {
            foreach (var tile in _allTiles)
                tile.Selected = macro != null && tile.Macro.Id == macro.Id;
        }

        /// <summary>Apaga os blocos que têm mais teclas do que o modelo aceita.</summary>
        public void SetMaxKeys(int maxKeys)
        {
            foreach (var tile in _allTiles)
                tile.Available = tile.Macro.Kind != MacroKind.Keys || tile.Macro.Keys.Count <= maxKeys;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_chips == null)
                return;

            var width = ClientSize.Width;
            _searchBox.SetBounds(0, 0, width, Theme.Scale(this, 38));

            var chipsHeight = _chips.GetPreferredSize(new Size(width, 0)).Height;
            _chips.SetBounds(0, _searchBox.Bottom + Theme.Scale(this, 14), width, chipsHeight);

            var tilesTop = _chips.Bottom + Theme.Scale(this, 8);
            _tiles.SetBounds(0, tilesTop, width, Math.Max(0, ClientSize.Height - tilesTop));
            _emptyLabel.SetBounds(0, tilesTop, width, Theme.Scale(this, 80));
        }

        private void PlaceSearchInput()
        {
            var inputHeight = _searchInput.PreferredHeight;
            var left = Theme.Scale(this, 34);
            _searchInput.SetBounds(left, (_searchBox.Height - inputHeight) / 2, _searchBox.Width - left - Theme.Scale(this, 12), inputHeight);
        }

        private void SelectCategory(string category)
        {
            _category = category;
            foreach (var chip in _chips.Controls.OfType<PillButton>())
                chip.Selected = chip.Text == category;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var search = _searchInput.Text.Trim();
            var visibleCount = 0;

            _tiles.SuspendLayout();
            foreach (var tile in _allTiles)
            {
                var inCategory = _category == AllCategories || tile.Macro.Category == _category;
                var matchesSearch = search.Length == 0
                    || Contains(tile.Macro.Title, search)
                    || Contains(tile.Macro.Category, search)
                    || Contains(tile.Macro.Hint, search)
                    || Contains(ShortcutText.Describe(tile.Macro), search);

                tile.Visible = inCategory && matchesSearch;
                if (tile.Visible)
                    visibleCount++;
            }
            _tiles.ResumeLayout();
            _tiles.AutoScrollPosition = Point.Empty;
            _emptyLabel.Visible = visibleCount == 0;
            if (_emptyLabel.Visible)
                _emptyLabel.BringToFront();
        }

        private void ResizeTiles()
        {
            // Duas ou três colunas conforme a largura, com todos os blocos do mesmo tamanho
            var gap = Theme.Scale(this, 10);
            var usableWidth = _tiles.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            var columns = Math.Max(1, usableWidth / Theme.Scale(this, 230));
            var tileWidth = (usableWidth - gap * columns) / columns;

            _tiles.SuspendLayout();
            foreach (var tile in _allTiles)
                tile.Width = Math.Max(Theme.Scale(this, 160), tileWidth);
            _tiles.ResumeLayout();
        }

        private static bool Contains(string text, string search)
        {
            // Compara sem diferenciar maiúsculas e sem acentos: "musica" encontra "Música"
            var options = System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace;
            return !string.IsNullOrEmpty(text)
                && System.Globalization.CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, search, options) >= 0;
        }
    }
}
