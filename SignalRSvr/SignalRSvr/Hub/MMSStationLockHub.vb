Imports Microsoft.AspNet.SignalR
Imports Microsoft.AspNet.SignalR.Hubs
Imports System.Threading.Tasks

Public Class MMSStationLockHub
    Inherits Hub

    Private Const STATION_DTL_SEPARATOR As String = "/"
    Private Const STATION_LOCK_SEPARATOR As String = "#"

    Sub New()
        '設定Timer
        AddHandler _Fab1_HfOzoneTimer.Elapsed, AddressOf Fab1_HfOzoneTimer_Elapsed
        AddHandler _Fab2_HfOzoneTimer.Elapsed, AddressOf Fab2_HfOzoneTimer_Elapsed
        AddHandler _Fab5_HfOzoneTimer.Elapsed, AddressOf Fab5_HfOzoneTimer_Elapsed
    End Sub

    Public Overrides Function OnConnected() As Task
        Return MyBase.OnConnected()
    End Function

    Public Sub GetServerVersion()
        Clients.Client(Context.ConnectionId).showServerVersion(CURRENT_VERSION)
    End Sub

#Region "Station Lock"

#Region "Client"

    ''' <summary>
    ''' Client檢查顯示狀態 通知Client更新Lock畫面
    ''' </summary>
    ''' <param name="fab">Fab</param>
    ''' <remarks></remarks>
    Public Sub ShowStationLockForMMS(ByVal fab As String)
        Try
            Clients.Client(Context.ConnectionId).showStationLock(GLOBAL_FAB_STATION_LOCK(fab).isLock, GLOBAL_FAB_STATION_LOCK(fab).multiLockInfo)
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' Client檢查顯示狀態 通知指定Fab的Client更新Lock畫面
    ''' </summary>
    ''' <param name="fabGroup">Fab</param>
    ''' <remarks></remarks>
    Public Sub ShowStationLockForMMSFab(ByVal fabGroup As String)
        Try
            Clients.Group(fabGroup).showStationLock(GLOBAL_FAB_STATION_LOCK(fabGroup).isLock, GLOBAL_FAB_STATION_LOCK(fabGroup).multiLockInfo)
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' Client顯示狀態
    ''' </summary>
    ''' <param name="fabGroup">Fab</param>
    ''' <remarks></remarks>
    Public Sub SetStationLockVisibleForMMSFab(ByVal fabGroup As String, ByVal isVisible As Boolean, ByVal station As String)
        Try
            Clients.Group(fabGroup).showStationLock(isVisible, station)
        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "Server"

    ''' <summary>
    ''' 新增Connection ID到Fab_Station群組
    ''' </summary>
    ''' <param name="fab">Fab</param>
    ''' <remarks></remarks>
    Public Sub AddMMSStationClient(ByVal fab As String)
        Groups.Add(Context.ConnectionId, fab)
    End Sub

    Public Sub StartHfOzoneTimer(ByVal fab As String)
        Try
            If fab = GROUP_F01C Then
                If _Fab1_HfOzoneTimer.Enabled = False Then _Fab1_HfOzoneTimer.Start()
            ElseIf fab = GROUP_F02C Then
                If _Fab2_HfOzoneTimer.Enabled = False Then _Fab2_HfOzoneTimer.Start()
            ElseIf fab = GROUP_F05C Then
                If _Fab5_HfOzoneTimer.Enabled = False Then _Fab5_HfOzoneTimer.Start()
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub StopHfOzoneTimer(ByVal fab As String)
        Try
            If fab = GROUP_F01C Then
                _Fab1_HfOzoneTimer.Stop()
            ElseIf fab = GROUP_F02C Then
                _Fab2_HfOzoneTimer.Stop()
            ElseIf fab = GROUP_F05C Then
                _Fab5_HfOzoneTimer.Stop()
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub StopHfOzoneTimerByFab(ByVal fabGroup As String, ByVal station As String)
        Try
            If (fabGroup = GROUP_F01C AndAlso _Fab1_HfOzoneTimer.Enabled = True) OrElse (fabGroup = GROUP_F02C AndAlso _Fab2_HfOzoneTimer.Enabled = True) _
                OrElse (fabGroup = GROUP_F05C AndAlso _Fab5_HfOzoneTimer.Enabled = True) Then

                If GLOBAL_FAB_STATION_LOCK(fabGroup).isLock = True Then
                    '無lock 關閉Timer
                    StopHfOzoneTimer(fabGroup)
                    GLOBAL_FAB_STATION_LOCK(fabGroup).isLock = False
                    SetStationLockVisibleForMMSFab(fabGroup, False, station)
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

