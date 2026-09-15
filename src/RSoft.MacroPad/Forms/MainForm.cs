using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RSoft.MacroPad.BLL;
using RSoft.MacroPad.BLL.Infrasturture;
using RSoft.MacroPad.BLL.Infrasturture.Configuration;
using RSoft.MacroPad.BLL.Infrasturture.Model;
using RSoft.MacroPad.BLL.Infrasturture.Physical;
using RSoft.MacroPad.BLL.Infrasturture.UsbDevice;
using RSoft.MacroPad.BLL.Macros;
using RSoft.MacroPad.Controls;
using RSoft.MacroPad.Controls.Cards;
using RSoft.MacroPad.Controls.Pages;
using RSoft.MacroPad.Infrastructure;

namespace RSoft.MacroPad.Forms
{
    public class MainForm : Form
    {
        private const string ProjectUrl = "https://github.com/rOzzy1987/MacroPad";
        private const string UserMacrosFileName = "meus-atalhos.txt";
        private const string AutoApplyProfileKey = "auto-profile";

        private readonly IUsb _usb = new HidLibUsb();
        private readonly ComposerRepository _composerRepository = new ComposerRepository();
        private readonly AssignmentStore _assignments = new AssignmentStore("assignments.json");
        private readonly ProfileStore _profiles = new ProfileStore("perfis");
        private readonly RelayStore _relayStore = new RelayStore("relay.json");
        private readonly RelayAgent _relayAgent;
        private readonly KeyboardLayout[] _layouts;
        private KeyboardLayout _layout;
        private Macro _stagedMacro;
        private (ushort VendorId, ushort ProductId)? _unknownDevice;

        private readonly Label _appTitle;
        private readonly Label _appSubtitle;
        private readonly RoundedPanel _connectionPill;
        private readonly Label _connectionDot;
        private readonly Label _connectionText;
        private readonly DeviceCard _deviceCard;
        private readonly SummaryCard _summaryCard;
        private readonly TipCard _tipCard;
        private readonly RoundedPanel _actionsCard;
        private readonly SegmentedControl _tabs;
        private readonly PresetsPage _presetsPage;
        private readonly TextPage _textPage;
        private readonly RecordPage _recordPage;
        private readonly KitsPage _kitsPage;
        private readonly LightPage _lightPage;
        private readonly RelayPage _relayPage;
        private readonly ProfilesPage _profilesPage;
        private readonly Control[] _pages;
        private readonly LinkLabel _footer;
        private readonly PillButton _themeButton;
        private readonly Timer _connectionTimer;
        private int _ticksSinceDeviceScan;

