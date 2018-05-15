Public Class MainForm
    Private p As Parallax
    Private f As Font

    Private refreshBumpMap As Boolean = True

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If p IsNot Nothing Then
            p.Dispose()
            p = Nothing
        End If
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        f = New Font("Tahoma", 28, FontStyle.Bold, GraphicsUnit.Point)
        p = New Parallax(Me, 4, 4)

        ComboBoxMode.SelectedIndex = 0

        AddHandler TextBoxMsg.TextChanged, Sub() refreshBumpMap = True
        AddHandler Me.SizeChanged, Sub() refreshBumpMap = True
        AddHandler ComboBoxMode.SelectedIndexChanged, Sub()
                                                          refreshBumpMap = True
                                                          p.Mode = [Enum].Parse(GetType(Parallax.Modes), ComboBoxMode.SelectedIndex)
                                                      End Sub
    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        If p IsNot Nothing Then
            If refreshBumpMap Then
                p.BumpMampGraphics.Clear(Color.Black)
                p.BumpMampGraphics.DrawString(TextBoxMsg.Text, f, Brushes.White,
                                                (p.Image.Width - p.BumpMampGraphics.MeasureString(TextBoxMsg.Text, f).Width) / 2,
                                                (p.Image.Height - f.Height) / 2 + If(p.Mode = Parallax.Modes.Watter, f.Height / 2, 0))
                refreshBumpMap = False
            End If

            e.Graphics.DrawImage(p.Image.Bitmap, Me.DisplayRectangle)
        End If
    End Sub
End Class
