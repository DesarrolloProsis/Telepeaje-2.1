Imports System.Data.SqlClient
Imports Oracle.DataAccess.Client
Imports System.IO
Imports System.Threading
Imports System.Text

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            CONTADOR = 0
            'Console.Title = "Listas Blancas V-2018 2.0"
            oConexion.ConnectionString = query
            'oConexion.Open()
            'inicia contador de tiempo 

            Parametros()
            hayarchivos = False
            ' Obtener todos los archivos .txt del directorio windows ( inclyendo subdirectorios )  
            For Each archivos As String In Directory.GetFiles(Origen, "*.txt")
                ' extraer el nombre de la ruta  
                archivos = archivos.Substring(archivos.LastIndexOf("\") + 1).ToString
                ' Agregar el valor al list 
                list.Add(archivos.ToString)
            Next
            ' Obtener todos los directorios del directorio c: ( un solo nivel )  

NumeroLineas:

            For Each LeerArchivos In list
                consulta = "Select * FROM dbo.Historial where Archivo Like '" & LeerArchivos & "';"
                cmd = New SqlCommand(consulta, oConexion)

                cmd.CommandTimeout = 3 * 60
                Dim ARCHIVO As String = cmd.ExecuteScalar()
                'Busco ver si ya se insertaron los archivos
                If String.IsNullOrEmpty(ARCHIVO) Then
                    InsertarLista()
                End If
            Next
            ''''''''''''''''''''''''''RESIDENTES'''''''''''''''''''''''''''''''''
            If NombreCaseta = "ALPUYECA" Or NombreCaseta = "AEROPUERTO" Or NombreCaseta = "PASOMORELOS" Or NombreCaseta = "Mac" Then
                For Each archivos As String In Directory.GetFiles(OrigenResidentes, "*.txt")
                    ' extraer el nombre de la ruta  
                    archivos = archivos.Substring(archivos.LastIndexOf("\") + 1).ToString
                    ' Agregar el valor al list 
                    ListResidentes.Add(archivos.ToString)
                Next

                For Each LeerArchivos In ListResidentes
                    consulta = "Select * FROM dbo.historial where Archivo Like '" & LeerArchivos & "';"
                    cmd = New SqlCommand(consulta, oConexion)
                    Dim ARCHIVO As String = cmd.ExecuteScalar()
                    If String.IsNullOrEmpty(ARCHIVO) Then
                        InsertarLista()
                    End If
                Next
            End If
            '''''''''''''''''''''''''FIN RESIDENTES ''''''''''''''''''''''''''''''

            If hayarchivos = True Then
                banderaAntifraude = True
                creararchivos()

            Else
                ''creamos el archivo Antifraude''
                PathTemporal = "c:\temporal\Antifraude\LSTABINT."
                ArchivoAntifraude()
                If banderaAntifraude = True Then
                    'encabezados()
                    'vDestino = DestinoAntifraude & "LSTABINT."
                    'CopiarCarpeta()
                    'AumentarExt()
                    creararchivos()
                    'despues de crear el archivo lo vuelve falso
                    banderaAntifraude = False
                End If
            End If
            list.Clear()


            'aTimer.Interval = 300000 '4 min esta en milisegundos 

            oConexion.Close()
            Me.Close()
        Catch ex As Exception
            If CONTADOR <= 3 Then
                CONTADOR = CONTADOR + 1
                Thread.Sleep(3000)
                GoTo NumeroLineas
            Else
                Dim path As String = "c:\temporal\LogsListas.txt"
                'Dim objStreamReader As StreamReader
                'Dim Escribir As StreamWriter
                'Dim strLine As String

                If File.Exists(path) Then


                    Using write As StreamWriter = New StreamWriter(path, True)
                        write.Write(DateTime.Now & " / " & ex.Message & vbCrLf)
                    End Using
                Else
                    ' Create or overwrite the file.
                    Dim fs As FileStream = File.Create(path)
                    ' Add text to the file.
                    Dim info As Byte() = New UTF8Encoding(True).GetBytes(ex.Message)
                    fs.Write(info, 0, info.Length)
                    fs.Close()
                End If
                Me.Close()

            End If
        End Try
    End Sub

    Sub InsertarLista()
        Try
            'consulta para mandar la primera linea vacia
            consulta = "select * from Lista where Saldo = 999999999999"
            cmd = New SqlCommand(consulta, oConexion)
            Dim resultado = cmd.ExecuteScalar()
            If resultado = Nothing Then
                bandera = False
            Else
                bandera = True
            End If

            Dim bString As String
            bString = Mid(LeerArchivos, 1, 1)
            'Si la lista es completa borra la anterior 
            If bString = "C" Then
                consulta = "TRUNCATE TABLE lista 
                            TRUNCATE TABLE listaTemporal
                            TRUNCATE TABLE listaantifraude 
                            TRUNCATE TABLE ListaValidaciones"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.ExecuteNonQuery()

                'inserta la lista que encontro a la base SQL
                consulta = "bulk insert lista from '" & Origen & "" & LeerArchivos & "' with ( FORMATFILE = '" & fmt & "');"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                'If bandera = False Then
                '    consulta = "INSERT INTO lista VALUES (0000000000000000000,00,00,999999999999,00,0000000000000000000000000000000000000000000000000);"
                '    cmd = New SqlCommand(consulta, oConexion)
                '    cmd.ExecuteNonQuery()
                '    consulta = "update Lista set saldo = cast(saldo as varchar(8)) where saldo > '99999999'"
                '    cmd = New SqlCommand(consulta, oConexion)
                '    cmd.CommandTimeout = 3 * 60
                '    cmd.ExecuteNonQuery()
                '    bandera = True
                'End If
                ''''''''Se insertan los residentes '''''''

                If NombreCaseta = "ALPUYECA" Or NombreCaseta = "AEROPUERTO" Or NombreCaseta = "XOCHITEPEC" Or NombreCaseta = "MAC" Then

                    consulta = "INSERT INTO ListaAntifraude SELECT * FROM  ListaResidentes lT WHERE NOT EXISTS (SELECT LT.TAG FROM ListaAntifraude LI WHERE LT.Tag = LI.Tag)"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()

                End If

                ''''''''''''''' FIN RESIDENTES'''''''''''''''''''''''''''''


                ''''''''''''''' Lista Antifraude'''''''''''''''''''''''''''

                consulta = "  INSERT INTO ListaAntifraude SELECT * FROM  lista lT WHERE NOT EXISTS (SELECT LT.TAG FROM ListaAntifraude LI WHERE LT.Tag = LI.Tag)"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                consulta = "update ListaAntifraude set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                consulta = "update ListaAntifraude set saldo = cast(saldo as varchar(8)) where saldo < 0"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                ''''''''''''''' Fin Antifraude''''''''''''''''''''''''''''''''''


                If Not NombreCaseta = "XOCHITEPEC" Or NombreCaseta = "AEROPUERTO" Or NombreCaseta = "EMILIANOZAPATA" Or NombreCaseta = "TRESMARIAS" Then


                    ''''''''''''''' Monto Minimo''''''''''''''''''''''''''''''''''
                    'consulta = "UPDATE ListaValidaciones SET Saldo = ListaTemporal.Saldo, Estatus = ListaTemporal.Estatus, Tipo = ListaTemporal.tipo  FROM ListaTemporal where ListaTemporal.Tag = ListaValidaciones.Tag and ListaValidaciones.EstatusResidente = '00'"
                    'cmd = New SqlCommand(consulta, oConexion)
                    'cmd.CommandTimeout = 3 * 60
                    'cmd.ExecuteNonQuery()
                    consulta = "  INSERT INTO ListaValidaciones SELECT * FROM  lista lT WHERE NOT EXISTS (SELECT LT.TAG FROM ListaValidaciones LI WHERE LT.Tag = LI.Tag)"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()
                    consulta = "update ListaValidaciones set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()
                    consulta = "update ListaValidaciones set saldo = cast(saldo as varchar(8)) where saldo < 0"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()

                    ''''''''''''''' Fin Minimo'''''''''''''''''''''''''''''''''

                End If
            End If
            'Lista con archivos temporales
            If bString = "I" Then
                'Borro lista anterior

                consulta = "TRUNCATE TABLE listaTemporal"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.ExecuteNonQuery()
                'inserto nueva lista
                consulta = "bulk insert listaTemporal from '" & Origen & "" & LeerArchivos & "' with ( FORMATFILE = '" & fmt & "');"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                'consulta = "update ListaTemporal set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
                'cmd = New SqlCommand(consulta, oConexion)
                'cmd.CommandTimeout = 3 * 60
                'cmd.ExecuteNonQuery()
                consulta = "update ListaTemporal set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                consulta = "update ListaTemporal set saldo = cast(saldo as varchar(8)) where saldo < 0"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                'uno con la complementaria 
                consulta = "UPDATE lista SET Saldo = ListaTemporal.Saldo, Estatus = ListaTemporal.Estatus, Tipo = ListaTemporal.tipo  FROM ListaTemporal where ListaTemporal.Tag = Lista.Tag and Lista.EstatusResidente = '00'"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                'Se insertan los tags que no existian anteriormente 
                consulta = "INSERT INTO lista SELECT * FROM ListaTemporal lT WHERE NOT EXISTS (SELECT LT.TAG FROM Lista LI WHERE LT.Tag = LI.Tag)"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()

                'If NombreCaseta = "ALPUYECA" Or NombreCaseta = "AEROPUERTO" Or NombreCaseta = "XOCHITEPEC" Or NombreCaseta = "MAC" Then

                '    '''''''''''''''''''RESIDENTES''''''''''''''''''''''''''''''''
                '    consulta = "UPDATE listaResidentes SET Saldo = ListaTemporal.Saldo, Estatus = ListaTemporal.Estatus, Tipo = ListaTemporal.tipo  FROM ListaTemporal where ListaTemporal.Tag = ListaResidentes.Tag and ListaResidentes.EstatusResidente = '00'"
                '    cmd = New SqlCommand(consulta, oConexion)
                '    cmd.CommandTimeout = 3 * 60
                '    cmd.ExecuteNonQuery()
                '    'consulta = "update ListaResidentes set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
                '    'cmd = New SqlCommand(consulta, oConexion)
                '    'cmd.CommandTimeout = 3 * 60
                '    'cmd.ExecuteNonQuery()
                '    'consulta = "update ListaResidentes set saldo = cast(saldo as varchar(8)) where saldo < 0"
                '    'cmd = New SqlCommand(consulta, oConexion)
                '    'cmd.CommandTimeout = 3 * 60
                '    'cmd.ExecuteNonQuery()
                '    consulta = "INSERT INTO listaresidentes SELECT * FROM  lista lT WHERE NOT EXISTS (SELECT LT.TAG FROM listaresidentes LI WHERE LT.Tag = LI.Tag)"
                '    cmd = New SqlCommand(consulta, oConexion)
                '    cmd.CommandTimeout = 3 * 60
                '    cmd.ExecuteNonQuery()
                '    ''''''''''''''' FIN RESIDENTES''''''''''''''''''''''''''''''''
                'End If

                If Not NombreCaseta = "XOCHITEPEC" Or NombreCaseta = "AEROPUERTO" Or NombreCaseta = "EMILIANOZAPATA" Or NombreCaseta = "TRESMARIAS" Then


                    ''''''''''''''' Monto Minimo''''''''''''''''''''''''''''''''''
                    consulta = "UPDATE ListaValidaciones SET Saldo = ListaTemporal.Saldo, Estatus = ListaTemporal.Estatus, Tipo = ListaTemporal.tipo  FROM ListaTemporal where ListaTemporal.Tag = ListaValidaciones.Tag and ListaValidaciones.EstatusResidente = '00'"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()
                    consulta = "update ListaValidaciones set saldo = cast(saldo as varchar(8)) where saldo > '99999999'"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    consulta = "update ListaValidaciones set saldo = cast(saldo as varchar(8)) where saldo > '99999999'"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()
                    consulta = "update ListaValidaciones set saldo = cast(saldo as varchar(8)) where saldo < 0"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()
                    consulta = "  INSERT INTO ListaValidaciones SELECT * FROM  lista lT WHERE NOT EXISTS (SELECT LT.TAG FROM ListaValidaciones LI WHERE LT.Tag = LI.Tag)"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()
                    ''''''''''''''' Fin Minimo'''''''''''''''''''''''''''''''''

                End If
                consulta = "UPDATE ListaAntifraude SET Saldo = ListaTemporal.Saldo, Estatus = ListaTemporal.Estatus, Tipo = ListaTemporal.tipo  FROM ListaTemporal where ListaTemporal.Tag = ListaAntifraude.Tag and ListaAntifraude.EstatusResidente = '00'"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()

                consulta = "update ListaAntifraude set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                consulta = "update ListaAntifraude set saldo = cast(saldo as varchar(8)) where saldo < 0"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()

            End If


            'agregar los dos 0
            consulta = " update dbo.Lista set tag = SUBSTRING(tag,0,4) + '00' + SUBSTRING (tag,4,16) where LEN(tag)  = 19  ;"
            cmd = New SqlCommand(consulta, oConexion)
            cmd.CommandTimeout = 3 * 60
            cmd.ExecuteNonQuery()
            consulta = " update dbo.ListaAntifraude set tag = SUBSTRING(tag,0,4) + '00' + SUBSTRING (tag,4,16) where LEN(tag)  = 19  ;"
            cmd = New SqlCommand(consulta, oConexion)
            cmd.CommandTimeout = 3 * 60
            cmd.ExecuteNonQuery()
            'consulta = "update lista set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
            'cmd = New SqlCommand(consulta, oConexion)
            'cmd.CommandTimeout = 3 * 60
            'cmd.ExecuteNonQuery()

            If bString = "R" Then
                consulta = "Truncate Table Listaresidentes"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.ExecuteNonQuery()
                consulta = "bulk insert ListaResidentes from '" & OrigenResidentes & "" & LeerArchivos & "' with ( FORMATFILE = '" & fmtResidentes & "');"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                ''''''''trunca la primera fila para que el primer tag aparezca ''''''''''
                If bandera = False Then
                    consulta = "INSERT INTO lista VALUES (0000000000000000000,00,00,999999999999,00,0000000000000000000000000000000000000000000000000);"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.ExecuteNonQuery()
                    bandera = True
                End If

                consulta = "update lista set saldo = cast(saldo as varchar(8)) where saldo > '99999999'"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()

                consulta = "update lista set saldo = cast(saldo as varchar(8)) where saldo < 0"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()

                'Se insertan los tags que no existian anteriormente 
                'consulta = "  INSERT INTO listaresidentes SELECT * FROM  lista lT WHERE NOT EXISTS (SELECT LT.TAG FROM listaresidentes LI WHERE LT.Tag = LI.Tag)"
                'cmd = New SqlCommand(consulta, oConexion)
                'cmd.CommandTimeout = 3 * 60
                'cmd.ExecuteNonQuery()

                consulta = "INSERT INTO ListaAntifraude SELECT * FROM  listaresidentes lT WHERE NOT EXISTS (SELECT LT.TAG FROM ListaAntifraude LI WHERE LT.Tag = LI.Tag)"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()

                'consulta = "update ListaAntifraude set saldo = cast(saldo as varchar(8)) where saldo > 99999999"
                'cmd = New SqlCommand(consulta, oConexion)
                'cmd.CommandTimeout = 3 * 60
                'cmd.ExecuteNonQuery()

                'consulta = "update ListaAntifraude set saldo = cast(saldo as varchar(8)) where saldo > '99999999'"
                'cmd = New SqlCommand(consulta, oConexion)
                'cmd.CommandTimeout = 3 * 60
                'cmd.ExecuteNonQuery()

            End If


            'crea el archivo lstbind



            Dim bString2 As String
            bString2 = Mid(LeerArchivos, 1, 1)
            'se valida el nombre del archivo procesado para mandar el mensaje de que se actualizo 
            If bString2 = "C" Or bString2 = "I" Or bString2 = "R" Then
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("El archivo :" & LeerArchivos & " se  cargo")
                consulta = "INSERT INTO dbo.historial (Archivo, fecha, extension) VALUES ('" & LeerArchivos & "' , '" & DateTime.Now.ToString("yyyy-dd-MM HH:mm:ss") & "'," & extension & ");"
                'consulta = "INSERT INTO dbo.historial (Archivo, fecha, extension) VALUES ('" & LeerArchivos & "' , '" & DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") & "'," & extension & ");"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.ExecuteNonQuery()
                hayarchivos = True
            End If
            'insertar a la base historial de archivos creados

        Catch ex As Exception
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine(ex.ToString & vbLf & "Pinche mario!")
            'Console.ReadKey()

        End Try
    End Sub

    Sub encabezados()

        'Dim di As DirectoryInfo = New DirectoryInfo("A:\Prueba1.txt")
        aplicaciondate = DateTime.Now.ToString("yyyyMMddHHmm")
        creationdate = DateTime.Now.ToString("yyyyMMddHHmm")
        Dim objReader As New StreamReader(PathTemporal & extension)

        'se cuentan las lineas totales y se deja solo a 6 digitos
        Dim lines As [String]() = System.IO.File.ReadAllLines(PathTemporal & extension)
        CountLins = lines.LongLength
        CountLins = CountLins.Substring(0, 6)

        'Se quita la linea del encabezado
        CountLins = CountLins - 1
        CountLins = CountLins.PadLeft(6, "0")
        objReader.Close()

        lines(0) = "63" & aplicaciondate & creationdate & "01" & PlazaNumber & extension & CountLins
        System.IO.File.WriteAllLines(PathTemporal & extension, lines)

    End Sub

    Sub creararchivos()
        'validamos si existe el directorio si no lo creamos

        If NombreCaseta = "MAC" Then
            ''''creamos el archivo Normal''
            'PathTemporal = "c:\temporal\LSTABINT."
            'ArchivoNormal()
            'encabezados()
            'CopiarCarpeta()

            'creamos el archivo Residentes'
            'PathTemporal = "c:\temporal\Residentes\LSTABINT."
            'ArchivoResidentes()
            'encabezados()
            'vDestino = DestinoResidentes & "LSTABINT."
            'CopiarCarpeta()

            ''creamos el archivo MontoMinimo''
            'PathTemporal = "c:\temporal\MontoMinimo\LSTABINT."
            'ArchivoMontoMinimo()
            'encabezados()
            'vDestino = DestinoMontominimo & "LSTABINT."
            'CopiarCarpeta()
            'borrararchivosMontominimo()
            'creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        ElseIf NombreCaseta = "ALPUYECA" Then
            'PathTemporal = "c:\temporal\LSTABINT."
            'ArchivoNormal()
            'encabezados()
            'CopiarCarpeta()
            PathTemporal = "c:\temporal\MontoMinimo\LSTABINT."
            ArchivoMontoMinimo()
            encabezados()
            vDestino = DestinoMontominimo & "LSTABINT."
            CopiarCarpeta()
            BorrararchivosMontominimo()
            ''creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        ElseIf NombreCaseta = "PASOMORELOS" Then
            PathTemporal = "c:\temporal\LSTABINT."
            ArchivoNormal()
            encabezados()
            CopiarCarpeta()
        ElseIf NombreCaseta = "PALOBLANCO" Then
            PathTemporal = "c:\temporal\LSTABINT."
            ArchivoNormal()
            encabezados()
            CopiarCarpeta()
        ElseIf NombreCaseta = "LAVENTA" Then
            ''creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        ElseIf NombreCaseta = "XOCHITEPEC" Then
            'creamos el archivo Residentes'
            PathTemporal = "c:\temporal\Residentes\LSTABINT."
            ArchivoResidentes()
            encabezados()
            vDestino = DestinoResidentes & "LSTABINT."
            CopiarCarpeta()
        ElseIf NombreCaseta = "AEROPUERTO" Then
            ''creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        ElseIf NombreCaseta = "EMILIANOZAPATA" Then
            'PathTemporal = "c:\temporal\LSTABINT."
            'ArchivoNormal()
            'encabezados()
            'CopiarCarpeta()

            ''creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                'vDestino = "\\192.168.0.90\geaint\PARAM\DK"
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        ElseIf NombreCaseta = "TLALPAN" Then
            PathTemporal = "c:\temporal\LSTABINT."
            ArchivoNormal()
            encabezados()
            CopiarCarpeta()
        ElseIf NombreCaseta = "TRESMARIAS" Then
            'PathTemporal = "c:\temporal\MontoMinimo\LSTABINT."
            'ArchivoMontoMinimo()
            'encabezados()
            ''vDestino = "\\192.168.0.85\ListasTelepeaje\MontoMinimo\LSTABINT."
            'vDestino = DestinoMontominimo & "LSTABINT."
            'CopiarCarpeta()
            'borrararchivosMontominimo()
            ''creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                'vDestino = "\\192.168.0.90\geaint\PARAM\DK"
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        ElseIf NombreCaseta = "FRANCISCOVELASCO" Then
            PathTemporal = "c:\temporal\MontoMinimo\LSTABINT."
            ArchivoMontoMinimo()
            encabezados()
            vDestino = DestinoMontominimo & "LSTABINT."
            CopiarCarpeta()
            BorrararchivosMontominimo()
            ''creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        ElseIf NombreCaseta = "CerroGordo" Then
            PathTemporal = "c:\temporal\MontoMinimo\LSTABINT."
            ArchivoMontoMinimo()
            encabezados()
            vDestino = DestinoMontominimo & "LSTABINT."
            CopiarCarpeta()
            BorrararchivosMontominimo()
            ''creamos el archivo Antifraude''
            PathTemporal = "c:\temporal\Antifraude\LSTABINT."
            ArchivoAntifraude()
            If banderaAntifraude = True Then
                encabezados()
                vDestino = DestinoAntifraude & "LSTABINT."
                CopiarCarpeta()
                banderaAntifraude = False
            End If
        End If



        AumentarExt()

    End Sub

    Private Sub CopiarCarpeta()
        Try
            exists = System.IO.Directory.Exists(vDestino & extension)
            If exists = False Then
                System.IO.File.Move(PathTemporal & extension, vDestino & extension)
            End If

        Catch ex As Exception
            'Console.ForegroundColor = ConsoleColor.Red
            'Console.WriteLine("Error al mover a la carpeta de destino")
            'Console.ReadKey()
        End Try

    End Sub

    Private Sub AumentarExt()
        'Se incrementa un digito a la extension y se reinicia al ser 999
        'Console.WriteLine("Archivo actualizado: " & extension)

        If extension = 999 Then
            extension = 1
            extension = extension.PadLeft(3, "0")
        Else
            extension = extension + 1
            extension = extension.PadLeft(3, "0")
        End If
        'consulta = "INSERT INTO dbo.historial (Archivo, fecha) VALUES ('" & LeerArchivos & "' , '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "');"

        consulta = "UPDATE parametrizable Set listbindEXT= '" & extension & "'"
        cmd = New SqlCommand(consulta, oConexion)
        cmd.ExecuteNonQuery()
    End Sub

    Sub Errur()
        For Each archivos As String In Directory.GetFiles("\\" & CASETAIP & "\geaint\PARAM\ERREUR\", "*")
            'Console.ForegroundColor = ConsoleColor.Red
            ' extraer el nombre de la ruta  
            archivos = archivos.Substring(archivos.LastIndexOf("\") + 1).ToString
            'Console.WriteLine("Los siguientes archivos estan la carpeta error: " & archivos.ToString)
        Next
    End Sub

    Sub ArchivoNormal()
        exists = System.IO.Directory.Exists("c:\temporal\")
        If exists = False Then
            System.IO.Directory.CreateDirectory("c:\temporal\")
        End If
        consulta = " Exec Master ..xp_Cmdshell 'bcp ""Select tag + REPLICATE ('' '', 24 - DATALENGTH(tag)) + ( tipo + Estatus  + REPLICATE (''0'', 8 - DATALENGTH(Saldo)) + saldo + SUBSTRING(tag,0,14) + REPLICATE ('' '', 19 - DATALENGTH(SUBSTRING(tag,0,14)))) + (EstatusResidente + ResidenteComplementario)  FROM Telepeaje.dbo.lista  order by tag asc""  queryout  ""C:\temporal\LSTABINT." & extension & """ -S " & ServidorSql & " -T -c -t\0'"
        'Exec Master ..xp_Cmdshell 'bcp "Select tag + REPLICATE ('' '', 24 - DATALENGTH(tag)) as tag , (tipo + Estatus  + REPLICATE (''0'', 8 - DATALENGTH(Saldo)) + Saldo + SUBSTRING(tag,0,14) + REPLICATE ('' '', 18 - DATALENGTH(SUBSTRING(tag,0,14))))  as Unidos, (EstatusResidente + ResidenteComplementario) as unidos2  FROM database_name.dbo.lista  order by tag asc"  queryout  "A:\LSTABINT.023" -S DESARROLLO2\SQLEXPRESS -T  -c -t0 '
        cmd = New SqlCommand(consulta, oConexion)
        'tiempo de espera sql
        cmd.CommandTimeout = 3 * 60
        cmd.ExecuteNonQuery()
    End Sub

    Sub ArchivoResidentes()
        'crear archivo para residentes
        'validamos si existe el directorio si no lo creamos
        exists = System.IO.Directory.Exists("c:\temporal\Residentes\")
        If exists = False Then
            System.IO.Directory.CreateDirectory("c:\temporal\Residentes\")
        End If

        consulta = " Exec Master ..xp_Cmdshell 'bcp ""Select tag + REPLICATE ('' '', 24 - DATALENGTH(tag)) + ( tipo + Estatus  + REPLICATE (''0'', 8 - DATALENGTH(Saldo)) + saldo + SUBSTRING(tag,0,14) + REPLICATE ('' '', 19 - DATALENGTH(SUBSTRING(tag,0,14)))) + (EstatusResidente + ResidenteComplementario)  FROM Telepeaje.dbo.ListaResidentes  order by tag asc""  queryout """ & PathTemporal & "" & extension & """ -S " & ServidorSql & " -T -c -t\0'"
        cmd = New SqlCommand(consulta, oConexion)
        cmd.CommandTimeout = 3 * 60
        cmd.ExecuteNonQuery()
    End Sub

    Sub ArchivoAntifraude()
        Try
            exists = System.IO.Directory.Exists("c:\temporal\Antifraude\")
            If exists = False Then
                System.IO.Directory.CreateDirectory("c:\temporal\Antifraude\")
            End If
            ConexionOracle.Open()
            consultaOracle = "Select contenu_iso from transaction where date_transaction >= TO_DATE('" & Format(DateTime.Now.AddMinutes(-minutos), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS') and ID_OBS_PASSAGE = 0 and ID_PAIEMENT = 15 group by CONTENU_ISO HAVING count(*)>" & cruzes
            'consultaOracle = "Select contenu_iso from transaction where date_transaction >= TO_DATE('" & Format(DateTime.Now.AddMinutes(-5), "yyyyMMddHHmmss") & "','YYYYMMDDHH24MISS') and ID_PAIEMENT = 15 group by CONTENU_ISO HAVING count(*)>1"
            Dim tag As String

            cmdOracle.CommandText = consultaOracle
            cmdOracle.Connection = ConexionOracle
            Dim dataReader As OracleDataReader = cmdOracle.ExecuteReader()
            'cambiar a estado invalido tag'
            While dataReader.Read
                tag = dataReader.Item("contenu_iso")
                tag = Trim(Mid(tag, 1, 16))
                consulta = "select tag from  listanegra where tag = '" & tag & "'"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                Dim taglistanegra = cmd.ExecuteScalar()

                If taglistanegra = "" Then

                    consulta = "update listaantifraude set Estatus = '00' where tag = '" & tag & "'"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()

                    consulta = "insert into ListaNegra values ('" & tag & "', '" & Format(DateTime.Now, "yyyy-dd-MM HH:mm:ss") & "')"
                    cmd = New SqlCommand(consulta, oConexion)
                    cmd.CommandTimeout = 3 * 60
                    cmd.ExecuteNonQuery()
                    banderaAntifraude = True
                End If
            End While
            dataReader.Close()
            Dim array As New ArrayList()
            ConexionOracle.Close()
            'quitar de la regla
            consulta = "Select tag from ListaNegra where Fecha <= '" & Format(DateTime.Now.AddMinutes(-minutos), "dd-MM-yyyy HH:mm:ss") & "'"
            'consulta = "Select tag from ListaNegra where Fecha <= '" & Format(DateTime.Now.AddMinutes(-minutos), "yyyy-MM-dd HH:mm:ss") & "'"
            cmd = New SqlCommand(consulta, oConexion)
            cmd.CommandTimeout = 3 * 60
            'Console.WriteLine(consulta)
            'Console.ReadKey()

            Dim Datareadersql As SqlDataReader = cmd.ExecuteReader()
            While Datareadersql.Read()
                array.Add(Datareadersql.Item("tag"))
            End While
            Datareadersql.Close()
            For Each tag In array
                consulta = "update listaantifraude set Estatus = '01' where tag = '" & tag & "'"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.ExecuteNonQuery()
                consulta = "IF NOT EXISTS (SELECT tAG FROM ListaNegraHistorico WHERE tag ='" & tag & "' AND Fecha < '" & Format(DateTime.Now.AddDays(-1), "dd/MM/yyyy HH:mm:ss") & "' ) insert into ListaNegraHistorico values('" & tag & "','" & Format(DateTime.Now, "dd/MM/yyyy HH:mm:ss") & "')"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                consulta = " DELETE From ListaNegra Where  tag = '" & tag & "'"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()
                'no va porque no hay modificacion en la lista antifraude
                banderaAntifraude = True
            Next


            If banderaAntifraude = True Then
                'genera los archivos '
                consulta = " Exec Master ..xp_Cmdshell 'bcp ""Select tag + REPLICATE ('' '', 24 - DATALENGTH(tag)) + ( tipo + Estatus  + REPLICATE (''0'', 8 - DATALENGTH(Saldo)) + saldo + SUBSTRING(tag,0,14) + REPLICATE ('' '', 19 - DATALENGTH(SUBSTRING(tag,0,14)))) + (EstatusResidente + ResidenteComplementario)  FROM Telepeaje.dbo.ListaAntifraude  order by tag asc""  queryout ""c:\temporal\Antifraude\LSTABINT." & extension & """ -S " & ServidorSql & " -T -c -t\0'"
                cmd = New SqlCommand(consulta, oConexion)
                cmd.CommandTimeout = 3 * 60
                cmd.ExecuteNonQuery()

            End If


        Catch ex As Exception
            'Console.ForegroundColor = ConsoleColor.Red
            'Console.WriteLine(ex.ToString & vbLf & "Pinche mario!")
        End Try

    End Sub

    Sub ArchivoMontoMinimo()
        '''''''''''''''''''Crear archivo con validaciones'''''''''''''''''''''''''''''''''''''''''''''''

        'validamos si existe el directorio si no lo creamos
        exists = System.IO.Directory.Exists("c:\temporal\MontoMinimo\")
        If exists = False Then
            System.IO.Directory.CreateDirectory("c:\temporal\MontoMinimo\")
        End If
        'creamos el archivo en un directorio diferente
        consulta = "UPDATE listaValidaciones SET Estatus = '00' WHERE Saldo < " & montominimo & " AND SALDO > 0 AND Estatus = '01'"
        cmd = New SqlCommand(consulta, oConexion) With {
            .CommandTimeout = 3 * 60
        }
        cmd.ExecuteNonQuery()
        consulta = " Exec Master ..xp_Cmdshell 'bcp ""Select tag + REPLICATE ('' '', 24 - DATALENGTH(tag)) + ( tipo + Estatus  + REPLICATE (''0'', 8 - DATALENGTH(Saldo)) + saldo + SUBSTRING(tag,0,14) + REPLICATE ('' '', 19 - DATALENGTH(SUBSTRING(tag,0,14)))) + (EstatusResidente + ResidenteComplementario)  FROM Telepeaje.dbo.listaValidaciones  order by tag asc""  queryout  ""c:\temporal\MontoMinimo\LSTABINT." & extension & """ -S " & ServidorSql & " -T -c -t\0'"
        cmd = New SqlCommand(consulta, oConexion)
        cmd.CommandTimeout = 3 * 60
        cmd.ExecuteNonQuery()
    End Sub

    Private Sub BorrararchivosMontominimo()
        Dim Extensionantifraude As String
        Extensionantifraude = extension - 1
        Extensionantifraude = Extensionantifraude.PadLeft(3, "0")

        Try
            My.Computer.FileSystem.DeleteFile(DestinoMontominimo & "LSTABINT." & Extensionantifraude)
        Catch ex As Exception
            GoTo LINEA
        End Try

LINEA:
    End Sub

    Sub Parametros()
        Try

            'Dim lista As New List(Of String)
            consulta = "select * from parametrizable"
            cmd = New SqlCommand(consulta, oConexion)
            Dim da As New SqlDataAdapter(cmd)
            Dim dt As New DataTable() 'Acá tendrás los datos de la consulta SQL
            da.Fill(dt)  'El tipo de dato depende de la columna de la tabla de la BD
            'validamos que el datable no valla vacio 
            If dt.Rows.Count > 0 Then
                'Do success
                Origen = Convert.ToString(dt.Rows(0)("Origen"))
                Destino = Convert.ToString(dt.Rows(0)("Destino"))
                montominimo = Convert.ToString(dt.Rows(0)("MontoRegla"))
                extension = Convert.ToString(dt.Rows(0)("listbindEXT"))
                fmt = Convert.ToString(dt.Rows(0)("fmt"))
                fmtResidentes = Convert.ToString(dt.Rows(0)("fmtResidentes"))
                OrigenResidentes = Convert.ToString(dt.Rows(0)("OrigenResidentes"))
                DestinoResidentes = Convert.ToString(dt.Rows(0)("DestinoResidentes"))
                DestinoAntifraude = Convert.ToString(dt.Rows(0)("DestinoAntifraude"))
                DestinoMontominimo = Convert.ToString(dt.Rows(0)("DestinoMontoMinimo"))
                cruzes = Convert.ToString(dt.Rows(0)("ReglaCruzes"))
                minutos = Convert.ToString(dt.Rows(0)("ReglaTiempoMinutos"))
                NombreCaseta = Convert.ToString(dt.Rows(0)("Nombre"))
                CASETAIP = Convert.ToString(dt.Rows(0)("IpServidor"))
                extension = extension.PadLeft(3, "0")
            End If

            ConexionOracle.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST= " & CASETAIP & ")(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=GEAPROD)));User Id=GEADBA;Password=FGEUORJVNE;"

            'Console.ForegroundColor = ConsoleColor.DarkGreen
            'Console.WriteLine("CONTROL DE LISTAS BLANCAS VERSION: 1.5" + vbLf + "Fecha y hora inicio de la aplicación: " & DateTime.Now)
            'Console.WriteLine("Caseta: " & NombreCaseta)
            'Console.WriteLine("Los Parametros actuales son:" + vbLf + "Ruta de archivo de origen: " + Origen + vbLf + "Ruta de destino: " + Destino + vbLf + "Monto minimo de cruze: " + montominimo + vbLf + "Extension actual LSTABINT: " + extension)
            ''Console.WriteLine("los parametros son correctos 's' 'n'? ")
            'escorrecto = Console.ReadLine()

            vDestino = Destino & "LSTABINT."
        Catch ex As Exception
            'Console.ForegroundColor = ConsoleColor.Red
            'Console.WriteLine(ex.ToString & vbLf & "Pinche mario!")

        End Try
    End Sub

End Class
