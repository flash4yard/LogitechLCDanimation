﻿Imports System.Runtime.InteropServices
Imports System.Threading

Public Class Form1

    '-------------------------
    ' Load function from the kernel32 library to set the path to the dll used
    ' because the dll from the Logitech driver can be installed in different
    ' locations.
    '-------------------------
    Public Declare Function SetDllDirectoryA Lib "kernel32" (ByVal lpPathName As String) As Long
    '-------------------------
    ' Import the functions needed in the projectrom the logitech driver dll
    '-------------------------
    <DllImport("LogitechLcd.dll", CharSet:=CharSet.Unicode)>
    Public Shared Function LogiLcdInit(ByVal friendlyName As String, ByVal lcdType As Integer) As Boolean
    End Function

    <DllImport("LogitechLcd.dll", CharSet:=CharSet.Unicode)>
    Public Shared Function LogiLcdMonoSetBackground(ByVal bmp() As Byte) As Boolean
    End Function

    <DllImport("LogitechLcd.dll")>
    Public Shared Sub LogiLcdUpdate()
    End Sub

    <DllImport("LogitechLcd.dll")>
    Public Shared Sub LogiLcdShutdown()
    End Sub

    Private trd As Thread

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        '-------------
        ' Start the main program in a thread
        '-------------
        trd = New Thread(AddressOf Logi)
        trd.Start()

    End Sub

    Private Sub Form1_Close(sender As Object, e As EventArgs) Handles MyBase.Closed
        '-------------
        ' If the form gets closed the Thread and the connection to the keyboard gets disconnected
        '-------------
        trd.Abort()
        LogiLcdShutdown()

    End Sub

    Private Sub Logi()

        '-------------------------
        ' Define all variables
        '-------------------------
        Dim path As String
        Dim animationName As String
        Dim animation(11) As Bitmap

        '-------------------------
        ' Assign values to vars
        '-------------------------
        path = My.Computer.FileSystem.ReadAllText("config\logitechpath.txt")
        ' Default Path =  C:\Program Files\Logitech Gaming Software\SDK\LCD\x86
        animationName = My.Computer.FileSystem.ReadAllText("config\logitechaction.txt")

        ' Load 12 frames from the specified folder into the Bitmap array
        For i As Integer = 0 To 11
            animation(i) = New Bitmap("" & animationName & "\" & (i + 1) & ".bmp")
        Next

        '-------------------------
        ' Use the function from the kernel32 library
        ' It adds the path to the dll to a list. So when Windows tries to find the dll
        ' it first look at the given path
        '-------------------------
        SetDllDirectoryA(path)

        '-------------
        ' Initialise the LCD with name and display type
        '-------------
        LogiLcdInit("Blinkit", 1)


        '-------------
        ' "print" them after another wait between each frame
        '-------------
        For i As Integer = 0 To 11
            LogiLcdMonoSetBackground(CreateBMParray(animation(i)))
            LogiLcdUpdate()

            System.Threading.Thread.Sleep(300)
        Next

        '-------------
        ' Close everything after animation is done
        '-------------
        LogiLcdShutdown()
        Application.Exit()


    End Sub
    '-------------
    ' Function to convert BMP to Logitech LCD format
    '-------------
    Private Function CreateBMParray(ByVal bmp As Bitmap) As Byte()
        Dim bmpA(6879) As Byte
        Dim counter As Integer = 0
        Dim colorC As Color
        Dim color As Integer

        For y As Integer = 0 To 42

            For x As Integer = 0 To 159
                colorC = bmp.GetPixel(x, y)

                color = colorC.R * 256 * 256 + colorC.G * 256 + colorC.B

                If color >= 12624546 Then
                    bmpA(counter) = 255
                Else
                    bmpA(counter) = 0
                End If
                counter = counter + 1
            Next

        Next
        Return bmpA
    End Function
End Class