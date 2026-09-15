using System.Collections.Generic;
using System.Linq;
using RSoft.MacroPad.BLL.Infrasturture.Model;

namespace RSoft.MacroPad.BLL.Macros
{
    /// <summary>
    /// Macros, kits e dicas que já vêm prontos no app.
    /// </summary>
    public static class MacroCatalog
    {
        private const Modifier None = Modifier.None;
        private const Modifier Ctrl = Modifier.LeftCtrl;
        private const Modifier Shift = Modifier.LeftShift;
        private const Modifier Alt = Modifier.LeftAlt;
        private const Modifier Win = Modifier.LeftWin;

        public static IReadOnlyList<string> Categories { get; } = new[]
        {
            "Essenciais", "Navegador", "Janelas", "Windows", "Mídia", "Mouse",
            "Reuniões", "Office", "Programação", "Criação", "Streaming", "Texto",
        };

        public static IReadOnlyList<Macro> Macros { get; } = new List<Macro>
        {
            // Essenciais
            Shortcut("copy", "Copiar", "Essenciais", "\uE8C8", KeyCode.C, Ctrl),
            Shortcut("paste", "Colar", "Essenciais", "\uE77F", KeyCode.V, Ctrl),
            Shortcut("cut", "Recortar", "Essenciais", "\uE8C6", KeyCode.X, Ctrl),
            Shortcut("undo", "Desfazer", "Essenciais", "\uE7A7", KeyCode.Z, Ctrl),
            Shortcut("redo", "Refazer", "Essenciais", "\uE7A6", KeyCode.Y, Ctrl),
            Shortcut("select-all", "Selecionar tudo", "Essenciais", "\uE8B3", KeyCode.A, Ctrl),
            Shortcut("save", "Salvar", "Essenciais", "\uE74E", KeyCode.S, Ctrl, "No Office em português é Ctrl + B"),
            Shortcut("find", "Localizar", "Essenciais", "\uE721", KeyCode.F, Ctrl),
            Shortcut("print", "Imprimir", "Essenciais", "\uE749", KeyCode.P, Ctrl),
            Shortcut("paste-plain", "Colar sem formatação", "Essenciais", "\uE77F", KeyCode.V, Ctrl | Shift, "Na maioria dos apps"),
            Shortcut("clipboard-history", "Histórico de cópias", "Essenciais", "\uE81C", KeyCode.V, Win),
            Shortcut("rename", "Renomear", "Essenciais", "\uE70F", KeyCode.F2, None),
            Shortcut("refresh", "Atualizar", "Essenciais", "\uE72C", KeyCode.F5, None),
            Shortcut("delete", "Excluir", "Essenciais", "\uE74D", KeyCode.Del, None),
            Shortcut("enter", "Enter", "Essenciais", "\uE751", KeyCode.Enter, None),
            Shortcut("escape", "Esc", "Essenciais", "Esc", KeyCode.Esc, None),

            // Navegador
            Shortcut("new-tab", "Nova aba", "Navegador", "\uE710", KeyCode.T, Ctrl),
            Shortcut("close-tab", "Fechar aba", "Navegador", "\uE711", KeyCode.W, Ctrl),
            Shortcut("reopen-tab", "Reabrir aba fechada", "Navegador", "\uE81C", KeyCode.T, Ctrl | Shift),
            Shortcut("next-tab", "Próxima aba", "Navegador", "\uE72A", KeyCode.Tab, Ctrl),
            Shortcut("previous-tab", "Aba anterior", "Navegador", "\uE72B", KeyCode.Tab, Ctrl | Shift),
            Shortcut("incognito", "Janela anônima", "Navegador", "\uE72E", KeyCode.N, Ctrl | Shift, "Chrome e Edge"),
            Shortcut("address-bar", "Barra de endereço", "Navegador", "\uE774", KeyCode.L, Ctrl),
            Shortcut("hard-reload", "Recarregar sem cache", "Navegador", "\uE72C", KeyCode.F5, Ctrl),
            Shortcut("back", "Voltar", "Navegador", "\uE76B", KeyCode.ArrowLeft, Alt),
            Shortcut("forward", "Avançar", "Navegador", "\uE76C", KeyCode.ArrowRight, Alt),
            Shortcut("zoom-in", "Aumentar zoom", "Navegador", "\uE8A3", KeyCode.Plus, Ctrl),
            Shortcut("zoom-out", "Diminuir zoom", "Navegador", "\uE71F", KeyCode.Minus, Ctrl),
            Shortcut("zoom-reset", "Zoom 100%", "Navegador", "\uE721", KeyCode.D0, Ctrl),
            Shortcut("fullscreen", "Tela cheia", "Navegador", "\uE740", KeyCode.F11, None),
            Shortcut("bookmark", "Favoritar página", "Navegador", "\uE734", KeyCode.D, Ctrl),
            Shortcut("history", "Histórico", "Navegador", "\uE81C", KeyCode.H, Ctrl),
            Shortcut("downloads", "Downloads", "Navegador", "\uE896", KeyCode.J, Ctrl),

            // Janelas
            Shortcut("switch-window", "Alternar janela", "Janelas", "\uE8AB", KeyCode.Tab, Alt),
            Shortcut("task-view", "Visão de tarefas", "Janelas", "\uE7C4", KeyCode.Tab, Win),
            Shortcut("show-desktop", "Mostrar área de trabalho", "Janelas", "\uE7F4", KeyCode.D, Win),
            Shortcut("close-window", "Fechar janela", "Janelas", "\uE8BB", KeyCode.F4, Alt),
            Shortcut("maximize", "Maximizar", "Janelas", "\uE922", KeyCode.ArrowUp, Win),
            Shortcut("minimize", "Minimizar", "Janelas", "\uE921", KeyCode.ArrowDown, Win),
            Shortcut("snap-left", "Encaixar à esquerda", "Janelas", "\uE76B", KeyCode.ArrowLeft, Win),
            Shortcut("snap-right", "Encaixar à direita", "Janelas", "\uE76C", KeyCode.ArrowRight, Win),
            Shortcut("snap-layouts", "Layouts de encaixe", "Janelas", "\uE8A9", KeyCode.Z, Win, "Windows 11"),
            Shortcut("new-desktop", "Nova área de trabalho", "Janelas", "\uE710", KeyCode.D, Ctrl | Win),
            Shortcut("next-desktop", "Próxima área de trabalho", "Janelas", "\uE72A", KeyCode.ArrowRight, Ctrl | Win),
            Shortcut("previous-desktop", "Área de trabalho anterior", "Janelas", "\uE72B", KeyCode.ArrowLeft, Ctrl | Win),
            Shortcut("move-monitor", "Mandar p/ outro monitor", "Janelas", "\uE7F4", KeyCode.ArrowRight, Win | Shift),

            // Windows
            Shortcut("lock", "Bloquear o PC", "Windows", "\uE72E", KeyCode.L, Win),
            Shortcut("snip", "Print de uma área", "Windows", "\uE924", KeyCode.S, Win | Shift),
            Shortcut("screenshot", "Print da tela (salva)", "Windows", "\uE722", KeyCode.PrtSc, Win),
            Shortcut("record-screen", "Gravar a tela", "Windows", "\uE7C8", KeyCode.R, Win | Shift, "Ferramenta de Captura do Windows 11"),
            Shortcut("explorer", "Explorador de arquivos", "Windows", "\uE838", KeyCode.E, Win),
            Shortcut("settings", "Configurações", "Windows", "\uE713", KeyCode.I, Win),
            Shortcut("run", "Executar", "Windows", "\uE756", KeyCode.R, Win),
            Shortcut("task-manager", "Gerenciador de tarefas", "Windows", "\uE950", KeyCode.Esc, Ctrl | Shift),
            Shortcut("emoji", "Emojis", "Windows", "\uE76E", KeyCode.Period, Win),
            Shortcut("dictation", "Ditado por voz", "Windows", "\uE720", KeyCode.H, Win),
            Shortcut("search", "Pesquisar", "Windows", "\uE721", KeyCode.S, Win),
            Shortcut("notifications", "Notificações", "Windows", "\uE7E7", KeyCode.N, Win),
            Shortcut("quick-settings", "Configurações rápidas", "Windows", "\uE9E9", KeyCode.A, Win),
            Shortcut("game-bar", "Xbox Game Bar", "Windows", "\uE7FC", KeyCode.G, Win),
            Shortcut("project", "Projetar tela", "Windows", "\uE7F4", KeyCode.P, Win),
            Shortcut("magnifier-in", "Lupa: aumentar", "Windows", "\uE8A3", KeyCode.Plus, Win),
            Shortcut("magnifier-out", "Lupa: diminuir", "Windows", "\uE71F", KeyCode.Minus, Win),
            Shortcut("power-menu", "Menu rápido (Win + X)", "Windows", "\uE700", KeyCode.X, Win),

            // Mídia
            Media("play-pause", "Play / Pausa", "\uE768", MediaKey.PlayPause),
            Media("next-track", "Próxima música", "\uE893", MediaKey.NextTrack),
            Media("previous-track", "Música anterior", "\uE892", MediaKey.PrevTrack),
            Media("volume-up", "Aumentar volume", "\uE995", MediaKey.VolUp),
            Media("volume-down", "Diminuir volume", "\uE993", MediaKey.VolDn),
            Media("mute", "Mudo", "\uE74F", MediaKey.VolMute),

            // Mouse
            Mouse("left-click", "Clique", "\uE962", MouseButton.Left, None),
            Mouse("right-click", "Clique direito", "\uE962", MouseButton.Right, None),
            Mouse("middle-click", "Clique do meio", "\uE962", MouseButton.Middle, None),
            Mouse("ctrl-click", "Ctrl + clique", "\uE8A7", MouseButton.Left, Ctrl, "Abre link em nova aba"),
            Mouse("scroll-up", "Rolar para cima", "\uE74A", MouseButton.ScrollUp, None),
            Mouse("scroll-down", "Rolar para baixo", "\uE74B", MouseButton.ScrollDown, None),
            Mouse("scroll-zoom-in", "Zoom + com rolagem", "\uE8A3", MouseButton.ScrollUp, Ctrl, "Funciona em quase todo app"),
            Mouse("scroll-zoom-out", "Zoom − com rolagem", "\uE71F", MouseButton.ScrollDown, Ctrl, "Funciona em quase todo app"),
            Mouse("scroll-right", "Rolar para o lado", "\uE72A", MouseButton.ScrollDown, Shift, "Na maioria dos apps"),
            Mouse("scroll-left", "Rolar para o outro lado", "\uE72B", MouseButton.ScrollUp, Shift, "Na maioria dos apps"),

            // Reuniões
            Shortcut("teams-mic", "Teams: microfone", "Reuniões", "\uE720", KeyCode.M, Ctrl | Shift),
            Shortcut("teams-camera", "Teams: câmera", "Reuniões", "\uE714", KeyCode.O, Ctrl | Shift),
            Shortcut("teams-hand", "Teams: levantar a mão", "Reuniões", "\uE7C9", KeyCode.K, Ctrl | Shift),
            Shortcut("meet-mic", "Meet: microfone", "Reuniões", "\uE720", KeyCode.D, Ctrl),
            Shortcut("meet-camera", "Meet: câmera", "Reuniões", "\uE714", KeyCode.E, Ctrl),
            Shortcut("zoom-mic", "Zoom: microfone", "Reuniões", "\uE720", KeyCode.A, Alt),
            Shortcut("zoom-camera", "Zoom: câmera", "Reuniões", "\uE714", KeyCode.V, Alt),
            Shortcut("zoom-share", "Zoom: compartilhar tela", "Reuniões", "\uE8A7", KeyCode.S, Alt),
            Shortcut("zoom-hand", "Zoom: levantar a mão", "Reuniões", "\uE7C9", KeyCode.Y, Alt),
            Shortcut("discord-mute", "Discord: microfone", "Reuniões", "\uE720", KeyCode.M, Ctrl | Shift),
            Shortcut("discord-deafen", "Discord: ensurdecer", "Reuniões", "\uE7F6", KeyCode.D, Ctrl | Shift),
            Shortcut("windows-mic", "Mutar mic (Windows 11)", "Reuniões", "\uE720", KeyCode.K, Win | Alt, "Em apps de chamada compatíveis"),

            // Office (atalhos do Office em português)
            Shortcut("office-save", "Salvar", "Office", "\uE74E", KeyCode.B, Ctrl, "Office em português"),
            Shortcut("office-bold", "Negrito", "Office", "\uE8DD", KeyCode.N, Ctrl, "Office em português"),
            Shortcut("office-italic", "Itálico", "Office", "\uE8DB", KeyCode.I, Ctrl),
            Shortcut("office-underline", "Sublinhado", "Office", "\uE8DC", KeyCode.S, Ctrl, "Office em português"),
            Shortcut("excel-autosum", "Excel: AutoSoma", "Office", "Σ", KeyCode.Plus, Alt),
            Shortcut("excel-new-sheet", "Excel: nova planilha", "Office", "\uE710", KeyCode.F11, Shift),
            Shortcut("ppt-start", "PowerPoint: apresentar", "Office", "\uE8AE", KeyCode.F5, None),
            Shortcut("ppt-current", "Apresentar deste slide", "Office", "\uE8AE", KeyCode.F5, Shift),

            // Programação (VS Code)
            Shortcut("vscode-palette", "Paleta de comandos", "Programação", "\uE756", KeyCode.P, Ctrl | Shift, "VS Code"),
            Shortcut("vscode-quick-open", "Abrir arquivo", "Programação", "\uE8E5", KeyCode.P, Ctrl, "VS Code"),
            Shortcut("vscode-format", "Formatar documento", "Programação", "\uE8E4", KeyCode.F, Shift | Alt, "VS Code"),
            Shortcut("vscode-duplicate-line", "Duplicar linha", "Programação", "\uE8C8", KeyCode.ArrowDown, Shift | Alt, "VS Code"),
            Shortcut("vscode-line-up", "Mover linha para cima", "Programação", "\uE74A", KeyCode.ArrowUp, Alt, "VS Code"),
            Shortcut("vscode-line-down", "Mover linha para baixo", "Programação", "\uE74B", KeyCode.ArrowDown, Alt, "VS Code"),
            Shortcut("vscode-rename", "Renomear símbolo", "Programação", "\uE70F", KeyCode.F2, None, "VS Code"),
            Shortcut("vscode-definition", "Ir para definição", "Programação", "\uE943", KeyCode.F12, None, "VS Code"),
            Shortcut("vscode-next-match", "Selecionar próxima igual", "Programação", "\uE8B3", KeyCode.D, Ctrl, "VS Code"),
            Sequence("vscode-save-all", "Salvar tudo", "Programação", "\uE74E", "VS Code",
                (KeyCode.K, Ctrl), (KeyCode.S, None)),
            Shortcut("debug-start", "Executar / depurar", "Programação", "\uE768", KeyCode.F5, None),
            Shortcut("debug-stop", "Parar depuração", "Programação", "\uE978", KeyCode.F5, Shift),

            // Streaming: F13 a F24 não existem no teclado comum, então não brigam com nenhum atalho
            Shortcut("f13", "Tecla livre F13", "Streaming", "F13", KeyCode.F13, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f14", "Tecla livre F14", "Streaming", "F14", KeyCode.F14, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f15", "Tecla livre F15", "Streaming", "F15", KeyCode.F15, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f16", "Tecla livre F16", "Streaming", "F16", KeyCode.F16, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f17", "Tecla livre F17", "Streaming", "F17", KeyCode.F17, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f18", "Tecla livre F18", "Streaming", "F18", KeyCode.F18, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f19", "Tecla livre F19", "Streaming", "F19", KeyCode.F19, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f20", "Tecla livre F20", "Streaming", "F20", KeyCode.F20, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f21", "Tecla livre F21", "Streaming", "F21", KeyCode.F21, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f22", "Tecla livre F22", "Streaming", "F22", KeyCode.F22, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f23", "Tecla livre F23", "Streaming", "F23", KeyCode.F23, None, "Use no OBS, Discord ou jogos"),
            Shortcut("f24", "Tecla livre F24", "Streaming", "F24", KeyCode.F24, None, "Use no OBS, Discord ou jogos"),

            // Criação (Photoshop, Premiere e Figma usam as mesmas teclas na versão em português)
            Shortcut("ps-undo-steps", "Photoshop: voltar passos", "Criação", "\uE7A7", KeyCode.Z, Ctrl | Alt),
            Shortcut("ps-new-layer", "Photoshop: nova camada", "Criação", "\uE81E", KeyCode.N, Ctrl | Shift),
            Shortcut("ps-merge", "Photoshop: mesclar camadas", "Criação", "\uE8C4", KeyCode.E, Ctrl),
            Shortcut("ps-transform", "Photoshop: transformação livre", "Criação", "\uE7A8", KeyCode.T, Ctrl),
            Shortcut("ps-deselect", "Photoshop: desmarcar", "Criação", "\uE8E6", KeyCode.D, Ctrl),
            Shortcut("ps-invert-selection", "Photoshop: inverter seleção", "Criação", "\uE8AB", KeyCode.I, Ctrl | Shift),
            Shortcut("ps-brush", "Photoshop: pincel", "Criação", "\uE771", KeyCode.B, None),
            Shortcut("ps-eraser", "Photoshop: borracha", "Criação", "\uE75C", KeyCode.E, None),
            Shortcut("ps-move", "Photoshop: mover", "Criação", "\uE7C2", KeyCode.V, None),
            Shortcut("pr-cut", "Premiere: cortar", "Criação", "\uE8C6", KeyCode.C, None),
            Shortcut("pr-select", "Premiere: seleção", "Criação", "\uE8B0", KeyCode.V, None),
            Shortcut("pr-marker", "Premiere: marcador", "Criação", "\uE7C1", KeyCode.M, None),
            Shortcut("pr-in", "Premiere: marcar entrada", "Criação", "\uE892", KeyCode.I, None),
            Shortcut("pr-out", "Premiere: marcar saída", "Criação", "\uE893", KeyCode.O, None),
            Shortcut("pr-export", "Premiere: exportar", "Criação", "\uE896", KeyCode.M, Ctrl),
            Shortcut("figma-frame", "Figma: moldura", "Criação", "\uE8A9", KeyCode.F, None),
            Shortcut("figma-rectangle", "Figma: retângulo", "Criação", "\uE739", KeyCode.R, None),
            Shortcut("figma-text", "Figma: texto", "Criação", "\uE8D2", KeyCode.T, None),
            Shortcut("figma-group", "Figma: agrupar", "Criação", "\uE8B3", KeyCode.G, Ctrl),
            Shortcut("figma-export", "Figma: exportar", "Criação", "\uE896", KeyCode.E, Ctrl | Shift),

            // Programação: ferramentas do navegador
            Shortcut("devtools", "Ferramentas do desenvolvedor", "Programação", "\uE943", KeyCode.F12, None, "Chrome, Edge e Firefox"),
            Shortcut("devtools-console", "Console do navegador", "Programação", "\uE756", KeyCode.J, Ctrl | Shift, "Chrome e Edge"),
            Shortcut("devtools-inspect", "Inspecionar elemento", "Programação", "\uE721", KeyCode.C, Ctrl | Shift, "Chrome e Edge"),

            // Office: mais alguns do Excel
            Shortcut("excel-filter", "Excel: filtro", "Office", "\uE71C", KeyCode.L, Ctrl | Shift),
            Shortcut("excel-table", "Excel: criar tabela", "Office", "\uE8A9", KeyCode.T, Ctrl),
            Shortcut("excel-repeat", "Excel: repetir última ação", "Office", "\uE72C", KeyCode.F4, None),

            // Windows: extras
            Shortcut("restart-graphics", "Reiniciar driver de vídeo", "Windows", "\uE7F4", KeyCode.B, Win | Ctrl | Shift, "Salva a tela travada sem reiniciar o PC"),
            Shortcut("desktop-peek", "Espiar a área de trabalho", "Windows", "\uE7B3", KeyCode.Clear, Win),
            Shortcut("focus-taskbar", "Ir para a barra de tarefas", "Windows", "\uE700", KeyCode.T, Win),

            // Texto
            Text("text-good-morning", "Bom dia!"),
            Text("text-good-afternoon", "Boa tarde!"),
            Text("text-good-night", "Boa noite!"),
            Text("text-thanks", "Obrigado!"),
            Text("text-deal", "Ok, combinado!"),
            Text("text-regards", "Atenciosamente,"),
            Text("text-one-moment", "Um momento..."),
            Text("text-laugh", "kkkkkkkk"),
            Text("text-gg", "gg wp"),
        };

