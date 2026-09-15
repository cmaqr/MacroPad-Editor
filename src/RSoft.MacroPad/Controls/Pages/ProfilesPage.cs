using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL.Macros;

namespace RSoft.MacroPad.Controls.Pages
{
    /// <summary>
    /// Salva a configuração inteira com nome, aplica de novo, exporta e importa.
    /// </summary>
    internal class ProfilesPage : Panel
    {
        private readonly Label _title;
        private readonly Label _description;
        private readonly RoundedPanel _nameBox;
        private readonly TextBox _nameInput;
        private readonly PillButton _saveButton;
        private readonly PillButton _importButton;
        private readonly FlowLayoutPanel _list;
        private readonly Label _empty;
        private string _autoApplyProfile;

        public event EventHandler<string> SaveRequested;
        public event EventHandler<Profile> ApplyRequested;
        public event EventHandler<Profile> DeleteRequested;
        public event EventHandler<Profile> ExportRequested;
        public event EventHandler ImportRequested;
        /// <summary>Perfil que deve entrar sozinho quando o macropad for conectado (null para nenhum).</summary>
        public event EventHandler<string> AutoApplyChanged;

        public ProfilesPage()
        {
            Theme.Register(this, TextRole.Primary, SurfaceRole.Card);

            _title = NewLabel("Perfis", Theme.Title, TextRole.Primary);
            _description = NewLabel(
                "Um perfil guarda tudo que você já enviou para o modelo atual: todas as teclas, todas as camadas. "
                + "Dá para voltar a ele com um clique, levar para outro PC ou mandar para um amigo.", Theme.Callout, TextRole.Secondary);

            _nameBox = new RoundedPanel { Surface = SurfaceRole.Background, Radius = 10 };
            _nameInput = Theme.Register(new TextBox { BorderStyle = BorderStyle.None, Font = Theme.Body, PlaceholderText = "Nome do perfil, ex.: Edição de vídeo" }, TextRole.Primary, SurfaceRole.Background);
            _nameBox.Controls.Add(_nameInput);
            _nameBox.Resize += (s, e) => PlaceNameInput();

            _saveButton = new PillButton { Text = "Salvar configuração atual", Style = PillStyle.Primary, Icon = "\uE74E", Height = Theme.Scale(this, 34) };
            _saveButton.FitWidthToText();
            _saveButton.Click += (s, e) => SaveRequested?.Invoke(this, _nameInput.Text.Trim());

            _importButton = new PillButton { Text = "Importar perfil", Style = PillStyle.Secondary, Icon = "\uE896", Height = Theme.Scale(this, 34) };
            _importButton.FitWidthToText();
            _importButton.Click += (s, e) => ImportRequested?.Invoke(this, EventArgs.Empty);

            _list = Theme.Register(new FlowLayoutPanel { AutoScroll = true, WrapContents = false, FlowDirection = FlowDirection.TopDown }, TextRole.Primary, SurfaceRole.Card);
            _empty = NewLabel("Nenhum perfil salvo ainda. Configure as teclas do jeito que você gosta e salve aqui.", Theme.Callout, TextRole.Tertiary);

            Controls.AddRange(new Control[] { _title, _description, _nameBox, _saveButton, _importButton, _empty, _list });
        }

        public void ShowProfiles(IReadOnlyList<Profile> profiles, string autoApplyProfile)
        {
            _autoApplyProfile = autoApplyProfile;
            _list.Controls.Clear();
            foreach (var profile in profiles)
                _list.Controls.Add(CreateCard(profile));

            _empty.Visible = profiles.Count == 0;
            _nameInput.Text = "";
            ResizeCards();
            PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_list == null)
                return;

            var width = ClientSize.Width;
            var gap = Theme.Scale(this, 10);
            _title.SetBounds(0, 0, width, Theme.Scale(this, 30));
            _description.SetBounds(0, _title.Bottom, width, Theme.Scale(this, 42));

            var rowTop = _description.Bottom + gap;
            _saveButton.Location = new Point(width - _saveButton.Width, rowTop);
            _importButton.Location = new Point(_saveButton.Left - gap - _importButton.Width, rowTop);
            _nameBox.SetBounds(0, rowTop, Math.Max(Theme.Scale(this, 120), _importButton.Left - gap), Theme.Scale(this, 34));

