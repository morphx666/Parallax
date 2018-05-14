Imports System.Threading

' This code was implemented from the javascript algorithm from:
' http://mrdoob.com/lab/javascript/effects/water/02/

Public Class Parallax
    Implements IDisposable

    Public Enum Modes
        Watter = 0
        Fire = 1
    End Enum

    Private qualityX As Integer = 4
    Private qualityY As Integer = 4
    Private canvasWidth As Integer
    Private canvasHeight As Integer
    Private canvasSize As Integer

    Private imageWaterHeightMap() As Byte
    Private imageCoolingMap() As Byte
    Private parentForm As Form

    Private mImage As DirectBitmap
    Private mBumpMap As DirectBitmap
    Private mBumpMapGraphics As Graphics

    Private mMode As Modes = Modes.Watter

    Private waterBuffer1() As Integer
    Private waterBuffer2() As Integer
    Private fireBuffer1() As Integer
    Private fireBuffer2() As Integer
    Private tmpBuffer() As Integer
    Private indices() As Integer
    Private levels() As Integer
    Private pixels() As Byte

    Private isUserInteracting As Boolean
    Private pointers As New List(Of Point)
    Private pixel As Integer
    Private index As Integer
    Private tmp As Integer
    Private yz As Integer
    Private yStart As Double = 0
    Private coolingBrightness As Integer = 30

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

    Public Property Mode As Modes
        Get
            Return mMode
        End Get
        Set(value As Modes)
            mMode = value
            Initialize()
        End Set
    End Property

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
            PerlinNoise.Init()

            canvasWidth = Math.Floor(parentForm.DisplayRectangle.Width / qualityX)
            canvasHeight = Math.Floor(parentForm.DisplayRectangle.Height / qualityY)
            canvasSize = canvasWidth * canvasHeight

            If mImage IsNot Nothing Then mImage.Dispose()
            mImage = New DirectBitmap(canvasWidth, canvasHeight)

            If mBumpMap IsNot Nothing Then mBumpMap.Dispose()
            mBumpMap = New DirectBitmap(canvasWidth, canvasHeight)

            If mBumpMapGraphics IsNot Nothing Then mBumpMapGraphics.Dispose()
            mBumpMapGraphics = Graphics.FromImage(mBumpMap)

            ReDim imageWaterHeightMap(canvasSize * 4 - 1)
            ReDim imageCoolingMap(canvasSize * 4 - 1)
            ReDim indices(canvasHeight - 1)

            ReDim waterBuffer1(canvasSize - 1)
            ReDim waterBuffer2(canvasSize - 1)
            For i As Integer = 0 To canvasSize - 1
                waterBuffer1(i) = 128
                waterBuffer2(i) = 128
            Next
            ReDim fireBuffer1(canvasSize - 1)
            ReDim fireBuffer2(canvasSize - 1)
            ReDim tmpBuffer(canvasSize - 1)

            ReDim levels(canvasHeight - 1)
            ReDim pixels(canvasHeight - 1)
        End SyncLock
    End Sub

    Private Sub Emit(x As Integer, y As Integer, pixelSize As Integer)
        If x >= canvasWidth Then x = canvasWidth - 1
        If y >= canvasHeight Then y = canvasHeight - 1
        waterBuffer1(x + y * canvasWidth) = 255


        If pixelSize > 1 Then
            For i As Integer = x - pixelSize To x + pixelSize
                For j As Integer = y - pixelSize To y + pixelSize
                    If i >= canvasWidth Then i = canvasWidth - 1
                    If j >= canvasHeight Then j = canvasHeight - 1
                    fireBuffer1(i + j * canvasWidth) = 255
                Next
            Next
        Else
            fireBuffer1(x + y * canvasWidth) = 255
        End If
    End Sub

    ' FIXME: This is too slow!
    ' Need to find a way to only process the area affected by the text and not the whole image
    Private Sub ProcessBumpMap()
        For y As Integer = 0 To canvasHeight - 1
            For x As Integer = 0 To canvasWidth - 1
                ' Both do the same thing...
                If mBumpMap.Bits((x + y * canvasWidth) * 4 + 0) > 0 Then Emit(x, y, 1)
                'If mBumpMap.Pixel(x, y).R <> 0 Then Emit(x, y)
            Next
        Next
    End Sub

    Private Sub ProcessWater()
        For i As Integer = canvasWidth To canvasSize - canvasWidth - 1
            pixel = ((waterBuffer1(i - 1) + waterBuffer1(i + 1) + waterBuffer1(i - canvasWidth) + waterBuffer1(i + canvasWidth)) >> 1) - waterBuffer2(i)
            pixel -= (pixel - 128) >> 4
            waterBuffer2(i) = pixel

            imageWaterHeightMap(i * 4) = ToByte(pixel)
        Next

        Array.Copy(waterBuffer1, tmpBuffer, canvasSize)
        Array.Copy(waterBuffer2, waterBuffer1, canvasSize)
        Array.Copy(tmpBuffer, waterBuffer2, canvasSize)
    End Sub

    Private Sub ProcessParallax()
        For x = 0 To canvasWidth - 1
            Array.Clear(pixels, 0, pixels.Length)
            Array.Clear(levels, 0, levels.Length)

            For y = 0 To canvasHeight - 1
                index = (x + y * canvasWidth) * 4
                indices(y) = index

                pixel = imageWaterHeightMap(index)
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
    End Sub

    Private Sub ProcessFire()
        For i As Integer = canvasWidth To canvasSize - canvasWidth - 1
            pixel = ((fireBuffer1(i - 1) + fireBuffer1(i + 1) + fireBuffer1(i - canvasWidth) + fireBuffer1(i + canvasWidth)) >> 2) - imageCoolingMap(i)
            pixel = If(pixel < 0, 0, pixel)
            fireBuffer2(i - canvasWidth) = pixel

            pixel = ToByte(pixel)
            mImage.Pixel(i * 4) = Color.FromArgb(pixel, pixel, pixel * (1 - imageCoolingMap(i) / coolingBrightness), 0)
        Next

        Array.Copy(fireBuffer1, tmpBuffer, canvasSize)
        Array.Copy(fireBuffer2, fireBuffer1, canvasSize)
        Array.Copy(tmpBuffer, fireBuffer2, canvasSize)
    End Sub

    Private Sub ProcessCoolingMap()
        Dim xOffset As Double = 0.0
        Dim yOffset As Double = 0.0
        Dim inc As Double = 0.02
        Dim b As Double

        For x As Double = 0 To canvasWidth - 1
            xOffset += inc
            yOffset = yStart
            For y As Double = 1 To canvasHeight - 1
                yOffset += inc

                b = PerlinNoise.Perlin(xOffset, yOffset, yOffset) * coolingBrightness

                imageCoolingMap(x + (y - 1) * canvasWidth) = b
                'mCoolingMap.Pixel(x, y - 1) = Color.FromArgb(b, b, b, b)
            Next
        Next

        yStart += inc
    End Sub

    Private Sub MainLoop()
        Do
            SyncLock lockObj
                If isUserInteracting Then
                    For i As Integer = 0 To pointers.Count - 1
                        Emit(pointers(i).X, pointers(i).Y, 2)
                    Next
                End If

                Select Case mMode
                    Case Modes.Watter
                        ProcessWater()
                        ProcessBumpMap()
                        ProcessParallax()
                    Case Modes.Fire
                        ProcessFire()
                        ProcessBumpMap()
                        ProcessCoolingMap()
                End Select
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
