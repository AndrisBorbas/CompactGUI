﻿Imports Microsoft.Toolkit.Mvvm.ComponentModel

<PropertyChanged.AddINotifyPropertyChangedInterface>
Public Class WatchedFolder : Inherits ObservableObject

    Public Property Folder As String
    Public Property DisplayName As String
    Public Property IsSteamGame As Boolean
    Public Property LastCompressedDate As DateTime
    Public Property LastCompressedSize As Long
    Public Property LastUncompressedSize As Long
    Public Property LastSystemModifiedDate As DateTime
    Public Property LastCheckedDate As DateTime
    Public Property LastCheckedSize As Long
    Public Property CompressionLevel As Core.CompressionAlgorithm

    Public ReadOnly Property DecayPercentage As Decimal
        Get
            If LastCompressedSize = 0 Then Return 1
            Return If(LastUncompressedSize = LastCompressedSize OrElse LastCompressedSize > LastUncompressedSize, 0D, Math.Clamp((LastCheckedSize - LastCompressedSize) / (LastUncompressedSize - LastCompressedSize), 0, 1))
        End Get
    End Property

    Public Sub RefreshProperties()
        For Each prop In Me.GetType.GetProperties
            Me.OnPropertyChanged(prop.Name)
        Next
    End Sub

End Class


