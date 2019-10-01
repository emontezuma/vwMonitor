Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports System.Data
Imports System.Net.Mail
Imports System.Net

Module basico
    Public errorBD As String
    Public horaDesde As DateTime
    Public ultimaFalla
    Public autenticado As Boolean
    Public usuarioCerrar As String
    Sub Main()

        Dim mensajesDS As DataSet
        Dim eMensaje = ""
        Dim audiosGen = 0
        Dim audiosNGen = 0
        Dim mTotal = 0
        'Escalada 4
        Dim miError As String = ""
        Dim optimizar_correo As Boolean = False
        Dim mantenerPrioridad As Boolean = False
        Dim correo_titulo_falla As Boolean
        Dim correo_titulo As String
        Dim correo_cuerpo As String
        Dim correo_firma As String
        Dim correo_cuenta As String
        Dim correo_puerto As String
        Dim correo_ssl As Boolean
        Dim correo_clave As String
        Dim correo_host As String
        Dim mensajeGenerado As Boolean = False
        Dim regsAfectados = 0

        Dim cadSQL As String = "SELECT * FROM sigma.vw_configuracion"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            optimizar_correo = ValNull(reader!optimizar_correo, "A") = "S"
            mantenerPrioridad = ValNull(reader!optimizar_mmcall, "A") = "S"
            correo_titulo_falla = ValNull(reader!correo_titulo_falla, "A") = "S"
            correo_titulo = ValNull(reader!correo_titulo, "A")
            correo_cuerpo = ValNull(reader!correo_cuerpo, "A")
            correo_firma = ValNull(reader!correo_firma, "A")
            correo_cuenta = ValNull(reader!correo_cuenta, "A")
            correo_clave = ValNull(reader!correo_clave, "A")
            correo_puerto = ValNull(reader!correo_puerto, "A")
            correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
            correo_host = ValNull(reader!correo_host, "A")
        End If

        If Not optimizar_correo Then
            cadSQL = "SELECT *, 1 as cuenta  FROM sigma.vw_mensajes WHERE canal = 3 AND estatus = 'A' ORDER BY prioridad DESC"
        ElseIf mantenerPrioridad Then
            cadSQL = "SELECT prioridad, canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 3 AND estatus = 'A' GROUP BY prioridad, canal, destino ORDER BY prioridad DESC"
        Else
            cadSQL = "SELECT canal, destino, count(*) as cuenta FROM sigma.vw_mensajes WHERE canal = 3 AND estatus = 'A' GROUP BY canal, destino ORDER BY prioridad DESC"
        End If
        'Se preselecciona la voz
        mensajesDS = consultaSEL(cadSQL)
        eMensaje = ""
        audiosGen = 0
        audiosNGen = 0
        mTotal = 0

        If mensajesDS.Tables(0).Rows.Count > 0 Then
            Dim enlazado = False
            Dim errorCorreo = ""
            Dim smtpServer As New SmtpClient()

            Try
                smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                smtpServer.Port = correo_puerto
                smtpServer.Host = correo_host '"smtp.live.com" '"smtp.gmail.com"
                smtpServer.EnableSsl = correo_ssl
                enlazado = True
            Catch ex As Exception
                errorCorreo = ex.Message
            End Try
            If enlazado Then

                For Each elmensaje In mensajesDS.Tables(0).Rows

                    Dim tituloMensaje As String = correo_titulo
                    If optimizar_correo Then
                        If elmensaje!cuenta = 1 Then
                            Dim fPrioridad = ""
                            If mantenerPrioridad Then
                                fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                            End If
                            'Doble cic en el mensaje
                            cadSQL = "SELECT mensaje FROM sigma.vw_mensajes WHERE canal = 3 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad & " "
                            Dim dbMensajes As DataSet = consultaSEL(cadSQL)
                            If dbMensajes.Tables(0).Rows.Count > 0 Then

                                eMensaje = ValNull(dbMensajes.Tables(0).Rows(0)!mensaje, "A")
                            End If
                            If correo_titulo_falla Then
                                tituloMensaje = eMensaje
                            End If
                            If tituloMensaje.Length = 0 Then tituloMensaje = correo_titulo
                            If tituloMensaje.Length = 0 Then tituloMensaje = "Este es un mensaje del Monitor de fallas"

                        Else
                            eMensaje = ""
                            tituloMensaje = tituloMensaje & ": Tiene " & elmensaje!cuenta & " mensaje(s) por atender"
                            cadSQL = "SELECT fecha, mensaje FROM sigma.vw_mensajes WHERE canal = 3 AND destino = '" & elmensaje!destino & "' AND estatus = 'A'"
                            Dim dbMensajesDS As DataSet = consultaSEL(cadSQL)
                            If dbMensajesDS.Tables(0).Rows.Count > 0 Then
                                For Each dbMensajes In dbMensajesDS.Tables(0).Rows
                                    eMensaje = eMensaje & Format(dbMensajes!fecha, "dd/MM/yyyy HH:mm:ss") & ": " & ValNull(dbMensajes!mensaje, "A") & vbCrLf
                                Next
                            End If

                        End If
                    Else
                        eMensaje = ValNull(elmensaje!mensaje, "A")
                    End If
                    mTotal = mTotal + elmensaje!cuenta
                    'Se crea el audio
                    mensajeGenerado = False

                    If eMensaje.Length > 0 Then
                        Dim mail As New MailMessage
                        Try
                            mail.From = New MailAddress(correo_cuenta) 'TextBox1.Text & "@gmail.com")
                            mail.To.Add(elmensaje!destino)
                            mail.Subject = tituloMensaje
                            mail.Body = correo_cuerpo & vbCrLf & vbCrLf & eMensaje & vbCrLf & vbCrLf & correo_firma
                            smtpServer.Send(mail)
                            'Dim nNvoMensaje As Object
                            'nNvoMensaje = mail
                            '
                            '                            AddHandler smtpServer.SendCompleted, AddressOf smtpClient_SendCompleted
                            '                           smtpServer.SendAsync(mail, nNvoMensaje)
                            audiosGen = audiosGen + 1
                            mensajeGenerado = True
                        Catch ex As Exception
                            agregarLOG("Errores en la generación de correos electrónicos. No se generaron " & audiosNGen & " correos electrónicos. Error: " & ex.Message, 1, 0)
                        End Try
                    Else
                        mensajeGenerado = True
                    End If
                    If mensajeGenerado Then
                        If optimizar_correo Then
                            Dim fPrioridad = ""
                            If mantenerPrioridad Then
                                fPrioridad = " AND (prioridad = '" & elmensaje!prioridad & "' OR ISNULL(prioridad)) "
                            End If
                            'Doble cic en el mensaje
                            regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE canal = 3 AND destino = '" & elmensaje!destino & "' AND estatus = 'A' " & fPrioridad)
                        Else
                            regsAfectados = consultaACT("UPDATE sigma.vw_mensajes SET estatus = 'Z' WHERE id = " & elmensaje!id)
                        End If
                    End If
                Next
            End If
            If enlazado Then
                If audiosGen > 0 Then
                    agregarLOG("Se " & IIf(audiosGen = 1, " envío 1 correo electrónico ", "enviaron " & audiosGen & " correos electrónicos") & " que incluye(n) " & mTotal & " notifación(es)", 1, 0)
                Else
                    If audiosNGen > 0 Then
                        agregarLOG("Errores en la generación de correos electrónicos. No se generaron " & audiosNGen & " correo(s) electrónico(s). Error: " & miError, 1, 0)
                    End If
                End If

            Else
                agregarLOG("Hubo un error en la conexión al servidor de correos. El error es: " & errorCorreo, 7, 0)
            End If
            smtpServer.Dispose()
        End If
        miError = ""
    End Sub
    Private Sub agregarLOG(cadena As String, tipo As Integer, reporte As Integer, Optional aplicacion As Integer = 1)
        'Se agrega a la base de datos
        'tipo 1: Info
        'tipo 2: Incongruencia en los datos (usuario)
        'tipo 8: Error crítico de Base de datos infofallas
        'tipo 9: Error crítico de Base de datos sigma
        Dim regsAfectados = consultaACT("INSERT INTO sigma.vw_log (aplicacion, tipo, reporte, texto) VALUES (20, " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
        If aplicacion = 10 Then
            regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET flag_monitor = 'S'")
        End If
    End Sub
    Public Function consultaACT(cadena As String) As Integer
        Dim miConexion = New MySqlConnection

        miConexion.ConnectionString = cadenaConexion()

        miConexion.Open()
        consultaACT = 0
        errorBD = ""
        If miConexion.State = ConnectionState.Open Then
            Try
                Dim comandoSQL As MySqlCommand = New MySqlCommand(cadena)
                comandoSQL.Connection = miConexion
                consultaACT = comandoSQL.ExecuteNonQuery()

            Catch ex As Exception
                errorBD = ex.Message
            End Try
        End If
        miConexion.Dispose()
        miConexion.Close()
        miConexion = Nothing
    End Function

    Public Function consultaSEL(cadena As String) As Data.DataSet

        Try
            errorBD = ""
            Dim miConexion = New MySqlConnection

            miConexion.ConnectionString = cadenaConexion()

            miConexion.Open()

            If miConexion.State = ConnectionState.Open Then
                Try
                    Dim comandoSQL As MySqlCommand = New MySqlCommand(cadena)
                    comandoSQL.Connection = miConexion
                    Dim adapter As New MySqlDataAdapter(comandoSQL)
                    Dim LaData As New DataSet
                    adapter.Fill(LaData, "miData")

                    Return LaData
                Catch ex As Exception
                    errorBD = ex.Message
                End Try
            End If
            miConexion.Dispose()
            miConexion.Close()
            miConexion = Nothing
        Catch ex As Exception
            errorBD = ex.Message
        End Try

    End Function

    Function ValNull(ByVal ArVar As Object, ByVal arTipo As String) As Object
        Try
            'para columnas vacias sin datos
            If ArVar.Equals(System.DBNull.Value) Then
                Select Case arTipo
                    Case "A"
                        ValNull = ""
                    Case "N"
                        ValNull = 0
                    Case "D"
                        ValNull = 0
                    Case "F"
                        ValNull = CDate("00/00/0000")
                    Case "DT"
                        ValNull = New DateTime(1, 1, 1)
                    Case Else
                        ValNull = ""
                End Select
                Exit Function
            End If

            If Len(ArVar) > 0 Then
                Select Case arTipo
                    Case "A"
                        ValNull = ArVar
                    Case "N"
                        ValNull = Val(ArVar)
                    Case "D"
                        ValNull = CDec(ArVar)
                    Case "F"
                        If ArVar = "0" Then
                            ValNull = ""
                        Else
                            If InStr(ArVar, "/") > 0 Then
                                ValNull = ArVar
                            Else
                                ValNull = Format(ArVar, "dd/MM/yyyy")
                            End If
                        End If
                    Case Else
                        ValNull = ArVar
                End Select
            Else
                Select Case arTipo
                    Case "A"
                        ValNull = ""
                    Case "N"
                        ValNull = 0
                    Case "D"
                        ValNull = 0
                    Case "F"
                        ValNull = CDate("dd/MM/yyyy")
                    Case Else
                        ValNull = ""
                End Select
            End If
        Catch ex As Exception
            Select Case arTipo
                Case "A"
                    ValNull = ""
                Case "N"
                    ValNull = 0
                Case "D"
                    ValNull = 0
                Case "F"
                    ValNull = CDate("00000000")
                Case Else
                    ValNull = " "
            End Select
        End Try
    End Function

    Function cadenaConexion() As String
        cadenaConexion = "server=localhost;user id=root;password=usbw;port=3307;Convert Zero Datetime=True"
    End Function


End Module