        public static IReadOnlyList<MacroKit> Kits { get; } = new[]
        {
            new MacroKit
            {
                Id = "kit-media",
                Title = "Música e vídeo",
                Description = "Botões tocam, pausam e trocam de música. O knob controla o volume.",
                Icon = "\uE768",
                ButtonMacroIds = new[] { "previous-track", "play-pause", "next-track", "mute", "volume-down", "volume-up", "fullscreen", "switch-window", "show-desktop", "lock", "snip", "task-manager" },
                KnobMacroIds = new[] { ("volume-down", "mute", "volume-up"), ("previous-track", "play-pause", "next-track"), ("scroll-up", "middle-click", "scroll-down") },
            },
            new MacroKit
            {
                Id = "kit-productivity",
                Title = "Produtividade",
                Description = "Copiar, colar, desfazer e salvar sempre à mão. O knob dá zoom.",
                Icon = "\uE8C8",
                ButtonMacroIds = new[] { "copy", "paste", "undo", "redo", "save", "find", "cut", "select-all", "snip", "clipboard-history", "show-desktop", "lock" },
                KnobMacroIds = new[] { ("zoom-out", "zoom-reset", "zoom-in"), ("volume-down", "mute", "volume-up"), ("scroll-up", "middle-click", "scroll-down") },
            },
            new MacroKit
            {
                Id = "kit-browser",
                Title = "Navegador",
                Description = "Abas, voltar e avançar. O knob troca de aba.",
                Icon = "\uE774",
                ButtonMacroIds = new[] { "new-tab", "close-tab", "reopen-tab", "back", "forward", "refresh", "address-bar", "bookmark", "downloads", "history", "incognito", "fullscreen" },
                KnobMacroIds = new[] { ("previous-tab", "new-tab", "next-tab"), ("zoom-out", "zoom-reset", "zoom-in"), ("volume-down", "mute", "volume-up") },
            },
            new MacroKit
            {
                Id = "kit-meetings",
                Title = "Reuniões no Teams",
                Description = "Microfone, câmera e mão levantada em um toque. O knob controla o volume.",
                Icon = "\uE714",
                ButtonMacroIds = new[] { "teams-mic", "teams-camera", "teams-hand", "switch-window", "snip", "lock", "copy", "paste", "show-desktop", "windows-mic", "screenshot", "task-view" },
                KnobMacroIds = new[] { ("volume-down", "mute", "volume-up"), ("previous-tab", "new-tab", "next-tab"), ("scroll-up", "middle-click", "scroll-down") },
            },
            new MacroKit
            {
                Id = "kit-windows",
                Title = "Janelas e áreas de trabalho",
                Description = "Encaixe janelas nos cantos. O knob passa entre áreas de trabalho.",
                Icon = "\uE7C4",
                ButtonMacroIds = new[] { "snap-left", "maximize", "snap-right", "switch-window", "task-view", "show-desktop", "minimize", "snap-layouts", "move-monitor", "close-window", "explorer", "lock" },
                KnobMacroIds = new[] { ("previous-desktop", "task-view", "next-desktop"), ("volume-down", "mute", "volume-up"), ("scroll-up", "middle-click", "scroll-down") },
            },
            new MacroKit
            {
                Id = "kit-streaming",
                Title = "Streaming (OBS)",
                Description = "Teclas F13 a F24 para ligar a cenas do OBS sem conflito. O knob controla o volume.",
                Icon = "\uE7C8",
                ButtonMacroIds = new[] { "f13", "f14", "f15", "f16", "f17", "f18", "f19", "f20", "f21", "f22", "f23", "f24" },
                KnobMacroIds = new[] { ("volume-down", "mute", "volume-up"), ("previous-track", "play-pause", "next-track"), ("scroll-up", "middle-click", "scroll-down") },
            },
            new MacroKit
            {
                Id = "kit-code",
                Title = "Programação (VS Code)",
                Description = "Paleta de comandos, formatar e depurar. O knob move a linha.",
                Icon = "\uE943",
                ButtonMacroIds = new[] { "vscode-palette", "vscode-quick-open", "vscode-format", "debug-start", "debug-stop", "vscode-definition", "vscode-rename", "vscode-duplicate-line", "vscode-next-match", "vscode-save-all", "undo", "redo" },
                KnobMacroIds = new[] { ("vscode-line-up", "vscode-duplicate-line", "vscode-line-down"), ("zoom-out", "zoom-reset", "zoom-in"), ("volume-down", "mute", "volume-up") },
            },
        };