        public MainForm()
        {
            Text = "MacroPad";
            BackColor = Theme.Background;
            Font = Theme.Body;
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            Theme.UseDark(PreferredDarkTheme(), this);

            // A janela nunca pode nascer maior que a área útil da tela (issue #38 do projeto original)
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            MinimumSize = new Size(
                Math.Min(Theme.Scale(this, 940), workingArea.Width),
                Math.Min(Theme.Scale(this, 600), workingArea.Height));
            ClientSize = new Size(
                Math.Min(Theme.Scale(this, 1240), workingArea.Width - Theme.Scale(this, 40)),
                Math.Min(Theme.Scale(this, 800), workingArea.Height - Theme.Scale(this, 60)));

            Theme.Register(this, TextRole.Primary, SurfaceRole.Background);
            _appTitle = Theme.Register(new Label { Text = "MacroPad", Font = Theme.LargeTitle, AutoSize = true }, TextRole.Primary, SurfaceRole.Background);
            _appSubtitle = Theme.Register(new Label { Text = "Escolha uma tecla, escolha o que ela faz e envie. Simples assim.", Font = Theme.Body, AutoSize = true }, TextRole.Secondary, SurfaceRole.Background);

            _connectionPill = new RoundedPanel { Radius = 17 };
            _connectionDot = Theme.Register(new Label { Text = "\u25CF", Font = Theme.Body, TextAlign = ContentAlignment.MiddleCenter }, TextRole.Tertiary, SurfaceRole.Card);
            _connectionText = Theme.Register(new Label { Font = Theme.CalloutSemibold, TextAlign = ContentAlignment.MiddleLeft, AutoSize = true }, TextRole.Primary, SurfaceRole.Card);

            _themeButton = new PillButton { Style = PillStyle.Secondary, Height = Theme.Scale(this, 34), Width = Theme.Scale(this, 44) };
            _themeButton.Click += (s, e) => ToggleTheme();
            _connectionPill.Controls.AddRange(new Control[] { _connectionDot, _connectionText });

            _deviceCard = new DeviceCard();
            _deviceCard.LayoutChosen += (s, layout) =>
            {
                _assignments.SetLastLayoutName(layout.Name);
                ApplyLayout(layout, detected: false);
            };
            _deviceCard.LayerChanged += (s, layer) => RefreshSelection();
            _deviceCard.ActionSelected += (s, action) => RefreshSelection();
            _deviceCard.UnknownDeviceAccepted += (s, e) => AcceptUnknownDevice();

            _summaryCard = new SummaryCard();
            _summaryCard.SendRequested += (s, e) => SendStagedMacro();
            _summaryCard.TestRequested += (s, e) => TestStagedMacro();
            _summaryCard.CheatSheetRequested += (s, e) => SaveCheatSheet();

            _tipCard = new TipCard();

            // Atalhos do usuário: um arquivo de texto ao lado do programa, sem precisar recompilar
            UserMacros.CreateExampleIfMissing(UserMacrosFileName);
            var userMacros = UserMacros.Read(UserMacrosFileName);
            var categories = userMacros.Macros.Count == 0
                ? MacroCatalog.Categories
                : MacroCatalog.Categories.Concat(new[] { UserMacros.Category }).ToList();

            _presetsPage = new PresetsPage(MacroCatalog.Macros.Concat(userMacros.Macros).ToList(), categories);
            _textPage = new TextPage();
            _recordPage = new RecordPage();
            _kitsPage = new KitsPage();
            _lightPage = new LightPage();
            _relayPage = new RelayPage(_relayStore);
            _profilesPage = new ProfilesPage();
            _presetsPage.MacroChosen += (s, macro) => StageMacro(macro);
            _textPage.MacroChosen += (s, macro) => StageMacro(macro);
            _recordPage.MacroChosen += (s, macro) => StageMacro(macro);
            _kitsPage.KitApplyRequested += (s, kit) => ApplyKit(kit);
            _lightPage.LightApplyRequested += (s, light) => ApplyLight(light.Mode, light.Color);
            _relayPage.MacroChosen += (s, macro) => StageMacro(macro);
            _relayPage.ActionsChanged += (s, e) => _relayAgent.Refresh();
            _profilesPage.SaveRequested += (s, name) => SaveProfile(name);
            _profilesPage.ApplyRequested += (s, profile) => ApplyProfile(profile);
            _profilesPage.DeleteRequested += (s, profile) => DeleteProfile(profile);
            _profilesPage.ExportRequested += (s, profile) => ExportProfile(profile);
            _profilesPage.ImportRequested += (s, e) => ImportProfile();
            _profilesPage.AutoApplyChanged += (s, name) => SetAutoApplyProfile(name);
            _pages = new Control[] { _presetsPage, _textPage, _recordPage, _kitsPage, _lightPage, _relayPage, _profilesPage };

            _tabs = new SegmentedControl { Items = new[] { "Atalhos", "Texto", "Gravar", "Kits", "Luz", "Turbo", "Perfis" }, Height = Theme.Scale(this, 34) };
            _tabs.SelectedIndexChanged += (s, e) => ShowPage(_tabs.SelectedIndex);

            _actionsCard = new RoundedPanel();
            _actionsCard.Controls.Add(_tabs);
            _actionsCard.Controls.AddRange(_pages);
            _actionsCard.Layout += (s, e) => LayoutActionsCard();

            _footer = new LinkLabel
            {
                Text = "Baseado no RSoft MacroPad, de Mihály Rozovits · código aberto (GPL-3.0)",
                Font = Theme.Caption,
                ForeColor = Theme.TertiaryText,
                LinkColor = Theme.SecondaryText,
                ActiveLinkColor = Theme.Accent,
                LinkBehavior = LinkBehavior.HoverUnderline,
                AutoSize = true,
            };
            _footer.LinkArea = new LinkArea(0, _footer.Text.Length);
            _footer.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo(ProjectUrl) { UseShellExecute = true });

