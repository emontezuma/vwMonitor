Imports DevExpress.XtraEditors
Imports DevExpress.Skins
Imports MySql.Data.MySqlClient
Imports System.Speech.Synthesis
Imports System.IO.Ports
Imports System.IO
Imports System.Text
Imports System.Net.Mail
Imports System.Net.Http
Imports System.Net
Imports System.ComponentModel

Public Class XtraForm1
    Dim Estado As Integer = 0
    Dim procesandoInfoFallas As Boolean = False
    Dim procesandoAudios As Boolean = False
    Dim procesandoCorte As Boolean = False
    Dim eSegundos = 0
    Dim procesandoEscalamientos As Boolean
    Dim procesandoRepeticiones As Boolean
    Dim estadoPrograma As Boolean
    Dim MensajeLlamada = ""
    Dim procesandoMensajes As Boolean = False
    Dim procesandoCorreos As Boolean = False
    Dim errorPuerto As Boolean = False
    Dim errorCorreos As String

    Private Sub XtraForm1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then

            Application.Exit()
        End If
        estadoPrograma = True
        ListBoxControl1.Items.Clear()
        agregarLOG("Se incicia el programa", 1, 0)
        Try
            agregarLOG("Se inicia la aplicación de Envío de correos", 1, 0)
            Shell(Application.StartupPath & "\vbCorreos.exe", AppWinStyle.MinimizedNoFocus)
        Catch ex As Exception
            agregarLOG("Error en la ejecución de la aplicación de envío de correos. Error: " & ex.Message, 7, 0)
        End Try
        horaDesde = Now
        Dim cadSQL As String = "SELECT flag_agregar FROM sigma.vw_configuracion"
        Dim reader As DataSet = consultaSEL(cadSQL)
        Dim regsAfectados = 0
        If errorBD.Length > 0 Then
            BarManager1.Items(2).Visibility = DevExpress.XtraBars.BarItemVisibility.Always
            BarManager1.Items(1).Visibility = DevExpress.XtraBars.BarItemVisibility.Never
            agregarLOG("No se logró la conexión con MySQL. Error: " + errorBD, 9, 0)

        Else
            BarManager1.Items(2).Visibility = DevExpress.XtraBars.BarItemVisibility.Never
            BarManager1.Items(1).Visibility = DevExpress.XtraBars.BarItemVisibility.Always
            agregarLOG("Conexión satisfactoria a MySQL", 1, 0)
            iniciarPantalla()
        End If
        enviarCorte()
        depurar()
        enviarCorreos()
        'Se inicia todo
        ContarLOG()
    End Sub

    Sub iniciarPantalla()
        Dim regsAfectados As Integer = 0
        'Se escribe en la base de datos
        regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET ejecutando_desde = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "'")
        If errorBD.Length > 0 Then
            'Error en la base de datos
            agregarLOG("Ocurrió un error al intentar ejecutar una actualización en la base de datos de SIGMA. Error: " + errorBD, 9, 0)
        ElseIf regsAfectados = 0 Then
            regsAfectados = consultaACT("INSERT INTO vw_configuracion (ejecutando_desde, revisar_cada) VALUES ('" & Format(horaDesde, "yyyy/MM/dd HH:mm:ss") & "', 60)")
        End If
        BarManager1.Items(3).Caption = "Ejecutandose desde: " + Format(horaDesde, "ddd, dd-MMM-yyyy HH:mm:ss")
        calcularRevision()
    End Sub

    Private Sub XtraForm1_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        ListBoxControl1.Width = Me.Width - 30
        GroupControl1.Width = ListBoxControl1.Width
        ListBoxControl1.Height = Me.Height - 250
        SimpleButton3.Left = Me.Width - SimpleButton3.Width - 20
        SimpleButton2.Left = Me.Width - SimpleButton2.Width - 20

    End Sub

    Private Sub SimpleButton1_Click(sender As Object, e As EventArgs) Handles SimpleButton1.Click
        If XtraMessageBox.Show("El log actual se quitará de la pantalla definitivamente. ¿Desea continuar?", "Inicializar LOG en pantalla", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.No Then
            Dim totalRegs As Integer = ListBoxControl1.Items.Count
            ListBoxControl1.Items.Clear()
            ListBoxControl1.Items.Add(Format(Now, "dd-MMM-yyyy HH:mm:ss") & ": " + "Se inicializa el LOG a solicitud del usuario. Se eliminan " & totalRegs & " registro(s) del LOG acumulandose desde " & Format(horaDesde, "dd-MMM-yyyy HH:mm:ss"))
            horaDesde = Now
            ContarLOG()
        End If
    End Sub

    Private Sub SimpleButton3_Click(sender As Object, e As EventArgs) Handles SimpleButton3.Click
        autenticado = False
        Dim Forma As New XtraForm2
        Forma.Text = "Detener aplicación"
        Forma.ShowDialog()
        If autenticado Then
            If XtraMessageBox.Show("Esta acción detendrá el envío de alertas. ¿Desea detener el monitor de las fallas?", "Detener la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
                Estado = 1
                SimpleButton3.Visible = False
                SimpleButton2.Visible = True
                ContextMenuStrip1.Items(1).Enabled = False
                ContextMenuStrip1.Items(2).Enabled = True
                estadoPrograma = False
                agregarLOG("La interfaz ha sido detenida por el usuario: " & usuarioCerrar, 9, 0)
            End If
        End If
    End Sub

    Private Sub SimpleButton2_Click(sender As Object, e As EventArgs) Handles SimpleButton2.Click
        If XtraMessageBox.Show("Esta acción reanudará el envío de alertas. ¿Desea reanudar el monitoreo de las fallas?", "Reanudar la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
            Estado = 1
            SimpleButton3.Visible = True
            SimpleButton2.Visible = False
            ContextMenuStrip1.Items(1).Enabled = True
            ContextMenuStrip1.Items(2).Enabled = False
            enviarCorte()
            enviarCorreos()
            estadoPrograma = True
            agregarLOG("La interfaz ha sido reanudada por un usuario", 9, 0)
        End If
    End Sub

    Private Sub agregarLOG(cadena As String, tipo As Integer, reporte As Integer, Optional aplicacion As Integer = 1)
        'Se agrega a la base de datos
        'tipo 1: Info
        'tipo 2: Incongruencia en los datos (usuario)
        'tipo 8: Error crítico de Base de datos infofallas
        'tipo 9: Error crítico de Base de datos sigma
        Dim regsAfectados = consultaACT("INSERT INTO sigma.vw_log (aplicacion, tipo, reporte, texto) VALUES (0, " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
        If aplicacion = 10 Then
            regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET flag_monitor = 'S'")
        End If

    End Sub

    Private Sub ContarLOG()
        If ListBoxControl1.Items.Count > 2000 Then
            For i = ListBoxControl1.Items.Count - 1 To 2000 Step -1
                ListBoxControl1.Items.RemoveAt(i)
            Next
        End If
        BarManager1.Items(4).Caption = IIf(ListBoxControl1.Items.Count = 0, "Ningún registro en el LOG", IIf(ListBoxControl1.Items.Count = 1, "Un registro en el LOG", ListBoxControl1.Items.Count & " registros en el LOG"))
    End Sub

    Private Sub HyperlinkLabelControl1_Click(sender As Object, e As EventArgs) Handles HyperlinkLabelControl1.Click
        System.Diagnostics.Process.Start("www.mmcallmexico.mx")
    End Sub

    Private Sub ComboBoxEdit2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxEdit2.SelectedIndexChanged
        Dim MiFuente As Font = New System.Drawing.Font("Lucida Sans", 9, FontStyle.Regular)

        If ComboBoxEdit2.SelectedIndex = 0 Then

            ListBoxControl1.Font = MiFuente

        ElseIf ComboBoxEdit2.SelectedIndex = 1 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 6, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        ElseIf ComboBoxEdit2.SelectedIndex = 2 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 7, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente

        ElseIf ComboBoxEdit2.SelectedIndex = 3 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 11, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        ElseIf ComboBoxEdit2.SelectedIndex = 4 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 13, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        ElseIf ComboBoxEdit2.SelectedIndex = 5 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 15, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        End If
    End Sub

    Private Sub revisaFlag_Tick(sender As Object, e As EventArgs) Handles revisaFlag.Tick
        If procesandoInfoFallas Or Not estadoPrograma Then Exit Sub

        procesandoInfoFallas = True
        revisaFlag.Enabled = False
        Dim cadSQL As String = "SELECT timeout_fallas FROM sigma.vw_configuracion WHERE timeout_fallas > 0 OR flag_agregar = 'S'"
        Dim reader As DataSet = consultaSEL(cadSQL)
        Dim regsAfectados = 0
        If errorBD.Length > 0 Then
            agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
        Else
            If reader.Tables(0).Rows.Count > 0 Then
                Dim segundosEspera = 0
                If reader.Tables(0).Rows(0).Item("timeout_fallas") > 0 Then
                    segundosEspera = reader.Tables(0).Rows(0).Item("timeout_fallas")
                End If
                BarManager1.Items(1).Caption = "Conectado (revisando fallas...)"
                Dim cadAdicional = ""
                If segundosEspera > 0 Then
                    cadAdicional = " AND DATE_ADD(fecha, INTERVAL " & segundosEspera & " SECOND) <= NOW()"
                End If
                cadSQL = "SELECT idk, falla, codigo, estacion, descripcion FROM sigma.vw_fallascronos WHERE estado = 0 AND eliminada = 'N' " & cadAdicional & " ORDER BY idk"
                Dim lFallasDS As DataSet = consultaSEL(cadSQL)
                If errorBD.Length > 0 Then
                    agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + errorBD, 1, 8)
                Else
                    If lFallasDS.Tables(0).Rows.Count > 0 Then
                        Dim idAlerta = 0
                        Dim nAlerta As String = ""
                        Dim idAlertaMascara = 0

                        For Each lFallas In lFallasDS.Tables(0).Rows
                            Dim fallaID = lFallas!idk
                            Dim fallaCod = ValNull(lFallas!codigo, "A")
                            Dim fallaDes = Microsoft.VisualBasic.Strings.UCase(ValNull(lFallas!descripcion, "A"))
                            Dim fallaEst = ValNull(lFallas!estacion, "A")
                            agregarLOG("Se ha detectado una falla en la base de datos de infofallas. ID: " & fallaID & ", Código: " & fallaCod & ", Descripción de origen: " & fallaDes & ", Estación:  " & fallaEst, 7, 0, 10)
                            'Se valida la falla dentro de SIGMA para ver si califica o no
                            'Se busca si hay una mascara por prioridad
                            Dim alertaEsc As Boolean = True
                            cadSQL = "SELECT vw_alertas.id, vw_alertas.nombre, vw_alerta_fallas.id as mascara FROM sigma.vw_alertas INNER JOIN sigma.vw_alerta_fallas ON vw_alertas.id = vw_alerta_fallas.alerta WHERE vw_alerta_fallas.estatus = 'A' AND vw_alertas.estatus = 'A' AND (vw_alerta_fallas.estacion = '(Cualquiera)' OR vw_alerta_fallas.estacion = '" & fallaEst & "') AND ((vw_alerta_fallas.comparacion = 1 AND UCASE(vw_alerta_fallas.prefijo) = '" & fallaDes & "') OR (vw_alerta_fallas.comparacion = 2 AND UCASE(vw_alerta_fallas.prefijo) <> '" & fallaDes & "') OR (vw_alerta_fallas.comparacion = 3 AND '" & fallaDes & "' LIKE UCASE(CONCAT(vw_alerta_fallas.prefijo, '%'))) OR (vw_alerta_fallas.comparacion = 4 AND '" & fallaDes & "' NOT LIKE UCASE(CONCAT(vw_alerta_fallas.prefijo, '%'))) OR (vw_alerta_fallas.comparacion = 5 AND '" & fallaDes & "' LIKE UCASE(CONCAT('%', vw_alerta_fallas.prefijo, '%'))) OR (vw_alerta_fallas.comparacion = 6 AND '" & fallaDes & "' NOT LIKE UCASE(CONCAT('%', vw_alerta_fallas.prefijo, '%'))) OR (vw_alerta_fallas.comparacion = 7 AND '" & fallaDes & "' LIKE UCASE(CONCAT('%', vw_alerta_fallas.prefijo))) OR (vw_alerta_fallas.comparacion = 8 AND '" & fallaDes & "' NOT LIKE UCASE(CONCAT('%', vw_alerta_fallas.prefijo))) OR (vw_alerta_fallas.comparacion = 9 AND '" & fallaDes & "' > UCASE(vw_alerta_fallas.prefijo)) OR (vw_alerta_fallas.comparacion = 10 AND '" & fallaDes & "' >= UCASE(vw_alerta_fallas.prefijo)) OR (vw_alerta_fallas.comparacion = 11 AND '" & fallaDes & "' < UCASE(vw_alerta_fallas.prefijo)) OR (vw_alerta_fallas.comparacion = 12 AND '" & fallaDes & "' >= UCASE(vw_alerta_fallas.prefijo))) ORDER BY vw_alertas.prioridad, vw_alertas.modificacion DESC LIMIT 1"
                            Dim mascaras As DataSet = consultaSEL(cadSQL)
                            Dim totalAlarmas = 0
                            If Not mascaras.Tables(0).Rows.Count > 0 Then
                                'Se busca la alerta de escape
                                cadSQL = "SELECT vw_alertas.id, vw_alertas.nombre FROM sigma.vw_alertas WHERE vw_alertas.estatus = 'A' AND vw_alertas.tipo = 2 AND (vw_alertas.escape_estacion = '(Cualquiera)' OR vw_alertas.escape_estacion = '" & fallaEst & "') ORDER BY vw_alertas.prioridad, vw_alertas.modificacion DESC LIMIT 1"
                                Dim fallas_escape As DataSet = consultaSEL(cadSQL)
                                If fallas_escape.Tables(0).Rows.Count > 0 Then
                                    idAlerta = fallas_escape.Tables(0).Rows(0)!id
                                    nAlerta = ValNull(fallas_escape.Tables(0).Rows(0)!nombre, "A")
                                End If
                            Else
                                For Each mascara In mascaras.Tables(0).Rows
                                    totalAlarmas = totalAlarmas + 1
                                    If totalAlarmas = 1 Then
                                        idAlerta = mascara!id
                                        nAlerta = ValNull(mascara!nombre, "A")
                                        idAlertaMascara = mascara!mascara
                                    End If
                                Next
                                alertaEsc = False
                            End If
                            If idAlerta = 0 Then
                                agregarLOG("Se encontró una falla sin alerta asociada. Revise la sección de fallas en la alerta o cree una alerta de Escape. Código de falla: " & fallaCod, 2, 0)
                                regsAfectados = consultaACT("UPDATE sigma.vw_configuracion set ultima_falla = " & fallaID & ", ultima_revision = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "'")
                                regsAfectados = consultaACT("UPDATE infofallas.fallascronos SET estado = 2 WHERE idk = " & fallaID)
                            Else
                                If Not alertaEsc Then
                                    If totalAlarmas = 1 Then
                                        agregarLOG("Se encontró una alerta asociada con la falla. Alerta: " & idAlerta & "-" & nAlerta & " línea de la alerta (máscara): " & idAlertaMascara, 1, 0)
                                    Else
                                        agregarLOG("Se encontraron " & totalAlarmas & " alertas asociadas con la falla " & fallaCod, 1, 0)
                                    End If
                                Else
                                    agregarLOG("Se encontró una alerta ESCAPE asociada con esta falla. Alerta: " & idAlerta & "-" & nAlerta, 2, 0)
                                End If

                                'Revisar la lógica de la alerta
                                If idAlerta > 0 Then revisarAlerta(idAlerta, lFallas!idk)

                            End If
                        Next
                    End If
                End If
                BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"

                If errorBD.Length > 0 Then
                    'Error en la base de datos
                    agregarLOG("Ocurrió un error al intentar ejecutar una actualización en la base de datos de SIGMA. Error: " + errorBD, 9, 0)
                End If
            End If
        End If
        cadSQL = "SELECT * FROM sigma.vw_fallascronos WHERE eliminada = 'S' AND estado <> 9 LIMIT 1"
        Dim eliminadosDS As DataSet = consultaSEL(cadSQL)
        If errorBD.Length > 0 Then
            agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + errorBD, 9, 0)
        Else
            If eliminadosDS.Tables(0).Rows.Count > 0 Then

                regsAfectados = consultaACT("INSERT INTO sigma.vw_alarmas (alerta, accion, falla, codigo, nombre, nave, descripcion, estacion, prioridad, inicio, tecnologia, responsable) SELECT 0, 9, a.idk, a.codigo, a.falla, a.nave, a.descripcion, a.estacion, a.prioridad, a.fecha, a.tecnologia, IFNULL((SELECT responsable.NOMBRE FROM infofallas.responsable WHERE a.resp = responsable.TIPO_RESPONSABLE AND a.estacion = responsable.ESTACION LIMIT 1) , 'N/A') FROM sigma.vw_fallascronos a WHERE a.eliminada = 'S' AND a.estado = 0")

                regsAfectados = consultaACT("UPDATE sigma.vw_fallascronos SET estado = 3 WHERE eliminada = 'S' AND estado <> 9;")

                regsAfectados = consultaACT("UPDATE sigma.vw_alarmas INNER JOIN sigma.vw_fallascronos ON vw_alarmas.falla = vw_fallascronos.idk SET fin = vw_fallascronos.cierre, tiempo = TIME_TO_SEC(TIMEDIFF(vw_fallascronos.cierre, inicio)) WHERE vw_fallascronos.estado = 3;")
                If regsAfectados > 0 Then agregarLOG("Se " & IIf(regsAfectados = 1, "cerró una falla", "cerraron " & regsAfectados & " falla(s)"), 1, 0)
                regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET estado = 9, atendida = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), activada)) WHERE id NOT IN (SELECT reporte FROM sigma.vw_alarmas WHERE tiempo = 0) AND estado <> 9;")
                If regsAfectados > 0 Then
                    'Se crea el log masivo
                    regsAfectados = consultaACT("INSERT INTO sigma.vw_log (aplicacion, tipo, reporte, texto) SELECT 0, 1, 0, CONCAT('Se cerró el reporte ', id, ' y su(s) falla(s) asociada(s)') FROM sigma.vw_reportes WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N';INSERT INTO sigma.vw_mensajes (alerta, tipo, canal, destino, mensaje) SELECT alerta, (80 + tipo), canal, destino, CONCAT('ATENCION El reporte número ', alerta, ' ha sido atendido!') FROM sigma.vw_mensajes WHERE tipo <= 5 AND canal <> 4 AND alerta IN (SELECT id FROM sigma.vw_reportes WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N') GROUP BY alerta, (80 + tipo), canal, destino, CONCAT('ATENCION El reporte número ', alerta, ' ha sido atendido!');UPDATE sigma.vw_reportes SET informado = 'S' WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N'")
                    'Se informa a los involucrados

                    'cadSQL = "SELECT id FROM sigma.vw_reportes WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N'"
                    'Dim alertaDS As DataSet = consultaSEL(cadSQL)
                    'If alertaDS.Tables(0).Rows.Count > 0 Then
                    'For Each alerta In alertaDS.Tables(0).Rows
                    'agregarLOG("Se cerró el reporte " & alerta!id & " y su(s) falla(s) asociada(s)", 1, 0, 10)
                    'regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, (80 + tipo), canal, prioridad, destino, CONCAT('ATENCION El reporte número ', " & alerta!id & ", ' ha sido atendido!') FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 5 and canal <> 4")
                    'regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET informado = 'S' WHERE id = " & alerta!id)

                    'Next
                    'End If
                End If
                regsAfectados = consultaACT("UPDATE sigma.vw_fallascronos SET estado = 9 WHERE estado = 3")
            End If

        End If
        cadSQL = "SELECT id, texto, aplicacion FROM sigma.vw_log WHERE visto_pc = 'N' ORDER BY id"
        reader = consultaSEL(cadSQL)
        regsAfectados = 0
        If reader.Tables(0).Rows.Count > 0 Then
            For Each elmensaje In reader.Tables(0).Rows
                Dim aplicacion = elmensaje!aplicacion
                Dim cadAplicacion = IIf(aplicacion = 0, "MONITOR: ", IIf(aplicacion = 10, "LLAMADAS: ", IIf(aplicacion = 20, "CORREOS: ", IIf(aplicacion = 40, "REPORTES: ", "CORTE: "))))
                ListBoxControl1.Items.Insert(0, cadAplicacion & Format(Now, "dd-MMM HH:mm:ss") & ": " & elmensaje!texto)
                regsAfectados = consultaACT("UPDATE sigma.vw_log SET visto_pc = 'S' WHERE id = " & elmensaje!id)
            Next
            ContarLOG()
        End If
        procesandoInfoFallas = False
        revisaFlag.Enabled = True

    End Sub

    Private Sub revisarAlerta(idAlerta As Integer, idFalla As Integer)
        Dim regsAfectados = 0
        Dim cadSQL = "SELECT vw_fallascronos.*, IFNULL(responsable.NOMBRE, 'N/A') AS nombre FROM sigma.vw_fallascronos LEFT JOIN infofallas.responsable ON vw_fallascronos.resp = responsable.TIPO_RESPONSABLE AND vw_fallascronos.estacion = responsable.ESTACION WHERE idk = " & idFalla & " AND eliminada = 'N' LIMIT 1"
        Dim falla As DataSet = consultaSEL(cadSQL)
        If falla.Tables(0).Rows.Count > 0 Then
            Dim tecnologia = ValNull(falla.Tables(0).Rows(0)!tecnologia, "A")
            'Dim prioridad = ValNull(prioridadridad, "A")
            Dim descripcion = ValNull(falla.Tables(0).Rows(0)!falla, "A")
            Dim descripcion2 = ValNull(falla.Tables(0).Rows(0)!descripcion, "A")
            Dim responsable = ValNull(falla.Tables(0).Rows(0)!NOMBRE, "A")
            Dim codigo = ValNull(falla.Tables(0).Rows(0)!codigo, "A")
            Dim nave = ValNull(falla.Tables(0).Rows(0)!nave, "A")
            Dim estacion = ValNull(falla.Tables(0).Rows(0)!estacion, "A")
            Dim fechaFalla = Format(falla.Tables(0).Rows(0)!fecha, "yyyy/MM/dd HH:mm:ss")
            Dim prioridad = "0"
            Dim veces = 0
            cadSQL = "SELECT * FROM sigma.vw_alertas WHERE vw_alertas.id = " & idAlerta

            Dim alerta As DataSet = consultaSEL(cadSQL)
            Dim uID = 0

            If alerta.Tables(0).Rows.Count > 0 Then
                Dim fechaDesde
                Dim crearReporte As Boolean = False


                Dim porAcumulacion = False
                If ValNull(alerta.Tables(0).Rows(0)!acumular, "A") = "N" Then
                    'Se pregunta si hay un rperte activo y si es solapable
                    If ValNull(alerta.Tables(0).Rows(0)!solapar, "A") = "S" Then
                        crearReporte = True
                    Else
                        cadSQL = "SELECT * FROM sigma.vw_reportes WHERE vw_reportes.alerta = " & idAlerta & " AND estado <> 9 "
                        Dim solapar As DataSet = consultaSEL(cadSQL)
                        crearReporte = Not solapar.Tables(0).Rows.Count > 0
                    End If

                Else
                    porAcumulacion = True
                    If ValNull(alerta.Tables(0).Rows(0)!acumular_inicializar, "A") = "S" Then
                        If alerta.Tables(0).Rows(0)!acumular_tiempo > 0 Then
                            fechaDesde = DateAdd(DateInterval.Second, alerta.Tables(0).Rows(0)!acumular_tiempo * -1, Now)
                            cadSQL = "SELECT COUNT(*) as cuenta FROM sigma.vw_alarmas WHERE alerta = " & idAlerta & " AND reporte = 0 AND accion = 0 AND inicio >= '" & Format(fechaDesde, "yyyy/MM/dd HH:mm:ss") & "'"
                        Else
                            cadSQL = "SELECT COUNT(*) as cuenta FROM sigma.vw_alarmas WHERE alerta = " & idAlerta & " AND reporte = 0 AND accion = 0 "
                        End If
                    Else
                        If alerta.Tables(0).Rows(0)!acumular_tiempo > 0 Then
                            fechaDesde = DateAdd(DateInterval.Second, alerta.Tables(0).Rows(0)!acumular_tiempo * -1, Now)
                            cadSQL = "SELECT COUNT(*) as cuenta FROM sigma.vw_alarmas WHERE alerta = " & idAlerta & " AND inicio >= '" & Format(fechaDesde, "yyyy/MM/dd HH:mm:ss") & "'"
                        Else
                            cadSQL = "SELECT COUNT(*) as cuenta FROM sigma.vw_alarmas WHERE alerta = " & idAlerta
                        End If
                    End If

                    Dim acumulado = 0
                    Dim acum As DataSet = consultaSEL(cadSQL)
                    If acum.Tables(0).Rows.Count > 0 Then
                        acumulado = acum.Tables(0).Rows(0)!cuenta
                    End If
                    If acumulado + 1 >= alerta.Tables(0).Rows(0)!acumular_veces Then
                        If ValNull(alerta.Tables(0).Rows(0)!solapar, "A") = "S" Then
                            crearReporte = True
                        Else
                            cadSQL = "SELECT * FROM sigma.vw_reportes WHERE vw_reportes.alerta = " & idAlerta & " AND estado <> 9 "
                            Dim solapar As DataSet = consultaSEL(cadSQL)
                            crearReporte = Not solapar.Tables(0).Rows.Count > 0
                        End If

                    End If
                End If
                If crearReporte Then
                    regsAfectados = consultaACT("INSERT INTO sigma.vw_reportes (alerta, informar_resolucion, log, sms, correo, llamada, mmcall, lista, escalar1, tiempo1, lista1, log1, sms1, correo1, llamada1, mmcall1, repetir1, escalar2, tiempo2, lista2, log2, sms2, correo2, llamada2, mmcall2, repetir2, escalar3, tiempo3, lista3, log3, sms3, correo3, llamada3, mmcall3, repetir3, escalar4, tiempo4, lista4, log4, sms4, correo4, llamada4, mmcall4, repetir4, escalar5, tiempo5, lista5, log5, sms5, correo5, llamada5, mmcall5, repetir5, repetir, repetir_tiempo, repetir_log, repetir_sms, repetir_correo, repetir_llamada, repetir_mmcall, estado) SELECT id, informar_resolucion, log, sms, correo, llamada, mmcall, lista, escalar1, tiempo1, lista1, log1, sms1, correo1, llamada1, mmcall1, repetir1, escalar2, tiempo2, lista2, log2, sms2, correo2, llamada2, mmcall2, repetir2, escalar3, tiempo3, lista3, log3, sms3, correo3, llamada3, mmcall3, repetir3, escalar4, tiempo4, lista4, log4, sms4, correo4, llamada4, mmcall4, repetir4, escalar5, tiempo5, lista5, log5, sms5, correo5, llamada5, mmcall5, repetir5, repetir, repetir_tiempo, repetir_log, repetir_sms, repetir_correo, repetir_llamada, repetir_mmcall, 1 FROM sigma.vw_alertas WHERE id = " & alerta.Tables(0).Rows(0)!id)
                    'Se obtieneel último ID
                    cadSQL = "SELECT MAX(id) as ultimo FROM sigma.vw_reportes"
                    Dim ultimo As DataSet = consultaSEL(cadSQL)
                    If ultimo.Tables(0).Rows.Count > 0 Then
                        uID = ultimo.Tables(0).Rows(0)!ultimo
                    End If

                    If porAcumulacion Then
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_alarmas (falla, codigo, nombre, nave, descripcion, estacion, prioridad, inicio, tecnologia, responsable, alerta, reporte, accion) VALUES (" & idFalla & ", '" & codigo & "', '" & descripcion & "', '" & nave & "', '" & descripcion2 & "', '" & estacion & "', '" & prioridad & "', '" & fechaFalla & "', '" & tecnologia & "', '" & responsable & "', " & idAlerta & ", " & uID & ", 1)")
                        If alerta.Tables(0).Rows(0)!acumular_tiempo > 0 Then
                            regsAfectados = consultaACT("UPDATE sigma.vw_alarmas SET reporte = " & uID & " WHERE alerta = " & idAlerta & " And reporte = 0 And inicio >= '" & Format(fechaDesde, "yyyy/MM/dd HH:mm:ss") & "' AND accion = 0")
                        Else
                            regsAfectados = consultaACT("UPDATE sigma.vw_alarmas SET reporte = " & uID & " WHERE alerta = " & idAlerta & " And reporte = 0 AND accion = 0")
                        End If
                    Else
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_alarmas (falla, codigo, nombre, nave, descripcion, estacion, prioridad, inicio, tecnologia, responsable, alerta, reporte, accion) VALUES (" & idFalla & ", '" & codigo & "', '" & descripcion & "', '" & nave & "', '" & descripcion2 & "', '" & estacion & "', '" & prioridad & "', '" & fechaFalla & "', '" & tecnologia & "', '" & responsable & "', " & idAlerta & ", " & uID & ", 1)")
                    End If
                Else
                    'Se crear la alarma suelta
                    regsAfectados = consultaACT("INSERT INTO sigma.vw_alarmas (falla, codigo, nombre, nave, descripcion, estacion, prioridad, inicio, tecnologia, responsable, alerta, accion) VALUES (" & idFalla & ", '" & codigo & "', '" & descripcion & "', '" & nave & "', '" & descripcion2 & "', '" & estacion & "', '" & prioridad & "', '" & fechaFalla & "', '" & tecnologia & "', '" & responsable & "', " & idAlerta & ", " & IIf(porAcumulacion, 0, 4) & ")")

                End If

                If crearReporte Then
                    'Se generan los mensajes a enviar
                    Dim mensajeMMCall As String = Microsoft.VisualBasic.Strings.Left(ValNull(descripcion, "A"), 40).Trim
                    Dim mensaje As String = Microsoft.VisualBasic.Strings.Left(ValNull(descripcion2, "A"), 200).Trim

                    If ValNull(alerta.Tables(0).Rows(0)!acumular, "A") = "S" Then
                        If ValNull(alerta.Tables(0).Rows(0)!acumular_tipo_mensaje, "A") = "T" Then
                            mensaje = "Hay " & veces & " falla.tables(0).rows(0)(s) acumulada(s) por atender"
                            mensajeMMCall = "Hay " & veces & " falla.tables(0).rows(0)(s) por atender"
                        ElseIf ValNull(alerta.Tables(0).Rows(0)!acumular_tipo_mensaje, "A") = "P" Then
                            mensaje = Microsoft.VisualBasic.Strings.Left(ValNull(alerta.Tables(0).Rows(0)!acumular_mensaje, "A"), 200)
                            mensajeMMCall = Microsoft.VisualBasic.Strings.Left(ValNull(alerta.Tables(0).Rows(0)!acumular_mensaje, "A"), 40)
                        End If
                    End If
                    'Se cambian los caracteres especiales
                    mensajeMMCall = UCase(mensajeMMCall)
                    mensajeMMCall = "EST " & estacion & " " & mensajeMMCall
                    mensajeMMCall = Replace(mensajeMMCall, "Á", "A")
                    mensajeMMCall = Replace(mensajeMMCall, "É", "E")
                    mensajeMMCall = Replace(mensajeMMCall, "Í", "I")
                    mensajeMMCall = Replace(mensajeMMCall, "Ó", "O")
                    mensajeMMCall = Replace(mensajeMMCall, "Ú", "U")
                    mensajeMMCall = Replace(mensajeMMCall, "Ñ", "~")
                    mensajeMMCall = Replace(mensajeMMCall, ":", " ")

                    mensaje = UCase(mensaje)
                    mensaje = "EST " & estacion & " " & mensaje
                    mensaje = Replace(mensaje, "Á", "A")
                    mensaje = Replace(mensaje, "É", "E")
                    mensaje = Replace(mensaje, "Í", "I")
                    mensaje = Replace(mensaje, "Ó", "O")
                    mensaje = Replace(mensaje, "Ú", "U")
                    mensaje = Replace(mensaje, "Ñ", "~")

                    If mensaje.Length = 0 Then
                        mensaje = "EST " & estacion & "Hay fallas por atender"
                        agregarLOG("La alerta" & idAlerta & " no tiene un mensaje definido se tomó el mensaje por defecto", 1, 2)
                    End If
                    If mensajeMMCall.Length = 0 Then
                        mensajeMMCall = "EST " & estacion & "Hay fallas por atender"
                        agregarLOG("La alerta" & idAlerta & " no tiene un mensaje para MMCall definido se tomó el mensaje por defecto", 1, 2)
                    End If
                    If ValNull(alerta.Tables(0).Rows(0)!log, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_log (aplicacion, tipo, alerta, texto) VALUES (10, 1, " & idAlerta & ", '" & mensaje & "')")
                    End If

                    If ValNull(alerta.Tables(0).Rows(0)!llamada, "A") = "S" Then agregarMensaje("telefonos", alerta.Tables(0).Rows(0)!lista, uID, 0, 1, prioridad, mensaje)
                    If ValNull(alerta.Tables(0).Rows(0)!sms, "A") = "S" Then agregarMensaje("telefonos", alerta.Tables(0).Rows(0)!lista, uID, 0, 2, prioridad, mensaje)
                    If ValNull(alerta.Tables(0).Rows(0)!correo, "A") = "S" Then agregarMensaje("correos", alerta.Tables(0).Rows(0)!lista, uID, 0, 3, prioridad, mensaje)
                    If ValNull(alerta.Tables(0).Rows(0)!mmcall, "A") = "S" Then agregarMensaje("mmcall", alerta.Tables(0).Rows(0)!lista, uID, 0, 4, prioridad, mensajeMMCall)
                    agregarLOG("Se ha creado el reporte: " & uID & IIf(porAcumulacion, " por acumulación de fallas", ""), 1, uID)
                End If

                regsAfectados = consultaACT("UPDATE sigma.vw_fallascronos SET estado = 1 WHERE idk = " & idFalla)
            End If
        End If
    End Sub

    Private Sub calcularRevision()
        Dim cadSQL As String = "SELECT revisar_cada FROM sigma.vw_configuracion"
        Dim reader As DataSet = consultaSEL(cadSQL)
        If errorBD.Length > 0 Then
            agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + errorBD, 9, 0)
        Else
            If reader.Tables(0).Rows.Count > 0 Then
                If ValNull(reader.Tables(0).Rows(0)!revisar_cada, "N") = 0 Then
                    eSegundos = 60
                    Dim regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET revisar_cada = 60")
                    mensajes.Interval = 1000
                    mensajes.Enabled = False
                    mensajes.Enabled = True
                Else
                    eSegundos = ValNull(reader.Tables(0).Rows(0)!revisar_cada, "N")
                    If mensajes.Interval <> eSegundos * 1000 Then
                        mensajes.Interval = eSegundos * 1000
                        mensajes.Enabled = False
                        mensajes.Enabled = True
                    End If
                End If

            End If
        End If
        BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"
    End Sub

    Private Sub procesarMensajes()
        Dim regsAfectados = 0
        BarManager1.Items(1).Caption = "Conectado (procesando mensajes...)"
        agregarSolo("Revisando mensajes a enviar")
        'Escalada 4
        Dim miError As String = ""
        Dim optimizar_llamada As Boolean = False
        Dim optimizar_sms As Boolean = False
        Dim optimizar_correo As Boolean = False
        Dim optimizar_mmcall As Boolean = False
        Dim mantenerPrioridad As Boolean = False
        Dim rutaSMS
        Dim correo_titulo_falla As Boolean
        Dim correo_titulo As String
        Dim correo_cuerpo As String
        Dim correo_firma As String
        Dim correo_cuenta As String
        Dim correo_puerto As String
        Dim correo_ssl As Boolean
        Dim correo_clave As String
        Dim correo_host As String
        Dim voz_audio As String
        Dim mensajeGenerado As Boolean = False
        Dim escape_mmcall As Boolean = False
        Dim escape_mmcall_mensaje As String = ""
        Dim escape_mmcall_lista = 0
        Dim escape_mmcall_cancelar As Boolean = True
        Dim utilizar_arduino As Boolean = True
        Dim traducir As Boolean = False
        Dim server_mmcall As String = ""

        Dim rutaAudios
        Dim cadSQL As String = "SELECT * FROM sigma.vw_configuracion"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            optimizar_llamada = ValNull(reader!optimizar_llamada, "A") = "S"
            optimizar_sms = ValNull(reader!optimizar_sms, "A") = "S"
            optimizar_correo = ValNull(reader!optimizar_correo, "A") = "S"
            optimizar_mmcall = ValNull(reader!optimizar_mmcall, "A") = "S"
            mantenerPrioridad = ValNull(reader!optimizar_mmcall, "A") = "S"
            rutaSMS = ValNull(reader!ruta_sms, "A")
            rutaAudios = ValNull(reader!ruta_audios, "A")
            correo_titulo_falla = ValNull(reader!correo_titulo_falla, "A") = "S"
            correo_titulo = ValNull(reader!correo_titulo, "A")
            correo_cuerpo = ValNull(reader!correo_cuerpo, "A")
            correo_firma = ValNull(reader!correo_firma, "A")
            correo_cuenta = ValNull(reader!correo_cuenta, "A")
            correo_clave = ValNull(reader!correo_clave, "A")
            correo_puerto = ValNull(reader!correo_puerto, "A")
            correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
            correo_host = ValNull(reader!correo_host, "A")
            server_mmcall = ValNull(reader!server_mmcall, "A")
            voz_audio = ValNull(reader!voz_predeterminada, "A")
            escape_mmcall = ValNull(reader!escape_mmcall, "A") = "S"
            traducir = ValNull(reader!traducir, "A") = "S"
            escape_mmcall_cancelar = ValNull(reader!escape_mmcall_cancelar, "A") = "S"
            escape_mmcall_mensaje = ValNull(reader!escape_mmcall_mensaje, "A")
            escape_mmcall_lista = ValNull(reader!escape_mmcall_lista, "A")
            utilizar_arduino = ValNull(reader!utilizar_arduino, "A") = "S"
        End If
        If escape_mmcall_mensaje.Length = 0 Then escape_mmcall_mensaje = "TODOS LOS REQUESTERS DE MMCALL OCUPADOS..."

        If Not estadoPrograma Then
            Exit Sub
        End If
        'Llamadas telefónicas
        If Not My.Computer.FileSystem.DirectoryExists(rutaAudios) Then
            rutaAudios = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        End If
        If Not optimizar_llamada Then
            cadSQL = "SELECT *, 1 as cuenta  FROM sigma.vw_mensajes WHERE canal = 1 AND estatus = 'A' ORDER BY prioridad DESC"
        ElseIf mantenerPrioridad Then
            cadSQL = "SELECT prioridad, canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 1 AND estatus = 'A' GROUP BY prioridad, canal, destino ORDER BY prioridad DESC"
        Else
            cadSQL = "SELECT canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 1 AND estatus = 'A' GROUP BY canal, destino ORDER BY prioridad DESC"
        End If
        'Se preselecciona la voz
        Dim indice = 0

        Dim mensajesDS As DataSet = consultaSEL(cadSQL)
        Dim eMensaje = ""
        Dim audiosGen = 0
        Dim audiosNGen = 0
        Dim mTotal = 0

        If mensajesDS.Tables(0).Rows.Count > 0 Then
            Dim indiceVoz = 0
            Dim primeraVoz As String
            Dim synthesizer As New SpeechSynthesizer()
            For Each voice In synthesizer.GetInstalledVoices
                indiceVoz = indiceVoz + 1
                Dim info As VoiceInfo
                info = voice.VoiceInfo
                If voz_audio = info.Name Then
                    indiceVoz = -1
                    Exit For
                End If
                If indiceVoz = 1 Then primeraVoz = info.Name
            Next
            If indiceVoz > 0 Then
                agregarLOG("La voz especificada en el archivo de configuración NO esta registrada en el sistema, se tomará la voz por defecto del PC", 1, 0)
                voz_audio = primeraVoz
            ElseIf indiceVoz = 0 Then
                agregarLOG("No se generaron audios para llamadas porque no se encontró alguna voz para reproducir audios en la PC. Por favor revise e intente de nuevo", 1, 0)
            End If
            If indiceVoz <> 0 Then
                indice = 0
                For Each elmensaje In mensajesDS.Tables(0).Rows
                    indice = indice + 1
                    If optimizar_llamada Then
                        If elmensaje!cuenta = 1 Then
                            Dim fPrioridad = ""
                            If mantenerPrioridad Then
                                fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                            End If
                            'Doble cic en el mensaje
                            cadSQL = "SELECT mensaje FROM sigma.vw_mensajes WHERE canal = 1 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad & " "
                            Dim dbMensajes As DataSet = consultaSEL(cadSQL)
                            If dbMensajes.Tables(0).Rows.Count > 0 Then
                                eMensaje = ValNull(dbMensajes.Tables(0).Rows(0)!mensaje, "A")
                            End If
                        Else
                            eMensaje = "USTED TIENE " & elmensaje!cuenta & " mensaje(s) POR ATENDER"
                        End If
                    Else
                        eMensaje = ValNull(elmensaje!mensaje, "A")
                    End If
                    mTotal = mTotal + elmensaje!cuenta
                    'Se crea el audio
                    If eMensaje.Length > 0 Then
                        mensajeGenerado = False
                        Try
                            Dim synthesizer0 As New SpeechSynthesizer()
                            synthesizer0.SetOutputToWaveFile(rutaAudios & "\" & elmensaje!destino & Format(Now, "hhmmss") & indice & "_1.wav")
                            synthesizer0.SelectVoice(voz_audio)
                            synthesizer0.Volume = 100 '  // 0...100
                            synthesizer0.Rate = 0 '     // -10...10
                            Dim builder2 As New PromptBuilder()
                            If traducir Then eMensaje = traducirMensaje(eMensaje)
                            builder2.AppendText(eMensaje)
                            builder2.Culture = synthesizer0.Voice.Culture
                            synthesizer0.Speak(builder2)
                            synthesizer0.SetOutputToDefaultAudioDevice()
                            mensajeGenerado = True
                            audiosGen = audiosGen + 1
                        Catch ex As Exception
                            miError = ex.Message
                            audiosNGen = audiosNGen + 1
                        End Try
                    Else
                        mensajeGenerado = True
                    End If
                    If mensajeGenerado Then
                        If optimizar_llamada Then
                            Dim fPrioridad = ""
                            If mantenerPrioridad Then
                                fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                            End If
                            'Doble cic en el mensaje
                            regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE canal = 1 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad)
                        Else
                            regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE id = " & elmensaje!id)
                        End If
                    End If
                Next
                If audiosGen > 0 Then
                    agregarLOG("Se generaron " & audiosGen & " audio(s) para llamada de voz (" & mTotal & " notifación(es))" & IIf(audiosNGen > 0, " No se generaron " & audiosNGen & " audio(s) ", ""), 1, 0)
                Else
                    If audiosNGen > 0 Then
                        agregarLOG("Errores en la conversión de audios. No se generaron " & audiosNGen & " audio(s) para llamada por voz. Error: " & miError, 1, 0)
                    End If
                End If
            End If
        End If

        If Not estadoPrograma Then
            Exit Sub
        End If
        ''
        miError = ""
        'elmensaje de texto
        If Not My.Computer.FileSystem.DirectoryExists(rutaSMS) Then
            rutaSMS = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        End If
        If Not optimizar_sms Then
            cadSQL = "SELECT *, 1 as cuenta  FROM sigma.vw_mensajes WHERE canal = 2 AND estatus = 'A' ORDER BY prioridad DESC"
        ElseIf mantenerPrioridad Then
            cadSQL = "SELECT prioridad, canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 2 AND estatus = 'A' GROUP BY prioridad, canal, destino ORDER BY prioridad DESC"
        Else
            cadSQL = "SELECT canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 2 AND estatus = 'A' GROUP BY canal, destino ORDER BY prioridad DESC"
        End If
        'Se preselecciona la voz
        mensajesDS = consultaSEL(cadSQL)
        eMensaje = ""
        audiosGen = 0
        audiosNGen = 0
        mTotal = 0
        indice = 0

        If mensajesDS.Tables(0).Rows.Count > 0 Then
            For Each elmensaje In mensajesDS.Tables(0).Rows
                indice = indice + 1
                If optimizar_sms Then
                    If elmensaje!cuenta = 1 Then
                        Dim fPrioridad = ""
                        If mantenerPrioridad Then
                            fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                        End If
                        'Doble cic en el mensaje
                        cadSQL = "SELECT mensaje FROM sigma.vw_mensajes WHERE canal = 2 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad & " "
                        Dim dbMensajes As DataSet = consultaSEL(cadSQL)
                        If dbMensajes.Tables(0).Rows.Count > 0 Then

                            eMensaje = ValNull(dbMensajes.Tables(0).Rows(0)!mensaje, "A")
                        End If

                    Else
                        eMensaje = "USTED TIENE " & elmensaje!cuenta & " mensaje(s) POR ATENDER"
                    End If
                Else
                    eMensaje = ValNull(elmensaje!mensaje, "A")
                End If
                mTotal = mTotal + elmensaje!cuenta
                'Se crea el audio
                mensajeGenerado = False
                If eMensaje.Length > 0 Then
                    Try
                        System.IO.File.Create(rutaSMS & "\" & elmensaje!destino & Format(Now, "hhmmss") & indice & ".txt").Dispose()
                        Dim objWriter As New System.IO.StreamWriter(rutaSMS & "\" & elmensaje!destino & Format(Now, "hhmmss") & indice & ".txt", True)
                        objWriter.WriteLine(eMensaje)
                        objWriter.Close()
                        audiosGen = audiosGen + 1
                        mensajeGenerado = True
                    Catch ex As Exception
                        audiosNGen = audiosNGen + 1
                        miError = ex.Message
                    End Try
                Else
                    mensajeGenerado = True
                End If
                If mensajeGenerado Then
                    If optimizar_sms Then
                        Dim fPrioridad = ""
                        If mantenerPrioridad Then
                            fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                        End If
                        'Doble cic en el mensaje
                        regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE canal = 2 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad)
                    Else
                        regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE id = " & elmensaje!id)
                    End If
                End If
            Next
            If audiosGen > 0 Then
                agregarLOG("Se generaron " & audiosGen & " mensaje(s) de texto (" & mTotal & " notifación(es))" & IIf(audiosNGen > 0, " No se generaron " & audiosNGen & " audio(s) ", ""), 1, 0)
            Else
                If audiosNGen > 0 Then
                    agregarLOG("Errores en la generación de mensaje(s) de texto. No se generaron " & audiosNGen & " mensaje(s) de texto para llamada por voz. Error: " & miError, 1, 0)
                End If
            End If
        End If


        If Not estadoPrograma Then
            Exit Sub
        End If


        Try
            agregarSolo("Se inicia la aplicación de Envío de correos")
            Shell(Application.StartupPath & "\vbCorreos.exe", AppWinStyle.MinimizedNoFocus)
        Catch ex As Exception
            agregarLOG("Error en la ejecución de la aplicación de envío de correos. Error: " & ex.Message, 7, 0)
        End Try

        'Se copia el codigo en el sub EC

        If Not estadoPrograma Then
            Exit Sub
        End If


        If Not optimizar_mmcall Then
            cadSQL = "SELECT *, 1 as cuenta  FROM sigma.vw_mensajes WHERE canal = 4 AND estatus = 'A' ORDER BY prioridad DESC"
        ElseIf mantenerPrioridad Then
            cadSQL = "SELECT prioridad, canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 4 AND estatus = 'A' GROUP BY prioridad, canal, destino ORDER BY prioridad DESC"
        Else
            cadSQL = "SELECT canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 4 AND estatus = 'A' GROUP BY canal, destino ORDER BY prioridad DESC"
        End If
        'Se preselecciona la voz
        mensajesDS = consultaSEL(cadSQL)
        eMensaje = ""
        audiosGen = 0
        audiosNGen = 0
        mTotal = 0

        If mensajesDS.Tables(0).Rows.Count > 0 Then
            For Each elmensaje In mensajesDS.Tables(0).Rows
                Dim tituloMensaje = "Monitor VW"
                If optimizar_mmcall Then
                    If elmensaje!cuenta = 1 Then
                        Dim fPrioridad = ""
                        If mantenerPrioridad Then
                            fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                        End If
                        'Doble cic en el mensaje


                        cadSQL = "SELECT mensaje FROM sigma.vw_mensajes WHERE canal = 4 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad & " "
                        Dim dbMensajes As DataSet = consultaSEL(cadSQL)
                        If dbMensajes.Tables(0).Rows.Count > 0 Then

                            eMensaje = ValNull(dbMensajes.Tables(0).Rows(0)!mensaje, "A")
                        End If

                    Else
                        eMensaje = "USTED TIENE " & elmensaje!cuenta & " mensaje(s) POR ATENDER"
                    End If
                Else
                    eMensaje = ValNull(elmensaje!mensaje, "A")
                End If
                mTotal = mTotal + elmensaje!cuenta
                'Se crea el audio
                Dim cadena = ""

                mensajeGenerado = False
                Dim escapado As Boolean = False
                If eMensaje.Length > 0 Then
                    'Se busca un requester activo para la división
                    'Se busca la división en MMCall

                    'SE CAMBIA POR EL ENVIO DE MENSAJES
                    'cadSQL = "SELECT a.CODE FROM mmcall.requesters AS a WHERE (a.NAME LIKE '" & elmensaje!destino & "%' OR NAME = '" & elmensaje!destino & "') AND (SELECT COUNT(*) FROM mmcall.records WHERE records.requester = a.CODE AND ISNULL(end_time)) = 0 order by a.CODE LIMIT 1 "
                    'Se preselecciona la voz
                    'Dim requesters = consultaSEL(cadSQL)
                    'Dim elRequester = ""
                    'If requesters.Tables(0).Rows.Count > 0 Then
                    ' elRequester = requesters.Tables(0).Rows(0)!code
                    ' mensajeGenerado = False
                    Dim cadAdicional As String = "/locations/integration/page/number="
                    Dim mDestino = elmensaje!destino

                    If Microsoft.VisualBasic.Strings.Left(elmensaje!destino, 1) = "D" Then
                        cadAdicional = "/locations/integration/group_message/division="
                        mDestino = Microsoft.VisualBasic.Strings.Mid(elmensaje!destino, 2)
                    End If
                    Try
                        'Se intenta enviar al beeper indicado
                        'MsgBox("Se enviará este mensaje: " & server_mmcall & cadAdicional & mDestino & "&message=" & eMensaje)
                        agregarLOG("Se consume servicio de MMCall: " & server_mmcall & cadAdicional & mDestino & "&message=" & eMensaje, 1, 0)
                        Dim fr As System.Net.HttpWebRequest
                        Dim targetURI As New Uri(server_mmcall & cadAdicional & mDestino & "&message=" & eMensaje)
                        fr = DirectCast(HttpWebRequest.Create(targetURI), System.Net.HttpWebRequest)
                        If (fr.GetResponse().ContentLength > 0) Then
                            Dim str As New System.IO.StreamReader(fr.GetResponse().GetResponseStream())
                            cadena = str.ReadToEnd
                            str.Close()
                        End If
                        mensajeGenerado = cadena = "success"
                        If mensajeGenerado Then audiosGen = audiosGen + 1

                    Catch ex As System.Net.WebException
                        audiosNGen = audiosNGen + 1
                        miError = ex.Message
                    End Try
                End If
                If mensajeGenerado Then
                    If optimizar_mmcall Then
                        Dim fPrioridad = ""
                        If mantenerPrioridad Then
                            fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                        End If
                        'Doble cic en el mensaje
                        regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE canal = 4 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad)
                    Else
                        regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE id = " & elmensaje!id)
                    End If
                ElseIf cadena <> "success" Then
                    agregarLOG("Errores en la generación de llamada a MMCall. No se generaron " & audiosNGen & " llamada(s) a MMCall. Error: " & cadena, 1, 0)
                End If
            Next
            If audiosGen > 0 Then
                agregarLOG("Se generaron " & audiosGen & " mensaje(s) a MMCall (" & mTotal & " notifación(es))" & IIf(audiosNGen > 0, " No se generaron " & audiosNGen & " mensaje(s) a MMCall ", ""), 1, 0)
            Else
                If audiosNGen > 0 Then
                    agregarLOG("Errores en la generación de llamada a MMCall. No se generaron " & audiosNGen & " llamada(s) a MMCall. Error: " & miError, 1, 0)
                End If
            End If
        End If
        BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"
        If utilizar_arduino Then
            Try
                agregarSolo("Se inicia la aplicación de Arduino(r) ")
                Shell(Application.StartupPath & "\vwArduino.exe", AppWinStyle.MinimizedNoFocus)
            Catch ex As Exception
                agregarLOG("Error en la ejecución de la aplicación de llamadas y SMS a Arduino. Error: " & ex.Message, 7, 0)
            End Try

            'generarLlamadas()
        End If
    End Sub

    Private Sub generarLlamadas()
        If Not estadoPrograma Then Exit Sub
        BarManager1.Items(1).Caption = "Conectado (trabajando con arduino)"
        Dim rutaAudios
        Dim rutaSMS
        Dim ptoCOMM1 As String, ptoCOMM2 As String, ptoCOMM3 As String, ptoCOMM4 As String, ptoCOMM5 As String, ptoCOMM6 As String
        Dim ptoCOMM1P As String, ptoCOMM2P As String, ptoCOMM3P As String, ptoCOMM4P As String, ptoCOMM5P As String, ptoCOMM6P As String
        Dim escape_veces = 3
        Dim escape_accion = ""
        Dim escape_lista = 0
        Dim escape_mensaje = ""
        Dim veces_reproducir = 2
        Dim tOutLlamada = 20
        Dim tOutSMS = 5
        Dim escape_mensaje_propio As Boolean = True
        Dim cadSQL As String = "SELECT * FROM sigma.vw_configuracion"
        Dim readerDS As DataSet = consultaSEL(cadSQL)

        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            ptoCOMM1 = ValNull(reader!puerto_comm1, "A")
            ptoCOMM1P = ValNull(reader!puerto_comm1_par, "A")
            ptoCOMM2 = ValNull(reader!puerto_comm2, "A")
            ptoCOMM2P = ValNull(reader!puerto_comm2_par, "A")
            ptoCOMM3 = ValNull(reader!puerto_comm3, "A")
            ptoCOMM3P = ValNull(reader!puerto_comm3_par, "A")
            ptoCOMM4 = ValNull(reader!puerto_comm4, "A")
            ptoCOMM4P = ValNull(reader!puerto_comm4_par, "A")
            ptoCOMM5 = ValNull(reader!puerto_comm5, "A")
            ptoCOMM5P = ValNull(reader!puerto_comm5_par, "A")
            ptoCOMM6 = ValNull(reader!puerto_comm6, "A")
            rutaAudios = ValNull(reader!ruta_audios, "A")
            ptoCOMM6P = ValNull(reader!puerto_comm6_par, "A")
            rutaSMS = ValNull(reader!ruta_sms, "A")

            escape_veces = ValNull(reader!escape_llamadas, "N")
            escape_accion = ValNull(reader!escape_accion, "A")
            escape_lista = ValNull(reader!escape_lista, "A")
            escape_mensaje = ValNull(reader!escape_mensaje, "A")
            veces_reproducir = ValNull(reader!veces_reproducir, "N")
            tOutLlamada = ValNull(reader!timeout_llamadas, "N")
            tOutSMS = ValNull(reader!timeout_sms, "N")
            escape_mensaje_propio = ValNull(reader!escape_mensaje_propio, "A") = "S"
        ElseIf Not errorPuerto Then
            errorPuerto = True
            agregarLOG("No se ha configurado la interacción con Arduino, por favor configure la aplicación e intente de nuevo", 7, 0)
            Exit Sub
        End If
        If ptoCOMM1.Length = 0 And ptoCOMM2.Length = 0 And ptoCOMM3.Length = 0 And ptoCOMM4.Length = 0 And ptoCOMM5.Length = 0 And ptoCOMM6.Length = 0 Then
            If Not errorPuerto Then
                agregarLOG("No se ha configurado ningún puerto de comunicaciones para la interacción con Arduino, por favor configure la aplicación e intente de nuevo", 7, 0)
                errorPuerto = True
                Exit Sub
            End If
        End If
        If ptoCOMM1P.Length = 0 And ptoCOMM2P.Length = 0 And ptoCOMM3P.Length = 0 And ptoCOMM4P.Length = 0 And ptoCOMM5P.Length = 0 And ptoCOMM6P.Length = 0 Then
            If Not errorPuerto Then
                errorPuerto = True
                agregarLOG("No se ha configurado ningún puerto de comunicaciones para la interacción con Arduino, por favor configure la aplicación e intente de nuevo", 7, 0)
                Exit Sub
            End If
        End If
        If escape_veces = 0 Then escape_veces = 3
        If veces_reproducir = 0 Then veces_reproducir = 1
        ptoCOMM1 = ptoCOMM1.ToUpper
        ptoCOMM2 = ptoCOMM2.ToUpper
        ptoCOMM3 = ptoCOMM3.ToUpper
        ptoCOMM3 = ptoCOMM4.ToUpper
        ptoCOMM5 = ptoCOMM5.ToUpper
        ptoCOMM6 = ptoCOMM6.ToUpper
        'Llamadas telefónicas
        If Not My.Computer.FileSystem.DirectoryExists(rutaAudios) Then
            rutaAudios = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        End If
        Dim LlamadasPendientes = 0
        For Each FoundFile As String In My.Computer.FileSystem.GetFiles(
  rutaAudios, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.wav")
            LlamadasPendientes = LlamadasPendientes + 1
        Next
        If LlamadasPendientes > 0 Then
            Dim iniLlamadas = DateTime.Now
            'Dim Comando As OdbcCommand = New OdbcCommand(CadSQL)
            If Not ValPuerto(ptoCOMM1, ptoCOMM1P) Then
                agregarLOG("La aplicación de generar llamadas indicó que el puerto especificado no es válido. Puerto:" & ptoCOMM1 & " parámetros: " & ptoCOMM1P & ". No se emitieron " & LlamadasPendientes & " llamada(s) de voz", 7, 0)
            Else

                agregarLOG("Se intentarán generar " & LlamadasPendientes & " llamada(s) de voz...", 1, 0)

                For Each FoundFile As String In My.Computer.FileSystem.GetFiles(
      rutaAudios, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.wav")
                    If My.Computer.FileSystem.FileExists(FoundFile) Then
                        If Not estadoPrograma Then
                            Exit Sub
                        End If
                        Try
                            If Not SerialPort1.IsOpen Then
                                SerialPort1.Open()
                            End If
                            'ReceiveSerialData()
                            Dim Numero = Microsoft.VisualBasic.Strings.Left(Path.GetFileName(FoundFile), 10)
                            Dim nombreArchivo = Path.GetFileName(FoundFile)
                            If IsNumeric(Numero) Then
                                Try
                                    SerialPort1.Write("VOID" & vbNewLine)
                                    Demora(1)
                                    SerialPort1.Write("~CALL" & Numero & vbNewLine)
                                    Dim LimiteTimeout As Integer = tOutLlamada
                                    Dim Salir = False
                                    Dim TiempoInicial = DateTime.Now
                                    MensajeLlamada = ""
                                    Do While Not Salir
                                        If Not estadoPrograma Then
                                            Exit Sub
                                        End If
                                        'Se cuentan hasta 30seg
                                        Application.DoEvents()
                                        Dim TiempoFinal = DateTime.Now
                                        Dim TotalSegundos = TiempoFinal - TiempoInicial
                                        If TotalSegundos.Seconds >= LimiteTimeout Then
                                            MensajeLlamada = "timeout"
                                            Salir = True
                                        ElseIf MensajeLlamada.Length >= 0 Then
                                            If Microsoft.VisualBasic.Strings.InStr(MensajeLlamada, "CONNECTED") > 0 Then
                                                Salir = True
                                            End If
                                        End If
                                    Loop
                                    If MensajeLlamada = "timeout" Then
                                        SerialPort1.Write("VOID" & vbNewLine)
                                        'Se busca las veces que se ha repeoducido el audio
                                        Dim newFile = ""
                                        If escape_veces > 0 Then
                                            'Controlar las llamadas

                                            Dim eliminado = False
                                            If escape_veces <= 1 And Microsoft.VisualBasic.Strings.InStr(FoundFile, "_1.wav") > 0 Then
                                                eliminarArchivo(FoundFile)
                                                eliminado = True
                                            ElseIf escape_veces <= 2 And Microsoft.VisualBasic.Strings.InStr(FoundFile, "_2.wav") > 0 Then
                                                eliminarArchivo(FoundFile)
                                                eliminado = True
                                            ElseIf escape_veces <= 3 And Microsoft.VisualBasic.Strings.InStr(FoundFile, "_3.wav") > 0 Then
                                                eliminarArchivo(FoundFile)
                                                eliminado = True
                                            ElseIf escape_veces <= 4 And Microsoft.VisualBasic.Strings.InStr(FoundFile, "_4.wav") > 0 Then
                                                eliminarArchivo(FoundFile)
                                                eliminado = True
                                            ElseIf escape_veces <= 5 And Microsoft.VisualBasic.Strings.InStr(FoundFile, "_5.wav") > 0 Then
                                                eliminarArchivo(FoundFile)
                                                eliminado = True
                                            End If
                                            If Not eliminado Then
                                                newFile = FoundFile
                                                newFile = Microsoft.VisualBasic.Strings.Replace(newFile, "_4", "_5")
                                                newFile = Microsoft.VisualBasic.Strings.Replace(newFile, "_3", "_4")
                                                newFile = Microsoft.VisualBasic.Strings.Replace(newFile, "_2", "_3")
                                                newFile = Microsoft.VisualBasic.Strings.Replace(newFile, "_1", "_2")
                                                My.Computer.FileSystem.RenameFile(FoundFile, Path.GetFileName(newFile))
                                                agregarLOG("Se hizo una llamada sin respuesta al repositorio: " & Numero, 1, 0)
                                            ElseIf escape_accion = "L" Then
                                                'Se escapa la llamada
                                                If escape_mensaje.Length > 0 Then escape_mensaje = "se agotó el número de intentos de llamada a "
                                                If escape_lista > 0 Then
                                                    agregarMensaje("telefonos", escape_lista, 0, 99, 2, 0, escape_mensaje & Numero)
                                                    agregarMensaje("correos", escape_lista, 0, 99, 3, 0, escape_mensaje & Numero)
                                                End If
                                                If escape_mensaje_propio Then
                                                    Dim regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, tipo, canal, prioridad, destino, mensaje) VALUES (0, 99, 2, '0', '" & Numero & "', '" & escape_mensaje & Numero & "')")
                                                End If
                                                escaparLlamada(Numero)
                                            End If
                                        End If
                                    Else
                                        Dim fs As FileStream = New FileStream(FoundFile, FileMode.Open, FileAccess.Read)
                                        Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                        For i = 0 To veces_reproducir
                                            sp.PlaySync()
                                        Next i
                                        fs.Close()
                                        Salir = False
                                        TiempoInicial = DateTime.Now
                                        Dim TiempoFinal = DateTime.Now
                                        Do While Not Salir And My.Computer.FileSystem.FileExists(FoundFile)
                                            'Se cuentan hasta 30seg
                                            eliminarArchivo(FoundFile)
                                            TiempoFinal = DateTime.Now
                                            Dim TotalSegundos = TiempoFinal - TiempoInicial
                                            If TotalSegundos.Seconds > 1 Then
                                                Salir = True
                                            End If
                                        Loop
                                        If Salir Then
                                            agregarLOG("Se hizo una llamada, se reprodujo un audio pero no se eliminó correctamente el archivo...", 1, 0)
                                        Else
                                            agregarLOG("Se acaba de realizar una llamada satisfactoria al repositorio : " & Numero, 1, 0)
                                        End If
                                    End If
                                Catch ex As Exception
                                End Try
                            End If
                        Catch ex2 As Exception
                        End Try
                    End If
                    'Se mueven los archivos a otra carpeta
                Next
                Dim tSegundos = DateTime.Now - iniLlamadas

                agregarLOG("Se procesaron " & LlamadasPendientes & " llamada(s) de voz en " & tSegundos.Seconds & " segundo(s)...", 1, 0)

            End If
        End If
        'Se envian SMS

        If Not My.Computer.FileSystem.DirectoryExists(rutaSMS) Then
            rutaSMS = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        End If
        LlamadasPendientes = 0
        For Each FoundFile As String In My.Computer.FileSystem.GetFiles(
    rutaSMS, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.txt")
            LlamadasPendientes = LlamadasPendientes + 1
        Next
        If LlamadasPendientes > 0 Then
            Dim iniLlamadas = DateTime.Now

            If Not ValPuerto(ptoCOMM1, ptoCOMM1P) Then
                agregarLOG("La aplicación de generar llamadas indicó que el puerto especificado no es válido. Puerto:" & ptoCOMM1 & " parámetros: " & ptoCOMM1P & ". No se enviaron " & LlamadasPendientes & " mensaje(s) de texto (SMS)", 7, 0)
            Else
                agregarLOG("Se intentarán enviar " & LlamadasPendientes & " mensaje(s) de texto (SMS)...", 1, 0)

                For Each FoundFile As String In My.Computer.FileSystem.GetFiles(
      rutaSMS, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.txt")
                    If Not estadoPrograma Then
                        Exit Sub
                    End If
                    If My.Computer.FileSystem.FileExists(FoundFile) Then
                        Try
                            If Not SerialPort1.IsOpen Then
                                SerialPort1.Open()
                            End If
                            Dim miReader As StreamReader = My.Computer.FileSystem.OpenTextFileReader(FoundFile)
                            Dim elMensaje As String = miReader.ReadLine
                            elMensaje = UCase(elMensaje)
                            elMensaje = Replace(elMensaje, "Á", "A")
                            elMensaje = Replace(elMensaje, "É", "E")
                            elMensaje = Replace(elMensaje, "Í", "I")
                            elMensaje = Replace(elMensaje, "Ó", "O")
                            elMensaje = Replace(elMensaje, "Ú", "U")
                            elMensaje = Replace(elMensaje, "Ñ", "~")
                            elMensaje = Microsoft.VisualBasic.Strings.Left(elMensaje, 120)
                            miReader.Close()
                            If elMensaje.Length > 0 Then
                                Dim Numero = Microsoft.VisualBasic.Strings.Left(Path.GetFileName(FoundFile), 10)
                                Dim nombreArchivo = Path.GetFileName(FoundFile)
                                If IsNumeric(Numero) Then
                                    Try
                                        SerialPort1.Write("VOID" & vbNewLine)
                                        Demora(1)
                                        SerialPort1.Write("~SMS01" & Numero & elMensaje & vbNewLine)
                                        Dim LimiteTimeout As Integer = tOutSMS
                                        Dim Salir = False
                                        Dim TiempoInicial = DateTime.Now
                                        MensajeLlamada = ""
                                        Do While Not Salir
                                            If Not estadoPrograma Then
                                                Exit Sub
                                            End If
                                            'Se cuentan hasta 30seg
                                            Application.DoEvents()
                                            Dim TiempoFinal = DateTime.Now
                                            Dim TotalSegundos = TiempoFinal - TiempoInicial
                                            If TotalSegundos.Seconds >= LimiteTimeout Then
                                                MensajeLlamada = "timeout"
                                                Salir = True
                                            ElseIf MensajeLlamada.Length > 0 Then
                                                If Microsoft.VisualBasic.Strings.InStr(MensajeLlamada, "OK") Or Microsoft.VisualBasic.Strings.InStr(MensajeLlamada, "finalizada") Then
                                                    Salir = True
                                                Else
                                                    Salir = True
                                                    MensajeLlamada = "timeout"
                                                End If
                                            End If
                                        Loop
                                        If MensajeLlamada = "timeout" Then
                                            'Se busca las veces que se ha repeoducido el audio
                                            escaparSMS(Numero)
                                        Else
                                            agregarLOG("Se envío en mensaje de texto correctamente al repositorio: " & Numero, 1, 0)
                                        End If
                                        eliminarArchivo(FoundFile)
                                    Catch ex As Exception
                                    End Try
                                Else
                                    eliminarArchivo(FoundFile)
                                End If
                            Else
                                eliminarArchivo(FoundFile)
                            End If
                        Catch ex2 As Exception
                        End Try
                    End If
                    'Se mueven los archivos a otra carpeta
                Next
                Dim tSegundos = DateTime.Now - iniLlamadas

                agregarLOG("Se procesaron " & LlamadasPendientes & " mensaje(s) de texto (SMS) en " & tSegundos.Seconds & " segundo(s)...", 1, 0)

            End If
        End If
        BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"
    End Sub

    Sub eliminarArchivo(archivo)
        Try

            My.Computer.FileSystem.DeleteFile(archivo)
            File.Delete(archivo)
        Catch ex As Exception

        End Try

    End Sub

    Function ValPuerto(ePuerto As String, ePar As String) As Boolean

        ValPuerto = True

        Try
            SerialPort1.Close()
        Catch ex As Exception

        End Try



        Try
            SerialPort1.PortName = ePuerto

            Dim parametros = ePar.Split(New Char() {","c})

            SerialPort1.BaudRate = parametros(0) '19200
            SerialPort1.DataBits = parametros(1) '8
            SerialPort1.Parity = parametros(2) '0


            SerialPort1.StopBits = parametros(3) '1


            SerialPort1.Handshake = parametros(4) '2
            SerialPort1.RtsEnable = parametros(5) = "S" 'True

            SerialPort1.Open()

        Catch ex As Exception
            ValPuerto = False
        End Try


    End Function

    Sub Demora(Segundos As Integer)
        Dim Salir = False
        Dim TInicial = DateTime.Now
        Dim TotalSegundos As TimeSpan
        Do While Not Salir
            Dim TiempoFinal = DateTime.Now
            TotalSegundos = TiempoFinal - TInicial
            If TotalSegundos.Seconds > Segundos Then
                Salir = True
            End If
        Loop
    End Sub

    Function calcularTiempo(Seg) As String
        calcularTiempo = ""
        If Seg < 60 Then
            calcularTiempo = Seg & " seg"
        ElseIf Seg < 3600 Then
            calcularTiempo = Math.Round(Seg / 60, 1) & " min"
        Else
            calcularTiempo = Math.Round(Seg / 3600, 1) & " hr"
        End If
    End Function

    Sub escaparLlamada(numero)
        agregarLOG("Se agotó el número de intentos de llamada de voz al repositorio: " & numero, 1, 0)
    End Sub

    Sub escaparSMS(numero)
        agregarLOG("Se agotó el número de intentos de envio de SMS al repositorio: " & numero, 1, 0)
    End Sub

    Sub escaparMMCall()
        agregarLOG("Todos los requesters de MMCall están ocupados... ", 1, 0)
    End Sub

    Sub agregarMensaje(campo As String, LD As Integer, reporte As Integer, tipo As Integer, canal As Integer, prioridad As String, mensaje As String)

        Dim canales As DataSet
        Dim cadSQL = "SELECT " & campo & " as cadena FROM sigma.cat_distribucion WHERE id = " & LD & " AND estatus = 'A'"
        canales = consultaSEL(cadSQL)
        If canales.Tables(0).Rows.Count > 0 Then
            Dim todosCanales As String()
            Dim tempArray As String()
            Dim totalItems = 0
            Dim cadCanales As String = ValNull(canales.Tables(0).Rows(0)!cadena, "A")
            If cadCanales.Length > 0 Then
                Dim arreCanales = cadCanales.Split(New Char() {";"c})
                For i = LBound(arreCanales) To UBound(arreCanales)
                    'Redimensionamos el Array temporal y preservamos el valor  
                    ReDim Preserve todosCanales(totalItems + i)
                    todosCanales(totalItems + i) = arreCanales(i)
                Next
                tempArray = todosCanales
                totalItems = todosCanales.Length

                Dim x As Integer, y As Integer
                Dim z As Integer

                For x = 0 To UBound(todosCanales)
                    z = 0
                    For y = 0 To UBound(todosCanales) - 1
                        'Si el elemento del array es igual al array temporal  
                        If todosCanales(x) = tempArray(z) And y <> x Then
                            'Entonces Eliminamos el valor duplicado  
                            todosCanales(y) = ""
                        End If
                        z = z + 1
                    Next y
                Next x

                For Each movil In todosCanales
                    If movil.Length > 0 Then
                        Dim regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, tipo, canal, prioridad, destino, mensaje, lista) VALUES (" & reporte & ", " & tipo & ", " & canal & ", '" & prioridad & "', '" & movil & "', '" & mensaje.Trim & "', " & LD & ")")
                    End If
                Next

            End If

        End If
    End Sub

    Private Sub escalamiento_Tick(sender As Object, e As EventArgs) Handles escalamiento.Tick
        If procesandoEscalamientos Or Not estadoPrograma Then Exit Sub
        escalamiento.Enabled = False

        BarManager1.Items(1).Caption = "Conectado (revisando escalamientos...)"
        procesandoEscalamientos = True
        Dim regsAfectados = 0
        Dim cadSQL = ""

        'Escalada 5
        cadSQL = "SELECT sigma.vw_reportes.*, sigma.vw_alertas.nombre FROM sigma.vw_reportes LEFT JOIN sigma.vw_alertas ON vw_reportes.alerta = vw_alertas.id WHERE vw_reportes.escalar1 <> 'N' AND vw_reportes.escalar2 <> 'N' AND vw_reportes.escalar3 <> 'N' AND vw_reportes.escalar4 <> 'N' AND vw_reportes.escalar5 <> 'N' AND ((vw_reportes.estado = 5) OR (vw_reportes.estado >= 5 AND vw_reportes.estado < 9 AND vw_reportes.repetir5 = 'S'))"
        Dim alertaDS As DataSet = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    escalamiento.Enabled = True
                    Exit Sub
                End If
                Dim repeticiones As Integer = alerta!es5
                If alerta!estado > 5 Then
                    repeticiones = repeticiones + 1
                End If

                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada5.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada4, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada5, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo5 Then
                    agregarSolo("Generando escalamientos...")
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempo(segundos)

                    'Se generan los mensajes a enviar
                    'Se busca una copia del mensaje anterior

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal <> 4 LIMIT 1"
                    Dim EMensaje As String
                    Dim miMensaje As DataSet = consultaSEL(cadSQL)
                    Dim prioridad = "0"
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        prioridad = ValNull(miMensaje.Tables(0).Rows(0)!prioridad, "A")
                        EMensaje = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E5 " & tiempoCad
                    End If
                    EMensaje = EMensaje & IIf(alerta!estado > 5, " *R" & repeticiones, "")

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " And tipo = 0 And canal = 4 LIMIT 1"
                    Dim EMensajeMMCall As String = "MENSAJE ESCALADO *E5 " & tiempoCad
                    miMensaje = consultaSEL(cadSQL)
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        EMensajeMMCall = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E5 " & tiempoCad
                    Else
                        EMensajeMMCall = EMensaje
                    End If
                    EMensajeMMCall = EMensajeMMCall & IIf(alerta!estado > 5, " *R" & repeticiones, "")
                    EMensajeMMCall = Microsoft.VisualBasic.Strings.Left(EMensajeMMCall, 40)
                    If EMensaje.Length = 0 Then EMensaje = EMensajeMMCall

                    If ValNull(alerta!escalar5, "A") = "T" And alerta!estado = 5 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 15, canal, prioridad, destino, '" & EMensaje & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 4 and canal <> 4;INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 15, canal, prioridad, destino, '" & EMensajeMMCall & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 4 and canal = 4")
                    End If


                    If ValNull(alerta!llamada5, "A") = "S" Then agregarMensaje("telefonos", alerta!lista5, uID, 5, 1, prioridad, EMensaje)
                    If ValNull(alerta!sms5, "A") = "S" Then agregarMensaje("telefonos", alerta!lista5, uID, 5, 2, prioridad, EMensaje)
                    If ValNull(alerta!correo5, "A") = "S" Then agregarMensaje("correos", alerta!lista5, uID, 5, 3, prioridad, EMensaje)
                    If ValNull(alerta!mmcall5, "A") = "S" Then agregarMensaje("mmcall", alerta!lista5, uID, 5, 4, prioridad, EMensajeMMCall)
                    Dim cadAdic = ""

                    If alerta!estado > 5 Then
                        agregarLOG("Se crea una repetición " & repeticiones & " del escalamiento de NIVEL 5 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    Else
                        cadAdic = ", estado = 6"

                        agregarLOG("Se crea escalamiento de NIVEL 5 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    End If
                    regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET escalamientos = 5, escalada5 = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "', es5 = " & repeticiones & cadAdic & " WHERE id = " & alerta!id)
                End If
            Next
        End If

        regsAfectados = 0
        'Escalada 4
        cadSQL = "SELECT sigma.vw_reportes.*, sigma.vw_alertas.nombre FROM sigma.vw_reportes LEFT JOIN sigma.vw_alertas ON vw_reportes.alerta = vw_alertas.id WHERE vw_reportes.escalar1 <> 'N' AND vw_reportes.escalar2 <> 'N' AND vw_reportes.escalar3 <> 'N' AND vw_reportes.escalar4 <> 'N' AND ((vw_reportes.estado = 4) OR (vw_reportes.estado >= 4 AND vw_reportes.estado < 9 AND vw_reportes.repetir4 = 'S'))"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then
            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    escalamiento.Enabled = True
                    Exit Sub
                End If
                Application.DoEvents()
                Dim repeticiones As Integer = alerta!es4
                If alerta!estado > 4 Then
                    repeticiones = repeticiones + 1
                End If

                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada4.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada3, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada4, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo4 Then
                    agregarSolo("Generando escalamientos...")
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempo(segundos)

                    'Se generan los mensajes a enviar
                    'Se busca una copia del mensaje anterior

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal <> 4 LIMIT 1"
                    Dim EMensaje As String
                    Dim miMensaje As DataSet = consultaSEL(cadSQL)
                    Dim prioridad = "0"
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        prioridad = ValNull(miMensaje.Tables(0).Rows(0)!prioridad, "A")
                        EMensaje = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E4 " & tiempoCad
                    End If
                    EMensaje = EMensaje & IIf(alerta!estado > 4, " *R" & repeticiones, "")

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " And tipo = 0 And canal = 4 LIMIT 1"
                    Dim EMensajeMMCall As String = "MENSAJE ESCALADO *E4 " & tiempoCad
                    miMensaje = consultaSEL(cadSQL)
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        EMensajeMMCall = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E4 " & tiempoCad
                    Else
                        EMensajeMMCall = EMensaje
                    End If
                    EMensajeMMCall = EMensajeMMCall & IIf(alerta!estado > 4, " *R" & repeticiones, "")
                    EMensajeMMCall = Microsoft.VisualBasic.Strings.Left(EMensajeMMCall, 40)
                    If EMensaje.Length = 0 Then EMensaje = EMensajeMMCall

                    If ValNull(alerta!escalar4, "A") = "T" And alerta!estado = 4 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 14, canal, prioridad, destino, '" & EMensaje & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 3 and canal <> 4;INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 14, canal, prioridad, destino, '" & EMensajeMMCall & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 3 and canal = 4")
                    End If


                    If ValNull(alerta!llamada4, "A") = "S" Then agregarMensaje("telefonos", alerta!lista4, uID, 4, 1, prioridad, EMensaje)
                    If ValNull(alerta!sms4, "A") = "S" Then agregarMensaje("telefonos", alerta!lista4, uID, 4, 2, prioridad, EMensaje)
                    If ValNull(alerta!correo4, "A") = "S" Then agregarMensaje("correos", alerta!lista4, uID, 4, 3, prioridad, EMensaje)
                    If ValNull(alerta!mmcall4, "A") = "S" Then agregarMensaje("mmcall", alerta!lista4, uID, 4, 4, prioridad, EMensajeMMCall)
                    Dim cadAdic = ""

                    If alerta!estado > 4 Then
                        agregarLOG("Se crea una repetición " & repeticiones & " del escalamiento de NIVEL 4 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    Else
                        agregarLOG("Se crea escalamiento de NIVEL 4 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                        cadAdic = ", estado = 5"

                    End If
                    regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET escalamientos = 4, escalada4 = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "', es4 = " & repeticiones & cadAdic & " WHERE id = " & alerta!id)
                End If
            Next
        End If

        regsAfectados = 0
        'Escalada 3
        cadSQL = "SELECT sigma.vw_reportes.*, sigma.vw_alertas.nombre FROM sigma.vw_reportes LEFT JOIN sigma.vw_alertas ON vw_reportes.alerta = vw_alertas.id WHERE vw_reportes.escalar1 <> 'N' AND vw_reportes.escalar2 <> 'N' AND vw_reportes.escalar3 <> 'N' AND ((vw_reportes.estado = 3) OR (vw_reportes.estado >= 3 AND vw_reportes.estado < 9 AND vw_reportes.repetir3 = 'S'))"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    escalamiento.Enabled = True
                    Exit Sub
                End If
                Application.DoEvents()
                Dim repeticiones As Integer = alerta!es3
                If alerta!estado > 3 Then
                    repeticiones = repeticiones + 1
                End If

                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada3.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada2, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada3, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo3 Then
                    agregarSolo("Generando escalamientos...")
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempo(segundos)

                    'Se generan los mensajes a enviar
                    'Se busca una copia del mensaje anterior

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal <> 4 LIMIT 1"
                    Dim EMensaje As String
                    Dim miMensaje As DataSet = consultaSEL(cadSQL)
                    Dim prioridad = "0"
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        prioridad = ValNull(miMensaje.Tables(0).Rows(0)!prioridad, "A")
                        EMensaje = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E3 " & tiempoCad
                    End If
                    EMensaje = EMensaje & IIf(alerta!estado > 3, " *R" & repeticiones, "")

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " And tipo = 0 And canal = 4 LIMIT 1"
                    Dim EMensajeMMCall As String = "MENSAJE ESCALADO *E3 " & tiempoCad
                    miMensaje = consultaSEL(cadSQL)
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        EMensajeMMCall = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E3 " & tiempoCad
                    Else
                        EMensajeMMCall = EMensaje
                    End If
                    EMensajeMMCall = EMensajeMMCall & IIf(alerta!estado > 3, " *R" & repeticiones, "")
                    EMensajeMMCall = Microsoft.VisualBasic.Strings.Left(EMensajeMMCall, 40)
                    If EMensaje.Length = 0 Then EMensaje = EMensajeMMCall

                    If ValNull(alerta!escalar3, "A") = "T" And alerta!estado = 3 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 13, canal, prioridad, destino, '" & EMensaje & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 2 and canal <> 4;INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 13, canal, prioridad, destino, '" & EMensajeMMCall & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 2 and canal = 4")
                    End If


                    If ValNull(alerta!llamada3, "A") = "S" Then agregarMensaje("telefonos", alerta!lista3, uID, 3, 1, prioridad, EMensaje)
                    If ValNull(alerta!sms3, "A") = "S" Then agregarMensaje("telefonos", alerta!lista3, uID, 3, 2, prioridad, EMensaje)
                    If ValNull(alerta!correo3, "A") = "S" Then agregarMensaje("correos", alerta!lista3, uID, 3, 3, prioridad, EMensaje)
                    If ValNull(alerta!mmcall3, "A") = "S" Then agregarMensaje("mmcall", alerta!lista3, uID, 3, 4, prioridad, EMensajeMMCall)
                    Dim cadAdic = ""

                    If alerta!estado > 3 Then
                        agregarLOG("Se crea una repetición " & repeticiones & " del escalamiento de NIVEL 3 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    Else
                        cadAdic = ", estado = 4"

                        agregarLOG("Se crea escalamiento de NIVEL 3 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    End If
                    regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET escalamientos = 3, escalada3 = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "', es3 = " & repeticiones & cadAdic & " WHERE id = " & alerta!id)
                End If
            Next
        End If

        regsAfectados = 0
        'Escalada 2
        cadSQL = "Select sigma.vw_reportes.*, sigma.vw_alertas.nombre FROM sigma.vw_reportes LEFT JOIN sigma.vw_alertas ON vw_reportes.alerta = vw_alertas.id WHERE vw_reportes.escalar1 <> 'N' AND vw_reportes.escalar2 <> 'N' AND ((vw_reportes.estado = 2) OR (vw_reportes.estado >= 2 AND vw_reportes.estado < 9 AND vw_reportes.repetir2 = 'S'))"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    escalamiento.Enabled = True
                    Exit Sub
                End If
                Application.DoEvents()
                Dim repeticiones As Integer = alerta!es2
                If alerta!estado > 2 Then
                    repeticiones = repeticiones + 1
                End If

                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada2.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada1, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada2, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo2 Then
                    agregarSolo("Generando escalamientos...")
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempo(segundos)

                    'Se generan los mensajes a enviar
                    'Se busca una copia del mensaje anterior

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal <> 4 LIMIT 1"
                    Dim EMensaje As String
                    Dim miMensaje As DataSet = consultaSEL(cadSQL)
                    Dim prioridad = "0"
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        prioridad = ValNull(miMensaje.Tables(0).Rows(0)!prioridad, "A")
                        EMensaje = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E2 " & tiempoCad
                    End If
                    EMensaje = EMensaje & IIf(alerta!estado > 2, " *R" & repeticiones, "")

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " And tipo = 0 And canal = 4 LIMIT 1"
                    Dim EMensajeMMCall As String = "MENSAJE ESCALADO *E2 " & tiempoCad
                    miMensaje = consultaSEL(cadSQL)
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        EMensajeMMCall = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E2 " & tiempoCad
                    Else
                        EMensajeMMCall = EMensaje
                    End If
                    EMensajeMMCall = EMensajeMMCall & IIf(alerta!estado > 2, " *R" & repeticiones, "")
                    EMensajeMMCall = Microsoft.VisualBasic.Strings.Left(EMensajeMMCall, 40)
                    If EMensaje.Length = 0 Then EMensaje = EMensajeMMCall

                    If ValNull(alerta!escalar2, "A") = "T" And alerta!estado = 2 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 12, canal, prioridad, destino, '" & EMensaje & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 1 and canal <> 4;INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 12, canal, prioridad, destino, '" & EMensajeMMCall & "' FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 1 and canal = 4")
                    End If


                    If ValNull(alerta!llamada2, "A") = "S" Then agregarMensaje("telefonos", alerta!lista2, uID, 2, 1, prioridad, EMensaje)
                    If ValNull(alerta!sms2, "A") = "S" Then agregarMensaje("telefonos", alerta!lista2, uID, 2, 2, prioridad, EMensaje)
                    If ValNull(alerta!correo2, "A") = "S" Then agregarMensaje("correos", alerta!lista2, uID, 2, 3, prioridad, EMensaje)
                    If ValNull(alerta!mmcall2, "A") = "S" Then agregarMensaje("mmcall", alerta!lista2, uID, 2, 4, prioridad, EMensajeMMCall)
                    Dim cadAdic = ""

                    If alerta!estado > 2 Then
                        agregarLOG("Se crea una repetición " & repeticiones & " del escalamiento de NIVEL 2 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    Else
                        cadAdic = ", estado = 3"

                        agregarLOG("Se crea escalamiento de NIVEL 2 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    End If
                    regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET escalamientos = 2, escalada2 = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "', es2 = " & repeticiones & cadAdic & " WHERE id = " & alerta!id)
                End If
            Next
        End If

        'Escalada 1
        cadSQL = "Select sigma.vw_reportes.*, sigma.vw_alertas.nombre FROM sigma.vw_reportes LEFT JOIN sigma.vw_alertas On vw_reportes.alerta = vw_alertas.id WHERE vw_reportes.escalar1 <> 'N' AND ((vw_reportes.estado = 1) OR (vw_reportes.estado >= 1 AND vw_reportes.estado < 9 AND vw_reportes.repetir1 = 'S'))"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then
            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    escalamiento.Enabled = True
                    Exit Sub
                End If
                Application.DoEvents()
                Dim repeticiones As Integer = alerta!es1
                If alerta!estado > 1 Then
                    repeticiones = repeticiones + 1
                End If
                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada1.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada1, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo1 Then
                    agregarSolo("Generando escalamientos...")
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempo(segundos)

                    'Se generan los mensajes a enviar
                    'Se busca una copia del mensaje anterior

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal <> 4 LIMIT 1"
                    Dim EMensaje As String
                    Dim miMensaje As DataSet = consultaSEL(cadSQL)
                    Dim prioridad = "0"
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        prioridad = ValNull(miMensaje.Tables(0).Rows(0)!prioridad, "A")
                        EMensaje = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E1 " & tiempoCad
                    End If
                    EMensaje = EMensaje & IIf(alerta!estado > 1, " *R" & repeticiones, "")

                    cadSQL = "Select * FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " And tipo = 0 And canal = 4 LIMIT 1"
                    Dim EMensajeMMCall As String = "MENSAJE ESCALADO *E1 " & tiempoCad
                    miMensaje = consultaSEL(cadSQL)
                    If miMensaje.Tables(0).Rows.Count > 0 Then

                        EMensajeMMCall = ValNull(miMensaje.Tables(0).Rows(0)!mensaje, "A") & " *E1 " & tiempoCad
                    Else
                        EMensajeMMCall = EMensaje
                    End If
                    EMensajeMMCall = EMensajeMMCall & IIf(alerta!estado > 1, " *R" & repeticiones, "")
                    EMensajeMMCall = Microsoft.VisualBasic.Strings.Left(EMensajeMMCall, 40)
                    If EMensaje.Length = 0 Then EMensaje = EMensajeMMCall


                    If ValNull(alerta!escalar1, "A") = "T" And alerta!estado = 1 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) Select alerta, lista, 11, canal, prioridad, destino, CONCAT(mensaje, ' *E1 " & tiempoCad & "') FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0")
                    End If


                    If ValNull(alerta!llamada1, "A") = "S" Then agregarMensaje("telefonos", alerta!lista1, uID, 1, 1, prioridad, EMensaje)
                    If ValNull(alerta!sms1, "A") = "S" Then agregarMensaje("telefonos", alerta!lista1, uID, 1, 2, prioridad, EMensaje)
                    If ValNull(alerta!correo1, "A") = "S" Then agregarMensaje("correos", alerta!lista1, uID, 1, 3, prioridad, EMensaje)
                    If ValNull(alerta!mmcall1, "A") = "S" Then agregarMensaje("mmcall", alerta!lista1, uID, 1, 4, prioridad, EMensajeMMCall)
                    Dim cadAdic = ""
                    If alerta!estado > 1 Then
                        agregarLOG("Se crea una repetición " & repeticiones & " del escalamiento de NIVEL 1 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    Else
                        cadAdic = ", estado = 2"
                        agregarLOG("Se crea escalamiento de NIVEL 1 en el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, alerta!id, 10)
                    End If
                    regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET escalamientos = 1, escalada1 = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "', es1 = " & repeticiones & cadAdic & " WHERE id = " & alerta!id)
                End If
            Next
        End If

        regsAfectados = 0
        cadSQL = "SELECT sigma.vw_reportes.*, sigma.vw_alertas.nombre FROM sigma.vw_reportes LEFT JOIN sigma.vw_alertas ON vw_reportes.alerta = vw_alertas.id WHERE ((vw_reportes.estado = 1 AND vw_reportes.repetir = 'S') OR (vw_reportes.estado >= 1 AND vw_reportes.estado < 9 AND vw_reportes.repetir = 'T'))  AND vw_reportes.repetir_tiempo > 0"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            Dim segundos = 0
            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    escalamiento.Enabled = True
                    Exit Sub
                End If

                Application.DoEvents()
                Dim repeticiones As Integer = alerta!repeticiones + 1
                'Se verifica que no se haya repetido antes
                If alerta!repetida.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!repetida, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                If segundos >= alerta!repetir_tiempo Then
                    'Se generan los mensajes a enviar
                    'Se busca una copia del mensaje anterior
                    agregarSolo("Generando repeticiones...")
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    Dim tiempoCad = calcularTiempo(segundos)
                    If ValNull(alerta!repetir_llamada, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 9, canal, prioridad, destino, CONCAT(mensaje, ' *R" & repeticiones & " " & tiempoCad & "') FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal = 1")
                    End If
                    If ValNull(alerta!repetir_sms, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 9, canal, prioridad, destino, CONCAT(mensaje, ' *R" & repeticiones & " " & tiempoCad & "') FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal = 2")
                    End If
                    If ValNull(alerta!repetir_correo, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 9, canal, prioridad, destino, CONCAT(mensaje, ' *R" & repeticiones & " " & tiempoCad & "') FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal = 3")
                    End If
                    If ValNull(alerta!repetir_mmcall, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, 9, canal, prioridad, destino, CONCAT(mensaje, ' *R" & repeticiones & " " & tiempoCad & "') FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo = 0 AND canal = 4")
                    End If
                    agregarLOG("Se envía repetición " & repeticiones & " de alerta para el reporte: " & alerta!id & "-" & alerta!nombre & " a " & tiempoCad, 1, 1, 10)

                    regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET repetida = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "', repeticiones = repeticiones + 1 WHERE id = " & alerta!id)
                End If
            Next
        End If

        escalamiento.Enabled = True
        procesandoEscalamientos = False
        BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"
    End Sub

    Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        MensajeLlamada = SerialPort1.ReadLine
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Try
            Me.Visible = True
            NotifyIcon1.Visible = False
            Me.WindowState = FormWindowState.Maximized
        Catch ex As Exception

        End Try

    End Sub

    Private Sub ContextMenuStrip1_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles ContextMenuStrip1.Opening

    End Sub

    Private Sub XtraForm1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Me.Visible = False
        NotifyIcon1.Visible = True
    End Sub

    Private Sub VerElLogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VerElLogToolStripMenuItem.Click
        Try
            Me.Visible = True
            NotifyIcon1.Visible = False
            Me.WindowState = FormWindowState.Maximized
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ReanudarElMonitorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReanudarElMonitorToolStripMenuItem.Click
        If XtraMessageBox.Show("Esta acción reanudará el envío de alertas. ¿Desea reanudar el monitoreo de las fallas?", "Reanudar la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
            Estado = 1
            SimpleButton3.Visible = True
            SimpleButton2.Visible = False
            ContextMenuStrip1.Items(1).Enabled = True
            ContextMenuStrip1.Items(2).Enabled = False
            estadoPrograma = True
            agregarLOG("La interfaz ha sido reanudada por un usuario", 9, 0)
        End If
    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        If XtraMessageBox.Show("Esta acción detendrá el envío de alertas. ¿Desea detener el monitor de las fallas?", "Detener la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
            Estado = 1
            SimpleButton3.Visible = False
            SimpleButton2.Visible = True
            ContextMenuStrip1.Items(1).Enabled = False
            ContextMenuStrip1.Items(2).Enabled = True
            estadoPrograma = False
            agregarLOG("La interfaz ha sido detenida por un usuario", 9, 0)
        End If
    End Sub

    Private Sub XtraForm1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Dim f As Form
        f = sender

        'Check if the form is minimized
        If f.WindowState = FormWindowState.Minimized Then
            Me.Visible = False
            NotifyIcon1.Visible = True
        End If

    End Sub

    'Sub smtpClient_SendCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs)
    '  Dim mail As MailMessage = e.UserState
    '  If Not e.Cancelled Then
    '  Dim emonteo = 1
    '  End If
    '  End Sub

    Function traducirMensaje(mensaje As String) As String
        traducirMensaje = mensaje
        Dim cadCanales As String = ValNull(mensaje, "A")
        If cadCanales.Length > 0 Then
            traducirMensaje = ""
            Dim arreCanales = cadCanales.Split(New Char() {" "c})
            For i = LBound(arreCanales) To UBound(arreCanales)
                'Redimensionamos el Array temporal y preservamos el valor  
                Dim cadSQL As String = "SELECT traduccion FROM sigma.traduccion WHERE literal = '" & arreCanales(i) & "'"
                Dim reader As DataSet = consultaSEL(cadSQL)
                If reader.Tables(0).Rows.Count > 0 Then
                    traducirMensaje = traducirMensaje & " " & ValNull(reader.Tables(0).Rows(0)!traduccion, "A")
                Else
                    traducirMensaje = traducirMensaje & " " & arreCanales(i)
                End If
            Next
        End If


    End Function

    Private Sub XtraForm1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        autenticado = False
        Dim Forma As New XtraForm2
        Forma.Text = "Detener aplicación"
        Forma.ShowDialog()
        If autenticado Then
            If XtraMessageBox.Show("Esta acción CERRARÁ la aplicación para el envío de alertas. ¿Desea CERRAR el monitor de las fallas?", "Detener la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Stop) <> DialogResult.No Then
                agregarLOG("La aplicación se cerró el usuario: " & usuarioCerrar, 9, 0)
            Else
                e.Cancel = True
            End If
        Else
            e.Cancel = True
        End If
    End Sub

    Private Sub corte_Tick(sender As Object, e As EventArgs) Handles corte.Tick
        If procesandoCorte Or Not estadoPrograma Then Exit Sub
        If Format(Now, "mm") >= "05" Then Exit Sub
        corte.Enabled = False
        procesandoCorte = True
        enviarCorte()
        depurar()
        corte.Enabled = True
        procesandoCorte = False
    End Sub

    Private Sub correos_Tick(sender As Object, e As EventArgs) Handles correos.Tick
        'Se envía correo
        If procesandoCorreos Or Not estadoPrograma Then Exit Sub
        If Format(Now, "mm") >= "05" Then Exit Sub
        correos.Enabled = False
        procesandoCorreos = True
        enviarCorreos()
        procesandoCorreos = False
        correos.Enabled = True
    End Sub



    Private Sub mensajes_Tick(sender As Object, e As EventArgs) Handles mensajes.Tick
        If procesandoMensajes Or Not estadoPrograma Then Exit Sub
        mensajes.Enabled = False
        procesandoMensajes = True
        procesarMensajes()
        procesandoMensajes = False
        mensajes.Enabled = True
    End Sub
    Sub agregarSolo(cadena As String)
        ListBoxControl1.Items.Insert(0, "MONITOR: " & Format(Now, "dd-MMM HH:mm:ss") & ": " & cadena)
        ContarLOG()
    End Sub

    Sub enviarCorte()
        Try
            agregarSolo("Se inicia la aplicación de corte por hora")
            Shell(Application.StartupPath & "\vwCorte.exe", AppWinStyle.MinimizedNoFocus)
        Catch ex As Exception
            agregarLOG("Error en la ejecución de la aplicación de corte por hora. Error: " & ex.Message, 7, 0)
        End Try

    End Sub

    Sub enviarCorreos()
        Try
            agregarSolo("Se inicia la aplicación de Envío de reportes por correo")
            Shell(Application.StartupPath & "\vwReportes.exe", AppWinStyle.MinimizedNoFocus)
        Catch ex As Exception
            agregarLOG("Error en la ejecución de la aplicación de envío de correos. Error: " & ex.Message, 7, 0)
        End Try

    End Sub

    Sub Anterior()
        If procesandoInfoFallas Or Not estadoPrograma Then Exit Sub

        procesandoInfoFallas = True
        revisaFlag.Enabled = False
        Dim cadSQL As String = "SELECT timeout_fallas, flag_agregar FROM sigma.vw_configuracion"
        Dim reader As DataSet = consultaSEL(cadSQL)
        Dim regsAfectados = 0
        If errorBD.Length > 0 Then
            agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
        Else
            If reader.Tables(0).Rows.Count > 0 Then
                If ValNull(reader.Tables(0).Rows(0).Item("flag_agregar"), "A") = "S" Then
                    BarManager1.Items(1).Caption = "Conectado (revisando fallas...)"
                    cadSQL = "SELECT idk, falla, codigo, estacion FROM infofallas.fallascronos WHERE estado = 0"
                    Dim lFallasDS As DataSet = consultaSEL(cadSQL)
                    If errorBD.Length > 0 Then
                        agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + errorBD, 1, 8)
                    Else
                        If lFallasDS.Tables(0).Rows.Count > 0 Then
                            Dim idAlerta = 0
                            Dim nAlerta As String = ""
                            Dim idAlertaMascara = 0

                            For Each lFallas In lFallasDS.Tables(0).Rows
                                Dim fallaID = lFallas!idk
                                Dim fallaCod = ValNull(lFallas!codigo, "A")
                                Dim fallaEst = ValNull(lFallas!estacion, "A")
                                agregarLOG("Se ha detectado una falla en la base de datos de infofallas. ID: " & fallaID & ", Código: " & fallaCod & ", Descripción de origen: " & ValNull(lFallas!falla, "A") & ", Estación:  " & fallaEst, 7, 0, 10)
                                'Se valida la falla dentro de SIGMA para ver si califica o no
                                'Se busca si hay una mascara por prioridad
                                Dim alertaEsc As Boolean = True
                                cadSQL = "SELECT vw_alertas.id, vw_alertas.nombre, vw_alerta_fallas.id as mascara FROM sigma.vw_alertas INNER JOIN sigma.vw_alerta_fallas ON vw_alertas.id = vw_alerta_fallas.alerta WHERE vw_alerta_fallas.estatus = 'A' AND vw_alertas.estatus = 'A' AND (vw_alerta_fallas.estacion = '(Cualquiera)' OR vw_alerta_fallas.estacion = '" & fallaEst & "') AND ((vw_alerta_fallas.comparacion = 1 AND vw_alerta_fallas.prefijo = '" & fallaCod & "') OR (vw_alerta_fallas.comparacion = 2 AND vw_alerta_fallas.prefijo <> '" & fallaCod & "') OR (vw_alerta_fallas.comparacion = 3 AND '" & fallaCod & "' LIKE CONCAT(vw_alerta_fallas.prefijo, '%')) OR (vw_alerta_fallas.comparacion = 4 AND '" & fallaCod & "' NOT LIKE CONCAT(vw_alerta_fallas.prefijo, '%')) OR (vw_alerta_fallas.comparacion = 5 AND '" & fallaCod & "' LIKE CONCAT('%', vw_alerta_fallas.prefijo, '%')) OR (vw_alerta_fallas.comparacion = 6 AND '" & fallaCod & "' NOT LIKE CONCAT('%', vw_alerta_fallas.prefijo, '%')) OR (vw_alerta_fallas.comparacion = 7 AND '" & fallaCod & "' LIKE CONCAT('%', vw_alerta_fallas.prefijo)) OR (vw_alerta_fallas.comparacion = 8 AND '" & fallaCod & "' NOT LIKE CONCAT('%', vw_alerta_fallas.prefijo)) OR (vw_alerta_fallas.comparacion = 9 AND '" & fallaCod & "' > vw_alerta_fallas.prefijo) OR (vw_alerta_fallas.comparacion = 10 AND '" & fallaCod & "' >= vw_alerta_fallas.prefijo) OR (vw_alerta_fallas.comparacion = 11 AND '" & fallaCod & "' < vw_alerta_fallas.prefijo) OR (vw_alerta_fallas.comparacion = 12 AND '" & fallaCod & "' >= vw_alerta_fallas.prefijo)) ORDER BY vw_alertas.prioridad, vw_alertas.modificacion DESC"
                                Dim mascaras As DataSet = consultaSEL(cadSQL)
                                Dim totalAlarmas = 0
                                If Not mascaras.Tables(0).Rows.Count > 0 Then
                                    'Se busca la alerta de escape
                                    cadSQL = "SELECT vw_alertas.id, vw_alertas.nombre FROM sigma.vw_alertas WHERE vw_alertas.estatus = 'A' AND vw_alertas.tipo = 2 AND (vw_alertas.escape_estacion = '(Cualquiera)' OR vw_alertas.escape_estacion = '" & fallaEst & "') ORDER BY vw_alertas.prioridad, vw_alertas.modificacion DESC LIMIT 1"
                                    Dim fallas_escape As DataSet = consultaSEL(cadSQL)
                                    If fallas_escape.Tables(0).Rows.Count > 0 Then
                                        idAlerta = fallas_escape.Tables(0).Rows(0)!id
                                        nAlerta = ValNull(fallas_escape.Tables(0).Rows(0)!nombre, "A")
                                    End If
                                Else
                                    For Each mascara In mascaras.Tables(0).Rows
                                        totalAlarmas = totalAlarmas + 1
                                        If totalAlarmas = 1 Then
                                            idAlerta = mascara!id
                                            nAlerta = ValNull(mascara!nombre, "A")
                                            idAlertaMascara = mascara!mascara
                                        End If
                                    Next
                                    alertaEsc = False
                                End If
                                If idAlerta = 0 Then
                                    agregarLOG("Se encontró una falla sin alerta asociada. Revise la sección de fallas en la alerta o cree una alerta de Escape. Código de falla: " & fallaCod, 2, 0)
                                    regsAfectados = consultaACT("UPDATE sigma.vw_configuracion set ultima_falla = " & fallaID & ", ultima_revision = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "'")
                                    regsAfectados = consultaACT("UPDATE infofallas.fallascronos SET estado = 2 WHERE idk = " & fallaID)
                                Else
                                    If Not alertaEsc Then
                                        If totalAlarmas = 1 Then
                                            agregarLOG("Se encontró una alerta asociada con la falla. Alerta: " & idAlerta & "-" & nAlerta & " línea de la alerta (máscara): " & idAlertaMascara, 1, 0)
                                        Else
                                            agregarLOG("Se encontraron " & totalAlarmas & " alertas asociadas con la falla " & fallaCod, 1, 0)
                                        End If
                                    Else
                                        agregarLOG("Se encontró una alerta ESCAPE asociada con esta falla. Alerta: " & idAlerta & "-" & nAlerta, 2, 0)
                                    End If

                                    'Revisar la lógica de la alerta
                                    If idAlerta > 0 Then revisarAlerta(idAlerta, lFallas!idk)

                                End If
                            Next
                        End If
                    End If
                    BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"

                    regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET flag_agregar = 'N'")
                    If errorBD.Length > 0 Then
                        'Error en la base de datos
                        agregarLOG("Ocurrió un error al intentar ejecutar una actualización en la base de datos de SIGMA. Error: " + errorBD, 9, 0)
                    End If
                End If
            End If
            cadSQL = "SELECT falla FROM sigma.vw_fallas_salientes WHERE estado = 1 LIMIT 1"
            Dim eliminadosDS As DataSet = consultaSEL(cadSQL)
            If errorBD.Length > 0 Then
                agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + errorBD, 9, 0)
            Else
                If eliminadosDS.Tables(0).Rows.Count > 0 Then
                    regsAfectados = consultaACT("UPDATE sigma.vw_fallas_salientes SET estado = 1;")

                    regsAfectados = consultaACT("UPDATE sigma.vw_alarmas SET fin = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), inicio)) WHERE vw_alarmas.falla IN (SELECT falla FROM sigma.vw_fallas_salientes WHERE estado = 1);")
                    If regsAfectados > 0 Then agregarLOG("Se cerraron " & regsAfectados & " falla(s)", 1, 0)
                    regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET estado = 9, atendida = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), activada)) WHERE id NOT IN (SELECT reporte FROM sigma.vw_alarmas WHERE tiempo = 0) AND estado <> 9;")
                    If regsAfectados > 0 Then
                        'Se crea el log masivo
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_log (aplicacion, tipo, reporte, texto) SELECT 0, 1, 0, CONCAT('Se cerró el reporte', id, ' y su(s) falla(s) asociada(s)') FROM sigma.vw_reportes WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N'")
                        regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, (80 + tipo), canal, prioridad, destino, CONCAT('ATENCION El reporte número ', reporte, ' ha sido atendido!') FROM sigma.vw_mensajes WHERE tipo <= 5 and canal <> 4 AND alerta IN (SELECT id FROM sigma.vw_reportes WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N')")
                        regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET informado = 'S' WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N'")
                        'Se informa a los involucrados

                        'cadSQL = "SELECT id FROM sigma.vw_reportes WHERE estado = 9 AND informar_resolucion = 'S' AND informado = 'N'"
                        'Dim alertaDS As DataSet = consultaSEL(cadSQL)
                        'If alertaDS.Tables(0).Rows.Count > 0 Then
                        'For Each alerta In alertaDS.Tables(0).Rows
                        'agregarLOG("Se cerró el reporte " & alerta!id & " y su(s) falla(s) asociada(s)", 1, 0, 10)
                        'regsAfectados = consultaACT("INSERT INTO sigma.vw_mensajes (alerta, lista, tipo, canal, prioridad, destino, mensaje) SELECT alerta, lista, (80 + tipo), canal, prioridad, destino, CONCAT('ATENCION El reporte número ', " & alerta!id & ", ' ha sido atendido!') FROM sigma.vw_mensajes WHERE alerta = " & alerta!id & " AND tipo <= 5 and canal <> 4")
                        'regsAfectados = consultaACT("UPDATE sigma.vw_reportes SET informado = 'S' WHERE id = " & alerta!id)

                        'Next
                        'End If
                    End If
                    regsAfectados = consultaACT("DELETE FROM sigma.vw_fallas_salientes WHERE estado = 1")
                End If

            End If
        End If
        cadSQL = "SELECT id, texto, aplicacion FROM sigma.vw_log WHERE visto_pc = 'N' ORDER BY id"
        reader = consultaSEL(cadSQL)
        regsAfectados = 0
        If reader.Tables(0).Rows.Count > 0 Then
            For Each elmensaje In reader.Tables(0).Rows
                Dim aplicacion = elmensaje!aplicacion
                Dim cadAplicacion = IIf(aplicacion = 0, "MONITOR: ", IIf(aplicacion = 10, "LLAMADAS: ", IIf(aplicacion = 20, "CORREOS: ", IIf(aplicacion = 30, "REPORTES: ", "CORTE: "))))
                ListBoxControl1.Items.Insert(0, cadAplicacion & Format(Now, "dd-MMM HH:mm:ss") & ": " & elmensaje!texto)
                regsAfectados = consultaACT("UPDATE sigma.vw_log SET visto_pc = 'S' WHERE id = " & elmensaje!id)
            Next
            ContarLOG()
        End If
        procesandoInfoFallas = False
        revisaFlag.Enabled = True

    End Sub

    Sub depurar()
        'Se depura la BD
        Dim cadSQL As String = "SELECT gestion_meses, gestion_log FROM sigma.vw_configuracion WHERE gestion_meses > 0 AND (ISNULL(gestion_log) OR gestion_log < '" & Format(Now(), "yyyyMM") & "')"
        Dim reader As DataSet = consultaSEL(cadSQL)
        Dim regsAfectados = 0
        Dim eliminados = 0
        If reader.Tables(0).Rows.Count > 0 Then
            Dim mesesAtras = reader.Tables(0).Rows(0)!gestion_meses
            regsAfectados = consultaACT("DELETE FROM sigma.vw_fallascronos WHERE cierre < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00' AND estado = 9")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM sigma.vw_resumen WHERE creado < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00'")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM sigma.vw_reportes WHERE atendida < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00' AND estado = 9")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM sigma.vw_alarmas WHERE fin < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00'")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM sigma.vw_log WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00'")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM sigma.vw_control WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyyMM") & "0100'")
            eliminados = eliminados + regsAfectados
            agregarLOG("Se ejecutó la depuración de la base de datos para el período " & Format(Now(), "MMMM-yyyy") & " (todo lo anterior al día: " & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01). Se eliminaron permanentemente " & eliminados & " registro(s)", 7, 0)
            regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET gestion_log = '" & Format(Now(), "yyyyMM") & "'")

        End If

    End Sub

End Class

