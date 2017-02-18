Imports Microsoft.VisualBasic
Imports Microsoft.Owin
Imports Owin

<Assembly: OwinStartup(GetType(SignalRSvr.Startup))>
Namespace SignalRSvr
    Public Class Startup
        Public Sub Configuration(ByVal app As IAppBuilder)
            app.MapSignalR()
        End Sub
    End Class

End Namespace