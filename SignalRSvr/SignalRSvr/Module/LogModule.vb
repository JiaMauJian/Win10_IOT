Imports Microsoft.AspNet.SignalR.Hubs

Public Class LogModule
    Inherits HubPipelineModule

    'Protected Overrides Sub OnIncomingError(ex As System.Exception, context As Microsoft.AspNet.SignalR.Hubs.IHubIncomingInvokerContext)
    '    Debug.WriteLine("=> Exception " & ex.Message)
    '    'WriteLogFile("Exception: " & ex.Message)
    '    If (ex.InnerException IsNot Nothing) Then

    '        Debug.WriteLine("=> Inner Exception " & ex.InnerException.Message)
    '        'WriteLogFile("Inner Exception: " & ex.InnerException.Message)
    '    End If
    '    MyBase.OnIncomingError(ex, context)
    'End Sub

    Protected Overrides Function OnBeforeConnect(ByVal hub As IHub) As Boolean
        Debug.WriteLine("OnBeforeConnect: {0}", hub.Context.Request.Url)
        Return True
    End Function

    Protected Overrides Sub OnAfterConnect(ByVal hub As IHub)
        Debug.WriteLine("OnAfterConnect")
    End Sub

    Protected Overrides Function OnBeforeIncoming(ByVal context As IHubIncomingInvokerContext) As Boolean
        Debug.WriteLine("OnBeforeIncoming: {0}, {1}", context.Hub.Context.ConnectionId, context.MethodDescriptor.Name)
        Return True
    End Function

    Protected Overrides Function OnAfterIncoming(ByVal result As Object, ByVal context As IHubIncomingInvokerContext) As Object
        Debug.WriteLine("OnAfterIncoming: {0}, {1}, {2}", context.Hub.Context.ConnectionId, context.MethodDescriptor.Name, result)
        Return result
    End Function

    Protected Overrides Function OnBeforeOutgoing(ByVal context As IHubOutgoingInvokerContext) As Boolean
        Debug.WriteLine(String.Format("OnBeforeOutgoing: {0}", context.Invocation.Method))
        Return True
    End Function

    Protected Overrides Sub OnAfterOutgoing(ByVal context As IHubOutgoingInvokerContext)
        Debug.WriteLine("OnAfterOutgoing")
    End Sub

    'Protected Overrides Function OnBeforeDisconnect(ByVal hub As IHub) As Boolean
    '    Debug.WriteLine("OnBeforeDisconnect")
    '    Return True
    'End Function

    'Protected Overrides Sub OnAfterDisconnect(ByVal hub As IHub)
    '    Debug.WriteLine("OnAfterDisconnect")
    'End Sub

    Protected Overrides Function OnBeforeReconnect(ByVal hub As IHub) As Boolean
        Debug.WriteLine("OnBeforeReconnect")
        Return True
    End Function

    Protected Overrides Sub OnAfterReconnect(ByVal hub As IHub)
        Debug.WriteLine("OnAfterReconnect")
    End Sub
End Class