Imports MySql.Data.MySqlClient
Imports System.IO.Ports
Imports System.IO
Imports System.Text
Imports System.Net.Mail
Imports System.Net
Imports System.ComponentModel
Imports System.Data
Imports System.Windows.Forms
Imports DevExpress.XtraCharts
Imports DevExpress.XtraGauges.Win
Imports DevExpress.XtraGauges.Win.Base
Imports DevExpress.XtraGauges.Win.Gauges.Circular
Imports DevExpress.XtraGauges.Core.Model
Imports DevExpress.XtraGauges.Core.Base
Imports DevExpress.XtraGauges.Core.Drawing
Imports System.Drawing
Imports System.Drawing.Imaging


Public Class Form1

    Dim Estado As Integer = 0
    Dim procesandoAudios As Boolean = False
    Dim eSegundos = 0
    Dim procesandoEscalamientos As Boolean
    Dim procesandoRepeticiones As Boolean
    Dim estadoPrograma As Boolean
    Dim MensajeLlamada = ""
    Dim errorCorreos As String = ""
    Dim cad_consolidado As String = "CONSOLIDADO"
    Dim bajo_color As String
    Dim medio_color As String
    Dim alto_color As String
    Dim escaladas_color As String
    Dim noatendio_color As String
    Dim alto_etiqueta As String
    Dim escaladas_etiqueta As String
    Dim noatendio_etiqueta As String
    Dim bajo_hasta As Integer
    Dim medio_hasta As Integer


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        estadoPrograma = True
        enviarReportes()
        Me.Close()
    End Sub

    Private Sub enviarReportes()
        'Se envía correo

        Dim cadSQL As String = "SELECT * FROM sigma.vw_control WHERE fecha = '" & Format(Now, "yyyyMMddHH") & "'"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        If readerDS.Tables(0).Rows.Count > 0 Then
            Exit Sub
        End If


        Dim regsAfectados = 0
        'Escalada 4
        Dim miError As String = ""
        Dim correo_cuenta As String
        Dim correo_puerto As String
        Dim correo_ssl As Boolean
        Dim correo_clave As String
        Dim correo_host As String
        Dim rutaFiles As String

        cadSQL = "SELECT * FROM sigma.vw_configuracion"
        readerDS = consultaSEL(cadSQL)
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            correo_cuenta = ValNull(reader!correo_cuenta, "A")
            correo_clave = ValNull(reader!correo_clave, "A")
            correo_puerto = ValNull(reader!correo_puerto, "A")
            correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
            correo_host = ValNull(reader!correo_host, "A")
            rutaFiles = ValNull(reader!ruta_archivos_enviar, "A")
            alto_etiqueta = ValNull(reader!alto_etiqueta, "A")
            escaladas_etiqueta = ValNull(reader!escaladas_etiqueta, "A")
            noatendio_etiqueta = ValNull(reader!noatendio_etiqueta, "A")
            cad_consolidado = ValNull(reader!cad_consolidado, "A")
            alto_color = ValNull(reader!alto_color, "A")
            medio_color = ValNull(reader!medio_color, "A")
            bajo_color = ValNull(reader!bajo_color, "A")
            escaladas_color = ValNull(reader!escaladas_color, "A")
            noatendio_color = ValNull(reader!noatendio_color, "A")
            bajo_hasta = ValNull(reader!bajo_hasta, "N")
            medio_hasta = ValNull(reader!medio_hasta, "N")
        End If
        If bajo_hasta = 0 Then bajo_hasta = 50
        If medio_hasta = 0 Then medio_hasta = 75
        If alto_etiqueta.Length = 0 Then alto_etiqueta = "Buenas"
        If escaladas_etiqueta.Length = 0 Then escaladas_etiqueta = "Escaladas"
        If noatendio_etiqueta.Length = 0 Then noatendio_etiqueta = "No atendidas"
        alto_color = Microsoft.VisualBasic.Strings.Replace(alto_color, "HEX", "#")
        escaladas_color = Microsoft.VisualBasic.Strings.Replace(escaladas_color, "HEX", "#")
        noatendio_color = Microsoft.VisualBasic.Strings.Replace(noatendio_color, "HEX", "#")
        If alto_color.Length = 0 Then alto_color = System.Drawing.Color.LimeGreen.ToString
        If escaladas_color.Length = 0 Then escaladas_color = System.Drawing.Color.OrangeRed.ToString
        If noatendio_color.Length = 0 Then noatendio_color = System.Drawing.Color.Tomato.ToString


        If rutaFiles.Length = 0 Then
            rutaFiles = My.Computer.FileSystem.SpecialDirectories.MyDocuments
        Else
            If Not My.Computer.FileSystem.DirectoryExists(rutaFiles) Then
                Try
                    My.Computer.FileSystem.CreateDirectory(rutaFiles)
                Catch ex As Exception
                    rutaFiles = My.Computer.FileSystem.SpecialDirectories.MyDocuments
                End Try
            End If
        End If

        If Not estadoPrograma Then
            Exit Sub
        End If
        cadSQL = "Select * FROM sigma.cat_correos WHERE estatus = 'A'"
        'Se preselecciona la voz
        Dim indice = 0

        Dim mensajesDS As DataSet = consultaSEL(cadSQL)
        Dim mensajeGenerado = False
        Dim tMensajes = 0

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
                    'Se busca si hay uno del día y hra
                    Dim periodicidad As String = ValNull(elmensaje!frecuencia, "A")
                    If periodicidad.Length > 0 Then
                        Dim envio As String() = periodicidad.Split(New Char() {";"c})
                        If envio(0).Length > 0 And envio(1).Length > 0 Then
                            Dim enviarDia As Boolean = False
                            Dim diaSemana = DateAndTime.Weekday(Now)
                            Dim cadFrecuencia As String = "Este reporte se le envía Todos los días"
                            If envio(0) = "T" Then
                                enviarDia = True
                            ElseIf envio(0) = "LV" And diaSemana >= 2 And diaSemana <= 6 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía de lunes a viernes"
                            ElseIf envio(0) = "L" And diaSemana = 2 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los lunes"
                            ElseIf envio(0) = "M" And diaSemana = 3 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los martes"
                            ElseIf envio(0) = "MI" And diaSemana = 4 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los miércoles"
                            ElseIf envio(0) = "J" And diaSemana = 5 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los jueves"
                            ElseIf envio(0) = "V" And diaSemana = 6 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los viernes"
                            ElseIf envio(0) = "S" And diaSemana = 7 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los sábados"
                            ElseIf envio(0) = "D" And diaSemana = 1 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los domingos"
                            ElseIf envio(0) = "1M" And Val(Today.Day) = 1 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía el primer día del mes"
                            ElseIf envio(0) = "UM" And Val(Today.Day) = Date.DaysInMonth(Today.Year, Today.Month) Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía el último día del mes"
                            End If
                            If enviarDia Then
                                Dim enviar As Boolean = False
                                Dim hora = Val(Format(Now, "HH"))
                                If envio(1) = "T" Then
                                    enviar = True
                                    cadFrecuencia = cadFrecuencia & " a cada hora"
                                ElseIf Val(envio(1)) = Val(hora) Then
                                    cadFrecuencia = cadFrecuencia & IIf(Val(hora) = 1, " a la 1:00am", "a las " & Val(hora) & " horas")
                                    enviar = True
                                End If
                                If enviar Then
                                    Dim reportes As String() = elmensaje!reportes.Split(New Char() {";"c})
                                    Dim periodos As String() = elmensaje!periodos.Split(New Char() {";"c})
                                    Dim nperiodos As String() = elmensaje!nperiodos.Split(New Char() {";"c})
                                    Dim mail As New MailMessage
                                    Try
                                        Dim cuerpo As String = ValNull(elmensaje!cuerpo, "A")
                                        Dim titulo As String = ValNull(elmensaje!titulo, "A")
                                        If titulo.Length = 0 Then titulo = "Reportes automáticos, aplicación de monitor de fallas"
                                        If cuerpo.Length = 0 Then cuerpo = "Se le ha enviado este correo. No responda ya que esta cuenta no es monitoreada"

                                        mail.From = New MailAddress(correo_cuenta) 'TextBox1.Text & "@gmail.com")
                                        Dim mails As String = ValNull(elmensaje!para, "A")
                                        Dim mailsV As String() = mails.Split(New Char() {";"c})
                                        For Each cuenta In mailsV
                                            If cuenta.Length > 0 Then
                                                mail.To.Add(cuenta)
                                            End If
                                        Next
                                        mails = ValNull(elmensaje!copia, "A")
                                        mailsV = mails.Split(New Char() {";"c})
                                        For Each cuenta In mailsV
                                            If cuenta.Length > 0 Then
                                                mail.CC.Add(cuenta)
                                            End If
                                        Next
                                        mails = ValNull(elmensaje!oculta, "A")
                                        mailsV = mails.Split(New Char() {";"c})
                                        For Each cuenta In mailsV
                                            If cuenta.Length > 0 Then
                                                mail.Bcc.Add(cuenta)
                                            End If
                                        Next
                                        mail.Subject = titulo
                                        errorCorreos = ""
                                        cuerpo = cuerpo & vbCrLf & "Reportes a enviar: "
                                        If reportes(0) <> "N" Then

                                            Dim miReporte = generarReporte1(reportes(0), periodos(0), nperiodos(0), rutaFiles)
                                            If miReporte = -1 Then
                                                cuerpo = cuerpo & vbCrLf & "Reporte de fallas por estación (frecuencia): SIN DATOS (por error) " & errorCorreos
                                            Else

                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\fallas_por_estacion.csv") Then
                                                    cuerpo = cuerpo & vbCrLf & "Reporte de fallas por estación (frecuencia)"
                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\fallas_por_estacion.csv")
                                                    mail.Attachments.Add(archivo)
                                                Else
                                                    cuerpo = cuerpo & vbCrLf & "Reporte de fallas por estación (frecuencia): SIN DATOS"
                                                End If
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\fallas_por_estacion.png") Then

                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\fallas_por_estacion.png")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                            End If
                                        End If

                                        errorCorreos = ""
                                        If reportes(1) <> "N" Then

                                            Dim miReporte = generarReporte2(reportes(1), periodos(1), nperiodos(1), rutaFiles)
                                            If miReporte = -1 Then
                                                cuerpo = cuerpo & vbCrLf & "Reporte de fallas por estación (frecuencia y tiempo): SIN DATOS (por error) " & errorCorreos
                                            Else
                                                cuerpo = cuerpo & vbCrLf & "Reporte de fallas por estación (frecuencia y tiempo)"
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\fallas_por_estacion_tiempo.csv") Then
                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\fallas_por_estacion_tiempo.csv")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\fallas_por_estacion_tiempo.png") Then

                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\fallas_por_estacion_tiempo.png")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                            End If
                                        End If
                                        errorCorreos = ""
                                        If reportes(2) <> "N" Then
                                            Dim miReporte = generarReporte3(reportes(2), periodos(2), nperiodos(2), rutaFiles)
                                            If miReporte = -1 Then
                                                cuerpo = cuerpo & vbCrLf & "Reporte de fallas por tecnología (frecuencia y tiempo): SIN DATOS (por error) " & errorCorreos
                                            Else
                                                cuerpo = cuerpo & vbCrLf & "Reporte de fallas por tecnología (frecuencia y tiempo)"
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\fallas_por_tecnologia_tiempo.csv") Then
                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\fallas_por_tecnologia_tiempo.csv")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\fallas_por_tecnologia_tiempo.png") Then

                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\fallas_por_tecnologia_tiempo.png")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                            End If
                                        End If
                                        If reportes(3) <> "N" Then
                                            Dim miReporte = generarReporte4(reportes(3), periodos(3), nperiodos(3), rutaFiles)
                                            If miReporte = -1 Then
                                                cuerpo = cuerpo & vbCrLf & "Reporte de 10 fallas más altas (ordenado por frecuencia): SIN DATOS (por error) " & errorCorreos
                                            Else
                                                cuerpo = cuerpo & vbCrLf & "Reporte de 10 fallas más altas (ordenado por frecuencia)"
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\top_10_fallas.csv") Then
                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\top_10_fallas.csv")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                            End If
                                        End If
                                        If reportes(4) <> "N" Then
                                            Dim miReporte = generarReporte5(reportes(4), periodos(4), nperiodos(4), rutaFiles)
                                            If miReporte = -1 Then
                                                cuerpo = cuerpo & vbCrLf & "Reporte de rendimiento por staff: SIN DATOS (por error) " & errorCorreos
                                            Else
                                                cuerpo = cuerpo & vbCrLf & "Reporte de rendimiento por staff"
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\rendimiento_staff.csv") Then
                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\rendimiento_staff.csv")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\rendimiento_staff.png") Then

                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\rendimiento_staff.png")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                            End If
                                        End If
                                        If reportes(5) <> "N" Then
                                            Dim miReporte = generarReporte6(reportes(5), periodos(5), nperiodos(5), rutaFiles)
                                            If miReporte = -1 Then
                                                cuerpo = cuerpo & vbCrLf & "Reporte de estadistica de fallas: SIN DATOS (por error) " & errorCorreos
                                            Else
                                                cuerpo = cuerpo & vbCrLf & "Reporte de estadistica de fallas."
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\estadistica_de_fallas.csv") Then
                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\estadistica_de_fallas.csv")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                            End If
                                        End If

                                        If reportes(6) <> "N" Then
                                            Dim miReporte = generarReporte7(rutaFiles)
                                            If miReporte = -1 Then
                                                cuerpo = cuerpo & vbCrLf & "Reporte de fallas abiertas al momento: SIN DATOS (por error) " & errorCorreos
                                            Else
                                                cuerpo = cuerpo & vbCrLf & "Reporte de fallas abiertas al momento"
                                                If My.Computer.FileSystem.FileExists(rutaFiles & "\fallas_abiertas.csv") Then
                                                    Dim archivo As Attachment = New Attachment(rutaFiles & "\fallas_abiertas.csv")
                                                    mail.Attachments.Add(archivo)
                                                End If
                                            End If
                                        End If
                                        cuerpo = cadFrecuencia & vbCrLf & vbCrLf & cuerpo
                                        mail.Body = cuerpo
                                        smtpServer.Send(mail)
                                        tMensajes = tMensajes + 1
                                        mensajeGenerado = True
                                    Catch ex As Exception
                                        Dim miEror = ex.Message
                                    End Try
                                Else
                                    mensajeGenerado = True
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            If enlazado Then
                If tMensajes > 0 Then
                    agregarLOG("Se enviaron " & tMensajes & " reporte(s) vía correo electrónico  ", 1, 0)
                Else
                    agregarLOG("No se enviaron reportes vía correo electrónico", 1, 0)
                End If

            Else
                agregarLOG("Hubo un error en la conexión al servidor de correos. El error es: " & errorCorreo, 7, 0)
            End If
            smtpServer.Dispose()
        End If
        regsAfectados = consultaACT("INSERT INTO sigma.vw_control (fecha, mensajes) VALUES ('" & Format(Now, "yyyyMMddHH") & "', " & tMensajes & ")")
    End Sub

    Function generarReporte1(reporte As String, periodo As String, nperiodos As Integer, ruta As String) As Integer
        generarReporte1 = 0
        Try
            My.Computer.FileSystem.DeleteFile(ruta & "\fallas_por_estacion.csv")
            My.Computer.FileSystem.DeleteFile(ruta & "\fallas_por_estacion.png")


        Catch ex As Exception

        End Try

        Dim eDesde = Now()
        Dim eHasta = Now()
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        Dim cadPeriodo As String = nperiodos & " segundo(s) atras"
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & " minuto(s) atras"
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & " hora(s) atras"
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & " día(s) atras"
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & " semana(s) atras"
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & " mes(es) atras"
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & " año(s) atras"
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = "Lo que va del día de hoy"
        ElseIf periodo = 11 Then
            cadPeriodo = "Lo que va de la semana"
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = "Lo que va del mes"
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = "Lo que va del anyo"
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = "El día de ayer"
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = "La semana pasada"
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = "El mes pasado"
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        Dim fDesde = Format(eDesde, "yyyy/MM/dd HH:mm:ss")
        Dim fHasta = Format(eHasta, "yyyy/MM/dd HH:mm:ss")
        Dim cadSQL As String = "SELECT vw_alarmas.estacion AS estacion, SUM(IF(vw_alarmas.reporte > 0, 1, 0)) AS total, SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) AS buenas, SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0)) AS tarde, SUM(IF(vw_alarmas.tiempo = 0, 1, 0)) AS noatendio FROM sigma.vw_alarmas LEFT JOIN sigma.vw_reportes ON vw_alarmas.reporte = vw_reportes.id WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' GROUP BY vw_alarmas.estacion ORDER BY 2 DESC"
        Dim reader As DataSet = consultaSEL(cadSQL)
        Dim regsAfectados = 0
        If errorBD.Length > 0 Then
            agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
            errorCorreos = errorBD
            generarReporte1 = -1
        Else
            If reader.Tables(0).Rows.Count > 0 Then
                Dim cadExportar As String = ""

                'Creación de la gráfica

                '
                If reporte = "T" Or reporte = "D" Then
                    cadExportar = "Reporte de fallas por estacion (frecuencia)" & vbCrLf
                    cadExportar = cadExportar & "Extraccion de datos: " & cadPeriodo & vbCrLf
                    cadExportar = cadExportar & "Generado el: " & Format(Now, "ddd dd-MM-yyyy HH:mm:ss") & vbCrLf
                    cadExportar = cadExportar & "Extrayendo los datos desde: " & Format(eDesde, "dd-MM-yyyy HH:mm:ss") & " hasta: " & Format(eHasta, "dd-MM-yyyy HH:mm:ss") & vbCrLf & vbCrLf

                    cadExportar = cadExportar & "Atencion"
                    For Each lineas In reader.Tables(0).Rows
                        Dim miEstacion As String = ValNull(lineas!estacion, "A")
                        cadExportar = cadExportar & "," & IIf(miEstacion.Length > 0, miEstacion, "N/A")
                    Next
                    cadExportar = cadExportar & vbCrLf
                    cadExportar = cadExportar & "Total"

                    For Each lineas In reader.Tables(0).Rows
                        cadExportar = cadExportar & "," & lineas!total
                    Next
                    cadExportar = cadExportar & vbCrLf
                    cadExportar = cadExportar & alto_etiqueta
                    For Each lineas In reader.Tables(0).Rows
                        cadExportar = cadExportar & "," & lineas!buenas
                    Next
                    cadExportar = cadExportar & vbCrLf
                    cadExportar = cadExportar & escaladas_etiqueta
                    For Each lineas In reader.Tables(0).Rows
                        cadExportar = cadExportar & "," & lineas!tarde
                    Next
                    cadExportar = cadExportar & vbCrLf
                    cadExportar = cadExportar & noatendio_etiqueta
                    For Each lineas In reader.Tables(0).Rows
                        cadExportar = cadExportar & "," & lineas!noatendio
                    Next
                    cadExportar = cadExportar & vbCrLf & vbCrLf
                    cadExportar = cadExportar & "Total estacion(es): " & reader.Tables(0).Rows.Count

                    Try
                        System.IO.File.Create(ruta & "\fallas_por_estacion.csv").Dispose()
                        Dim objWriter As New System.IO.StreamWriter(ruta & "\fallas_por_estacion.csv", True)
                        objWriter.WriteLine(cadExportar)
                        objWriter.Close()
                        generarReporte1 = reader.Tables(0).Rows.Count
                    Catch ex As Exception
                        errorCorreos = ex.Message
                        generarReporte1 = -1
                        agregarLOG("Ocurrió un error al intentar construir un archivo de adjunto de reporte. Error: " + ex.Message, 7, 0)
                    End Try
                End If
                If reporte = "T" Or reporte = "G" Then
                    'Se produce el gráfico
                    Dim Titulo As New ChartTitle()

                    Titulo.Text = "    Gráfica de Fallas por estación    "
                    Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                    Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                    Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                    Titulo.Font = miFuenteAlto


                    ' Create an empty table.
                    Dim datos As New DataTable("grafico")

                    ' Add two columns to the table.
                    datos.Columns.Add("estacion", GetType(String))
                    datos.Columns.Add("total", GetType(Int32))
                    datos.Columns.Add("buenas", GetType(Int32))
                    datos.Columns.Add("tarde", GetType(Int32))
                    datos.Columns.Add("noatendio", GetType(Int32))

                    ' Add data rows to the table.
                    Dim row As DataRow = Nothing
                    For Each lineas In reader.Tables(0).Rows
                        row = datos.NewRow()
                        row("estacion") = lineas!estacion
                        row("total") = lineas!total
                        row("buenas") = lineas!buenas
                        row("tarde") = lineas!tarde
                        row("noatendio") = lineas!noatendio
                        datos.Rows.Add(row)
                    Next
                    Dim series As New Series("Todas las llamadas", ViewType.Bar)


                    ChartControl1.Series.Add(series)
                    series.DataSource = datos
                    series.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series.View.Color = Color.Gray
                    series.ArgumentScaleType = ScaleType.Qualitative
                    series.ArgumentDataMember = "estacion"
                    series.ValueScaleType = ScaleType.Numerical
                    series.ValueDataMembers.AddRange(New String() {"total"})
                    series.Label.BackColor = Color.LightGray
                    series.Label.TextColor = Color.Black
                    series.Label.Font = miFuente

                    series = New Series(alto_etiqueta, ViewType.Bar)
                    ChartControl1.Series.Add(series)
                    series.DataSource = datos
                    series.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series.View.Color = ColorTranslator.FromHtml(alto_color)
                    series.ArgumentScaleType = ScaleType.Qualitative
                    series.ArgumentDataMember = "estacion"
                    series.ValueScaleType = ScaleType.Numerical
                    series.ValueDataMembers.AddRange(New String() {"buenas"})
                    series.Label.BackColor = Color.LightGray
                    series.Label.TextColor = Color.Black
                    series.Label.Font = miFuente


                    series = New Series(escaladas_etiqueta, ViewType.Bar)
                    ChartControl1.Series.Add(series)
                    series.DataSource = datos
                    series.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series.View.Color = ColorTranslator.FromHtml(escaladas_color)
                    series.ArgumentScaleType = ScaleType.Qualitative
                    series.ArgumentDataMember = "estacion"
                    series.ValueScaleType = ScaleType.Numerical
                    series.ValueDataMembers.AddRange(New String() {"tarde"})
                    series.Label.BackColor = Color.LightGray
                    series.Label.TextColor = Color.Black
                    series.Label.Font = miFuente


                    series = New Series(noatendio_etiqueta, ViewType.Bar)
                    ChartControl1.Series.Add(series)
                    series.DataSource = datos
                    series.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series.View.Color = ColorTranslator.FromHtml(noatendio_color)
                    series.ArgumentScaleType = ScaleType.Qualitative
                    series.ArgumentDataMember = "estacion"
                    series.ValueScaleType = ScaleType.Numerical
                    series.ValueDataMembers.AddRange(New String() {"noatendio"})
                    series.Label.BackColor = Color.LightGray
                    series.Label.TextColor = Color.Black
                    series.Label.Font = miFuente

                    ' Set some properties to get a nice-looking chart.
                    CType(series.View, SideBySideBarSeriesView).ColorEach = False
                    CType(series.Label, SideBySideBarSeriesLabel).ResolveOverlappingMode = ResolveOverlappingMode.HideOverlapped
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = "Número de llamadas en el período"
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = "    Estaciones con llamada    "
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True

                    ChartControl1.Titles.Add(Titulo)
                    Dim Titulo2 As New ChartTitle()

                    Titulo2.Font = miFuente
                    Titulo2.Text = "Extraccion de datos: " & cadPeriodo
                    ChartControl1.Titles.Add(Titulo2)
                    Dim Titulo3 As New ChartTitle()
                    Titulo3.Font = miFuente
                    Titulo3.Text = "Generado el: " & Format(Now, "ddd dd-MM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo3)
                    Dim Titulo4 As New ChartTitle()
                    Titulo4.Font = miFuente
                    Titulo4.Text = "Extrayendo los datos desde: " & Format(eDesde, "dd-MM-yyyy HH:mm:ss") & " hasta: " &
                        Format(eHasta, "dd-MM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo4)
                    ChartControl1.Width = 1000
                    ChartControl1.Height = 700
                    Try
                        Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(ruta & "\fallas_por_estacion.png", "\", "\\")
                        SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                        Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                        image.Save(rutaImagen)

                    Catch ex As Exception
                        agregarLOG("Ocurrió un error al intentar construir un archivo de adjunto de reporte (gráfico). Error: " + ex.Message, 7, 0)
                    End Try


                    'No hay datos, notificar
                End If
            End If
        End If
    End Function

    Function generarReporte2(reporte As String, periodo As String, nperiodos As Integer, ruta As String) As Integer


        generarReporte2 = 0

        Dim archivoSaliente = ruta & "\fallas_por_estacion_tiempo.csv"
        Dim archivoImagen = ruta & "\fallas_por_estacion_tiempo.png"
        If My.Computer.FileSystem.FileExists(archivoSaliente) Then
            Try
                My.Computer.FileSystem.DeleteFile(archivoSaliente)
            Catch ex As Exception
                errorCorreos = ex.Message
                agregarLOG("Error al construir el reporte. " + ex.Message, 7, 0)
                generarReporte2 = -1
                Exit Function
            End Try
        End If
        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)
        Catch ex As Exception

        End Try


        archivoSaliente = archivoSaliente.Replace("\", "\\")

        Dim eDesde = Now()
        Dim eHasta = Now()
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        Dim cadPeriodo As String = nperiodos & " segundo(s) atras"
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & " minuto(s) atras"
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & " hora(s) atras"
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & " día(s) atras"
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & " semana(s) atras"
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & " mes(es) atras"
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & " año(s) atras"
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = "Lo que va del día de hoy"
        ElseIf periodo = 11 Then
            cadPeriodo = "Lo que va de la semana"
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = "Lo que va del mes"
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = "Lo que va del anyo"
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = "El día de ayer"
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = "La semana pasada"
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = "El mes pasado"
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        Dim fDesde = Format(eDesde, "yyyy/MM/dd HH:mm:ss")
        Dim fHasta = Format(eHasta, "yyyy/MM/dd HH:mm:ss")

        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)
        If reporte = "T" Or reporte = "D" Then

            Dim regsAfectados = consultaACT("USE sigma;
        SELECT * FROM 
        (SELECT 'Reporte de fallas atendidas por estacion - Frecuencia y tiempo (" & cadPeriodo & ")','' as b,'' as c,'' as d 
        UNION 
        (SELECT CONCAT('Reporte generado en fecha: ', NOW()),'','','') 
        UNION 
        (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'),'','','') 
        UNION 
        (SELECT 'Estacion','Frecuencia','Tiempo total (min)','Tiempo total (seg)') 
        UNION
        (SELECT IFNULL(vw_alarmas.estacion, 'N/A'), COUNT(*), ROUND(SUM(vw_alarmas.tiempo / 60), 0), SUM(vw_alarmas.tiempo) FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' AND vw_alarmas.tiempo > 0 GROUP BY vw_alarmas.estacion ORDER BY 2 DESC) 
        UNION 
        (SELECT 'TOTAL reporte: ', COUNT(*), ROUND(SUM(vw_alarmas.tiempo / 60), 0), SUM(vw_alarmas.tiempo) FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' AND vw_alarmas.tiempo > 0)) as query01  
        INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")
            If errorBD.Length > 0 Then
                errorCorreos = errorBD
                agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
                generarReporte2 = -1
            End If
        End If
        If reporte = "T" Or reporte = "G" Then
            'Se produce el gráfico
            Dim cadSQL As String = "SELECT IFNULL(vw_alarmas.estacion, 'N/A') as estacion, COUNT(*) as total, ROUND(SUM(vw_alarmas.tiempo / 60), 0) as tiempo FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' AND vw_alarmas.tiempo > 0 GROUP BY vw_alarmas.estacion ORDER BY 2 DESC"
            Dim reader As DataSet = consultaSEL(cadSQL)
            Dim regsAfectados = 0
            If errorBD.Length > 0 Then
                agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
                errorCorreos = errorBD
                generarReporte2 = -1
            Else
                If reader.Tables(0).Rows.Count > 0 Then
                    ChartControl1.Series.Clear()
                    ChartControl1.Titles.Clear()
                    Dim Titulo As New ChartTitle()
                    Titulo.Text = "    Gráfica de Fallas por estación (frecuencia y tiempo)   "
                    Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                    Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                    Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                    Titulo.Font = miFuenteAlto


                    ' Create an empty table.
                    Dim datos As New DataTable("grafico")

                    ' Add two columns to the table.
                    datos.Columns.Add("estacion", GetType(String))
                    datos.Columns.Add("total", GetType(Int32))
                    datos.Columns.Add("tiempo", GetType(Double))

                    ' Add data rows to the table.
                    Dim row As DataRow = Nothing
                    For Each lineas In reader.Tables(0).Rows
                        row = datos.NewRow()
                        row("estacion") = lineas!estacion
                        row("total") = lineas!total
                        row("tiempo") = lineas!tiempo
                        datos.Rows.Add(row)
                    Next
                    Dim series1 As New Series("Frecuencia", ViewType.Bar)
                    Dim series2 As New Series("Tiempo (minuto)", ViewType.Spline)

                    ChartControl1.Series.Add(series1)
                    series1.DataSource = datos
                    series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series1.View.Color = Color.SkyBlue
                    series1.ArgumentScaleType = ScaleType.Qualitative
                    series1.ArgumentDataMember = "estacion"
                    series1.ValueScaleType = ScaleType.Numerical
                    series1.ValueDataMembers.AddRange(New String() {"total"})
                    series1.Label.BackColor = Color.DarkBlue
                    series1.Label.TextColor = Color.White
                    series1.Label.Font = miFuente

                    ChartControl1.Series.Add(series2)
                    series2.DataSource = datos
                    series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series2.View.Color = Color.DarkGray
                    series2.ArgumentScaleType = ScaleType.Qualitative
                    series2.ArgumentDataMember = "estacion"
                    series2.ValueScaleType = ScaleType.Numerical
                    series2.ValueDataMembers.AddRange(New String() {"tiempo"})
                    series2.Label.BackColor = Color.SlateGray
                    series2.Label.TextColor = Color.White
                    series2.Label.Font = miFuente

                    ' Set some properties to get a nice-looking chart.
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = "Número de llamadas en el período"
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True

                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = "    Estaciones con llamada    "
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True

                    Dim myAxisY As New SecondaryAxisY("Y2Axis")
                    CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                    CType(series2.View, LineSeriesView).AxisY = myAxisY

                    myAxisY.Title.Text = "Tiempo total (en minutos)"
                    myAxisY.Title.Font = miFuenteAlto
                    myAxisY.Title.Visible = True

                    ChartControl1.Titles.Add(Titulo)
                    Dim Titulo2 As New ChartTitle()

                    Titulo2.Font = miFuente
                    Titulo2.Text = "Extraccion de datos: " & cadPeriodo
                    ChartControl1.Titles.Add(Titulo2)
                    Dim Titulo3 As New ChartTitle()
                    Titulo3.Font = miFuente
                    Titulo3.Text = "Generado el: " & Format(Now, "ddd dd-MM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo3)
                    Dim Titulo4 As New ChartTitle()
                    Titulo4.Font = miFuente
                    Titulo4.Text = "Extrayendo los datos desde: " & Format(eDesde, "dd-MM-yyyy HH:mm:ss") & " hasta: " &
                                Format(eHasta, "dd-MM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo4)
                    ChartControl1.Width = 1000
                    ChartControl1.Height = 700
                    Try
                        Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                        SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                        Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                        image.Save(rutaImagen)

                    Catch ex As Exception
                        agregarLOG("Ocurrió un error al intentar construir un archivo de adjunto de reporte (gráfico). Error: " + ex.Message, 7, 0)
                    End Try


                    'No hay datos, notificar
                End If
            End If
        End If
    End Function

    Function generarReporte3(reporte As String, periodo As String, nperiodos As Integer, ruta As String) As Integer

        generarReporte3 = 0

        Dim archivoSaliente = ruta & "\fallas_por_tecnologia_tiempo.csv"
        Dim archivoImagen = ruta & "\fallas_por_tecnologia_tiempo.png"

        If My.Computer.FileSystem.FileExists(archivoSaliente) Then
            Try
                My.Computer.FileSystem.DeleteFile(archivoSaliente)

            Catch ex As Exception
                errorCorreos = ex.Message
                agregarLOG("Error al construir el reporte. " + ex.Message, 7, 0)
                generarReporte3 = -1
                Exit Function
            End Try
        End If
        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)
        Catch ex As Exception

        End Try

        archivoSaliente = archivoSaliente.Replace("\", "\\")

        Dim eDesde = Now()
        Dim eHasta = Now()
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        Dim cadPeriodo As String = nperiodos & " segundo(s) atras"
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & " minuto(s) atras"
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & " hora(s) atras"
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & " día(s) atras"
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & " semana(s) atras"
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & " mes(es) atras"
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & " año(s) atras"
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = "Lo que va del día de hoy"
        ElseIf periodo = 11 Then
            cadPeriodo = "Lo que va de la semana"
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = "Lo que va del mes"
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = "Lo que va del anyo"
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = "El día de ayer"
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = "La semana pasada"
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = "El mes pasado"
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        Dim fDesde = Format(eDesde, "yyyy/MM/dd HH:mm:ss")
        Dim fHasta = Format(eHasta, "yyyy/MM/dd HH:mm:ss")
        If reporte = "T" Or reporte = "D" Then

            Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)
            Dim regsAfectados = consultaACT("USE sigma;
SELECT * FROM 
(SELECT 'Reporte de fallas atendidas por tecnoología - Frecuencia y tiempo(" & cadPeriodo & ")','' as b,'' as c,'' as d 
UNION 
(SELECT CONCAT('Reporte generado en fecha: ', NOW()),'','','') 
UNION 
(SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'),'','','') 
UNION 
(SELECT 'Tecnologia','Frecuencia','Tiempo total (min)','Tiempo total (seg)') 
UNION
(SELECT IFNULL(tecnologia, 'N/A'), COUNT(*), ROUND(SUM(vw_alarmas.tiempo / 60), 0), SUM(vw_alarmas.tiempo) AS tiempo FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' AND vw_alarmas.tiempo > 0 GROUP BY tecnologia ORDER BY 2 DESC) 
UNION 
(SELECT 'TOTAL reporte: ', COUNT(*), ROUND(SUM(vw_alarmas.tiempo / 60), 0), SUM(vw_alarmas.tiempo) FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' AND vw_alarmas.tiempo > 0)) as query01  
INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")
            If errorBD.Length > 0 Then
                errorCorreos = errorBD
                agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
                generarReporte3 = -1
            End If
        End If
        If reporte = "T" Or reporte = "G" Then
                'Se produce el gráfico
                Dim cadSQL As String = "SELECT IFNULL(tecnologia, 'N/A') as estacion, COUNT(*) as total, ROUND(SUM(vw_alarmas.tiempo / 60), 0) as tiempo FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' AND vw_alarmas.tiempo > 0 GROUP BY tecnologia ORDER BY 2 DESC"
                Dim reader As DataSet = consultaSEL(cadSQL)
                Dim regsAfectados = 0
                If errorBD.Length > 0 Then
                    agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
                    errorCorreos = errorBD
                    generarReporte3 = -1
                Else
                ChartControl1.Series.Clear()
                ChartControl1.Titles.Clear()

                If reader.Tables(0).Rows.Count > 0 Then
                    Dim Titulo As New ChartTitle()
                    Titulo.Text = "    Gráfica de Fallas por tecnología (frecuencia y tiempo)   "
                    Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                    Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                    Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                    Titulo.Font = miFuenteAlto


                    ' Create an empty table.
                    Dim datos As New DataTable("grafico")

                    ' Add two columns to the table.
                    datos.Columns.Add("estacion", GetType(String))
                    datos.Columns.Add("total", GetType(Int32))
                    datos.Columns.Add("tiempo", GetType(Double))

                    ' Add data rows to the table.
                    Dim row As DataRow = Nothing
                    For Each lineas In reader.Tables(0).Rows
                        row = datos.NewRow()
                        row("estacion") = lineas!estacion
                        row("total") = lineas!total
                        row("tiempo") = lineas!tiempo
                        datos.Rows.Add(row)
                    Next
                    Dim series1 As New Series("Frecuencia", ViewType.Bar)
                    Dim series2 As New Series("Tiempo (minuto)", ViewType.Spline)

                    ChartControl1.Series.Add(series1)
                    series1.DataSource = datos
                    series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series1.View.Color = Color.SkyBlue
                    series1.ArgumentScaleType = ScaleType.Qualitative
                    series1.ArgumentDataMember = "estacion"
                    series1.ValueScaleType = ScaleType.Numerical
                    series1.ValueDataMembers.AddRange(New String() {"total"})
                    series1.Label.BackColor = Color.DarkBlue
                    series1.Label.TextColor = Color.White
                    series1.Label.Font = miFuente

                    ChartControl1.Series.Add(series2)
                    series2.DataSource = datos
                    series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series2.View.Color = Color.DarkGray
                    series2.ArgumentScaleType = ScaleType.Qualitative
                    series2.ArgumentDataMember = "estacion"
                    series2.ValueScaleType = ScaleType.Numerical
                    series2.ValueDataMembers.AddRange(New String() {"tiempo"})
                    series2.Label.BackColor = Color.SlateGray
                    series2.Label.TextColor = Color.White
                    series2.Label.Font = miFuente

                    ' Set some properties to get a nice-looking chart.
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = "Número de llamadas en el período"
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True

                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = "    Tecnologías    "
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True

                    Dim myAxisY As New SecondaryAxisY("Y2Axis")
                    CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                    CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                    CType(series2.View, LineSeriesView).AxisY = myAxisY

                    myAxisY.Title.Text = "Tiempo total (en minutos)"
                    myAxisY.Title.Font = miFuenteAlto
                    myAxisY.Title.Visible = True

                    ChartControl1.Titles.Add(Titulo)
                    Dim Titulo2 As New ChartTitle()

                    Titulo2.Font = miFuente
                    Titulo2.Text = "Extraccion de datos: " & cadPeriodo
                    ChartControl1.Titles.Add(Titulo2)
                    Dim Titulo3 As New ChartTitle()
                    Titulo3.Font = miFuente
                    Titulo3.Text = "Generado el: " & Format(Now, "ddd dd-MM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo3)
                    Dim Titulo4 As New ChartTitle()
                    Titulo4.Font = miFuente
                    Titulo4.Text = "Extrayendo los datos desde: " & Format(eDesde, "dd-MM-yyyy HH:mm:ss") & " hasta: " &
                                    Format(eHasta, "dd-MM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo4)
                    ChartControl1.Width = 1000
                    ChartControl1.Height = 700
                    Try
                        Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                        SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                        Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                        image.Save(rutaImagen)

                    Catch ex As Exception
                        agregarLOG("Ocurrió un error al intentar construir un archivo de adjunto de reporte (gráfico). Error: " + ex.Message, 7, 0)
                    End Try


                    'No hay datos, notificar
                End If
            End If
            End If

    End Function

    Function generarReporte4(reporte As String, periodo As String, nperiodos As Integer, ruta As String) As Integer
        generarReporte4 = 0

        Dim archivoSaliente = ruta & "\top_10_fallas.csv"
        If My.Computer.FileSystem.FileExists(archivoSaliente) Then
            Try
                My.Computer.FileSystem.DeleteFile(archivoSaliente)
            Catch ex As Exception
                errorCorreos = ex.Message
                agregarLOG("Error al construir el reporte. " + ex.Message, 7, 0)
                generarReporte4 = -1
                Exit Function
            End Try
        End If
        archivoSaliente = archivoSaliente.Replace("\", "\\")

        Dim eDesde = Now()
        Dim eHasta = Now()
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        Dim cadPeriodo As String = nperiodos & " segundo(s) atras"
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & " minuto(s) atras"
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & " hora(s) atras"
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & " día(s) atras"
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & " semana(s) atras"
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & " mes(es) atras"
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & " año(s) atras"
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = "Lo que va del día de hoy"
        ElseIf periodo = 11 Then
            cadPeriodo = "Lo que va de la semana"
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = "Lo que va del mes"
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = "Lo que va del anyo"
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = "El día de ayer"
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = "La semana pasada"
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = "El mes pasado"
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        Dim fDesde = Format(eDesde, "yyyy/MM/dd HH:mm:ss")
        Dim fHasta = Format(eHasta, "yyyy/MM/dd HH:mm:ss")

        Dim comillas = Chr(34)
        comillas = Microsoft.VisualBasic.Strings.Left(comillas, 1)
        Dim regsAfectados = consultaACT("USE sigma;SET @'ROWE':= 0;SELECT * FROM 
(SELECT 'Reporte de fallas más comunes (" & cadPeriodo & ")','' as b,'' as c,'' as d,'' as e,'' as f ,'' as g 
UNION 
(SELECT CONCAT('Reporte generado en fecha: ', NOW()),'','','','', '', '') 
UNION 
(SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'),'','','','', '', '') 
UNION 
(SELECT '#','Estacion','Falla','Responsable','Frecuencia (veces)','Tiempo total (min)','Tiempo total (seg)') 
UNION
(SELECT @'ROWE':= @'ROWE' + 1, estacion, descripcion, responsable, total, mins, segs FROM (SELECT estacion, codigo, responsable, COUNT(*) as total, ROUND(SUM(vw_alarmas.tiempo / 60), 0) as mins, SUM(vw_alarmas.tiempo) as segs FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" + fDesde + "' AND vw_alarmas.inicio <= '" + fHasta + "' AND vw_alarmas.tiempo > 0 GROUP BY estacion, codigo, responsable ORDER BY 5 DESC LIMIT 10) AS temp2) 
UNION 
(SELECT '','','','',COUNT(*), ROUND(SUM(vw_alarmas.tiempo / 60), 0), SUM(vw_alarmas.tiempo) FROM sigma.vw_alarmas WHERE vw_alarmas.inicio >= '" & fDesde & "' AND vw_alarmas.inicio <= '" & fHasta & "' AND vw_alarmas.tiempo > 0)) as query01  
INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")
        If errorBD.Length > 0 Then
            errorCorreos = errorBD
            agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
            generarReporte4 = -1
        End If

    End Function

    Function generarReporte5(reporte As String, periodo As String, nperiodos As Integer, ruta As String) As Integer
        generarReporte5 = 0

        Dim archivoSaliente = ruta & "\rendimiento_staff.csv"


        If My.Computer.FileSystem.FileExists(archivoSaliente) Then
            Try
                My.Computer.FileSystem.DeleteFile(archivoSaliente)

            Catch ex As Exception
                errorCorreos = ex.Message
                agregarLOG("Error al construir el reporte. " + ex.Message, 7, 0)
                generarReporte5 = -1
                Exit Function
            End Try
        End If
        Dim archivoImagen = ruta & "\rendimiento_staff.png"
        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)
        Catch ex As Exception

        End Try
        archivoSaliente = archivoSaliente.Replace("\", "\\")

        Dim eDesde = Now()
        Dim eHasta = Now()
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        Dim cadPeriodo As String = nperiodos & " segundo(s) atras"
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & " minuto(s) atras"
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & " hora(s) atras"
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & " día(s) atras"
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & " semana(s) atras"
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & " mes(es) atras"
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & " año(s) atras"
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = "Lo que va del día de hoy"
        ElseIf periodo = 11 Then
            cadPeriodo = "Lo que va de la semana"
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = "Lo que va del mes"
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = "Lo que va del anyo"
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = "El día de ayer"
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = "La semana pasada"
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = "El mes pasado"
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        Dim fDesde = Format(eDesde, "yyyy/MM/dd HH:mm:ss")
        Dim fHasta = Format(eHasta, "yyyy/MM/dd HH:mm:ss")
        If reporte = "T" Or reporte = "D" Then

            Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)
            Dim regsAfectados = consultaACT("USE sigma;SELECT 'Reporte de rendimiento por STAFF (" & cadPeriodo & ")','','','','','','' UNION ALL SELECT CONCAT('Reporte generado en fecha: ', NOW()),'','','','','','' UNION ALL SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'),'','','','','','' UNION ALL 
SELECT 'Responsable','Total fallas atendidas','Rendimiento (sólo cerradas)','Promedio','Atendidas en tiempo','Atendidas ya escaladas','Sin atender' 
UNION ALL 
SELECT vw_alarmas.responsable, SUM(IF(vw_alarmas.tiempo > 0, 1, 0)), ROUND(SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) / (SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) + SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0))) * 100, 0), SEC_TO_TIME(ROUND(SUM(vw_alarmas.tiempo) / SUM(IF(vw_alarmas.tiempo > 0, 1, 0)), 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0)), SUM(IF(vw_alarmas.tiempo = 0, 1, 0)) FROM sigma.vw_alarmas LEFT JOIN sigma.vw_reportes ON vw_alarmas.reporte = vw_reportes.id WHERE vw_alarmas.inicio >= '" & fDesde & "' AND vw_alarmas.inicio <= '" & fHasta & "' GROUP BY vw_alarmas.responsable 
UNION ALL 
SELECT '" & cad_consolidado & "', SUM(IF(vw_alarmas.tiempo > 0, 1, 0)), ROUND(SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) / (SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) + SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0))) * 100, 0), SEC_TO_TIME(ROUND(SUM(vw_alarmas.tiempo) / COUNT(*), 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0)), SUM(IF(vw_alarmas.tiempo = 0, 1, 0)) FROM sigma.vw_alarmas LEFT JOIN sigma.vw_reportes ON vw_alarmas.reporte = vw_reportes.id WHERE vw_alarmas.inicio >= '" & fDesde & "' AND vw_alarmas.inicio <= '" & fHasta & "' 
INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")
            If errorBD.Length > 0 Then
                errorCorreos = errorBD
                agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
                generarReporte5 = -1
            End If
        End If
        If reporte = "T" Or reporte = "G" Then
            'Se produce el gráfico
            Dim cadSQL As String = "SELECT IFNULL(vw_alarmas.responsable, 'N/A') as responsable, SUM(IF(vw_alarmas.tiempo > 0, 1, 0)) tiempo, ROUND(SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) / (SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) + SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0))) * 100, 0) as prom, SEC_TO_TIME(ROUND(SUM(vw_alarmas.tiempo) / SUM(IF(vw_alarmas.tiempo > 0, 1, 0)), 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0)), SUM(IF(vw_alarmas.tiempo = 0, 1, 0)) FROM sigma.vw_alarmas LEFT JOIN sigma.vw_reportes ON vw_alarmas.reporte = vw_reportes.id WHERE vw_alarmas.inicio >= '" & fDesde & "' AND vw_alarmas.inicio <= '" & fHasta & "' GROUP BY vw_alarmas.responsable"
            Dim reader As DataSet = consultaSEL(cadSQL)
            Dim regsAfectados = 0
            If errorBD.Length > 0 Then
                agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
                errorCorreos = errorBD
                generarReporte5 = -1
            Else
                Dim lineaGauge = 0
                Dim miFuente = New Drawing.Font("Lucida Sans", 12, FontStyle.Regular)
                Dim circular As New CircularGauge
                For Each lineas In reader.Tables(0).Rows
                    If lineaGauge = 7 Then
                        Exit For
                    End If

                    circular = GaugeControl1.Gauges(lineaGauge)
                    circular.BeginUpdate()

                    circular.Scales(0).Ranges(0).EndValue = bajo_hasta
                    circular.Scales(0).Ranges(1).StartValue = bajo_hasta
                    circular.Scales(0).Ranges(1).EndValue = medio_hasta
                    circular.Scales(0).Ranges(2).StartValue = medio_hasta
                    circular.Scales(0).Ranges(0).AppearanceRange.ContentBrush = New SolidBrushObject(ColorTranslator.FromHtml(bajo_color))
                    circular.Scales(0).Ranges(1).AppearanceRange.ContentBrush = New SolidBrushObject(ColorTranslator.FromHtml(medio_color))
                    circular.Scales(0).Ranges(2).AppearanceRange.ContentBrush = New SolidBrushObject(ColorTranslator.FromHtml(alto_color))

                    Dim promedio = ValNull(lineas!prom, "N")

                    Dim label As LabelComponent = New LabelComponent("myLabel")
                    label.AppearanceText.TextBrush = New SolidBrushObject(Color.Black)
                    label.Position = New PointF2D(125, 250)
                    label.Size = New Size(300, 300)
                    label.ZOrder = -10000
                    label.Text = lineas!responsable
                    Dim label2 As LabelComponent = New LabelComponent("myLabel")
                    label2.AppearanceText.TextBrush = New SolidBrushObject(Color.Black)
                    label2.Position = New PointF2D(125, 270)
                    label2.Size = New Size(300, 300)
                    label2.ZOrder = -10000
                    label2.Text = "Rendimiento: " & promedio & "%"
                    circular.Scales(0).Labels.Add(label)
                    circular.Scales(0).Labels.Add(label2)
                    circular.Scales(0).Value = promedio
                    circular.Scales(0).Labels(0).AppearanceText.Font = miFuente
                    circular.Scales(0).Labels(1).AppearanceText.Font = miFuente
                    circular.EndUpdate()
                    lineaGauge = lineaGauge + 1

                Next
                If lineaGauge < 7 Then
                    For i = 7 To lineaGauge + 1 Step -1
                        Dim circular2 As CircularGauge = CType(GaugeControl1.Gauges(i), CircularGauge)
                        GaugeControl1.Gauges.Remove(circular2)
                    Next
                End If
                cadSQL = "SELECT SUM(IF(vw_alarmas.tiempo > 0, 1, 0)) as tiempo, ROUND(SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) / (SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)) + SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0))) * 100, 0) as prom, SEC_TO_TIME(ROUND(SUM(vw_alarmas.tiempo) / SUM(IF(vw_alarmas.tiempo > 0, 1, 0)), 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos = 0, 1, 0)), SUM(IF(vw_alarmas.tiempo > 0 AND vw_reportes.escalamientos > 0, 1, 0)), SUM(IF(vw_alarmas.tiempo = 0, 1, 0)) FROM sigma.vw_alarmas LEFT JOIN sigma.vw_reportes ON vw_alarmas.reporte = vw_reportes.id WHERE vw_alarmas.inicio >= '" & fDesde & "' AND vw_alarmas.inicio <= '" & fHasta & "'"
                '''
                reader = consultaSEL(cadSQL)
                If errorBD.Length > 0 Then
                    agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
                    errorCorreos = errorBD
                    generarReporte5 = -1
                Else
                    circular = GaugeControl1.Gauges(GaugeControl1.Gauges.Count - 1)
                    circular.BeginUpdate()

                    circular.Scales(0).Ranges(0).EndValue = bajo_hasta
                    circular.Scales(0).Ranges(1).StartValue = bajo_hasta
                    circular.Scales(0).Ranges(1).EndValue = medio_hasta
                    circular.Scales(0).Ranges(2).StartValue = medio_hasta
                    circular.Scales(0).Ranges(0).AppearanceRange.ContentBrush = New SolidBrushObject(ColorTranslator.FromHtml(bajo_color))
                    circular.Scales(0).Ranges(1).AppearanceRange.ContentBrush = New SolidBrushObject(ColorTranslator.FromHtml(medio_color))
                    circular.Scales(0).Ranges(2).AppearanceRange.ContentBrush = New SolidBrushObject(ColorTranslator.FromHtml(alto_color))

                    Dim promedio = ValNull(reader.Tables(0).Rows(0)!prom, "N")

                    Dim label As LabelComponent = New LabelComponent("myLabel")
                    label.AppearanceText.TextBrush = New SolidBrushObject(Color.Black)
                    label.Position = New PointF2D(125, 250)
                    label.Size = New Size(300, 300)
                    label.ZOrder = -10000
                    label.Text = cad_consolidado
                    Dim label2 As LabelComponent = New LabelComponent("myLabel")
                    label2.AppearanceText.TextBrush = New SolidBrushObject(Color.Black)
                    label2.Position = New PointF2D(125, 270)
                    label2.Size = New Size(300, 300)
                    label2.ZOrder = -10000
                    label2.Text = "Rendimiento: " & promedio & "%"
                    circular.Scales(0).Labels.Add(label)
                    circular.Scales(0).Labels.Add(label2)
                    circular.Scales(0).Value = promedio
                    circular.Scales(0).Labels(0).AppearanceText.Font = miFuente
                    circular.Scales(0).Labels(1).AppearanceText.Font = miFuente
                    circular.EndUpdate()
                    '''
                End If
                GaugeControl1.Refresh()
                GaugeControl1.Width = 1500
                GaugeControl1.Height = 900
                Try
                    Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                    SaveGaugeImageToFile(GaugeControl1, ImageFormat.Png, rutaImagen)
                    Dim image As Image = GetGaugeImage(GaugeControl1, ImageFormat.Png)
                    image.Save(rutaImagen)

                Catch ex As Exception
                    agregarLOG("Ocurrió un error al intentar construir un archivo de adjunto de reporte (gráfico). Error: " + ex.Message, 7, 0)
                End Try
            End If
        End If


    End Function

    Function generarReporte6(reporte As String, periodo As String, nperiodos As Integer, ruta As String) As Integer
        generarReporte6 = 0
        Dim archivoSaliente = ruta & "\estadistica_de_fallas.csv"
        If My.Computer.FileSystem.FileExists(archivoSaliente) Then
            Try
                My.Computer.FileSystem.DeleteFile(archivoSaliente)
            Catch ex As Exception
                errorCorreos = ex.Message
                agregarLOG("Error al construir el reporte. " + ex.Message, 7, 0)
                generarReporte6 = -1
                Exit Function
            End Try
        End If
        archivoSaliente = archivoSaliente.Replace("\", "\\")

        Dim eDesde = Now()
        Dim eHasta = Now()
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        Dim cadPeriodo As String = nperiodos & " segundo(s) atras"
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & " minuto(s) atras"
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & " hora(s) atras"
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & " día(s) atras"
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & " semana(s) atras"
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & " mes(es) atras"
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & " año(s) atras"
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = "Lo que va del día de hoy"
        ElseIf periodo = 11 Then
            cadPeriodo = "Lo que va de la semana"
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = "Lo que va del mes"
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = "Lo que va del anyo"
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = "El día de ayer"
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = "La semana pasada"
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = "El mes pasado"
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        Dim fDesde = Format(eDesde, "yyyy/MM/dd HH:mm:ss")
        Dim fHasta = Format(eHasta, "yyyy/MM/dd HH:mm:ss")

        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)
        Dim regsAfectados = consultaACT("USE sigma;SELECT 'Reporte de estadistica de fallas (" & cadPeriodo & ")','','','','','','','','','','','' UNION ALL SELECT CONCAT('Reporte generado en fecha: ', NOW()),'','','','','','','','','','','' UNION ALL SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'),'','','','','','','','','','','' UNION ALL SELECT 'Desde','Hasta','Nave','Estacion','Responsable','Tecnología','Falla','Fallas generadas en el lapso','Fallas cerradas en el lapso','Cerradas con escalamiento',' Cerradas en tiempo','Abiertas' UNION ALL SELECT desde, hasta, nave,estacion, responsable, tecnologia, codigo, fallas_generadas, fallas_cerradas, fallas_escaladas, fallas_entiempo, fallas_total FROM sigma.vw_resumen WHERE desde >= '" + fDesde + "' AND desde <= '" + fHasta + "' UNION ALL
SELECT '', '', '', '', '', '', 'Total resumen', SUM(fallas_generadas), SUM(fallas_cerradas), SUM(fallas_escaladas), SUM(fallas_entiempo), SUM(fallas_total) FROM vw_resumen WHERE desde >= '" + fDesde + "' AND desde <= '" + fHasta + "' INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")
        If errorBD.Length > 0 Then
            errorCorreos = errorBD
            agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
            generarReporte6 = -1
        End If


    End Function


    Function generarReporte7(ruta As String) As Integer
        generarReporte7 = 0
        Dim archivoSaliente = ruta & "\fallas_abiertas.csv"
        If My.Computer.FileSystem.FileExists(archivoSaliente) Then
            Try
                My.Computer.FileSystem.DeleteFile(archivoSaliente)
            Catch ex As Exception
                errorCorreos = ex.Message
                agregarLOG("Error al construir el reporte. " + ex.Message, 7, 0)
                generarReporte7 = -1
                Exit Function
            End Try
        End If
        archivoSaliente = archivoSaliente.Replace("\", "\\")

        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)
        Dim regsAfectados = consultaACT("USE sigma;SELECT 'Reporte de fallas al momento','','','','','','','','','','','','','','','','','' UNION ALL SELECT CONCAT('Reporte generado en fecha: ', NOW()),'','','','','','','','','','','','','','','','','' UNION ALL SELECT 'ID','Codigo','Descripcion','Estacion','Responsable','Tecnología','Nave','Prioridad','Fecha de inicio','Tiempo transcurrido en segundos','Tiempo HH:MM:SS','Repeticiones','Nivel de escalamiento','Fecha de escalamiento (1)','Fecha de escalamiento (2)','Fecha de escalamiento (3)','Fecha de escalamiento (4)','Fecha de escalamiento (5)' UNION ALL SELECT vw_alarmas.falla, vw_alarmas.codigo, vw_alarmas.descripcion, vw_alarmas.estacion, vw_alarmas.responsable, vw_alarmas.tecnologia, vw_alarmas.nave, vw_alarmas.prioridad, vw_alarmas.inicio, TIME_TO_SEC(TIMEDIFF(NOW(), vw_alarmas.inicio)), SEC_TO_TIME(TIME_TO_SEC(TIMEDIFF(NOW(), vw_alarmas.inicio))), IFNULL(vw_reportes.repeticiones, 0) AS repeticiones, IFNULL(vw_reportes.escalamientos, 0) AS escalamientos, IFNULL(vw_reportes.escalada1, ''), IFNULL(vw_reportes.escalada2, ''), IFNULL(vw_reportes.escalada3, ''), IFNULL(vw_reportes.escalada4, ''), IFNULL(vw_reportes.escalada5, '') FROM sigma.vw_alarmas LEFT JOIN sigma.vw_reportes ON vw_alarmas.reporte = vw_reportes.id WHERE vw_alarmas.tiempo = 0 UNION ALL
SELECT CONCAT('Total fallas abiertas al momento: ', COUNT(*)), '', '', '', '', '', '', '', '', '', '','', '', '', '', '','','' FROM sigma.vw_alarmas WHERE vw_alarmas.tiempo = 0 INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")

        If errorBD.Length > 0 Then
            errorCorreos = errorBD
            agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
            generarReporte7 = -1
        End If

    End Function



    Function calcularPromedio(tiempo As Integer) As String
        calcularPromedio = ""
        tiempo = Math.Round(tiempo, 0)
        Dim horas = tiempo / 3600
        Dim minutos = (tiempo Mod 3600) / 60
        Dim segundos = tiempo Mod 60
        If segundos > 30 Then
            minutos = minutos + 1
        End If
        If minutos = 0 And horas = 0 Then
            minutos = 1
        End If
        calcularPromedio = Format(Math.Floor(horas), "00") & ":" & Format(Math.Floor(minutos), "00")
    End Function

    Private Sub agregarLOG(cadena As String, tipo As Integer, reporte As Integer, Optional aplicacion As Integer = 1)
        'Se agrega a la base de datos
        'tipo 1: Info
        'tipo 2: Incongruencia en los datos (usuario)
        'tipo 8: Error crítico de Base de datos infofallas
        'tipo 9: Error crítico de Base de datos sigma
        Dim regsAfectados = consultaACT("INSERT INTO sigma.vw_log (aplicacion, tipo, reporte, texto) VALUES (40, " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
        If aplicacion = 10 Then
            regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET flag_monitor = 'S'")
        End If

    End Sub

    Private Function GetChartImage(ByVal chart As ChartControl, ByVal format As ImageFormat) As Image
        ' Create an image.  
        Dim image As Image = Nothing

        ' Create an image of the chart.  
        Using s As New MemoryStream()
            chart.ExportToImage(s, format)
            image = System.Drawing.Image.FromStream(s)
        End Using

        ' Return the image.  
        Return image
    End Function

    Private Function GetGaugeImage(ByVal chart As GaugeControl, ByVal format As ImageFormat) As Image
        ' Create an image.  
        Dim image As Image = Nothing

        ' Create an image of the chart.  
        Using s As New MemoryStream()
            chart.ExportToImage(s, format)
            image = System.Drawing.Image.FromStream(s)
        End Using

        ' Return the image.  
        Return image
    End Function

    Private Sub SaveChartImageToFile(ByVal chart As ChartControl, ByVal format As ImageFormat, ByVal fileName As String)
        ' Create an image in the specified format from the chart  
        ' and save it to the specified path.  
        chart.ExportToImage(fileName, format)
    End Sub

    Private Sub SaveGaugeImageToFile(ByVal chart As GaugeControl, ByVal format As ImageFormat, ByVal fileName As String)
        ' Create an image in the specified format from the chart  
        ' and save it to the specified path.  
        chart.ExportToImage(fileName, format)
    End Sub

End Class

