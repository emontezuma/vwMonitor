Imports MySql.Data.MySqlClient
Imports System.IO.Ports
Imports System.IO
Imports System.Text
Imports System.Net.Mail
Imports System.Net
Imports System.ComponentModel
Imports System.Data
Imports System.Windows.Forms
Imports System.Speech.Synthesis


Public Class Form1

    Dim Estado As Integer = 0
    Dim procesandoAudios As Boolean = False
    Dim eSegundos = 0
    Dim procesandoEscalamientos As Boolean
    Dim procesandoRepeticiones As Boolean
    Dim estadoPrograma As Boolean
    Dim MensajeLlamada = ""
    Dim errorPuerto As Boolean = False
    Dim ptoError As String = ""
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        estadoPrograma = True
        generarLlamadas()
        Me.Close()
    End Sub

    Private Sub generarLlamadas()
        Dim rutaAudios
        Dim rutaSMS
        Dim ptoCOMM1 As String = "", ptoCOMM2 As String = "", ptoCOMM3 As String = "", ptoCOMM4 As String = "", ptoCOMM5 As String = "", ptoCOMM6 As String = ""
        Dim ptoCOMM1P As String = "", ptoCOMM2P As String = "", ptoCOMM3P As String = "", ptoCOMM4P As String = "", ptoCOMM5P As String = "", ptoCOMM6P As String = ""
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
                agregarLOG("La aplicación de generar llamadas indicó que el puerto especificado no es válido. Puerto:" & ptoCOMM1 & " parámetros: " & ptoCOMM1P & ". No se emitieron " & LlamadasPendientes & " llamada(s) de voz. Error: " & ptoError, 7, 0)
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
                                        ElseIf MensajeLlamada.Length > 0 Then
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
                                            ElseIf escape_accion = "E" Then
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
                agregarLOG("La aplicación de generar llamadas indicó que el puerto especificado no es válido. Puerto:" & ptoCOMM1 & " parámetros: " & ptoCOMM1P & ". No se enviaron " & LlamadasPendientes & " mensaje(s) de texto (SMS)Error: " & ptoError, 7, 0)
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
            ptoError = ex.Message
            Exit Function

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
            ptoError = ex.Message
            Exit Function
        End Try


    End Function

    Private Sub agregarLOG(cadena As String, tipo As Integer, reporte As Integer, Optional aplicacion As Integer = 1)
        'Se agrega a la base de datos
        'tipo 1: Info
        'tipo 2: Incongruencia en los datos (usuario)
        'tipo 8: Error crítico de Base de datos infofallas
        'tipo 9: Error crítico de Base de datos sigma
        Dim regsAfectados = consultaACT("INSERT INTO sigma.vw_log (aplicacion, tipo, reporte, texto) VALUES (10, " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
        If aplicacion = 10 Then
            regsAfectados = consultaACT("UPDATE sigma.vw_configuracion SET flag_monitor = 'S'")
        End If
    End Sub

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

    Sub escaparLlamada(numero)
        agregarLOG("Se agotó el número de intentos de llamada de voz al repositorio: " & numero, 1, 0)
    End Sub

    Sub escaparSMS(numero)
        agregarLOG("Se agotó el número de intentos de envio de SMS al repositorio: " & numero, 1, 0)
    End Sub

    Sub escaparMMCall()
        agregarLOG("Todos los requesters de MMCall están ocupados... ", 1, 0)
    End Sub

    Private Sub SerialPort1_DataReceived(sender As Object, e As SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        MensajeLlamada = SerialPort1.ReadLine

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub
End Class