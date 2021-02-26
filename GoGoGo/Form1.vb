﻿Imports System
Imports System.IO.Ports

Public Class Form1
    Dim vpb_sy, vpb_ly As Integer
    Dim TempL, HumL As Integer
    Dim Temp, Hum, TempResult, HumResult As String
    Dim TempToProgressBar As Single
    Dim ChartLimit As Integer = 30
    Dim StrSerialIn, StrSerialInRam As String
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.CenterToScreen()
        PanelConnection.Focus()
        CircularProgressBarHumidity.Value = 0
        ComboBoxBaudRate.SelectedIndex = 0

        For i = 0 To 30 Step 1
            Chart1.Series("Humidity       ").Points.AddY(0)
            If Chart1.Series(0).Points.Count = ChartLimit Then
                Chart1.Series(0).Points.RemoveAt(0)
            End If

            Chart2.Series("Temperature").Points.AddY(0)
            If Chart2.Series(0).Points.Count = ChartLimit Then
                Chart2.Series(0).Points.RemoveAt(0)
            End If
        Next

        Chart1.ChartAreas(0).AxisY.Maximum = 180
        Chart1.ChartAreas(0).AxisY.Minimum = -20
        Chart1.ChartAreas("ChartArea1").AxisX.LabelStyle.Enabled = False
        Chart2.ChartAreas(0).AxisY.Maximum = 70
        Chart2.ChartAreas(0).AxisY.Minimum = -30
        Chart2.ChartAreas("ChartArea1").AxisX.LabelStyle.Enabled = False

        PictureBoxPBTemp.Height = 0
    End Sub

    Private Sub ComboBoxPort_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxPort.SelectedIndexChanged
        PanelConnection.Focus()
    End Sub

    Private Sub ComboBoxPort_DropDown(sender As Object, e As EventArgs) Handles ComboBoxPort.DropDown
        PanelConnection.Focus()
    End Sub

    Private Sub ComboBoxPort_Click(sender As Object, e As EventArgs) Handles ComboBoxPort.Click
        If LabelStatus.Text = "Status : Connected" Then
            MsgBox("Conncetion in progress, please Disconnect to change COM.", MsgBoxStyle.Critical, "Warning !!!")
            Return
        End If
    End Sub

    Private Sub ComboBoxBaudRate_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxBaudRate.SelectedIndexChanged
        PanelConnection.Focus()
    End Sub

    Private Sub ComboBoxBaudRate_DropDown(sender As Object, e As EventArgs) Handles ComboBoxBaudRate.DropDown
        PanelConnection.Focus()
    End Sub

    Private Sub ComboBoxBaudRate_Click(sender As Object, e As EventArgs) Handles ComboBoxBaudRate.Click
        If LabelStatus.Text = "Status : Connected" Then
            MsgBox("Conncetion in progress, please Disconnect to change Baud Rate.", MsgBoxStyle.Critical, "Warning !!!")
            Return
        End If
    End Sub

    Private Sub ButtonScanPort_Click(sender As Object, e As EventArgs) Handles ButtonScanPort.Click
        PanelConnection.Focus()
        If LabelStatus.Text = "Status : Connected" Then
            MsgBox("Conncetion in progress, please Disconnect to scan the new port.", MsgBoxStyle.Critical, "Warning !!!")
            Return
        End If
        ComboBoxPort.Items.Clear()
        Dim myPort As Array
        Dim i As Integer
        myPort = IO.Ports.SerialPort.GetPortNames()
        ComboBoxPort.Items.AddRange(myPort)
        i = ComboBoxPort.Items.Count
        i = i - i
        Try
            ComboBoxPort.SelectedIndex = i
            ButtonConnect.Enabled = True
        Catch ex As Exception
            MsgBox("Com port not detected", MsgBoxStyle.Critical, "Warning !!!")
            ComboBoxPort.Text = ""
            ComboBoxPort.Items.Clear()
            Return
        End Try
        ComboBoxPort.DroppedDown = True
    End Sub

    Private Sub ButtonConnect_Click(sender As Object, e As EventArgs) Handles ButtonConnect.Click
        PanelConnection.Focus()
        Try
            SerialPort1.BaudRate = ComboBoxBaudRate.SelectedItem
            SerialPort1.PortName = ComboBoxPort.SelectedItem
            SerialPort1.Open()
            TimerSerial.Start()

            LabelStatus.Text = "Status : Connected"
            ButtonConnect.SendToBack()
            ButtonDisconnect.BringToFront()
            PictureBoxStatusConnection.BackColor = Color.Green
        Catch ex As Exception
            MsgBox("Please check the Hardware, COM, Baud Rate and try again.", MsgBoxStyle.Critical, "Connection failed !!!")
        End Try
    End Sub

    Private Sub ButtonDisconnect_Click(sender As Object, e As EventArgs) Handles ButtonDisconnect.Click
        PanelConnection.Focus()
        TimerSerial.Stop()
        SerialPort1.Close()
        ButtonDisconnect.SendToBack()
        ButtonConnect.BringToFront()
        LabelStatus.Text = "Status : Disconnect"
        PictureBoxStatusConnection.Visible = True
        PictureBoxStatusConnection.BackColor = Color.Red
    End Sub

    'The function to convert temperature values to PictureBoxPBTemp size so that it looks like a progress bar.
    'Reference from: http://kursuselektronaku.blogspot.com/2016/04/cara-mengubah-adc-mikrokontroller-10.html
    Function MapVPB(ByVal X As Single, ByVal In_min As Single, ByVal In_max As Single, ByVal Out_min As Single, ByVal Out_max As Single) As Integer
        Dim A As Single
        Dim B As Single
        A = X - In_min
        B = Out_max - Out_min
        A = A * B
        B = In_max - In_min
        A = A / B
        MapVPB = A + Out_min
    End Function

    Private Sub TimerSerial_Tick(sender As Object, e As EventArgs) Handles TimerSerial.Tick
        Try
            StrSerialIn = SerialPort1.ReadExisting  '--> Read incoming serial data

            Dim TB As New TextBox
            TB.Multiline = True
            TB.Text = StrSerialIn   '--> Enter serial data into the textbox

            If TB.Lines.Count > 0 Then
                If TB.Lines(0) = "Failed to read from DHT sensor!" Then '--> Check Arduino if it fails to read the DHT sensor, if this happens the connection is disconnected
                    TimerSerial.Stop()
                    SerialPort1.Close()
                    LabelStatus.Text = "Status : Disconnect"
                    ButtonDisconnect.SendToBack()
                    ButtonConnect.BringToFront()
                    PictureBoxStatusConnection.Visible = True
                    PictureBoxStatusConnection.BackColor = Color.Red
                    MsgBox("Failed to read from DHT sensor !!!, Please check the Hardware and Please connect again.", MsgBoxStyle.Critical, "Connection failed !!!")
                    Return
                End If

                ' _______________________________________________________________________________
                '| Dim TB As New TextBox                                                         |
                '| TB.Multiline = True                                                           |
                '| TB.Text = StrSerialIn --> Enter serial data into the textbox / TB ┐           |
                '|  _________________________________________                        |           |
                '| |H60                                      | <---------------------┘           |
                '| |T33.40                                   |                                   |
                '|  ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾                                    |
                '| StrSerialInRam = TB.Lines(0).Substring(0, 1) ->  H60  -> StrSerialInRam = H   |
                '|                                           |      ↓                            |
                '|                                           └----> 1                            |
                '| If StrSerialInRam = "H" Then                                                  |
                '|     Hum = TB.Lines(0) -> Hum=H60                                              |
                '|     HumL = Hum.Length -> H60 -> HumL = 3                                      |
                '|                          ↓↓↓                                                  |
                '|                          123                                                  |
                '| Else                                                                          |
                '|     Hum = Hum                                                                 |
                '| End If                                                                        |
                '|                                                                               |
                '| HumResult = Mid(Hum, 2, HumL) -> Hum=H60 -> HumResult=60 -> Humidity value    |
                '|                      |    |           ↓|                                      |
                '|                      └----|---------> 2↓                                      |
                '|                           └----------> 3                                      |
                ' ‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾

                StrSerialInRam = TB.Lines(0).Substring(0, 1)
                If StrSerialInRam = "H" Then
                    Hum = TB.Lines(0)
                    HumL = Hum.Length
                Else
                    Hum = Hum
                End If

                StrSerialInRam = TB.Lines(1).Substring(0, 1)
                If StrSerialInRam = "T" Then
                    Temp = TB.Lines(1)
                    TempL = Temp.Length
                Else
                    Temp = Temp
                End If

                HumResult = Mid(Hum, 2, HumL)
                TempResult = Mid(Temp, 2, TempL)
                TempToProgressBar = TempResult

                CircularProgressBarHumidity.Value = HumResult
                CircularProgressBarHumidity.Text = CircularProgressBarHumidity.Value & " %"


                LabelTemperature.Text = TempResult & " °C"

                '-----------The process for making a progress bar using a picturebox (Vertical progress bar)-----------
                vpb_sy = MapVPB(TempToProgressBar, -20.0, 60.0, 0, 120)
                If vpb_sy > 120 Then
                    vpb_sy = 120
                End If
                If vpb_sy < 0 Then
                    vpb_sy = 0
                End If
                PictureBoxPBTemp.Height = PictureBoxPBTempBack.Height * vpb_sy / 120
                vpb_ly = (PictureBoxPBTempBack.Height - vpb_sy) + PictureBoxPBTempBack.Location.Y
                PictureBoxPBTemp.Location = New Point(PictureBoxPBTemp.Location.X, vpb_ly)
                '------------------------------------------------------------------------------------------------------

                '-----------Enter the temperature and humidity values into the chart-----------------------------------
                Chart1.Series("Humidity       ").Points.AddY(HumResult)
                If Chart1.Series(0).Points.Count = ChartLimit Then
                    Chart1.Series(0).Points.RemoveAt(0)
                End If

                Chart2.Series("Temperature").Points.AddY(TempResult)
                If Chart2.Series(0).Points.Count = ChartLimit Then
                    Chart2.Series(0).Points.RemoveAt(0)
                End If
                '------------------------------------------------------------------------------------------------------

                '-----------If the Then connection Is successful And running, PictureBoxStatusConnection will blink----
                If PictureBoxStatusConnection.Visible = True Then
                    PictureBoxStatusConnection.Visible = False
                ElseIf PictureBoxStatusConnection.Visible = False Then
                    PictureBoxStatusConnection.Visible = True
                End If
                '------------------------------------------------------------------------------------------------------
            End If
        Catch ex As Exception
            TimerSerial.Stop()
            SerialPort1.Close()
            LabelStatus.Text = "Status : Disconnect"
            ButtonDisconnect.SendToBack()
            ButtonConnect.BringToFront()
            PictureBoxStatusConnection.BackColor = Color.Red
            MsgBox("Please check the Hardware and Please connect again." & ex.Message, MsgBoxStyle.Critical, "Connection failed !!!")
            Return
        End Try
    End Sub

End Class
