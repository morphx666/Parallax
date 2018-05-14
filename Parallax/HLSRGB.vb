<Serializable()>
Public Class HLSRGB
    Private mRed As Byte = 0
    Private mGreen As Byte = 0
    Private mBlue As Byte = 0
    Private mAlpha As Byte = 255

    Private mHue As Double = 0.0
    Private mLuminance As Double = 0.0
    Private mSaturation As Double = 0.0

    Public Structure HueLumSat
        Private mH As Double
        Private mL As Double
        Private mS As Double

        Public Sub New(hue As Double, lum As Double, sat As Double)
            mH = hue
            mL = lum
            mS = sat
        End Sub

        Public Property Hue() As Double
            Get
                Return mH
            End Get
            Set(value As Double)
                mH = value
            End Set
        End Property

        Public Property Lum() As Double
            Get
                Return mL
            End Get
            Set(value As Double)
                mL = value
            End Set
        End Property

        Public Property Sat() As Double
            Get
                Return mS
            End Get
            Set(value As Double)
                mS = value
            End Set
        End Property
    End Structure

    Public Sub New(c As Color)
        mRed = c.R
        mGreen = c.G
        mBlue = c.B
        mAlpha = c.A
        ToHLS()
    End Sub

    Public Sub New(hue As Double, luminance As Double, saturation As Double)
        mHue = hue
        mLuminance = luminance
        mSaturation = saturation
        ToRGB()
    End Sub

    Public Sub New(alpha As Byte, red As Byte, green As Byte, blue As Byte)
        Me.New(Color.FromArgb(alpha, red, green, blue))
    End Sub

    Public Sub New(hlsrgb As HLSRGB)
        mRed = hlsrgb.Red
        mBlue = hlsrgb.Blue
        mGreen = hlsrgb.Green
        mLuminance = hlsrgb.Luminance
        mHue = hlsrgb.Hue
        mSaturation = hlsrgb.Saturation
    End Sub

    Public Sub New(value As Byte)
        Me.New(value, value, value, value)
    End Sub

    Public Sub New()
    End Sub

    Public Property Red() As Byte
        Get
            Return mRed
        End Get
        Set(value As Byte)
            mRed = value
            ToHLS()
        End Set
    End Property

    Public Property Green() As Byte
        Get
            Return mGreen
        End Get
        Set(value As Byte)
            mGreen = value
            ToHLS()
        End Set
    End Property

    Public Property Blue() As Byte
        Get
            Return mBlue
        End Get
        Set(value As Byte)
            mBlue = value
            ToHLS()
        End Set
    End Property

    Public Property Luminance() As Double
        Get
            Return mLuminance
        End Get
        Set(value As Double)
            mLuminance = CheckLum(value)
            ToRGB()
        End Set
    End Property

    Public Property Hue() As Double
        Get
            Return mHue
        End Get
        Set(value As Double)
            mHue = CheckHue(value)
            ToRGB()
        End Set
    End Property

    Public Property Saturation() As Double
        Get
            Return mSaturation
        End Get
        Set(value As Double)
            mSaturation = CheckSat(value)
            ToRGB()
        End Set
    End Property

    Public Property Alpha() As Byte
        Get
            Return mAlpha
        End Get
        Set(value As Byte)
            mAlpha = value
        End Set
    End Property

    Public Property HLS() As HueLumSat
        Get
            Return New HueLumSat(mHue, mLuminance, mSaturation)
        End Get
        Set(value As HueLumSat)
            mHue = CheckHue(value.Hue)
            mLuminance = CheckLum(value.Lum)
            mSaturation = CheckSat(value.Sat)
            ToRGB()
        End Set
    End Property

    Public Property Color() As Color
        Get
            Return Drawing.Color.FromArgb(mAlpha, mRed, mGreen, mBlue)
        End Get
        Set(value As Color)
            mRed = value.R
            mGreen = value.G
            mBlue = value.B
            mAlpha = value.A
            ToHLS()
        End Set
    End Property

    Public Sub LightenColor(lightenBy As Double)
        mLuminance *= (1.0F + lightenBy)
        If mLuminance > 1.0F Then Luminance = 1.0F
        ToRGB()
    End Sub

    Public Sub DarkenColor(darkenBy As Double)
        Luminance *= darkenBy
        ToRGB()
    End Sub

    Private Sub ToHLS()
        Dim minval As Byte = Math.Min(mRed, Math.Min(mGreen, mBlue))
        Dim maxval As Byte = Math.Max(mRed, Math.Max(mGreen, mBlue))

        Dim mdiff As Double = CSng(maxval) - CSng(minval)
        Dim msum As Double = CSng(maxval) + CSng(minval)

        mLuminance = msum / 510.0F

        If maxval = minval Then
            mSaturation = 0.0F
            mHue = 0.0F
        Else
            Dim rnorm As Double = (maxval - mRed) / mdiff
            Dim gnorm As Double = (maxval - mGreen) / mdiff
            Dim bnorm As Double = (maxval - mBlue) / mdiff

            mSaturation = If(mLuminance <= 0.5F, (mdiff / msum), mdiff / (510.0F - msum))

            If mRed = maxval Then mHue = 60.0F * (6.0F + bnorm - gnorm)
            If mGreen = maxval Then mHue = 60.0F * (2.0F + rnorm - bnorm)
            If mBlue = maxval Then mHue = 60.0F * (4.0F + gnorm - rnorm)
            If mHue > 360.0F Then mHue = Hue - 360.0F
        End If
    End Sub

    Private Sub ToRGB()
        If mSaturation = 0.0 Then
            Red = CByte(mLuminance * 255.0F)
            mGreen = mRed
            mBlue = mRed
        Else
            Dim rm1 As Double
            Dim rm2 As Double

            If mLuminance <= 0.5F Then
                rm2 = mLuminance + mLuminance * mSaturation
            Else
                rm2 = mLuminance + mSaturation - mLuminance * mSaturation
            End If
            rm1 = 2.0F * mLuminance - rm2
            mRed = ToRGB1(rm1, rm2, mHue + 120.0F)
            mGreen = ToRGB1(rm1, rm2, mHue)
            mBlue = ToRGB1(rm1, rm2, mHue - 120.0F)
        End If
    End Sub

    Private Function ToRGB1(rm1 As Double, rm2 As Double, rh As Double) As Byte
        If rh > 360.0F Then
            rh -= 360.0F
        ElseIf rh < 0.0F Then
            rh += 360.0F
        End If

        If (rh < 60.0F) Then
            rm1 = rm1 + (rm2 - rm1) * rh / 60.0F
        ElseIf (rh < 180.0F) Then
            rm1 = rm2
        ElseIf (rh < 240.0F) Then
            rm1 = rm1 + (rm2 - rm1) * (240.0F - rh) / 60.0F
        End If

        'TODO: Fix this... we shouldn't have to use a Try/Catch

        Return CByte(If(rm1 * 255 > 255, 255, rm1 * 255))

        'Try
        '    Return CByte(rm1 * 255)
        'Catch ex As Exception
        '    Debug.WriteLine("ToRGB1: " + ex.Message)
        '    Return CByte(255)
        'End Try
    End Function

    Private Function CheckHue(value As Double) As Double
        If value < 0.0F Then value = Math.Abs((360.0F + value) Mod 360.0F)
        If value > 360.0F Then value = value Mod 360.0F

        Return value
    End Function

    Private Function CheckLum(value As Double) As Double
        If (value < 0.0F) Or (value > 1.0F) Then
            If value < 0.0F Then value = Math.Abs(value)
            If value > 1.0F Then value = 1.0F
        End If

        Return value
    End Function

    Private Function CheckSat(value As Double) As Double
        If value < 0.0F Then value = Math.Abs(value)
        If value > 1.0F Then value = 1.0F

        Return value
    End Function

    Public Shared Widening Operator CType(value As Color) As HLSRGB
        Return New HLSRGB(value)
    End Operator

    Public Shared Narrowing Operator CType(value As HLSRGB) As Color
        Return value.Color
    End Operator

    Public Overrides Function ToString() As String
        Return $"H:{mHue:F2}, L:{mLuminance:F2}, S:{mSaturation:F2}, [A:{mAlpha}, R:{mRed}, G:{mGreen}, B:{mBlue}]"
    End Function


    Public Function ToHTML() As String
        Return String.Format("#{0}{1}{2}", mRed.ToString("X").PadLeft(2, "0"),
                                            mGreen.ToString("X").PadLeft(2, "0"),
                                            mBlue.ToString("X").PadLeft(2, "0"))
    End Function
End Class