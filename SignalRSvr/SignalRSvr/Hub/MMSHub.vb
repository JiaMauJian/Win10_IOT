Imports Microsoft.AspNet.SignalR
Imports Microsoft.AspNet.SignalR.Hubs
Imports System.Threading.Tasks

Public Class MMSHub
    Inherits Hub

    Private LockMsg As String = DEFAULT_RMS_LOCK_MSG

    Sub New()

    End Sub

    Public Overrides Function OnConnected() As Task
        Return MyBase.OnConnected()
    End Function

    'Public Overrides Function OnDisconnected() As Task
    '    RemovePCConnection(Context.ConnectionId)
    '    Return MyBase.OnDisconnected()
    'End Function

    Public Sub GetServerVersion()
        Clients.Client(Context.ConnectionId).showServerVersion(CURRENT_VERSION)
    End Sub

#Region "Lock"
    ''' <summary>
    ''' 從EPC更新Lock並通知MMS群組顯示狀態
    ''' </summary>
    ''' <param name="IsLock">lock狀態</param>
    ''' <param name="msg">lock訊息 每筆用#隔開:FAB;STATION;MACHINEID;LOTNO</param>
    Public Sub SetServerLockStatusFromEPC(ByVal IsLock As Boolean, ByVal msg As String)

        Dim fab As String = String.Empty
        Dim machine As String = String.Empty
        Dim lotno As String = String.Empty

        Try
            '單筆----------------------------------------------------------------------------------
            'Dim data() As String = msg.Split(";")
            'If data.Length > 0 Then
            '    'Dim lockMsg As String = "目前系統鎖定中：RMS比對Recipe異常"
            '    If data.Length >= 3 Then
            '        fab = data(0)
            '        machine = data(1)
            '        lotno = data(2)
            '    End If
            '    UpdateLockInfoByFab(IsLock, fab, machine, lotno)
            '    Clients.Group(fab).showLockStatus(IsLock, machine, lotno, GLOBAL_FAB_LOCK(fab).lockMsg)
            'End If
            '單筆----------------------------------------------------------------------------------
            '多筆----------------------------------------------------------------------------------
            Dim data() As String = msg.Split("#")(0).Split(";")
            If data.Length > 0 Then
                'Dim lockMsg As String = "目前系統鎖定中：RMS比對Recipe異常"
                If data.Length >= 3 Then : fab = data(0) : machine = data(1) : lotno = data(2) : End If
                UpdateLockInfoByFab(IsLock, fab, machine, lotno, msg)
                Clients.Group(fab).showLockStatus(IsLock, machine, lotno, msg)  '從EPC來的多筆字串直接傳給Client
            End If
            '多筆----------------------------------------------------------------------------------
        Catch ex As Exception
            'WriteLogFile(ex.Message)
        End Try
    End Sub
    ''' <summary>
    ''' Client檢查顯示狀態 通知Client更新Lock畫面
    ''' </summary>
    ''' <param name="fab">Fab</param>
    ''' <remarks></remarks>
    Public Sub GetLockStatusForMMS(ByVal fab As String)
        Clients.Client(Context.ConnectionId).showLockStatus(GLOBAL_FAB_RMS_LOCK(fab).isLock, GLOBAL_FAB_RMS_LOCK(fab).lockMachine, GLOBAL_FAB_RMS_LOCK(fab).lockLotno, GLOBAL_FAB_RMS_LOCK(fab).multiLockInfo)
    End Sub
    ''' <summary>
    ''' 通知各廠Client更新Lock畫面
    ''' </summary>
    ''' <param name="fab">Fab</param>
    ''' <remarks></remarks>
    Public Sub GetLockStatusByFab(ByVal fab As String)
        Clients.Client(Context.ConnectionId).showLockStatusByFab(GLOBAL_FAB_RMS_LOCK(fab).isLock, fab, GLOBAL_FAB_RMS_LOCK(fab).lockMachine, GLOBAL_FAB_RMS_LOCK(fab).lockLotno, GLOBAL_FAB_RMS_LOCK(fab).multiLockInfo)
    End Sub
    ''' <summary>
    ''' 設定Lock狀態並通知顯示狀態
    ''' </summary>
    ''' <param name="IsLock">lock狀態</param>
    ''' <param name="fab">Fab</param>
    Public Sub SetLockStatus(ByVal IsLock As Boolean, ByVal fab As String, ByVal machine As String, ByVal lotno As String, ByVal msg As String)
        Try
            GLOBAL_FAB_RMS_LOCK(fab).isLock = IsLock
            UpdateLockInfoByFab(IsLock, fab, machine, lotno, msg)
            '通知群組client顯示lock狀態
            'Clients.Group(fab).showLockStatus(GLOBAL_FAB_LOCK(fab).isLock, String.Empty, String.Empty, GLOBAL_FAB_LOCK(fab).lockMsg)
            Clients.Group(fab).showLockStatus(GLOBAL_FAB_RMS_LOCK(fab).isLock, String.Empty, String.Empty, msg)
            Clients.Group(GROUP_MANAGER).showLockStatusByFab(GLOBAL_FAB_RMS_LOCK(fab).isLock, fab, String.Empty, GLOBAL_FAB_RMS_LOCK(fab).lockMsg, GLOBAL_FAB_RMS_LOCK(fab).multiLockInfo)
        Catch ex As Exception
            'WriteLogFile(ex.Message)
        End Try
    End Sub
    ''' <summary>
    ''' 更新Lock訊息
    ''' </summary>
    ''' <param name="IsLock">lock狀態</param>
    ''' <param name="fab">Fab</param>
    ''' <param name="msg">lock訊息 每筆用#隔開:FAB;STATION;MACHINEID;LOTNO</param>
    Private Sub UpdateLockMsg(ByVal IsLock As Boolean, ByVal fab As String, ByVal msg As String)
        If IsLock = True Then
            If msg.Length > 0 Then
                GLOBAL_FAB_RMS_LOCK(fab).lockMsg = msg
                GLOBAL_FAB_RMS_LOCK(fab).multiLockInfo = msg
            Else
                GLOBAL_FAB_RMS_LOCK(fab).lockMsg = DEFAULT_RMS_LOCK_MSG
            End If
        Else
            GLOBAL_FAB_RMS_LOCK(fab).lockMsg = DEFAULT_RMS_LOCK_MSG
        End If
    End Sub

    Private Sub UpdateLockInfoByFab(ByVal isLock As Boolean, ByVal fab As String, ByVal machine As String, ByVal lotno As String, ByVal lockMsg As String)
        Try
            'Dim lockMsg As String = "目前系統鎖定中：RMS比對Recipe異常"
            If String.IsNullOrEmpty(lockMsg) Then lockMsg = DEFAULT_RMS_LOCK_MSG
            'If machine.Length >= 0 AndAlso lotno.Length > 0 Then
            '    lockMsg = String.Format("目前系統鎖定中：RMS比對Recipe異常" & vbCrLf & "Fab:{0}" & vbCrLf & "Location: {1}" & vbCrLf & "Lot No.: {2}", fab, machine, lotno)
            'End If
            '更新廠別lock資訊
            UpdateLockMsg(isLock, fab, lockMsg)
            GLOBAL_FAB_RMS_LOCK(fab).isLock = isLock
            GLOBAL_FAB_RMS_LOCK(fab).lockMachine = machine
            GLOBAL_FAB_RMS_LOCK(fab).lockLotno = lotno
            GLOBAL_FAB_RMS_LOCK(fab).lockMsg = lockMsg
            GLOBAL_FAB_RMS_LOCK(fab).multiLockInfo = lockMsg
        Catch ex As Exception
            'WriteLogFile(ex.Message)
        End Try
    End Sub