#End Region

    Public Sub UpdateStationLockInfo(ByVal fabGroup As String, ByVal lockInfo As String, ByVal station As String)
        Try
            '檢查廠別群組
            If String.IsNullOrEmpty(fabGroup) Then Exit Sub

            '檢查更新每筆lock資料
            Dim arrLock As String() = lockInfo.Split(STATION_LOCK_SEPARATOR)
            '檢查lock資料
            If arrLock.Length = 0 Then
                '無lock資料 關閉Timer
                StopHfOzoneTimerByFab(fabGroup, station)
                Exit Sub
            End If
            '現在只有HF 直接更新
            GLOBAL_FAB_STATION_LOCK(fabGroup).isLock = True
            GLOBAL_FAB_STATION_LOCK(fabGroup).multiLockInfo = lockInfo
            '關閉MMSStation Lock畫面 等Timer時間到會自動開啟畫面
            SetStationLockVisibleForMMSFab(fabGroup, False, station)
            '重開Timer
            StopHfOzoneTimer(fabGroup)
            StartHfOzoneTimer(fabGroup)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub AddStationLockInfo(ByVal fab As String, ByVal station As String, ByVal mach As String, ByVal lotno As String, ByVal remark As String)
        Try
            Dim infoString As String = GLOBAL_FAB_STATION_LOCK(fab).multiLockInfo
            Dim newLock As String = GetInfoString(fab, station, mach, lotno, remark)

            If String.IsNullOrEmpty(newLock) Then Exit Sub
            '檢查lock存在
            If infoString.IndexOf(newLock) > -1 Then Exit Sub

            '新增lock
            If String.IsNullOrEmpty(GLOBAL_FAB_STATION_LOCK(fab).multiLockInfo) Then
                GLOBAL_FAB_STATION_LOCK(fab).isLock = True
                GLOBAL_FAB_STATION_LOCK(fab).multiLockInfo = newLock
            Else
                '用#隔開
                'FAB/STATION/MACH/LOTNO/REMARK#FAB/STATION/MACH/LOTNO/REMARK
                GLOBAL_FAB_STATION_LOCK(fab).isLock = True
                GLOBAL_FAB_STATION_LOCK(fab).multiLockInfo &= STATION_LOCK_SEPARATOR & newLock
            End If

            '檢查HF Ozone Timer開關
            If fab = GROUP_F01C AndAlso _Fab1_HfOzoneTimer.Enabled = False Then StartHfOzoneTimer(fab)
            If fab = GROUP_F02C AndAlso _Fab2_HfOzoneTimer.Enabled = False Then StartHfOzoneTimer(fab)
            If fab = GROUP_F05C AndAlso _Fab5_HfOzoneTimer.Enabled = False Then StartHfOzoneTimer(fab)
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 回傳lock訊息字串
    ''' </summary>
    ''' <param name="fab">Fab</param>
    ''' <param name="station">Station</param>
    ''' <param name="mach">Machine ID</param>
    ''' <param name="remark">Remark</param>
    ''' <returns>lock訊息 用/隔開:FAB/STATION/MACH/LOTNO/REMARK</returns>
    Private Function GetInfoString(ByVal fab As String, ByVal station As String, ByVal mach As String, ByVal lotno As String, ByVal remark As String) As String
        Try
            Return String.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", STATION_DTL_SEPARATOR, fab, station, mach, lotno, remark)
        Catch ex As Exception

            Return String.Empty
        End Try
    End Function

#End Region

    Private Sub TimerEvent(ByVal fab As String)
        Try
            '檢查廠別群組
            If String.IsNullOrEmpty(fab) Then Exit Sub

            If String.IsNullOrEmpty(GLOBAL_FAB_STATION_LOCK(fab).multiLockInfo) Then
                StopHfOzoneTimer(fab)
            End If
            '通知Client更新Lock畫面
            ShowStationLockForMMSFab(fab)
        Catch ex As Exception

        End Try
    End Sub

    ''' <summary>
    ''' HF Ozone 每隔一段時間提醒S753量測
    ''' </summary>
    Private Sub Fab1_HfOzoneTimer_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs)
        TimerEvent(GROUP_F01C)
    End Sub
    ''' <summary>
    ''' HF Ozone 每隔一段時間提醒S753量測
    ''' </summary>
    Private Sub Fab2_HfOzoneTimer_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs)
        TimerEvent(GROUP_F02C)
    End Sub
    ''' <summary>
    ''' HF Ozone 每隔一段時間提醒S753量測
    ''' </summary>
    Private Sub Fab5_HfOzoneTimer_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs)
        TimerEvent(GROUP_F05C)
    End Sub

End Class
