' https://gist.github.com/Flafla2/1a0b9ebef678bbce3215
' http://flafla2.github.io/2014/08/09/perlinnoise.html
' https://mrl.nyu.edu/~perlin/noise/

Public Class PerlinNoise
    ' Hash lookup table As defined by Ken Perlin.  This is a randomly arranged array of all numbers from 0-255 inclusive
    Private Shared ReadOnly permutation() As Integer = {151, 160, 137, 91, 90, 15,
                                                        131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23,
                                                        190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
                                                        88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166,
                                                        77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244,
                                                        102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196,
                                                        135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123,
                                                        5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42,
                                                        223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
                                                        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228,
                                                        251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107,
                                                        49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254,
                                                        138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
                                                }

    Private Shared ReadOnly p(512 - 1) As Integer

    Public Shared Sub Init()
        For x As Integer = 0 To p.Length - 1
            p(x) = permutation(x Mod 256)
        Next
    End Sub

    Public Shared Function Perlin(x As Double, y As Double, z As Double) As Double
        Dim xi As Integer = Int(x) And 255
        Dim yi As Integer = Int(y) And 255
        Dim zi As Integer = Int(z) And 255
        Dim xf As Double = x - Int(x)
        Dim yf As Double = y - Int(y)
        Dim zf As Double = z - Int(z)
        Dim u As Double = Fade(xf)
        Dim v As Double = Fade(yf)
        Dim w As Double = Fade(zf)

        Dim a As Integer = p(xi) + yi
        Dim aa As Integer = p(a) + zi
        Dim ab As Integer = p(a + 1) + zi
        Dim b As Integer = p(xi + 1) + yi
        Dim ba As Integer = p(b) + zi
        Dim bb As Integer = p(b + 1) + zi

        Dim x1 As Double
        Dim x2 As Double
        Dim y1 As Double
        Dim y2 As Double

        x1 = Lerp(Grad(p(aa), xf, yf, zf), Grad(p(ba), xf - 1, yf, zf), u)
        x2 = Lerp(Grad(p(ab), xf, yf - 1, zf), Grad(p(bb), xf - 1, yf - 1, zf), u)
        y1 = Lerp(x1, x2, v)
        x1 = Lerp(Grad(p(aa + 1), xf, yf, zf - 1), Grad(p(ba + 1), xf - 1, yf, zf - 1), u)
        x2 = Lerp(Grad(p(ab + 1), xf, yf - 1, zf - 1), Grad(p(bb + 1), xf - 1, yf - 1, zf - 1), u)
        y2 = Lerp(x1, x2, v)
        Return (Lerp(y1, y2, w) + 1) / 2
    End Function

    Private Shared Function Grad(hash As Integer, x As Double, y As Double, z As Double) As Double
        Dim h As Integer = hash And 15
        Dim u As Double = If(h < 8, x, y)

        Dim v As Double

        If h < 4 Then
            v = y
        ElseIf h = 12 Or h = 14 Then
            v = x
        Else
            v = z
        End If

        Return If((h And 1) = 0, u, -u) + If((h And 2) = 0, v, -v)
    End Function

    Private Shared Function Fade(t As Double) As Double
        Return t * t * t * (t * (t * 6 - 15) + 10)
    End Function

    Private Shared Function Lerp(a As Double, b As Double, x As Double) As Double
        Return a + x * (b - a)
    End Function

    Public Shared Function OctavePerlin(x As Double, y As Double, z As Double, octaves As Integer, persistence As Double)
        Dim total As Double = 0.0
        Dim frequency As Double = 1.0
        Dim amplitude As Double = 1.0
        Dim maxValue As Double = 0.0

        For i As Integer = 0 To octaves - 1
            total += Perlin(x * frequency, y * frequency, z * frequency) * amplitude
            amplitude *= persistence
            frequency *= 2
            maxValue += amplitude
        Next

        Return total / maxValue
    End Function
End Class