        public static IReadOnlyList<string> Tips { get; } = new[]
        {
            "Clique numa tecla do desenho, escolha o que ela faz e aperte Enviar.",
            "Knobs têm 3 ações: girar para a esquerda, apertar e girar para a direita.",
            "As teclas F13 a F24 não existem no teclado comum. São perfeitas para atalhos no OBS, Discord ou jogos, sem conflito.",
            "Kits configuram todas as teclas de uma vez. Depois você troca só as que quiser.",
            "No Office em português, Salvar é Ctrl + B, Negrito é Ctrl + N e Sublinhado é Ctrl + S.",
            "Win + V abre o histórico de tudo que você copiou. Na primeira vez o Windows pede para ativar.",
            "Na aba Texto não dá para usar acentos, ç nem símbolos como ? ; : /. Eles mudam conforme o layout do PC.",
            "O modelo de 3 teclas guarda até 5 teclas por macro e só aplica Ctrl/Shift na primeira delas.",
            "Ctrl + rolar o mouse dá zoom em quase todo app. Tem pronto na categoria Mouse.",
            "Não achou o atalho? Use a aba Gravar e aperte a combinação no seu teclado.",
            "Os nomes nas teclas mostram o que foi enviado deste PC. O macropad não informa a configuração atual.",
            "Teclado não detectado? Desconecte e conecte o cabo USB de novo.",
            "Com mais de um macropad ligado, o app configura o primeiro que encontrar.",
            "Win + Shift + S tira print de só uma parte da tela.",
            "Ctrl + Shift + T reabre a última aba que você fechou no navegador.",
            "Teclados com camadas guardam configurações diferentes para cada camada. Escolha a camada antes de enviar.",
        };

