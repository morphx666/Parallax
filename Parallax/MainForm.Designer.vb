<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TextBoxMsg = New System.Windows.Forms.TextBox()
        Me.ComboBoxMode = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'TextBoxMsg
        '
        Me.TextBoxMsg.Location = New System.Drawing.Point(14, 14)
        Me.TextBoxMsg.Name = "TextBoxMsg"
        Me.TextBoxMsg.Size = New System.Drawing.Size(411, 23)
        Me.TextBoxMsg.TabIndex = 0
        Me.TextBoxMsg.Text = "Hello World!"
        '
        'ComboBoxMode
        '
        Me.ComboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBoxMode.FormattingEnabled = True
        Me.ComboBoxMode.Items.AddRange(New Object() {"Watter", "Fire"})
        Me.ComboBoxMode.Location = New System.Drawing.Point(431, 14)
        Me.ComboBoxMode.Name = "ComboBoxMode"
        Me.ComboBoxMode.Size = New System.Drawing.Size(121, 23)
        Me.ComboBoxMode.TabIndex = 1
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1040, 626)
        Me.Controls.Add(Me.ComboBoxMode)
        Me.Controls.Add(Me.TextBoxMsg)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Parallax"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TextBoxMsg As System.Windows.Forms.TextBox
    Friend WithEvents ComboBoxMode As ComboBox
End Class