#End Region

#Region "Broadcast"
    ''' <summary>
    ''' 依群組廣播訊息
    ''' </summary>
    ''' <param name="msg">廣播訊息</param>
    ''' <param name="group">廣播群組</param>
    Public Sub BroadcastMsgByGroup(ByVal msg As String, ByVal group As String)
        Try
            If IsGroupExist(group) = True Then
                If group = GROUP_MMS Then
                    '所有MMS廠別
                    For Each mmsGroup As String In GLOBAL_MMS_GROUP
                        Clients.Group(mmsGroup).showBroadcastMsg(msg)
                    Next
                Else
                    Clients.Group(group).showBroadcastMsg(msg)
                End If
            End If
        Catch ex As Exception
            'WriteLogFile(ex.Message)
        End Try
    End Sub

#End Region

#Region "Group"
    ''' <summary>
    ''' 新增Connection ID到Fab群組
    ''' </summary>
    ''' <param name="group">Fab</param>
    ''' <remarks></remarks>
    Public Sub AddGroup(ByVal group As String)
        Groups.Add(Context.ConnectionId, group)
    End Sub
    ''' <summary>
    ''' 新增MMS Client電腦連線資料
    ''' </summary>
    ''' <param name="pcname">電腦名稱</param>
    ''' <param name="fab">Fab</param>
    ''' <remarks></remarks>
    Public Sub AddMMSClient(ByVal pcname As String, ByVal fab As String)
        Try
            '新增電腦連線資料
            AddNewPCConnection(Context.ConnectionId, pcname, fab)
            '通知Client更新Lock畫面
            GetLockStatusForMMS(fab)
            '通知Client加入群組完成
            Clients.Client(Context.ConnectionId).addGroupComplete()
        Catch ex As Exception
            'WriteLogFile(ex.Message)
        End Try
    End Sub
    ''' <summary>
    ''' 新增RMS電腦連線資料
    ''' </summary>
    ''' <param name="pcname">電腦名稱</param>
    ''' <remarks></remarks>
    Public Sub AddRMSClient(ByVal pcname As String)
        Try
            '新增到RMS群組
            Groups.Add(Context.ConnectionId, GROUP_RMS)
            '新增電腦連線資料
            AddNewPCConnection(Context.ConnectionId, pcname, GROUP_RMS)
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 新增Manager電腦連線資料
    ''' </summary>
    ''' <param name="pcname">電腦名稱</param>
    ''' <remarks></remarks>
    Public Sub AddMANAGERClient(ByVal pcname As String)
        Try
            '新增到Manager群組
            Groups.Add(Context.ConnectionId, GROUP_MANAGER)
            '新增電腦連線資料
            AddNewPCConnection(Context.ConnectionId, pcname, GROUP_MANAGER)
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 從指定群組移除目前Connection ID
    ''' </summary>
    ''' <param name="group">群組:MMS所有廠,MMS各廠,RMS,Manager</param>
    ''' <remarks></remarks>
    Public Sub RemoveFromGroup(ByVal group As String)
        Groups.Remove(Context.ConnectionId, group)
    End Sub
    ''' <summary>
    ''' 從MMS所有廠群組移除目前Connection ID
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RemoveGroupMMS()
        Try
            Groups.Remove(Context.ConnectionId, GROUP_MMS)
            '移除Connection ID的連線資料
            RemovePCConnection(Context.ConnectionId)
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 從RMS群組移除目前Connection ID
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RemoveGroupRMS()
        Try
            Groups.Remove(Context.ConnectionId, GROUP_RMS)
            '移除Connection ID的連線資料
            RemovePCConnection(Context.ConnectionId)
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 從Manager群組移除目前Connection ID
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RemoveGroupMANAGER()
        Try
            Groups.Remove(Context.ConnectionId, GROUP_MANAGER)
            '移除Connection ID的連線資料
            RemovePCConnection(Context.ConnectionId)
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 檢查群組
    ''' </summary>
    ''' <param name="group">群組:MMS所有廠,MMS各廠,RMS,Manager</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function IsGroupExist(ByVal group As String) As Boolean
        If group.Length > 0 AndAlso GLOBAL_GROUP_LIST.Contains(group) = True Then
            Return True
        Else
            Return False
        End If
    End Function
#End Region

#Region "Connection List"
    ''' <summary>
    ''' 連線列表 通知Manager更新連線資料
    ''' </summary>
    Public Sub RefreshManagerConnectionList()
        Dim dt As DataTable = GLOBAL_DATATABLE_MMS_PC_CONNECTION.Copy()
        dt.Columns.RemoveAt(0)
        Clients.Group(GROUP_MANAGER).getConnectionList(dt)
    End Sub
    ''' <summary>
    ''' 移除Connection ID的連線資料
    ''' </summary>
    Public Sub RemoveFromConnectionList()
        Try
            If GLOBAL_DATATABLE_MMS_PC_CONNECTION IsNot Nothing AndAlso GLOBAL_DATATABLE_MMS_PC_CONNECTION.Rows.Count > 0 Then
                Dim dr() As DataRow = GLOBAL_DATATABLE_MMS_PC_CONNECTION.Select(String.Format("{0}='{1}'", LIST_COLUMN_CONNECTIONID, Context.ConnectionId))
                If dr.Length > 0 Then
                    GLOBAL_DATATABLE_MMS_PC_CONNECTION.Rows.Remove(dr(0))
                    '通知Manager更新連線資料
                    RefreshManagerConnectionList()
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 初始化連線資料的DataTable
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitListDataTable()
        Try
            GLOBAL_DATATABLE_MMS_PC_CONNECTION = New DataTable

            With GLOBAL_DATATABLE_MMS_PC_CONNECTION
                .Columns.Add(LIST_COLUMN_CONNECTIONID, GetType(String))
                .Columns.Add(LIST_COLUMN_PC_NAME, GetType(String))
                .Columns.Add(LIST_COLUMN_GROUP, GetType(String))
                .Columns.Add(LIST_COLUMN_CONNECTION_STATUS, GetType(String))
            End With
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 新增電腦連線資料
    ''' </summary>
    ''' <param name="ConnectionId">Connection ID</param>
    ''' <param name="pcname">電腦名稱</param>
    ''' <param name="group">Fab</param>
    ''' <remarks></remarks>
    Private Sub AddNewPCConnection(ByVal ConnectionId As String, ByVal pcname As String, ByVal group As String)
        Try
            '檢查連線資料的DataTable
            If GLOBAL_DATATABLE_MMS_PC_CONNECTION Is Nothing Then InitListDataTable()

            Dim GroupSelect As String = String.Empty
            '檢查群組
            If IsGroupExist(group) = True Then
                GroupSelect = String.Format(" AND GROUP='{0}'", group)
            Else
                group = String.Empty
            End If

            Dim dr() As DataRow = GLOBAL_DATATABLE_MMS_PC_CONNECTION.Select(String.Format("{0}='{1}' {2}", LIST_COLUMN_PC_NAME, pcname, GroupSelect))
            If dr.Length = 0 Then
                '新增連線資料
                Dim row As DataRow = GLOBAL_DATATABLE_MMS_PC_CONNECTION.NewRow

                row(LIST_COLUMN_CONNECTIONID) = ConnectionId
                row(LIST_COLUMN_PC_NAME) = pcname
                row(LIST_COLUMN_GROUP) = group
                row(LIST_COLUMN_CONNECTION_STATUS) = LIST_STATUS_CONNECT
                GLOBAL_DATATABLE_MMS_PC_CONNECTION.Rows.Add(row)
            Else
                '更新連線資料
                dr(0)(LIST_COLUMN_CONNECTIONID) = ConnectionId
                dr(0)(LIST_COLUMN_GROUP) = group
                dr(0)(LIST_COLUMN_CONNECTION_STATUS) = LIST_STATUS_CONNECT
            End If

            If group.Length > 0 Then
                '新增Connection ID到Fab群組
                AddGroup(group)
            End If
            '通知Manager更新連線資料
            RefreshManagerConnectionList()
        Catch ex As Exception

        End Try
    End Sub
    ''' <summary>
    ''' 移除Connection ID的連線資料
    ''' </summary>
    ''' <param name="ConnectionId">Connection ID</param>
    ''' <remarks></remarks>
    Private Sub RemovePCConnection(ByVal ConnectionId As String)
        Try
            '檢查連線資料的DataTable
            If GLOBAL_DATATABLE_MMS_PC_CONNECTION Is Nothing Then
                InitListDataTable()
                Exit Sub
            End If
            '檢查Connection ID的連線資料
            Dim dr() As DataRow = GLOBAL_DATATABLE_MMS_PC_CONNECTION.Select(String.Format("{0}='{1}'", LIST_COLUMN_CONNECTIONID, ConnectionId))
            If dr.Length > 0 Then
                For Each row As DataRow In dr
                    If row IsNot Nothing Then
                        '將連線狀態改為斷線
                        row.Item(LIST_COLUMN_CONNECTION_STATUS) = LIST_STATUS_DISCONNECT
                        Dim group As String = row.Item(LIST_COLUMN_GROUP)
                        '檢查群組
                        If IsGroupExist(group) = True Then
                            '從指定群組移除目前Connection ID
                            RemoveFromGroup(group)
                        End If
                    End If
                Next
            End If
            '通知Manager更新連線資料
            RefreshManagerConnectionList()
        Catch ex As Exception

        End Try
    End Sub
#End Region

End Class