            var listTop = _nameBox.Bottom + Theme.Scale(this, 14);
            _empty.SetBounds(0, listTop, width, Theme.Scale(this, 40));
            _list.SetBounds(0, listTop, width, Math.Max(0, ClientSize.Height - listTop));
        }

        private void PlaceNameInput()
        {
            var inner = Theme.Scale(this, 12);
            _nameInput.SetBounds(inner, (_nameBox.Height - _nameInput.PreferredHeight) / 2, _nameBox.Width - inner * 2, _nameInput.PreferredHeight);
        }

        private RoundedPanel CreateCard(Profile profile)
        {
            var card = new RoundedPanel { Surface = SurfaceRole.Background, Radius = 12, Margin = new Padding(0, 0, 0, Theme.Scale(this, 8)) };

            var name = Theme.Register(new Label { Text = profile.Name, Font = Theme.Headline, UseMnemonic = false, AutoEllipsis = true }, TextRole.Primary, SurfaceRole.Background);
            var keyCount = profile.Assignments?.Count ?? 0;
            var details = Theme.Register(new Label
            {
                Text = $"{DeviceName(profile.LayoutName)}  ·  {keyCount} ações  ·  salvo em {profile.SavedAt:dd/MM/yyyy HH:mm}",
                Font = Theme.Caption,
                UseMnemonic = false,
                AutoEllipsis = true,
            }, TextRole.Tertiary, SurfaceRole.Background);

            var apply = new PillButton { Text = "Aplicar", Style = PillStyle.Primary, Font = Theme.CaptionSemibold, Height = Theme.Scale(this, 28) };
            apply.FitWidthToText();
            apply.Click += (s, e) => ApplyRequested?.Invoke(this, profile);

            var auto = new PillButton
            {
                Text = "Entrar sozinho",
                Style = PillStyle.Chip,
                Selected = profile.Name == _autoApplyProfile,
                Font = Theme.CaptionSemibold,
                Height = Theme.Scale(this, 28),
            };
            auto.FitWidthToText();
            auto.Click += (s, e) =>
            {
                var newValue = auto.Selected ? null : profile.Name;
                AutoApplyChanged?.Invoke(this, newValue);
            };

            var export = new PillButton { Text = "Exportar", Style = PillStyle.Secondary, Font = Theme.CaptionSemibold, Height = Theme.Scale(this, 28) };
            export.FitWidthToText();
            export.Click += (s, e) => ExportRequested?.Invoke(this, profile);

            var delete = new PillButton { Text = "Excluir", Style = PillStyle.Secondary, Font = Theme.CaptionSemibold, Height = Theme.Scale(this, 28) };
            delete.FitWidthToText();
            delete.Click += (s, e) => DeleteRequested?.Invoke(this, profile);

            card.Controls.AddRange(new Control[] { name, details, apply, auto, export, delete });
            card.Layout += (s, e) =>
            {
                var padding = Theme.Scale(this, 14);
                var gap = Theme.Scale(this, 6);
                delete.Location = new Point(card.Width - delete.Width - padding, padding + Theme.Scale(this, 4));
                export.Location = new Point(delete.Left - gap - export.Width, delete.Top);
                auto.Location = new Point(export.Left - gap - auto.Width, delete.Top);
                apply.Location = new Point(auto.Left - gap - apply.Width, delete.Top);
                var textWidth = Math.Max(Theme.Scale(this, 80), apply.Left - padding - gap);
                name.SetBounds(padding, padding, textWidth, Theme.Scale(this, 22));
                details.SetBounds(padding, name.Bottom, textWidth, Theme.Scale(this, 20));
            };
            return card;
        }

        private void ResizeCards()
        {
            var width = _list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Theme.Scale(this, 2);
            foreach (Control card in _list.Controls)
                card.Size = new Size(Math.Max(Theme.Scale(this, 320), width), Theme.Scale(this, 74));
        }

        /// <summary>Os nomes do layouts.txt estão em inglês; aqui viram português.</summary>
        private static string DeviceName(string layoutName)
        {
            return (layoutName ?? "").Replace("buttons", "teclas").Replace("button", "tecla").Trim();
        }

        private static Label NewLabel(string text, Font font, TextRole role)
        {
            return Theme.Register(new Label { Text = text, Font = font, UseMnemonic = false }, role, SurfaceRole.Card);
        }
    }
}
