Public Class frmMain
    Private fileName As String
    Private Sub mnuOpen_Click(sender As Object, e As EventArgs) Handles mnuOpen.Click
        On Error GoTo Err
        txtOldName.Text = ""
        txtOldID.Text = ""

        cdlOpen.Multiselect = False
        cdlOpen.Filter = "Any file(*.*)|*.*"
        cdlOpen.FileName = ""
        cdlOpen.ShowDialog()

        If cdlOpen.FileName <> "" Then
            fileName = cdlOpen.FileName
            mnuSave.Enabled = True
            ReadImg()
        End If

Err:
        Exit Sub
    End Sub

    Private Sub ReadImg()
        Dim gameName(0 To 99) As Byte
        Dim strGameName As String
        Dim gameID(0 To 5) As Byte
        Dim strGameID As String
        Dim fileHead(0 To 3) As Byte
        Dim i As Integer

        Try
            FileSystem.FileOpen(1, fileName, OpenMode.Binary)
            FileSystem.FileGet(1, fileHead)
            ' The offset for wbfs file is different with iso or gcm file
            If fileHead(0) = 87 And fileHead(1) = 66 And fileHead(2) = 70 And fileHead(3) = 83 Then
                FileSystem.Seek(1, 2097153)
                FileSystem.FileGet(1, gameID)
                FileSystem.Seek(1, 2097185)
                FileSystem.FileGet(1, gameName)
            Else
                FileSystem.Seek(1, 1)
                FileSystem.FileGet(1, gameID)
                FileSystem.Seek(1, 33)
                FileSystem.FileGet(1, gameName)
            End If
            FileSystem.FileClose(1)

            strGameID = ""
            For i = 0 To 5
                strGameID = strGameID + Chr(gameID(i))
            Next
            txtOldID.Text = strGameID
            txtNewID.Text = strGameID

            strGameName = ""
            For i = 0 To 99
                If gameName(i) = 0 Then
                    Exit For
                End If
                strGameName = strGameName + Chr(gameName(i))
            Next
            txtOldName.Text = strGameName
            txtNewName.Text = strGameName

        Catch IOExcep As IO.IOException
            FileSystem.FileClose(1)
            Exit Sub
        End Try

    End Sub

    Private Sub mnuSave_Click(sender As Object, e As EventArgs) Handles mnuSave.Click
        If txtNewID.Text = "" Or txtNewName.Text = "" Or Len(txtNewID.Text) <> 6 Then
            MsgBox("Invalid value, please check", MsgBoxStyle.Critical, "NGC/Wii Game ID")
            Exit Sub
        ElseIf txtNewID.Text = txtOldID.Text And txtNewName.Text = txtOldName.Text Then
            MsgBox("File not change", MsgBoxStyle.Information, "NGC/Wii Game ID")
            Exit Sub
        End If

        SaveImg()

        txtNewID.Text = ""
        txtNewName.Text = ""
        txtOldID.Text = ""
        txtOldName.Text = ""
        mnuSave.Enabled = False
    End Sub

    Private Sub SaveImg()
        Dim gameID(0 To 5) As Byte
        Dim gameName() As Byte
        Dim fileHead(0 To 3) As Byte
        Dim overwriteLength As Integer
        Dim i As Integer

        Try
            For i = 0 To 5
                gameID(i) = Asc(Mid(txtNewID.Text, i + 1, 1))
            Next

            overwriteLength = IIf(Len(txtOldName.Text) > Len(txtNewName.Text), Len(txtOldName.Text) - 1, Len(txtNewName.Text) - 1)
            ReDim gameName(0 To overwriteLength)
            For i = 0 To Len(txtNewName.Text) - 1
                gameName(i) = Asc(Mid(txtNewName.Text, i + 1, 1))
            Next

            FileSystem.FileOpen(1, fileName, OpenMode.Binary)
            FileSystem.FileGet(1, fileHead)
            ' The offset for wbfs file is different with iso or gcm file
            If fileHead(0) = 87 And fileHead(1) = 66 And fileHead(2) = 70 And fileHead(3) = 83 Then
                ' WBFS file need change values in two positions
                FileSystem.Seek(1, 513)
                FileSystem.FilePut(1, gameID)
                FileSystem.Seek(1, 545)
                FileSystem.FilePut(1, gameName)

                FileSystem.Seek(1, 2097153)
                FileSystem.FilePut(1, gameID)
                FileSystem.Seek(1, 2097185)
                FileSystem.FilePut(1, gameName)
            Else
                FileSystem.Seek(1, 1)
                FileSystem.FilePut(1, gameID)
                FileSystem.Seek(1, 33)
                FileSystem.FilePut(1, gameName)
            End If
            FileSystem.FileClose(1)
            MsgBox("Image saved", MsgBoxStyle.Information, "NGC/Wii Game ID")
        Catch IOExcep As IO.IOException
            FileSystem.FileClose(1)
            Exit Sub
        End Try

    End Sub
End Class
