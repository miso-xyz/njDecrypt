Imports System.Security.Cryptography
Imports System.IO
Imports System.Text
Imports System.Reflection
Imports System.Resources
Imports dnlib.DotNet
Module Module1

    Sub Main(ByVal args As String())
        Console.Title = "njDecrypt v1.0"
        Console.WriteLine("njDecrypt v1.0 by misonothx - Decrypter & Unpacker for njCrypt")
        Console.WriteLine()
        Dim asm As Assembly
        Try
            asm = Assembly.LoadFile(Path.GetFullPath(args(0)))
        Catch ex As Exception
            Console.ForegroundColor = ConsoleColor.DarkRed
            Console.Write("Invalid file, please make sure the input file is a protected executable.")
            Console.ReadKey()
            End
        End Try
        Dim patchedApp As dnlib.DotNet.ModuleDef = dnlib.DotNet.ModuleDefMD.Load(args(0))
        Dim types As New List(Of String)
        Dim methods As New List(Of String)
        Dim strings As New List(Of String)
        For x = 0 To patchedApp.Types.Count - 1
            types.Add(patchedApp.Types(x).ToString)
        Next
        For x = 0 To patchedApp.Types(types.IndexOf("Stub.cMain")).Methods.Count - 1
            methods.Add(patchedApp.Types(types.IndexOf("Stub.cMain")).Methods(x).ToString.Split("::")(2))
        Next
        For x = 0 To patchedApp.Types(types.IndexOf("Stub.cMain")).Methods(methods.IndexOf("Main()")).Body.Instructions.Count - 1
            If patchedApp.Types(types.IndexOf("Stub.cMain")).Methods(methods.IndexOf("Main()")).Body.Instructions(x).OpCode.ToString = "ldstr" Then
                strings.Add(patchedApp.Types(types.IndexOf("Stub.cMain")).Methods(methods.IndexOf("Main()")).Body.Instructions(x).Operand.ToString)
            End If
        Next
        Console.ForegroundColor = ConsoleColor.Magenta
        Console.WriteLine("Encryption Password: " & strings(1))
        Console.ForegroundColor = ConsoleColor.Yellow
        Dim ms As New MemoryStream()
        asm.GetManifestResourceStream(strings(0)).CopyTo(ms)
        Console.WriteLine("Extracting """ & strings(0) & """...")
        If Not Directory.Exists("njDecrypt") Then
            Directory.CreateDirectory("njDecrypt")
        End If
        Directory.CreateDirectory("njDecrypt\" & Path.GetFileNameWithoutExtension(args(0)))
        Try
            File.WriteAllBytes("njDecrypt\" & Path.GetFileNameWithoutExtension(args(0)) & "\" & strings(0) & ".bin", Decrypt(ms.ToArray(), Encoding.Default.GetBytes(strings(1))))
        Catch ex As Exception
            Console.ForegroundColor = ConsoleColor.DarkRed
            Console.WriteLine("Failed to extract """ & strings(0) & """ (exception: " & ex.Message & ")")
            Console.ReadKey()
            End
        End Try
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("Successful extraction. Press any key to close the application")
        Console.ReadKey()
    End Sub

    Public Function Decrypt(ByVal bData As Byte(), ByVal bKey As Byte()) As Byte()
        Dim result As Byte() = Nothing
        Using memoryStream As MemoryStream = New MemoryStream()
            Using rijndaelManaged As RijndaelManaged = New RijndaelManaged()
                rijndaelManaged.KeySize = 256
                rijndaelManaged.BlockSize = 128
                Dim rfc2898DeriveBytes As Rfc2898DeriveBytes = New Rfc2898DeriveBytes(bKey, New Byte(7) {}, 1000)
                rijndaelManaged.Key = rfc2898DeriveBytes.GetBytes(rijndaelManaged.KeySize / 8)
                rijndaelManaged.IV = rfc2898DeriveBytes.GetBytes(rijndaelManaged.BlockSize / 8)
                rijndaelManaged.Mode = CipherMode.CBC
                Using cryptoStream As CryptoStream = New CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Write)
                    cryptoStream.Write(bData, 0, bData.Length)
                    cryptoStream.Close()
                End Using
                result = memoryStream.ToArray()
            End Using
        End Using
        Return result
    End Function
End Module
