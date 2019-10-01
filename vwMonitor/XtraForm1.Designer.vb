<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class XtraForm1
    Inherits DevExpress.XtraEditors.XtraForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(XtraForm1))
        Me.PictureEdit1 = New DevExpress.XtraEditors.PictureEdit()
        Me.TileBar1 = New DevExpress.XtraBars.Navigation.TileBar()
        Me.Bar3 = New DevExpress.XtraBars.Bar()
        Me.Bar4 = New DevExpress.XtraBars.Bar()
        Me.LabelControl1 = New DevExpress.XtraEditors.LabelControl()
        Me.LabelControl2 = New DevExpress.XtraEditors.LabelControl()
        Me.HyperlinkLabelControl1 = New DevExpress.XtraEditors.HyperlinkLabelControl()
        Me.GroupControl1 = New DevExpress.XtraEditors.GroupControl()
        Me.ComboBoxEdit2 = New DevExpress.XtraEditors.ComboBoxEdit()
        Me.LabelControl4 = New DevExpress.XtraEditors.LabelControl()
        Me.SimpleButton1 = New DevExpress.XtraEditors.SimpleButton()
        Me.Bar1 = New DevExpress.XtraBars.Bar()
        Me.BarManager1 = New DevExpress.XtraBars.BarManager(Me.components)
        Me.Bar6 = New DevExpress.XtraBars.Bar()
        Me.SkinBarSubItem1 = New DevExpress.XtraBars.SkinBarSubItem()
        Me.BarStaticItem4 = New DevExpress.XtraBars.BarStaticItem()
        Me.BarStaticItem2 = New DevExpress.XtraBars.BarStaticItem()
        Me.BarStaticItem1 = New DevExpress.XtraBars.BarStaticItem()
        Me.BarStaticItem3 = New DevExpress.XtraBars.BarStaticItem()
        Me.barDockControlTop = New DevExpress.XtraBars.BarDockControl()
        Me.barDockControlBottom = New DevExpress.XtraBars.BarDockControl()
        Me.barDockControlLeft = New DevExpress.XtraBars.BarDockControl()
        Me.barDockControlRight = New DevExpress.XtraBars.BarDockControl()
        Me.SimpleButton3 = New DevExpress.XtraEditors.SimpleButton()
        Me.SimpleButton2 = New DevExpress.XtraEditors.SimpleButton()
        Me.revisaFlag = New System.Windows.Forms.Timer(Me.components)
        Me.escalamiento = New System.Windows.Forms.Timer(Me.components)
        Me.SerialPort1 = New System.IO.Ports.SerialPort(Me.components)
        Me.NotifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.VerElLogToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DetenerElMonitorToolStripMenuItem = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReanudarElMonitorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ListBoxControl1 = New DevExpress.XtraEditors.ListBoxControl()
        Me.corte = New System.Windows.Forms.Timer(Me.components)
        Me.correos = New System.Windows.Forms.Timer(Me.components)
        Me.mensajes = New System.Windows.Forms.Timer(Me.components)
        CType(Me.PictureEdit1.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.GroupControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupControl1.SuspendLayout()
        CType(Me.ComboBoxEdit2.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.BarManager1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
        CType(Me.ListBoxControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureEdit1
        '
        Me.PictureEdit1.EditValue = CType(resources.GetObject("PictureEdit1.EditValue"), Object)
        Me.PictureEdit1.Location = New System.Drawing.Point(0, 0)
        Me.PictureEdit1.Name = "PictureEdit1"
        Me.PictureEdit1.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(CType(CType(250, Byte), Integer), CType(CType(183, Byte), Integer), CType(CType(2, Byte), Integer))
        Me.PictureEdit1.Properties.Appearance.Options.UseBackColor = True
        Me.PictureEdit1.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
        Me.PictureEdit1.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.[Auto]
        Me.PictureEdit1.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom
        Me.PictureEdit1.Size = New System.Drawing.Size(87, 72)
        Me.PictureEdit1.TabIndex = 3
        '
        'TileBar1
        '
        Me.TileBar1.BackColor = System.Drawing.Color.FromArgb(CType(CType(250, Byte), Integer), CType(CType(183, Byte), Integer), CType(CType(2, Byte), Integer))
        Me.TileBar1.Dock = System.Windows.Forms.DockStyle.Top
        Me.TileBar1.DropDownOptions.BeakColor = System.Drawing.Color.Empty
        Me.TileBar1.Location = New System.Drawing.Point(0, 0)
        Me.TileBar1.Name = "TileBar1"
        Me.TileBar1.ScrollMode = DevExpress.XtraEditors.TileControlScrollMode.ScrollButtons
        Me.TileBar1.Size = New System.Drawing.Size(1277, 72)
        Me.TileBar1.TabIndex = 2
        Me.TileBar1.Text = "TileBar1"
        '
        'Bar3
        '
        Me.Bar3.BarName = "Barra de estado"
        Me.Bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom
        Me.Bar3.DockCol = 0
        Me.Bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom
        Me.Bar3.OptionsBar.AllowQuickCustomization = False
        Me.Bar3.OptionsBar.DrawDragBorder = False
        Me.Bar3.OptionsBar.UseWholeRow = True
        Me.Bar3.Text = "Barra de estado"
        '
        'Bar4
        '
        Me.Bar4.BarName = "Barra de estado"
        Me.Bar4.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom
        Me.Bar4.DockCol = 0
        Me.Bar4.DockRow = 0
        Me.Bar4.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom
        Me.Bar4.OptionsBar.AllowQuickCustomization = False
        Me.Bar4.OptionsBar.DrawDragBorder = False
        Me.Bar4.OptionsBar.DrawSizeGrip = True
        Me.Bar4.OptionsBar.UseWholeRow = True
        Me.Bar4.Text = "Barra de estado"
        '
        'LabelControl1
        '
        Me.LabelControl1.Appearance.BackColor = System.Drawing.Color.FromArgb(CType(CType(250, Byte), Integer), CType(CType(183, Byte), Integer), CType(CType(2, Byte), Integer))
        Me.LabelControl1.Appearance.Font = New System.Drawing.Font("Lucida Sans", 12.0!, System.Drawing.FontStyle.Bold)
        Me.LabelControl1.Appearance.ForeColor = System.Drawing.Color.Black
        Me.LabelControl1.Appearance.Options.UseBackColor = True
        Me.LabelControl1.Appearance.Options.UseFont = True
        Me.LabelControl1.Appearance.Options.UseForeColor = True
        Me.LabelControl1.Location = New System.Drawing.Point(93, 7)
        Me.LabelControl1.Name = "LabelControl1"
        Me.LabelControl1.Size = New System.Drawing.Size(219, 23)
        Me.LabelControl1.TabIndex = 8
        Me.LabelControl1.Text = "MONITOR DE FALLAS"
        '
        'LabelControl2
        '
        Me.LabelControl2.Appearance.BackColor = System.Drawing.Color.FromArgb(CType(CType(250, Byte), Integer), CType(CType(183, Byte), Integer), CType(CType(2, Byte), Integer))
        Me.LabelControl2.Appearance.ForeColor = System.Drawing.Color.Black
        Me.LabelControl2.Appearance.Options.UseBackColor = True
        Me.LabelControl2.Appearance.Options.UseForeColor = True
        Me.LabelControl2.Location = New System.Drawing.Point(132, 28)
        Me.LabelControl2.Name = "LabelControl2"
        Me.LabelControl2.Size = New System.Drawing.Size(179, 17)
        Me.LabelControl2.TabIndex = 9
        Me.LabelControl2.Text = "Versión 1.00 (11Jul2019)"
        '
        'HyperlinkLabelControl1
        '
        Me.HyperlinkLabelControl1.Appearance.BackColor = System.Drawing.Color.FromArgb(CType(CType(250, Byte), Integer), CType(CType(183, Byte), Integer), CType(CType(2, Byte), Integer))
        Me.HyperlinkLabelControl1.Appearance.ForeColor = System.Drawing.Color.Tomato
        Me.HyperlinkLabelControl1.Appearance.LinkColor = System.Drawing.Color.Black
        Me.HyperlinkLabelControl1.Appearance.Options.UseBackColor = True
        Me.HyperlinkLabelControl1.Appearance.Options.UseForeColor = True
        Me.HyperlinkLabelControl1.Appearance.Options.UseLinkColor = True
        Me.HyperlinkLabelControl1.AppearanceHovered.ForeColor = System.Drawing.Color.Black
        Me.HyperlinkLabelControl1.AppearanceHovered.LinkColor = DevExpress.LookAndFeel.DXSkinColors.ForeColors.Question
        Me.HyperlinkLabelControl1.AppearanceHovered.Options.UseForeColor = True
        Me.HyperlinkLabelControl1.AppearanceHovered.Options.UseLinkColor = True
        Me.HyperlinkLabelControl1.Location = New System.Drawing.Point(93, 50)
        Me.HyperlinkLabelControl1.Name = "HyperlinkLabelControl1"
        Me.HyperlinkLabelControl1.Size = New System.Drawing.Size(231, 17)
        Me.HyperlinkLabelControl1.TabIndex = 3
        Me.HyperlinkLabelControl1.Text = "Ir al sitio de Cronos Integración"
        '
        'GroupControl1
        '
        Me.GroupControl1.AppearanceCaption.Font = New System.Drawing.Font("Lucida Sans", 9.0!, System.Drawing.FontStyle.Bold)
        Me.GroupControl1.AppearanceCaption.Options.UseFont = True
        Me.GroupControl1.Controls.Add(Me.ComboBoxEdit2)
        Me.GroupControl1.Controls.Add(Me.LabelControl4)
        Me.GroupControl1.Controls.Add(Me.SimpleButton1)
        Me.GroupControl1.Location = New System.Drawing.Point(9, 78)
        Me.GroupControl1.Name = "GroupControl1"
        Me.GroupControl1.Size = New System.Drawing.Size(561, 75)
        Me.GroupControl1.TabIndex = 16
        Me.GroupControl1.Text = "Visualización"
        '
        'ComboBoxEdit2
        '
        Me.ComboBoxEdit2.Location = New System.Drawing.Point(183, 37)
        Me.ComboBoxEdit2.Name = "ComboBoxEdit2"
        Me.ComboBoxEdit2.Properties.Buttons.AddRange(New DevExpress.XtraEditors.Controls.EditorButton() {New DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)})
        Me.ComboBoxEdit2.Properties.Items.AddRange(New Object() {"Normal", "Muy pequeña", "Pequeña", "Grande", "Muy grande", "Extra grande"})
        Me.ComboBoxEdit2.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
        Me.ComboBoxEdit2.Size = New System.Drawing.Size(172, 24)
        Me.ComboBoxEdit2.TabIndex = 3
        '
        'LabelControl4
        '
        Me.LabelControl4.Location = New System.Drawing.Point(12, 40)
        Me.LabelControl4.Name = "LabelControl4"
        Me.LabelControl4.Size = New System.Drawing.Size(149, 17)
        Me.LabelControl4.TabIndex = 2
        Me.LabelControl4.Text = "&Tamaño de la fuente"
        '
        'SimpleButton1
        '
        Me.SimpleButton1.ImageOptions.Image = CType(resources.GetObject("SimpleButton1.ImageOptions.Image"), System.Drawing.Image)
        Me.SimpleButton1.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter
        Me.SimpleButton1.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleLeft
        Me.SimpleButton1.Location = New System.Drawing.Point(373, 34)
        Me.SimpleButton1.Name = "SimpleButton1"
        Me.SimpleButton1.Size = New System.Drawing.Size(172, 29)
        Me.SimpleButton1.TabIndex = 4
        Me.SimpleButton1.Text = "Inicializar pantalla"
        '
        'Bar1
        '
        Me.Bar1.BarName = "Personalizada 3"
        Me.Bar1.DockCol = 0
        Me.Bar1.DockRow = 0
        Me.Bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top
        Me.Bar1.Text = "Personalizada 3"
        '
        'BarManager1
        '
        Me.BarManager1.Bars.AddRange(New DevExpress.XtraBars.Bar() {Me.Bar6})
        Me.BarManager1.DockControls.Add(Me.barDockControlTop)
        Me.BarManager1.DockControls.Add(Me.barDockControlBottom)
        Me.BarManager1.DockControls.Add(Me.barDockControlLeft)
        Me.BarManager1.DockControls.Add(Me.barDockControlRight)
        Me.BarManager1.Form = Me
        Me.BarManager1.Items.AddRange(New DevExpress.XtraBars.BarItem() {Me.SkinBarSubItem1, Me.BarStaticItem1, Me.BarStaticItem2, Me.BarStaticItem3, Me.BarStaticItem4})
        Me.BarManager1.MaxItemId = 7
        Me.BarManager1.StatusBar = Me.Bar6
        '
        'Bar6
        '
        Me.Bar6.BarName = "Barra de estado"
        Me.Bar6.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom
        Me.Bar6.DockCol = 0
        Me.Bar6.DockRow = 0
        Me.Bar6.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom
        Me.Bar6.LinksPersistInfo.AddRange(New DevExpress.XtraBars.LinkPersistInfo() {New DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, Me.SkinBarSubItem1, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph), New DevExpress.XtraBars.LinkPersistInfo(Me.BarStaticItem4), New DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, Me.BarStaticItem2, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph), New DevExpress.XtraBars.LinkPersistInfo(Me.BarStaticItem1), New DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, Me.BarStaticItem3, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)})
        Me.Bar6.OptionsBar.AllowQuickCustomization = False
        Me.Bar6.OptionsBar.DrawDragBorder = False
        Me.Bar6.OptionsBar.DrawSizeGrip = True
        Me.Bar6.OptionsBar.UseWholeRow = True
        Me.Bar6.Text = "Barra de estado"
        '
        'SkinBarSubItem1
        '
        Me.SkinBarSubItem1.Caption = "Temas"
        Me.SkinBarSubItem1.Id = 1
        Me.SkinBarSubItem1.ImageOptions.SvgImage = CType(resources.GetObject("SkinBarSubItem1.ImageOptions.SvgImage"), DevExpress.Utils.Svg.SvgImage)
        Me.SkinBarSubItem1.Name = "SkinBarSubItem1"
        '
        'BarStaticItem4
        '
        Me.BarStaticItem4.Caption = "210 registros en el visor"
        Me.BarStaticItem4.Id = 5
        Me.BarStaticItem4.ImageOptions.SvgImage = CType(resources.GetObject("BarStaticItem4.ImageOptions.SvgImage"), DevExpress.Utils.Svg.SvgImage)
        Me.BarStaticItem4.Name = "BarStaticItem4"
        Me.BarStaticItem4.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph
        '
        'BarStaticItem2
        '
        Me.BarStaticItem2.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right
        Me.BarStaticItem2.Caption = "Sin conexión"
        Me.BarStaticItem2.Id = 3
        Me.BarStaticItem2.ImageOptions.SvgImage = CType(resources.GetObject("BarStaticItem2.ImageOptions.SvgImage"), DevExpress.Utils.Svg.SvgImage)
        Me.BarStaticItem2.Name = "BarStaticItem2"
        Me.BarStaticItem2.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph
        '
        'BarStaticItem1
        '
        Me.BarStaticItem1.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right
        Me.BarStaticItem1.Caption = "Conectado"
        Me.BarStaticItem1.Id = 2
        Me.BarStaticItem1.ImageOptions.SvgImage = CType(resources.GetObject("BarStaticItem1.ImageOptions.SvgImage"), DevExpress.Utils.Svg.SvgImage)
        Me.BarStaticItem1.Name = "BarStaticItem1"
        Me.BarStaticItem1.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph
        Me.BarStaticItem1.Visibility = DevExpress.XtraBars.BarItemVisibility.Never
        '
        'BarStaticItem3
        '
        Me.BarStaticItem3.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Right
        Me.BarStaticItem3.Caption = "Funcionando desde Lun, 17-Jun-2019 12:00:19"
        Me.BarStaticItem3.Id = 4
        Me.BarStaticItem3.ImageOptions.SvgImage = CType(resources.GetObject("BarStaticItem3.ImageOptions.SvgImage"), DevExpress.Utils.Svg.SvgImage)
        Me.BarStaticItem3.Name = "BarStaticItem3"
        Me.BarStaticItem3.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph
        '
        'barDockControlTop
        '
        Me.barDockControlTop.CausesValidation = False
        Me.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.barDockControlTop.Location = New System.Drawing.Point(0, 0)
        Me.barDockControlTop.Manager = Me.BarManager1
        Me.barDockControlTop.Size = New System.Drawing.Size(1277, 0)
        '
        'barDockControlBottom
        '
        Me.barDockControlBottom.CausesValidation = False
        Me.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.barDockControlBottom.Location = New System.Drawing.Point(0, 557)
        Me.barDockControlBottom.Manager = Me.BarManager1
        Me.barDockControlBottom.Size = New System.Drawing.Size(1277, 32)
        '
        'barDockControlLeft
        '
        Me.barDockControlLeft.CausesValidation = False
        Me.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.barDockControlLeft.Location = New System.Drawing.Point(0, 0)
        Me.barDockControlLeft.Manager = Me.BarManager1
        Me.barDockControlLeft.Size = New System.Drawing.Size(0, 557)
        '
        'barDockControlRight
        '
        Me.barDockControlRight.CausesValidation = False
        Me.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right
        Me.barDockControlRight.Location = New System.Drawing.Point(1277, 0)
        Me.barDockControlRight.Manager = Me.BarManager1
        Me.barDockControlRight.Size = New System.Drawing.Size(0, 557)
        '
        'SimpleButton3
        '
        Me.SimpleButton3.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter
        Me.SimpleButton3.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter
        Me.SimpleButton3.ImageOptions.SvgImage = CType(resources.GetObject("SimpleButton3.ImageOptions.SvgImage"), DevExpress.Utils.Svg.SvgImage)
        Me.SimpleButton3.ImageOptions.SvgImageSize = New System.Drawing.Size(48, 48)
        Me.SimpleButton3.Location = New System.Drawing.Point(1191, 6)
        Me.SimpleButton3.Name = "SimpleButton3"
        Me.SimpleButton3.Size = New System.Drawing.Size(75, 61)
        Me.SimpleButton3.TabIndex = 26
        Me.SimpleButton3.ToolTip = "Detiene la aplicación"
        Me.SimpleButton3.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Warning
        Me.SimpleButton3.ToolTipTitle = "Operación delicada"
        '
        'SimpleButton2
        '
        Me.SimpleButton2.ImageOptions.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.LeftCenter
        Me.SimpleButton2.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter
        Me.SimpleButton2.ImageOptions.SvgImage = CType(resources.GetObject("SimpleButton2.ImageOptions.SvgImage"), DevExpress.Utils.Svg.SvgImage)
        Me.SimpleButton2.ImageOptions.SvgImageSize = New System.Drawing.Size(48, 48)
        Me.SimpleButton2.Location = New System.Drawing.Point(1191, 4)
        Me.SimpleButton2.Name = "SimpleButton2"
        Me.SimpleButton2.Size = New System.Drawing.Size(75, 63)
        Me.SimpleButton2.TabIndex = 0
        Me.SimpleButton2.ToolTip = "Reanuda la aplicación"
        Me.SimpleButton2.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Exclamation
        Me.SimpleButton2.Visible = False
        '
        'revisaFlag
        '
        Me.revisaFlag.Enabled = True
        Me.revisaFlag.Interval = 500
        '
        'escalamiento
        '
        Me.escalamiento.Enabled = True
        Me.escalamiento.Interval = 1000
        '
        'SerialPort1
        '
        '
        'NotifyIcon1
        '
        Me.NotifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info
        Me.NotifyIcon1.BalloonTipText = "Aplicación para monitorer fallas"
        Me.NotifyIcon1.BalloonTipTitle = "Cronos 2019"
        Me.NotifyIcon1.ContextMenuStrip = Me.ContextMenuStrip1
        Me.NotifyIcon1.Icon = CType(resources.GetObject("NotifyIcon1.Icon"), System.Drawing.Icon)
        Me.NotifyIcon1.Text = "Cronos::. Monitor de fallas"
        Me.NotifyIcon1.Visible = True
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Font = New System.Drawing.Font("Lucida Sans", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ContextMenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.VerElLogToolStripMenuItem, Me.DetenerElMonitorToolStripMenuItem, Me.ToolStripMenuItem1, Me.ReanudarElMonitorToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(230, 88)
        '
        'VerElLogToolStripMenuItem
        '
        Me.VerElLogToolStripMenuItem.Image = Global.vwMonitor.My.Resources.Resources.icons8_minutero_24
        Me.VerElLogToolStripMenuItem.Name = "VerElLogToolStripMenuItem"
        Me.VerElLogToolStripMenuItem.Size = New System.Drawing.Size(229, 26)
        Me.VerElLogToolStripMenuItem.Text = "Ver el log"
        '
        'DetenerElMonitorToolStripMenuItem
        '
        Me.DetenerElMonitorToolStripMenuItem.Name = "DetenerElMonitorToolStripMenuItem"
        Me.DetenerElMonitorToolStripMenuItem.Size = New System.Drawing.Size(226, 6)
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Image = Global.vwMonitor.My.Resources.Resources.icons8_pausa_24__1_
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        Me.ToolStripMenuItem1.Size = New System.Drawing.Size(229, 26)
        Me.ToolStripMenuItem1.Text = "Detener el monitor"
        '
        'ReanudarElMonitorToolStripMenuItem
        '
        Me.ReanudarElMonitorToolStripMenuItem.Enabled = False
        Me.ReanudarElMonitorToolStripMenuItem.Image = Global.vwMonitor.My.Resources.Resources.icons8_play_24
        Me.ReanudarElMonitorToolStripMenuItem.Name = "ReanudarElMonitorToolStripMenuItem"
        Me.ReanudarElMonitorToolStripMenuItem.Size = New System.Drawing.Size(229, 26)
        Me.ReanudarElMonitorToolStripMenuItem.Text = "Reanudar el monitor"
        '
        'ListBoxControl1
        '
        Me.ListBoxControl1.Items.AddRange(New Object() {"2019-Jun-14 23:00:15 Se generó un error", "2019-Jun-14 23:00:15 Se hizo la llamada"})
        Me.ListBoxControl1.Location = New System.Drawing.Point(12, 159)
        Me.ListBoxControl1.Name = "ListBoxControl1"
        Me.ListBoxControl1.Size = New System.Drawing.Size(580, 288)
        Me.ListBoxControl1.TabIndex = 31
        '
        'corte
        '
        Me.corte.Enabled = True
        Me.corte.Interval = 60000
        '
        'correos
        '
        Me.correos.Enabled = True
        Me.correos.Interval = 60000
        '
        'mensajes
        '
        Me.mensajes.Interval = 1000
        '
        'XtraForm1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 17.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1277, 589)
        Me.Controls.Add(Me.ListBoxControl1)
        Me.Controls.Add(Me.SimpleButton2)
        Me.Controls.Add(Me.SimpleButton3)
        Me.Controls.Add(Me.GroupControl1)
        Me.Controls.Add(Me.HyperlinkLabelControl1)
        Me.Controls.Add(Me.LabelControl2)
        Me.Controls.Add(Me.LabelControl1)
        Me.Controls.Add(Me.PictureEdit1)
        Me.Controls.Add(Me.TileBar1)
        Me.Controls.Add(Me.barDockControlLeft)
        Me.Controls.Add(Me.barDockControlRight)
        Me.Controls.Add(Me.barDockControlBottom)
        Me.Controls.Add(Me.barDockControlTop)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(610, 520)
        Me.Name = "XtraForm1"
        Me.Text = "Monitor de fallas"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        CType(Me.PictureEdit1.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.GroupControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupControl1.ResumeLayout(False)
        Me.GroupControl1.PerformLayout()
        CType(Me.ComboBoxEdit2.Properties, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.BarManager1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
        CType(Me.ListBoxControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PictureEdit1 As DevExpress.XtraEditors.PictureEdit
    Friend WithEvents TileBar1 As DevExpress.XtraBars.Navigation.TileBar
    Friend WithEvents Bar3 As DevExpress.XtraBars.Bar
    Friend WithEvents Bar4 As DevExpress.XtraBars.Bar
    Friend WithEvents LabelControl2 As DevExpress.XtraEditors.LabelControl
    Friend WithEvents LabelControl1 As DevExpress.XtraEditors.LabelControl
    Friend WithEvents HyperlinkLabelControl1 As DevExpress.XtraEditors.HyperlinkLabelControl
    Friend WithEvents GroupControl1 As DevExpress.XtraEditors.GroupControl
    Friend WithEvents SimpleButton1 As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents Bar1 As DevExpress.XtraBars.Bar
    Friend WithEvents BarManager1 As DevExpress.XtraBars.BarManager
    Friend WithEvents Bar6 As DevExpress.XtraBars.Bar
    Friend WithEvents SkinBarSubItem1 As DevExpress.XtraBars.SkinBarSubItem
    Friend WithEvents barDockControlTop As DevExpress.XtraBars.BarDockControl
    Friend WithEvents barDockControlBottom As DevExpress.XtraBars.BarDockControl
    Friend WithEvents barDockControlLeft As DevExpress.XtraBars.BarDockControl
    Friend WithEvents barDockControlRight As DevExpress.XtraBars.BarDockControl
    Friend WithEvents BarStaticItem1 As DevExpress.XtraBars.BarStaticItem
    Friend WithEvents BarStaticItem2 As DevExpress.XtraBars.BarStaticItem
    Friend WithEvents BarStaticItem3 As DevExpress.XtraBars.BarStaticItem
    Friend WithEvents BarStaticItem4 As DevExpress.XtraBars.BarStaticItem
    Friend WithEvents SimpleButton3 As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents SimpleButton2 As DevExpress.XtraEditors.SimpleButton
    Friend WithEvents ComboBoxEdit2 As DevExpress.XtraEditors.ComboBoxEdit
    Friend WithEvents LabelControl4 As DevExpress.XtraEditors.LabelControl
    Friend WithEvents revisaFlag As Timer
    Friend WithEvents SerialPort1 As IO.Ports.SerialPort
    Friend WithEvents escalamiento As Timer
    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents VerElLogToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DetenerElMonitorToolStripMenuItem As ToolStripSeparator
    Friend WithEvents ToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents ReanudarElMonitorToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ListBoxControl1 As DevExpress.XtraEditors.ListBoxControl
    Friend WithEvents corte As Timer
    Friend WithEvents correos As Timer
    Friend WithEvents mensajes As Timer
End Class