            Controls.AddRange(new Control[] { _appTitle, _appSubtitle, _connectionPill, _themeButton, _deviceCard, _summaryCard, _tipCard, _actionsCard, _footer });
            ApplyThemeToHeader();

            _layouts = new LayoutParser().Parse("layouts.txt");
            _deviceCard.SetLayouts(_layouts);
            // Abre no último modelo escolhido; na primeira vez, no único modelo testado pelo autor original
            var startLayout = _layouts.FirstOrDefault(l => l.Name == _assignments.GetLastLayoutName())
                ?? _layouts.FirstOrDefault(l => l.Products.Any(p => TestedProducts.IsTested(p.VendorId, p.ProductId)))
                ?? _layouts.First();
            ApplyLayout(startLayout, detected: false);

            var config = new ConfigurationReader().Read("config.txt");
            if (config != null)
                _usb.SupportedDevices = config.SupportedDevices;
            _usb.OnConnected += (s, e) => OnKeypadConnected();

            _relayAgent = new RelayAgent(_relayStore, this);
            _relayAgent.ActionExecuted += (s, message) => _summaryCard.ShowStatus(message, TextRole.Secondary);
            _relayAgent.Refresh();

            RefreshProfiles();
            ShowPage(0);
            ShowConnection(false);
            if (userMacros.BadLines.Count > 0)
                _summaryCard.ShowStatus($"No meus-atalhos.txt: {userMacros.BadLines[0]}", TextRole.Warning);

            // Mesmo esquema do app original: tenta conectar uma vez por segundo
            _connectionTimer = new Timer { Interval = 1000 };
            _connectionTimer.Tick += (s, e) => CheckConnection();
            _connectionTimer.Start();

            Deactivate += (s, e) => _recordPage.StopRecording();
            FormClosed += (s, e) => _relayAgent.Dispose();
        }

        private bool PreferredDarkTheme()
        {
            var saved = _assignments.GetSetting("theme");
            if (saved == "dark")
                return true;
            if (saved == "light")
                return false;
            return Theme.WindowsPrefersDark();
        }

        private void ToggleTheme()
        {
            Theme.UseDark(!Theme.IsDark, this);
            _assignments.SetSetting("theme", Theme.IsDark ? "dark" : "light");
            ApplyThemeToHeader();
            Extern.SetTitleBarColors(Handle, Theme.Background, Theme.Text);
        }

        private void ApplyThemeToHeader()
        {
            // Sol quando está escuro (clique para clarear), lua quando está claro
            _themeButton.Icon = Theme.IsDark ? "\uE706" : "\uE708";
            _themeButton.Invalidate();
            _footer.LinkColor = Theme.SecondaryText;
            _footer.ActiveLinkColor = Theme.Accent;
        }

        /// <summary>
        /// Procura macropad do mesmo fabricante que não está no config.txt e oferece tentar assim mesmo
        /// (issues #36 e #37 do projeto original).
        /// </summary>
        private void LookForUnknownDevice()
        {
            var supported = _usb.SupportedDevices.ToList();
            foreach (var vendorId in supported.Select(d => d.VendorId).Distinct())
            {
                var knownProducts = supported.Where(d => d.VendorId == vendorId).Select(d => d.ProductId).ToHashSet();
                var unknown = HidLib.FindConnectedProducts(vendorId).FirstOrDefault(p => !knownProducts.Contains(p.ProductId));
                if (unknown.ProductId == 0)
                    continue;

                _unknownDevice = (vendorId, unknown.ProductId);
                _deviceCard.ShowUnknownDevice($"Achei um teclado {vendorId}:{unknown.ProductId} que não está na lista de modelos conhecidos.");
                return;
            }
            _deviceCard.HideUnknownDevice();
        }

