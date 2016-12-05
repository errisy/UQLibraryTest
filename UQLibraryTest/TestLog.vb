Imports System.ComponentModel

Public Class TestLog
    Implements System.ComponentModel.INotifyPropertyChanged
    Public Property Action As String
    Public Property ActionBrush As Brush = Brushes.White
    Public Property Comment As String
    Public Property Result As String
    Public Property Failed As String

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class
