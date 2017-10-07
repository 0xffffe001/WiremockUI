﻿using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WiremockUI.Languages;

namespace WiremockUI
{
    public partial class FCTBTextBox
    {
        private bool enableOptions;
        private bool firstInput = true;

        #region JSON formatter
        
        private readonly Regex jsonKeyRegex = new Regex(@""".+""\:", SyntaxHighlighter.RegexCompiledOption);
        private readonly Regex jsonNumberRegex = new Regex(@"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b", SyntaxHighlighter.RegexCompiledOption);
        private readonly Regex jsonStringRegex = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", SyntaxHighlighter.RegexCompiledOption);
        private readonly Regex jsonKeywordRegex = new Regex(@"\b(true|false|null)\b", SyntaxHighlighter.RegexCompiledOption);

        private Style jsonKeyStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        public readonly Style jsonNumberStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        public readonly Style jsonStringStyle = new TextStyle(Brushes.Black, null, FontStyle.Italic);
        public readonly Style jsonKeywordStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);

        #endregion


        private LanguageSupported language;

        public enum LanguageSupported
        {
            Custom = 0,
            CSharp = 1,
            VB = 2,
            HTML = 3,
            XML = 4,
            SQL = 5,
            PHP = 6,
            JS = 7,
            Lua = 8,
            Json = 9
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsEdited { get; set; }

        [DefaultValue(LanguageSupported.Custom)]
        public LanguageSupported Language
        {
            get
            {
                return this.language;
            }
            set
            {
                this.language = value;
                txtContent.ClearStyle(StyleIndex.All);
                txtContent.ClearStylesBuffer();

                if (this.language == LanguageSupported.Json)
                {
                    // txtContent.Language = FastColoredTextBoxNS.Language.Custom;
                    this.txtContent.TextChangedDelayed += this.txtContent_IsJson_TextChangedDelayed;
                    SetJsonStyle();
                }
                else
                {
                    this.txtContent.TextChangedDelayed -= this.txtContent_IsJson_TextChangedDelayed;
                    txtContent.Language = GetLanguageFCTB(value);
                }

                txtContent.OnSyntaxHighlight(new TextChangedEventArgs(txtContent.Range));
            }
        }

        private void SetJsonStyle()
        {
            txtContent.Range.SetStyle(jsonKeyStyle, jsonKeyRegex);
            txtContent.Range.SetStyle(jsonStringStyle, jsonStringRegex);
            txtContent.Range.SetStyle(jsonNumberStyle, jsonNumberRegex);
            txtContent.Range.SetStyle(jsonKeywordStyle, jsonKeywordRegex);
        }

        private void txtContent_IsJson_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            //clear styles
            //txtContent.Range.ClearStyle(KeywordsJsonStyle, FunctionNameStyle);
            SetJsonStyle();
        }

        [DefaultValue(false)]
        public bool ShowLanguages { get; set; }

        [DefaultValue(true)]
        public bool EnableOptions
        {
            get => enableOptions;
            set
            {
                this.enableOptions = value;
                if (value)
                {
                    var menu = new ContextMenuStrip();
                    var jsonMenu = new ToolStripMenuItem(Resource.jsonText);
                    var xmlMenu = new ToolStripMenuItem(Resource.xmlText);
                    var editMenu = new ToolStripMenuItem(Resource.editMenu);
                    var languagesMenu = new ToolStripMenuItem(Resource.languagesMenu);

                    var copyMenu = new ToolStripMenuItem();
                    var cutMenu = new ToolStripMenuItem();
                    var pasteMenu = new ToolStripMenuItem();
                    var undoMenu = new ToolStripMenuItem();
                    var redoMenu = new ToolStripMenuItem();
                    var selectAllMenu = new ToolStripMenuItem();
                    var removeMenu = new ToolStripMenuItem();
                    var wordWrapMenu = new ToolStripMenuItem();

                    var formatJsonMenu = new ToolStripMenuItem();
                    var jsonEscapeMenu = new ToolStripMenuItem();
                    var jsonUnescapeMenu = new ToolStripMenuItem();
                    var minifyJsonMenu = new ToolStripMenuItem();
                    var editJsonValueMenu = new ToolStripMenuItem();

                    var formatXmlMenu = new ToolStripMenuItem();
                    var xmlEscapeMenu = new ToolStripMenuItem();
                    var xmlUnescapeMenu = new ToolStripMenuItem();
                    var minifyXmlMenu = new ToolStripMenuItem();
                    var editXmlValueMenu = new ToolStripMenuItem();
                    var searchMenu = new ToolStripMenuItem();

                    var langCSharpMenu = new ToolStripMenuItem("C#");
                    var langCustomMenu = new ToolStripMenuItem("Custom");
                    var langHTMLMenu = new ToolStripMenuItem("HTML");
                    var langJSMenu = new ToolStripMenuItem("JavaScript");
                    var langJsonMenu = new ToolStripMenuItem("JSON");
                    var langLuaMenu = new ToolStripMenuItem("Lua");
                    var langPHPMenu = new ToolStripMenuItem("PHP");
                    var langSQLMenu = new ToolStripMenuItem("SQL");
                    var langVBMenu = new ToolStripMenuItem("VB.NET");
                    var langXMLMenu = new ToolStripMenuItem("XML");

                    menu.Items.AddRange(new ToolStripItem[]
                    {
                        undoMenu,
                        redoMenu,
                        new ToolStripSeparator(),
                        editMenu,                        
                        searchMenu,
                        new ToolStripSeparator(),
                        jsonMenu,
                        xmlMenu,
                        languagesMenu,
                    });

                    editMenu.DropDownItems.AddRange(new ToolStripItem[]
                    {
                        wordWrapMenu,
                        new ToolStripSeparator(),
                        selectAllMenu,
                        copyMenu,
                        cutMenu,
                        pasteMenu,
                        removeMenu,
                    });

                    jsonMenu.DropDownItems.AddRange(new ToolStripMenuItem[]
                    {
                        formatJsonMenu,
                        jsonEscapeMenu,
                        jsonUnescapeMenu,
                        minifyJsonMenu,
                        editJsonValueMenu
                    });

                    jsonMenu.DropDownOpening += (o, s) =>
                    {
                        editJsonValueMenu.Visible = !string.IsNullOrEmpty(txtContent.SelectedText);
                    };

                    xmlMenu.DropDownItems.AddRange(new ToolStripMenuItem[]
                    {
                        formatXmlMenu,
                        xmlEscapeMenu,
                        xmlUnescapeMenu,
                        minifyXmlMenu,
                        editXmlValueMenu
                    });

                    xmlMenu.DropDownOpening += (o, s) =>
                    {
                        editXmlValueMenu.Visible = !string.IsNullOrEmpty(txtContent.SelectedText);
                    };

                    languagesMenu.DropDownItems.AddRange(new ToolStripMenuItem[]
                    {
                        langCSharpMenu,
                        langCustomMenu,
                        langHTMLMenu,
                        langJSMenu,
                        langJsonMenu,
                        langLuaMenu,
                        langPHPMenu,
                        langSQLMenu,
                        langVBMenu,
                        langXMLMenu,
                    });

                    // json
                    formatJsonMenu.Text = Resource.formatJsonMenu;
                    formatJsonMenu.ShortcutKeys = Keys.Control | Keys.D1;
                    formatJsonMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            TextValue = Helper.FormatToJson(TextValue);
                        else
                            txtContent.SelectedText = Helper.FormatToJson(txtContent.SelectedText);
                    };

                    // encode json
                    jsonEscapeMenu.Text = Resource.jsonEscapeMenu;
                    jsonEscapeMenu.ShortcutKeys = Keys.Control | Keys.D2;
                    jsonEscapeMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            TextValue = Helper.JsonEscape(TextValue);
                        else
                            txtContent.SelectedText = Helper.JsonEscape(txtContent.SelectedText);
                    };

                    // decode json
                    jsonUnescapeMenu.Text = Resource.jsonUnescapeMenu;
                    jsonUnescapeMenu.ShortcutKeys = Keys.Control | Keys.D3;
                    jsonUnescapeMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            txtContent.Text = Helper.JsonUnescape(TextValue);
                        else
                            txtContent.SelectedText = Helper.JsonUnescape(txtContent.SelectedText);
                    };

                    // minify json
                    minifyJsonMenu.Text = Resource.minifyMenu;
                    minifyJsonMenu.ShortcutKeys = Keys.Control | Keys.D4;
                    minifyJsonMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            TextValue = Helper.JsonMinify(TextValue);
                        else
                            txtContent.SelectedText = Helper.JsonMinify(txtContent.SelectedText);
                    };

                    // edit value json
                    editJsonValueMenu.Text = Resource.editValueMenu;
                    editJsonValueMenu.ShortcutKeys = Keys.Control | Keys.D5;
                    editJsonValueMenu.Click += (a, b) =>
                    {
                        var selectedValue = Helper.GetJsonValue(txtContent.SelectedText, out var hasQuote, out var hasError);

                        if (!hasError)
                        {
                            var frmEdit = new FormEditValue(selectedValue, result =>
                            {
                                if (hasQuote)
                                    txtContent.SelectedText = "\"" + Helper.JsonEscape(result) + "\"";
                                else
                                    txtContent.SelectedText = Helper.JsonEscape(result);
                            });

                            frmEdit.StartPosition = FormStartPosition.CenterParent;
                            frmEdit.ShowDialog();
                        }
                    };

                    // xml
                    formatXmlMenu.Text = Resource.formatXmlMenu;
                    formatXmlMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D1;
                    formatXmlMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            txtContent.Text = Helper.FormatToXml(TextValue);
                        else
                            txtContent.SelectedText = Helper.FormatToXml(txtContent.SelectedText);
                    };

                    
                    // encode xml
                    xmlEscapeMenu.Text = Resource.xmlEscapeMenu;
                    xmlEscapeMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D2;
                    xmlEscapeMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            TextValue = Helper.XmlEscape(TextValue);
                        else
                            txtContent.SelectedText = Helper.XmlEscape(txtContent.SelectedText);
                    };

                    // decode xml
                    xmlUnescapeMenu.Text = Resource.jsonUnescapeMenu;
                    xmlUnescapeMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D3;
                    xmlUnescapeMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            TextValue = Helper.XmlUnescape(TextValue);
                        else
                            txtContent.SelectedText = Helper.XmlUnescape(txtContent.SelectedText);
                    };

                    // minify xml
                    minifyXmlMenu.Text = Resource.minifyMenu;
                    minifyXmlMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D4;
                    minifyXmlMenu.Click += (a, b) =>
                    {
                        if (string.IsNullOrEmpty(txtContent.SelectedText))
                            TextValue = Helper.XmlMinify(TextValue);
                        else
                            txtContent.SelectedText = Helper.XmlMinify(txtContent.SelectedText);
                    };

                    // edit value json
                    editXmlValueMenu.Text = Resource.editValueMenu;
                    editXmlValueMenu.ShortcutKeys = Keys.Control | Keys.Shift | Keys.D5;
                    editXmlValueMenu.Click += (a, b) =>
                    {
                        var selectedValue = Helper.GetXmlValue(txtContent.SelectedText, out var hasTagInSelection, out var hasError);
                        if (!hasError)
                        {
                            var frmEdit = new FormEditValue(selectedValue, result =>
                            {
                                if (hasTagInSelection)
                                    txtContent.SelectedText = ">" + Helper.XmlEscape(result) + "</";
                                else
                                    txtContent.SelectedText = Helper.XmlEscape(result);
                            });

                            frmEdit.StartPosition = FormStartPosition.CenterParent;
                            frmEdit.ShowDialog();
                        }
                    };

                    // search
                    searchMenu.Text = Resource.searchMenu;
                    searchMenu.ShortcutKeyDisplayString = "Ctrl + F";
                    searchMenu.Click += (a, b) =>
                    {
                        txtContent.ShowReplaceDialog();
                    };

                    // copy menu
                    copyMenu.Text = Resource.copyMenu;
                    copyMenu.ShortcutKeyDisplayString = "Ctrl + C";
                    copyMenu.Click += (a, b) =>
                    {
                        txtContent.Copy();
                    };

                    // cut menu 
                    cutMenu.Text = Resource.cutMenu;
                    cutMenu.ShortcutKeyDisplayString = "Ctrl + X";
                    cutMenu.Click += (a, b) =>
                    {
                        txtContent.Cut();
                    };

                    // paste menu 
                    pasteMenu.Text = Resource.pasteMenu;
                    pasteMenu.ShortcutKeyDisplayString = "Ctrl + V";
                    pasteMenu.Click += (a, b) =>
                    {
                        txtContent.Paste();
                    };

                    // undo menu 
                    undoMenu.Text = Resource.undoMenu;
                    undoMenu.ShortcutKeyDisplayString = "Ctrl + Z";
                    undoMenu.Click += (a, b) =>
                    {
                        txtContent.Undo();
                    };

                    // retro menu 
                    redoMenu.Text = Resource.redoMenu;
                    redoMenu.ShortcutKeyDisplayString = "Ctrl + Y";
                    redoMenu.Click += (a, b) =>
                    {
                        txtContent.Redo();
                    };

                    // select all menu 
                    selectAllMenu.Text = Resource.selectAllMenu;
                    selectAllMenu.ShortcutKeyDisplayString = "Ctrl + A";
                    selectAllMenu.Click += (a, b) =>
                    {
                        txtContent.SelectAll();
                    };

                    // remove menu 
                    removeMenu.Text = Resource.removeMenu;
                    removeMenu.ShortcutKeyDisplayString = "Del";
                    removeMenu.Click += (a, b) =>
                    {
                        if (!string.IsNullOrEmpty(txtContent.SelectedText))
                            txtContent.SelectedText = "";
                    };

                    // wordWrap menu 
                    wordWrapMenu.Text = Resource.wordWrapMenu;
                    wordWrapMenu.Click += (a, b) =>
                    {
                        txtContent.WordWrap = !txtContent.WordWrap;
                    };

                    // languages
                    langCSharpMenu.Click += (a, b) => Language = LanguageSupported.CSharp;
                    langCustomMenu.Click += (a, b) => Language = LanguageSupported.Custom;
                    langHTMLMenu.Click += (a, b) => Language = LanguageSupported.HTML;
                    langJSMenu.Click += (a, b) => Language = LanguageSupported.JS;
                    langJsonMenu.Click += (a, b) => Language = LanguageSupported.Json;
                    langLuaMenu.Click += (a, b) => Language = LanguageSupported.Lua;
                    langPHPMenu.Click += (a, b) => Language = LanguageSupported.PHP;
                    langSQLMenu.Click += (a, b) => Language = LanguageSupported.SQL;
                    langVBMenu.Click += (a, b) => Language = LanguageSupported.VB;
                    langXMLMenu.Click += (a, b) => Language = LanguageSupported.XML;

                    menu.Opened += (o, e) =>
                    {
                        languagesMenu.Visible = ShowLanguages;

                        if (txtContent.WordWrap)
                            wordWrapMenu.Image = Properties.Resources.check;
                        else
                            wordWrapMenu.Image = null;

                        foreach (ToolStripItem l in languagesMenu.DropDownItems)
                            l.Image = null;

                        switch (language)
                        {
                            case LanguageSupported.CSharp:
                                langCSharpMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.HTML:
                                langHTMLMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.JS:
                                langJSMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.Lua:
                                langLuaMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.PHP:
                                langPHPMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.SQL:
                                langSQLMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.VB:
                                langVBMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.XML:
                                langXMLMenu.Image = Properties.Resources.check;
                                break;
                            case LanguageSupported.Json:
                                langJsonMenu.Image = Properties.Resources.check;
                                break;
                            default:
                                langCustomMenu.Image = Properties.Resources.check;
                                break;
                        }
                    };

                    txtContent.ContextMenuStrip = menu;
                }
                else
                {
                    txtContent.ContextMenuStrip = null;
                }
            }
        }

        #region RithTextBox Wrapper

        [Localizable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue("")]
        public string TextValue
        {
            get => txtContent.Text;
            set 
            {
                txtContent.Text = value;
                if (firstInput)
                {
                    txtContent.ClearUndo();
                    firstInput = false;
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get => txtContent.SelectionStart;
            set => txtContent.SelectionStart = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get => txtContent.SelectionLength;
            set => txtContent.SelectionLength = value;
        }

        [Browsable(false)]
        public int TextLength
        {
            get => txtContent.TextLength;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SelectionColor
        {
            get => txtContent.SelectionColor;
            set => txtContent.SelectionColor = value;
        }

        [Browsable(false)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get => txtContent.SelectedText;
            set => txtContent.SelectedText = value;
        }

        [DefaultValue(true)]
        public bool AcceptsTab
        {
            get => txtContent.AcceptsTab;
            set => txtContent.AcceptsTab = value;
        }

        [DefaultValue(true)]
        public bool Multiline
        {
            get => txtContent.Multiline;
            set => txtContent.Multiline = value;
        }

        //[DefaultValue(false)]
        //[Localizable(false)]
        //public bool WordWrap
        //{
        //    get => txtContent.WordWrap;
        //    set => txtContent.WordWrap = value;
        //}

        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public bool ReadOnly
        {
            get => txtContent.ReadOnly;
            set => txtContent.ReadOnly = value;
        }

        [DefaultValue(BorderStyle.Fixed3D)]
        public new BorderStyle BorderStyle
        {
            get => txtContent.BorderStyle;
            set
            {
                txtContent.BorderStyle = value;
            }
        }

        #endregion

        public FCTBTextBox()
        {
            InitializeComponent();
            EnableOptions = true;
        }

        public Language GetLanguageFCTB(LanguageSupported language)
        {
            switch (language)
            {
                case LanguageSupported.CSharp:
                    return FastColoredTextBoxNS.Language.CSharp;
                case LanguageSupported.HTML:
                    return FastColoredTextBoxNS.Language.HTML;
                case LanguageSupported.JS:
                    return FastColoredTextBoxNS.Language.JS;
                case LanguageSupported.Lua:
                    return FastColoredTextBoxNS.Language.Lua;
                case LanguageSupported.PHP:
                    return FastColoredTextBoxNS.Language.PHP;
                case LanguageSupported.SQL:
                    return FastColoredTextBoxNS.Language.SQL;
                case LanguageSupported.VB:
                    return FastColoredTextBoxNS.Language.VB;
                case LanguageSupported.XML:
                    return FastColoredTextBoxNS.Language.XML;
                default:
                    return FastColoredTextBoxNS.Language.Custom;
            }
        }

        private void txtContent_KeyDown(object sender, KeyEventArgs e)
        {
            var keysNotWrite = new Keys[]
            {
                Keys.ControlKey,
                Keys.Alt,
                Keys.ShiftKey,
                Keys.LShiftKey,
                Keys.RShiftKey,
                Keys.Menu,
                Keys.LMenu,
                Keys.RMenu,
                Keys.Apps,
                Keys.CapsLock,
                Keys.Control,                
                Keys.VolumeDown,
                Keys.VolumeMute,
                Keys.VolumeUp,
                Keys.XButton1,
                Keys.XButton2,
                Keys.Zoom,
                Keys.Attn,
                Keys.Capital,
                Keys.F1,
                Keys.F2,
                Keys.F3,
                Keys.F4,
                Keys.F5,
                Keys.F6,
                Keys.F7,
                Keys.F8,
                Keys.F9,
                Keys.F10,
                Keys.F11,
                Keys.F12,
                Keys.F13,
                Keys.F14,
                Keys.F15,
                Keys.F16,
                Keys.F17,
                Keys.F18,
                Keys.F19,
                Keys.F20,
                Keys.F21,
                Keys.F22,
                Keys.F23,
                Keys.F24,
                Keys.LWin,
                Keys.RWin,
                Keys.Escape,
                Keys.Down,
                Keys.Up,
            };

            var keyCanWrite = !keysNotWrite.Contains(e.KeyCode);

            if (keyCanWrite)
                IsEdited = true;

            OnKeyDown(e);
        }
    }
}