        private void AcceptUnknownDevice()
        {
            if (_unknownDevice == null)
                return;

            var (vendorId, productId) = _unknownDevice.Value;
            // Os dois caminhos ("mi_00" e "mi_01") cobrem as duas formas que esses teclados se apresentam ao Windows
            var added = new[]
            {
                (vendorId, productId, "mi_00", ProtocolType.Extended),
                (vendorId, productId, "mi_01", ProtocolType.Extended),
            };
            _usb.SupportedDevices = _usb.SupportedDevices.Concat(added).ToList();

            try
            {
                File.AppendAllText("config.txt", $"{Environment.NewLine}{vendorId}:{productId},mi_00,1{Environment.NewLine}{vendorId}:{productId},mi_01,1{Environment.NewLine}");
            }
            catch (IOException)
            {
                // Sem permissão de escrita na pasta o teclado ainda funciona nesta sessão, só não fica salvo
            }

            _deviceCard.HideUnknownDevice();
            _summaryCard.ShowStatus($"Vou tentar falar com o teclado {vendorId}:{productId}. Se não funcionar, ele usa outro protocolo.", TextRole.Secondary);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Sem foco inicial na busca: com foco o Windows esconde o texto de exemplo da caixa
            ActiveControl = null;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Extern.SetTitleBarColors(Handle, Theme.Background, Theme.Text);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_footer == null)
                return;

            var margin = Theme.Scale(this, 24);
            var gap = Theme.Scale(this, 16);
            var width = ClientSize.Width;
            var height = ClientSize.Height;

            _appTitle.Location = new Point(margin - Theme.Scale(this, 3), Theme.Scale(this, 14));
            _appSubtitle.Location = new Point(margin, _appTitle.Bottom - Theme.Scale(this, 2));

            var pillHeight = Theme.Scale(this, 34);
            _themeButton.SetBounds(width - margin - _themeButton.Width, Theme.Scale(this, 24), _themeButton.Width, pillHeight);
            var pillWidth = _connectionText.PreferredWidth + Theme.Scale(this, 48);
            _connectionPill.SetBounds(_themeButton.Left - Theme.Scale(this, 10) - pillWidth, Theme.Scale(this, 24), pillWidth, pillHeight);
            _connectionDot.SetBounds(Theme.Scale(this, 10), 0, Theme.Scale(this, 20), pillHeight);
            _connectionText.Location = new Point(Theme.Scale(this, 32), (pillHeight - _connectionText.PreferredHeight) / 2);

            _footer.Location = new Point(margin, height - Theme.Scale(this, 26));

            var top = _appSubtitle.Bottom + Theme.Scale(this, 18);
            var bottom = _footer.Top - Theme.Scale(this, 10);
            var leftWidth = Theme.Scale(this, 400);

            var summaryHeight = Theme.Scale(this, 262);
            var tipHeight = Theme.Scale(this, 108);

            // Em janela baixa a dica sai da frente para sobrar espaço para o desenho do teclado
            _tipCard.Visible = bottom - top > Theme.Scale(this, 520);
            var tipTop = _tipCard.Visible ? bottom - tipHeight : bottom + gap;
            _tipCard.SetBounds(margin, tipTop, leftWidth, tipHeight);
            _summaryCard.SetBounds(margin, tipTop - gap - summaryHeight, leftWidth, summaryHeight);
            _deviceCard.SetBounds(margin, top, leftWidth, _summaryCard.Top - gap - top);

