Imports System.Threading

' This code was implemented from the javascript algorithm from:
' http://mrdoob.com/lab/javascript/effects/water/02/

Public Class Parallax
    Implements IDisposable

    Private qualityX As Integer = 3
    Private qualityY As Integer = 3
    Private canvasWidth As Integer
    Private canvasHeight As Integer
    Private canvasSize As Integer

    Private imageHeightMap() As Byte
    Private parentForm As Form

    Private mImage As DirectBitmap
    Private mBumpMap As DirectBitmap
    Private mBumpMapGraphics As Graphics

    Private buffer1() As Integer
    Private buffer2() As Integer
    Private tmpBuffer() As Integer
    Private indices() As Integer
    Private levels() As Integer
    Private pixels() As Byte

    Private isUserInteracting As Boolean
    Private pointers As New List(Of Point)

    Private loopThread As Thread
    Private refreshThread As Thread
    Private abortThreads As Boolean

    Public lockObj As New Object()

    Public Sub New(parent As Form)
        parentForm = parent
        Initialize()

        AddHandler parentForm.SizeChanged, Sub() Initialize()

        AddHandler parentForm.MouseDown, Sub(sender1 As Object, e1 As MouseEventArgs)
                                             isUserInteracting = True
                                             pointers.Clear()
                                             pointers.Add(New Point((e1.X + 10) / parentForm.Width * canvasWidth, (e1.Y + 100) / parentForm.Height * canvasHeight))
                                         End Sub

        AddHandler parentForm.MouseMove, Sub(sender1 As Object, e1 As MouseEventArgs)
                                             pointers.Clear()
                                             pointers.Add(New Point((e1.X + 10) / parentForm.Width * canvasWidth, (e1.Y + 100) / parentForm.Height * canvasHeight))
                                         End Sub

        AddHandler parentForm.MouseUp, Sub() isUserInteracting = False

        loopThread = New Thread(AddressOf MainLoop)
        loopThread.Start()

        refreshThread = New Thread(Sub()
                                       Do
                                           parentForm.Invalidate()
                                           Thread.Sleep(33)
                                       Loop Until abortThreads
                                   End Sub)
        refreshThread.Start()
    End Sub

    Public ReadOnly Property Image As DirectBitmap
        Get
            Return mImage
        End Get
    End Property

    Public ReadOnly Property BumpMamp As DirectBitmap
        Get
            Return mBumpMap
        End Get
    End Property

    Public ReadOnly Property BumpMampGraphics As Graphics
        Get
            Return mBumpMapGraphics
        End Get
    End Property

    Public Sub Initialize()
        SyncLock lockObj
            canvasWidth = Math.Floor(parentForm.DisplayRectangle.Width / qualityX)
            canvasHeight = Math.Floor(parentForm.DisplayRectangle.Height / qualityY)
            canvasSize = canvasWidth * canvasHeight

            If mImage IsNot Nothing Then mImage.Dispose()
            mImage = New DirectBitmap(canvasWidth, canvasHeight)

            If mBumpMap IsNot Nothing Then mBumpMap.Dispose()
            mBumpMap = New DirectBitmap(canvasWidth, canvasHeight)

            If mBumpMapGraphics IsNot Nothing Then mBumpMapGraphics.Dispose()
            mBumpMapGraphics = Graphics.FromImage(mBumpMap)

            ReDim imageHeightMap(canvasSize * 4 - 1)
            ReDim indices(canvasHeight - 1)

            ReDim buffer1(canvasSize - 1)
            ReDim buffer2(canvasSize - 1)
            For i As Integer = 0 To canvasSize - 1
                buffer1(i) = 128
                buffer2(i) = 128
            Next
            ReDim tmpBuffer(canvasSize - 1)

            ReDim levels(canvasHeight - 1)
            ReDim pixels(canvasHeight - 1)
        End SyncLock
    End Sub

    Private Sub Emit(x As Integer, y As Integer)
        If x >= canvasWidth Then x = canvasWidth - 1
        If y >= canvasHeight Then y = canvasHeight - 1
        buffer1(x + y * canvasWidth) = 256
    End Sub

    ' FIXME: This is too slow!
    ' Need to find a way to only process the area affected by the text and not the whole image
    Private Sub ProcessBumpMap()
        For y As Integer = 0 To canvasHeight - 1
            For x As Integer = 0 To canvasWidth - 1
                ' Both do the same thing...
                If mBumpMap.Bits((x + y * canvasWidth) * 4 + 0) > 0 Then Emit(x, y)
                'If mBumpMap.Pixel(x, y).R <> 0 Then Emit(x, y)
            Next
        Next
    End Sub

    Private Sub MainLoop()
        Dim pixel As Integer
        Dim index As Integer
        Dim tmp As Integer
        Dim yz As Integer

        Do
            SyncLock lockObj
                If isUserInteracting Then
                    For i As Integer = 0 To pointers.Count - 1
                        Emit(pointers(i).X, pointers(i).Y)
                    Next
                End If

                ' Water
                For i As Integer = canvasWidth To canvasSize - canvasWidth - 1
                    pixel = ((buffer1(i - 1) + buffer1(i + 1) + buffer1(i - canvasWidth) + buffer1(i + canvasWidth)) >> 1) - buffer2(i)
                    pixel -= (pixel - 128) >> 4
                    buffer2(i) = pixel

                    imageHeightMap(i * 4) = ToByte(pixel)
                Next

                Tasks.Task.Factory.StartNew(Sub()
                                                Array.Copy(buffer1, tmpBuffer, canvasSize)
                                                Array.Copy(buffer2, buffer1, canvasSize)
                                                Array.Copy(tmpBuffer, buffer2, canvasSize)

                                                ProcessBumpMap()
                                            End Sub)

                'Array.Clear(indices, 0, indices.Length)

                For x = 0 To canvasWidth - 1
                    Array.Clear(pixels, 0, pixels.Length)
                    Array.Clear(levels, 0, levels.Length)

                    For y = 0 To canvasHeight - 1
                        index = (x + y * canvasWidth) * 4
                        indices(y) = index

                        pixel = imageHeightMap(index)
                        pixels(y) = pixel

                        tmp = y - (pixel * canvasHeight >> 10)
                        If tmp < 0 Then tmp += canvasHeight
                        levels(tmp) = y
                    Next

                    yz = -1
                    For y = 0 To canvasHeight - 1
                        If levels(y) > yz Then
                            yz = levels(y)
                            pixel = pixels(yz)
                        End If
                        pixels(y) = pixel
                        index = indices(y)

                        mImage.Bits(index + 0) = ToByte(pixel - 32 + (y >> 2))      ' B
                        mImage.Bits(index + 1) = ToByte(pixel - 64 + (y >> 2))      ' G
                        'mImage.Bits(index + 2) = 0                                  ' R
                        mImage.Bits(index + 3) = 255                                ' A
                    Next
                Next
            End SyncLock

            Thread.Sleep(10)
        Loop Until abortThreads
    End Sub

    Private Function ToByte(v As Integer) As Byte
        If v < 0 Then
            v = 0
        ElseIf v > 255 Then
            v = 255
        End If

        Return CByte(v)
    End Function

    Private Function OffsetToXY(offset As Integer) As Point
        Return New Point((offset / canvasWidth) * canvasWidth - offset, offset / canvasWidth)
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                abortThreads = True
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
