Module PublicConf
    Public Const CURRENT_VERSION As String = "20160811A"

    Public Const SYS_DEPT As String = "GRP999"
    Public Const LOG_PATH As String = ""

    Public Const HF_OZONE_TIMER_SEC As Double = 1200000.0 '20分鐘

    '連線資料的DataTable欄位名稱
    Public Const LIST_COLUMN_CONNECTIONID As String = "CONNECTIONID"
    Public Const LIST_COLUMN_PC_NAME As String = "PC_NAME"
    Public Const LIST_COLUMN_GROUP As String = "GROUP"
    Public Const LIST_COLUMN_CONNECTION_STATUS As String = "CONNECTION"
    '連線狀態
    Public Const LIST_STATUS_CONNECT As String = "CONNECT"
    Public Const LIST_STATUS_DISCONNECT As String = "DISCONNECT"

#Region "Group"

    'Default Group
    Public Const GROUP_MMS As String = "MMS"
    Public Const GROUP_RMS As String = "RMS"
    Public Const GROUP_MANAGER As String = "MANAGER"
    Public Const GROUP_F01C As String = "F01C"
    Public Const GROUP_F02C As String = "F02C"
    Public Const GROUP_F03C As String = "F03C"
    Public Const GROUP_F05C As String = "F05C"
    Public Const GROUP_F09C As String = "F09C"
    Public Const GROUP_F0AC As String = "F0AC"

#End Region

#Region "MMS RMS Recipe Lock"

    Public Const DEFAULT_RMS_LOCK_MSG As String = "目前系統鎖定中：RMS比對Recipe異常"

    Public GLOBAL_DATATABLE_MMS_PC_CONNECTION As DataTable

    Public GLOBAL_GROUP_LIST As List(Of String) = New List(Of String) From {GROUP_MMS, GROUP_RMS, GROUP_MANAGER, _
                                                                            GROUP_F01C, GROUP_F02C, GROUP_F03C, GROUP_F05C, GROUP_F09C, GROUP_F0AC}
    Public GLOBAL_MMS_GROUP As List(Of String) = New List(Of String) From {GROUP_F01C, GROUP_F02C, GROUP_F03C, GROUP_F05C, GROUP_F09C, GROUP_F0AC}

    '各廠Lock資訊
    Public GLOBAL_FAB_RMS_LOCK As Dictionary(Of String, PC_LOCK) = New Dictionary(Of String, PC_LOCK) _
                                                                   From {{GROUP_F01C, New PC_LOCK(False, String.Empty, String.Empty, String.Empty)}, _
                                                                         {GROUP_F02C, New PC_LOCK(False, String.Empty, String.Empty, String.Empty)}, _
                                                                         {GROUP_F03C, New PC_LOCK(False, String.Empty, String.Empty, String.Empty)}, _
                                                                         {GROUP_F05C, New PC_LOCK(False, String.Empty, String.Empty, String.Empty)}, _
                                                                         {GROUP_F09C, New PC_LOCK(False, String.Empty, String.Empty, String.Empty)}, _
                                                                         {GROUP_F0AC, New PC_LOCK(False, String.Empty, String.Empty, String.Empty)}}
#End Region

#Region "Station Lock"

    '各廠Lock資訊
    Public GLOBAL_FAB_STATION_LOCK As Dictionary(Of String, STATION_LOCK) = New Dictionary(Of String, STATION_LOCK) _
                                                                            From {{GROUP_F01C, New STATION_LOCK(False, String.Empty)}, _
                                                                                  {GROUP_F02C, New STATION_LOCK(False, String.Empty)}, _
                                                                                  {GROUP_F03C, New STATION_LOCK(False, String.Empty)}, _
                                                                                  {GROUP_F05C, New STATION_LOCK(False, String.Empty)}, _
                                                                                  {GROUP_F09C, New STATION_LOCK(False, String.Empty)}, _
                                                                                  {GROUP_F0AC, New STATION_LOCK(False, String.Empty)}}
    'Timer
    Public _Fab1_HfOzoneTimer As System.Timers.Timer = New System.Timers.Timer(HF_OZONE_TIMER_SEC)
    Public _Fab2_HfOzoneTimer As System.Timers.Timer = New System.Timers.Timer(HF_OZONE_TIMER_SEC)
    Public _Fab5_HfOzoneTimer As System.Timers.Timer = New System.Timers.Timer(HF_OZONE_TIMER_SEC)

#End Region

End Module

#Region "PC_LOCK"

Public Class PC_LOCK
    Public Property isLock As Boolean
    Public Property lockMachine As String
    Public Property lockLotno As String
    Public Property lockMsg As String
    Public Property multiLockInfo As String

    Sub New(ByVal isLock As Boolean, ByVal lockMachine As String, ByVal lockLotno As String, ByVal lockMsg As String)
        Me.isLock = isLock
        Me.lockMachine = lockMachine
        Me.lockLotno = lockLotno
        Me.lockMsg = lockMsg
        Me.multiLockInfo = lockMsg
    End Sub

    Public Sub reset()
        Me.isLock = False
        Me.lockMachine = String.Empty
        Me.lockLotno = String.Empty
        Me.lockMsg = String.Empty
        Me.multiLockInfo = String.Empty
    End Sub
End Class

#End Region

#Region "STATION_LOCK"

Public Class STATION_LOCK

    Public Property isLock As Boolean
    Public Property multiLockInfo As String

    Sub New(ByVal isLock As Boolean, ByVal lockMsg As String)
        Me.isLock = isLock
        Me.multiLockInfo = lockMsg
    End Sub

    Public Sub reset()
        Me.isLock = False
        Me.multiLockInfo = String.Empty
    End Sub

End Class

#End Region