            var rightLeft = margin + leftWidth + gap;
            _actionsCard.SetBounds(rightLeft, top, width - rightLeft - margin, bottom - top);
        }

        private void LayoutActionsCard()
        {
            var padding = Theme.Scale(this, 20);
            var width = _actionsCard.ClientSize.Width - padding * 2;
            _tabs.SetBounds(padding, padding, width, _tabs.Height);

            var pageTop = _tabs.Bottom + Theme.Scale(this, 20);
            foreach (var page in _pages)
                page.SetBounds(padding, pageTop, width, Math.Max(0, _actionsCard.ClientSize.Height - pageTop - padding));
        }

        private void ShowPage(int index)
        {
            for (var i = 0; i < _pages.Length; i++)
                _pages[i].Visible = i == index;
            if (_pages[index] != _recordPage)
                _recordPage.StopRecording();
        }

        private void ApplyLayout(KeyboardLayout layout, bool detected)
        {
            _layout = layout;
            _deviceCard.ShowLayout(layout, detected);
            _presetsPage.SetMaxKeys(layout.MaxCharacters);
            _textPage.SetMaxKeys(layout.MaxCharacters);
            _recordPage.SetMaxKeys(layout.MaxCharacters);
            _lightPage.SetLayout(layout);

            if (_stagedMacro != null && _stagedMacro.Kind == MacroKind.Keys && _stagedMacro.Keys.Count > layout.MaxCharacters)
                StageMacro(null);
            RefreshSelection();
        }

        private void OnKeypadConnected()
        {
            var layout = _layouts.FirstOrDefault(l => l.Products.Any(p => p.VendorId == _usb.VendorId && p.ProductId == _usb.ProductId));
            if (layout != null)
                ApplyLayout(layout, detected: true);
            _deviceCard.ShowUntestedWarning(!TestedProducts.IsTested(_usb.VendorId, _usb.ProductId));

            var autoProfileName = _assignments.GetSetting(AutoApplyProfileKey);
            var autoProfile = autoProfileName == null ? null : _profiles.List().FirstOrDefault(p => p.Name == autoProfileName);
            if (autoProfile != null)
                ApplyProfile(autoProfile);
        }

        private void CheckConnection()
        {
            var connected = _usb.Connect();
            ShowConnection(connected);

            // Enumerar dispositivos USB não é barato: com o teclado desconectado, procura a cada 5 segundos
            if (connected)
            {
                _ticksSinceDeviceScan = 0;
                _deviceCard.HideUnknownDevice();
                return;
            }
            if (++_ticksSinceDeviceScan >= 5)
            {
                _ticksSinceDeviceScan = 0;
                LookForUnknownDevice();
            }
        }

        private void ShowConnection(bool connected)
        {
            var text = connected ? "Macropad conectado" : "Conecte o macropad no USB";
            if (_connectionText.Text != text)
            {
                _connectionText.Text = text;
                    Theme.SetTextRole(_connectionDot, connected ? TextRole.Success : TextRole.Tertiary);
                PerformLayout();
            }
            _summaryCard.SetCanSend(connected && _stagedMacro != null);
        }

        private void StageMacro(Macro macro)
        {
            _stagedMacro = macro;
            _presetsPage.ShowSelected(macro);
            _summaryCard.ShowStatus("", TextRole.Secondary);
            RefreshSelection();
        }

        /// <summary>Atualiza o que depende da tecla, da camada ou da macro escolhida.</summary>
        private void RefreshSelection()
        {
            var layer = _deviceCard.Layer;
            var hasLayers = _layout.LayerCount > 1;

            _deviceCard.SetTitles(CurrentTitles());

            var target = DescribeAction(_deviceCard.SelectedAction);
            if (hasLayers)
                target += $"  ·  camada {layer}";
            _summaryCard.ShowSelection(target, _stagedMacro, _layout.SupportsDelay);
            _summaryCard.SetCanSend(_usb.IsConnected && _stagedMacro != null);

            _kitsPage.SetLayout(_layout, layer, hasLayers);
        }

        private Dictionary<InputAction, string> CurrentTitles()
        {
            var layer = _deviceCard.Layer;
            var titles = new Dictionary<InputAction, string>();
            foreach (var action in _layout.Controls.SelectMany(c => c.Actions))
            {
                var title = _assignments.GetTitle(_layout.Name, layer, action);
                if (title != null)
                    titles[action] = title;
            }
            return titles;
        }

        /// <summary>Executa a macro no próprio PC, para ver o que ela faz antes de gastar uma gravação.</summary>
        private void TestStagedMacro()
        {
            if (_stagedMacro == null || _stagedMacro.Kind == MacroKind.Mouse)
                return;

            _summaryCard.ShowStatus("Testando em 3 segundos: clique na janela onde você quer ver o resultado.", TextRole.Secondary);
            var countdown = new Timer { Interval = 3000 };
            countdown.Tick += (s, e) =>
            {
                countdown.Stop();
                countdown.Dispose();
                if (_stagedMacro.Kind == MacroKind.Media)
                    InputSender.SendMediaKey(_stagedMacro.MediaKey);
                else
                    InputSender.SendChords(_stagedMacro.Keys, _summaryCard.Delay);
                _summaryCard.ShowStatus("Teste enviado para a janela que estava na frente.", TextRole.Secondary);
            };
            countdown.Start();
        }

        private void SaveCheatSheet()
        {
            try
            {
                var filePath = CheatSheet.Save(_layout, _deviceCard.Layer, CurrentTitles(), "colinhas");
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                _summaryCard.ShowStatus("Colinha salva na pasta colinhas.", TextRole.Success);
            }
            catch (IOException exception)
            {
                _summaryCard.ShowStatus($"Não consegui salvar a colinha: {exception.Message}", TextRole.Danger);
            }
        }

        private void RefreshProfiles()
        {
            _profilesPage.ShowProfiles(_profiles.List(), _assignments.GetSetting(AutoApplyProfileKey));
        }

        private void SaveProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _summaryCard.ShowStatus("Dê um nome ao perfil antes de salvar.", TextRole.Danger);
                return;
            }

            var assignments = _assignments.Snapshot(_layout.Name);
            if (assignments.Count == 0)
            {
                _summaryCard.ShowStatus("Ainda não há nada enviado neste modelo para salvar.", TextRole.Danger);
                return;
            }

            _profiles.Save(new Profile { Name = name, LayoutName = _layout.Name, Assignments = assignments });
            RefreshProfiles();
            _summaryCard.ShowStatus($"Perfil \u201C{name}\u201D salvo com {assignments.Count} ações.", TextRole.Success);
        }

        private void ApplyProfile(Profile profile)
        {
            if (!_usb.IsConnected)
            {
                _summaryCard.ShowStatus("Conecte o macropad no USB para aplicar o perfil.", TextRole.Danger);
                return;
            }

            var composer = _composerRepository.Get(_usb.ProtocolType, _usb.Version);
            HidLog.ClearLog();
            Cursor = Cursors.WaitCursor;

            var written = 0;
            foreach (var assignment in profile.Assignments)
            {
                if (!Enum.TryParse<InputAction>(assignment.Action, out var action))
                    continue;
                var macro = assignment.Macro.ToMacro();
                if (macro.Kind == MacroKind.Keys && macro.Keys.Count > _layout.MaxCharacters)
                    continue;
                if (!MacroWriter.Write(_usb, composer, action, assignment.Layer, _summaryCard.Delay, macro))
                    break;
                _assignments.SetMacro(_layout.Name, assignment.Layer, action, macro);
                written++;
            }
            Cursor = Cursors.Default;

            RefreshSelection();
            if (written == profile.Assignments.Count)
                _summaryCard.ShowStatus($"\u2713  Perfil \u201C{profile.Name}\u201D aplicado em {written} ações.", TextRole.Success);
            else
                _summaryCard.ShowStatus($"Perfil aplicado só em {written} de {profile.Assignments.Count} ações.", TextRole.Danger);
        }

        private void DeleteProfile(Profile profile)
        {
            _profiles.Delete(profile.Name);
            if (_assignments.GetSetting(AutoApplyProfileKey) == profile.Name)
                _assignments.SetSetting(AutoApplyProfileKey, null);
            RefreshProfiles();
            _summaryCard.ShowStatus($"Perfil \u201C{profile.Name}\u201D excluído.", TextRole.Secondary);
        }

        private void ExportProfile(Profile profile)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Exportar perfil",
                Filter = "Perfil do MacroPad (*.json)|*.json",
                FileName = ProfileStore.SafeFileName(profile.Name) + ".json",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _profiles.Export(profile, dialog.FileName);
            _summaryCard.ShowStatus("Perfil exportado. Dá para mandar o arquivo para outra pessoa.", TextRole.Success);
        }

        private void ImportProfile()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Importar perfil",
                Filter = "Perfil do MacroPad (*.json)|*.json",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            var imported = _profiles.Import(dialog.FileName);
            RefreshProfiles();
            _summaryCard.ShowStatus(imported == null
                ? "Esse arquivo não é um perfil do MacroPad."
                : $"Perfil \u201C{imported.Name}\u201D importado.",
                imported == null ? TextRole.Danger : TextRole.Success);
        }

        private void SetAutoApplyProfile(string name)
        {
            _assignments.SetSetting(AutoApplyProfileKey, name);
            RefreshProfiles();
            _summaryCard.ShowStatus(name == null
                ? "Nenhum perfil entra sozinho agora."
                : $"O perfil \u201C{name}\u201D vai ser aplicado quando o macropad for conectado.",
                TextRole.Secondary);
        }

        private void SendStagedMacro()
        {
            var action = _deviceCard.SelectedAction;
            if (!_usb.IsConnected || _stagedMacro == null || action == InputAction.None)
                return;

            _recordPage.StopRecording();
            var composer = _composerRepository.Get(_usb.ProtocolType, _usb.Version);
            HidLog.ClearLog();

            var layer = _deviceCard.Layer;
            if (MacroWriter.Write(_usb, composer, action, layer, _summaryCard.Delay, _stagedMacro))
            {
                _assignments.SetMacro(_layout.Name, layer, action, _stagedMacro);
                _summaryCard.ShowStatus($"✓  Enviado às {DateTime.Now:HH:mm}. Já pode testar!", TextRole.Success);
                RefreshSelection();
            }
            else
            {
                _summaryCard.ShowStatus("Não deu certo. Desconecte e conecte o USB e tente de novo.", TextRole.Danger);
            }
        }

        private void ApplyKit(MacroKit kit)
        {
            if (!_usb.IsConnected)
            {
                _summaryCard.ShowStatus("Conecte o macropad no USB para aplicar o kit.", TextRole.Danger);
                return;
            }

            var composer = _composerRepository.Get(_usb.ProtocolType, _usb.Version);
            var layer = _deviceCard.Layer;
            var plan = KitPlanner.Plan(kit, _layout);
            HidLog.ClearLog();

            Cursor = Cursors.WaitCursor;
            var written = 0;
            foreach (var (action, macro) in plan)
            {
                // Uma macro grande demais para o modelo é pulada em vez de ser cortada no meio
                if (macro.Kind == MacroKind.Keys && macro.Keys.Count > _layout.MaxCharacters)
                    continue;
                if (!MacroWriter.Write(_usb, composer, action, layer, _summaryCard.Delay, macro))
                    break;
                _assignments.SetMacro(_layout.Name, layer, action, macro);
                written++;
            }
            Cursor = Cursors.Default;

            if (written == plan.Count)
                _summaryCard.ShowStatus($"✓  Kit “{kit.Title}” aplicado em {written} ações.", TextRole.Success);
            else
                _summaryCard.ShowStatus($"Kit aplicado só em {written} de {plan.Count} ações. Reconecte o USB e tente de novo.", TextRole.Danger);
            RefreshSelection();
        }

        private void ApplyLight(LedMode mode, LedColor color)
        {
            if (!_usb.IsConnected)
            {
                _summaryCard.ShowStatus("Conecte o macropad no USB para mudar a luz.", TextRole.Danger);
                return;
            }

            var composer = _composerRepository.Get(_usb.ProtocolType, _usb.Version);
            HidLog.ClearLog();
            if (MacroWriter.WriteLed(_usb, composer, _deviceCard.Layer, mode, color))
                _summaryCard.ShowStatus($"✓  Iluminação aplicada às {DateTime.Now:HH:mm}.", TextRole.Success);
            else
                _summaryCard.ShowStatus("Não deu certo mudar a luz. Reconecte o USB e tente de novo.", TextRole.Danger);
        }

        private string DescribeAction(InputAction action)
        {
            var control = _layout.Controls.FirstOrDefault(c => c.Actions.Contains(action));
            if (control == null)
                return "Nenhuma tecla";
            if (control is PhysicalButton)
                return $"Tecla {control.Name}";

            var actions = control.Actions.ToArray();
            var movement = "apertar";
            if (action == actions[0])
                movement = "girar à esquerda";
            else if (action == actions[2])
                movement = "girar à direita";
            return $"Knob {control.Name}  ·  {movement}";
        }
    }
}
