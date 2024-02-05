﻿Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Public Module SharedMethods


    Function verifyFolder(folder As String) As (isValid As Boolean, msg As String)

        If Not IO.Directory.Exists(folder) Then : Return (False, "Directory does not exist")
        ElseIf folder.Contains((Environment.GetFolderPath(Environment.SpecialFolder.Windows))) Then : Return (False, "Cannot compress system directory")
        ElseIf folder.EndsWith(":\") Then : Return (False, "Cannot compress root directory")
        ElseIf Not IO.Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Any() Then : Return (False, "Directory is empty")
        ElseIf DriveInfo.GetDrives().First(Function(f) folder.StartsWith(f.Name)).DriveFormat <> "NTFS" Then : Return (False, "Cannot compress a directory on a non-NTFS drive")
        End If

        Return (True, "")

    End Function

    Function GetFileSizeOnDisk(file As String) As Long
        Dim hosize As UInteger
        Dim losize As UInteger = GetCompressedFileSizeW(file, hosize)
        'INVALID_FILE_SIZE (0xFFFFFFFF)
        If losize = 4294967295 Then
            Dim errCode As Integer
            errCode = Marshal.GetLastPInvokeError()
            If errCode <> 0 Then
                Return -1
            End If
        End If
        Return CLng(hosize) << 32 Or losize
    End Function


    <Extension()>
    Function AsShortPathNames(filesList As IEnumerable(Of String)) As List(Of String)

        Return filesList.Select(Of String) _
            (Function(fl)
                 If fl.Length >= 255 Then
                     Dim sfp = GetShortPath(fl)
                     If sfp IsNot Nothing Then Return sfp
                 End If
                 Return fl
             End Function).ToList

    End Function


    Function GetShortPath(filePath As String) As String

        If String.IsNullOrWhiteSpace(filePath) Then Return Nothing
        Dim hasPrefix As Boolean = False

        If filePath.Length >= 255 AndAlso Not filePath.StartsWith("\\?\") Then
            filePath = "\\?\" & filePath
            hasPrefix = True
        End If

        Dim shortPath = New StringBuilder(1024)
        Dim res As Integer = GetShortPathName(filePath, shortPath, 1024)
        If res = 0 Then Return Nothing
        filePath = shortPath.ToString()
        If hasPrefix Then filePath = filePath.Substring(4)
        Return filePath

    End Function


    Function GetClusterSize(folderPath As String)

        Dim lpSectorsPerCluster As UInteger
        Dim lpBytesPerSector As UInteger
        Dim res As Integer = GetDiskFreeSpace(New DirectoryInfo(folderPath).Root.ToString, lpSectorsPerCluster, lpBytesPerSector, Nothing, Nothing)
        Return lpSectorsPerCluster * lpBytesPerSector

    End Function



#Region "DLL Imports"

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function GetCompressedFileSizeW(
    <[In](), MarshalAs(UnmanagedType.LPWStr)> lpFileName As String,
    <Out(), MarshalAs(UnmanagedType.U4)> ByRef lpFileSizeHigh As UInteger) _
    As UInteger
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
    Private Function GetShortPathName(
        <MarshalAs(UnmanagedType.LPTStr)> ByVal path As String,
        <MarshalAs(UnmanagedType.LPTStr)> ByVal shortPath As StringBuilder, ByVal shortPathLength As Integer) As Integer

    End Function



    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function GetDiskFreeSpace(
        ByVal lpRootPathName As String,
        <Out> ByRef lpSectorsPerCluster As UInteger,
        <Out> ByRef lpBytesPerSector As UInteger,
        <Out> ByRef lpNumberOfFreeClusters As UInteger,
        <Out> ByRef lpTotalNumberOfClusters As UInteger) As Boolean
    End Function


#End Region


End Module
