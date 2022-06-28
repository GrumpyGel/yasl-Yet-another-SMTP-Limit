
Option Explicit

'------------------------------------------------------------------
' yasl - Yet Another Smtp Limit - Version 1
'
' Usage : cscript //D //X yasl_client_test.vbs
'------------------------------------------------------------------

includeFile("EventHandlers.vbs")

Public Const TestAddress = "account1@example.com"

Class MyClient
	Public UserName
End Class

Class MyResult
	Public Value, Message
End Class

Class MyRecipients
	Public Count
End Class

Class MyMessage
	Public From, FromAddress, Recipients
End Class

Dim Result, i_Client, i_Recipients, i_Message

Set Result = new MyResult
Set i_Client = new MyClient
i_Client.UserName = TestAddress

Set i_Message = new MyMessage
i_Message.From = TestAddress
i_Message.FromAddress = TestAddress
'i_Message.From = "account2@example.com"
'i_Message.FromAddress = "account2@example.com"
Set i_Recipients = new MyRecipients
i_Recipients.Count = 1
Set i_Message.Recipients = i_Recipients

msgbox "Calling..."
OnAcceptMessage i_Client, i_Message
msgbox "Finished - " & Result.Value & "-" & Result.Message


Sub includeFile(p_Filename)
    With CreateObject("Scripting.FileSystemObject")
       executeGlobal .openTextFile(p_Filename).readAll()
    End With
End Sub