        public static Macro Find(string id) => Macros.FirstOrDefault(m => m.Id == id);

        private static Macro Shortcut(string id, string title, string category, string icon, KeyCode key, Modifier modifiers, string hint = "")
        {
            return new Macro
            {
                Id = id,
                Title = title,
                Category = category,
                Icon = icon,
                Hint = hint,
                Kind = MacroKind.Keys,
                Keys = new[] { (key, modifiers) },
            };
        }

        private static Macro Sequence(string id, string title, string category, string icon, string hint, params (KeyCode, Modifier)[] keys)
        {
            return new Macro
            {
                Id = id,
                Title = title,
                Category = category,
                Icon = icon,
                Hint = hint,
                Kind = MacroKind.Keys,
                Keys = keys,
            };
        }

        private static Macro Media(string id, string title, string icon, MediaKey key)
        {
            return new Macro
            {
                Id = id,
                Title = title,
                Category = "Mídia",
                Icon = icon,
                Kind = MacroKind.Media,
                MediaKey = key,
            };
        }

        private static Macro Mouse(string id, string title, string icon, MouseButton button, Modifier modifiers, string hint = "")
        {
            return new Macro
            {
                Id = id,
                Title = title,
                Category = "Mouse",
                Icon = icon,
                Hint = hint,
                Kind = MacroKind.Mouse,
                MouseButton = button,
                MouseModifiers = modifiers,
            };
        }

        private static Macro Text(string id, string text)
        {
            return new Macro
            {
                Id = id,
                Title = text,
                Category = "Texto",
                Icon = "\uE8D2",
                Hint = "Digita o texto",
                Kind = MacroKind.Keys,
                Keys = TextTyper.ToKeys(text).Keys,
            };
        }
    }
}
