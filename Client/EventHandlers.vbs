
Option Explicit

'------------------------------------------------------------------
' yasl - Yet Another Smtp Limit - Version 1
'------------------------------------------------------------------

'
' Configure the following to your site requirements.
'
' 'Result99' is when there is an unexpected exception on the server.
' 'OtherError' is when there is an unexpected exception in the client - eg can't connect to server.
'
Public Const c_AdminName  = """Email Admin"""     ' ## enter name here. N.B. leave """ as is. 
Public Const c_AdminEmail = "account1@example.com"   ' ## enter your email admin email address here
Public Const c_ServerUrl = "http://private.mydocz.com/yasl.aspx"
Public Const c_WarnAdminOnResult99 = true
Public Const c_WarnAdminOnOtherError = true
Public Const c_FailOnResult99 = false
Public Const c_FailOnOtherError = false


Public Const Result_Allow = "0"
Public Const Result_Warn_Day = "10"
Public Const Result_Warn_Hour = "11"
Public Const Result_Warn_Both = "12"
Public Const Result_Decline_Day = "20"
Public Const Result_Decline_Hour = "21"
Public Const Result_Decline_Both = "22"
Public Const Result_Decline_Qty = "30"
Public Const Result_Decline_From = "31"
Public Const Result_Error = "99"



Sub OnAcceptMessage(oClient, oMessage)
	Dim i_User, i_From, i_Qty, i_FromSame
	Dim i_XmlHttp, i_HttpError, i_PostData, i_Xml, i_Ele
	Dim i_Result

	Result.Value = 0

	If oCLient.UserName = "" Then
		Exit Sub
	End If

	i_Result = ""
	i_User = LCase(oClient.UserName)
	i_From = LCase(oMessage.FromAddress)
	i_Qty = oMessage.Recipients.Count
	i_FromSame = 0
	If i_User = i_From Then
		i_FromSame = 1
	End If
	i_PostData = "Address=" & Escape(i_User) & "&Qty=" & i_Qty & "&FromSame=" & i_FromSame

	i_HttpError = ""
	Set i_XmlHttp = CreateObject ("Microsoft.XMLHTTP")
	i_XmlHttp.open "POST", c_ServerUrl, false
	i_XmlHttp.SetRequestHeader "Content-Type", "application/x-www-form-urlencoded"
	i_XmlHttp.send i_PostData

	If i_XmlHttp.status = 200 Then
		Set i_Xml = i_XmlHttp.ResponseXml
		Set i_Ele = i_Xml.DocumentElement
		i_Result = ""
'		On Error Resume Next
		i_Result = i_Ele.GetAttribute("Result")
'		On Error Goto 0
'		If i_Ele.Attributes("Result") Then
'			i_Result = i_Ele.Attributes("Result")
'		Else
		If i_Result = "" Then
			i_HttpError = "Bad Response XML"
		End If
	Else
		i_HttpError = "Bad Http Status [" & i_XmlHttp.status & "]"
	End If

	Select case i_Result
	case Result_Allow
		Result.Value = 0
	case Result_Warn_Day
		Send_Warning i_User, oMessage.From, "Day", i_Xml
		Result.Value = 0
	case Result_Warn_Hour
		Send_Warning i_User, oMessage.From, "Hour", i_Xml
		Result.Value = 0
	case Result_Warn_Both
		Send_Warning i_User, oMessage.From, "Day and Hour", i_Xml
		Result.Value = 0
	case Result_Decline_Day
		Result.Message = "Your account has passed Daily SMTP outgoing limits."
		Result.Value = 2
	case Result_Decline_Hour
		Result.Message = "Your account has passed Hourly SMTP outgoing limits."
		Result.Value = 2
	case Result_Decline_Both
		Result.Message = "Your account has passed Daily and Hourly SMTP outgoing limits."
		Result.Value = 2
	case Result_Decline_Qty
		Result.Message = "No recipients in message, count is [" & i_Qty & "]"
		Result.Value = 2
	case Result_Decline_From
		Result.Message = "You are not allowed to send email from [" & i_From & "]"
		Result.Value = 2
	case Result_Error
		If c_WarnAdminOnResult99 Then
			Admin_Warn i_Result
		End If
		If c_FailOnResult99 Then
			Result.Message = "Please contact email admin as an error occured authorising your email."
			Result.Value = 2
		Else
			Result.Value = 0
		End If
	case else
		If c_WarnAdminOnOtherError Then
			Admin_Warn i_Result & "/" & i_HttpError
		End If
		If c_FailOnOtherError Then
			Result.Message = "Please contact email admin as an error occured authorising your email."
			Result.Value = 2
		Else
			Result.Value = 0
		End If
	End Select
End Sub


Sub Send_Warning(p_User, p_From, p_Type, p_Xml)
	Dim i_Message
	Dim i_Split
	Dim i_Name
	Dim i_Address
	Dim i_Text
	Dim nl

	nl = Chr(13) & Chr(10)
	if (InStr(1, p_From, "<", 1) > 0) Then
		i_Split = split(p_From, "<")
		i_Address = Replace(i_Split(1), ">", "")
		i_Name = Trim(i_Split(0))
		i_Name = Replace (i_Name, """", "")
	Else 
		i_Address = p_From
		i_Split = split(p_From, "@")
		i_Name = i_Split(0)
	End If 

	i_Text = "Hello " & i_Name & nl & nl
	i_Text = i_Text & "You will soon reach " & p_User & " account limits for the current " & p_Type & "." & nl & nl
	i_Text = i_Text & "Emails sent this hour = " & p_Xml.DocumentElement.GetAttribute("Qty_Hour") & ", limit = " + p_Xml.DocumentElement.GetAttribute("Limit_Hour") & "." & nl
	i_Text = i_Text & "Emails sent in last 24 hours = " & p_Xml.DocumentElement.GetAttribute("Qty_Day") & ", limit = " + p_Xml.DocumentElement.GetAttribute("Limit_Day") & "." & nl & nl
	i_Text = i_Text & "These limits are applied to protect the email server from being used to send spam from hacked accounts." & nl & nl
	i_Text = i_Text & "If you would like your limits altered, please contact " & c_AdminEmail & "." & nl & nl
	i_Text = i_Text & "Regards" & nl
	i_Text = i_Text & c_AdminEmail

	Set i_Message = CreateObject("hMailServer.Message")
	i_Message.From = c_AdminName & " <" & c_AdminEmail & ">"
	i_Message.FromAddress = c_AdminEmail
	i_Message.AddRecipient c_AdminName, c_AdminEmail
	i_Message.Subject = "Warning: Account limits will be reached soon"
	i_Message.Body = i_Text
	i_Message.Save

	Set i_Message = CreateObject("hMailServer.Message")
	i_Message.From = c_AdminName & " <" & c_AdminEmail & ">"
	i_Message.FromAddress = c_AdminEmail
	i_Message.AddRecipient i_Name, i_Address
	i_Message.Subject = "Warning: Account limits will be reached soon"
	i_Message.Body = i_Text
	i_Message.Save
End Sub

Sub Admin_Warn(p_Error)
	Dim i_Text
	Dim nl

	nl = Chr(13) & Chr(10)	
	i_Text = "Hello Admin," & nl & nl
	i_Text = i_Text & "An unknown error occurred checking Smtp Limits." & nl & nl
	i_Text = i_Text & "Best description is - " & p_Error & "." & nl & nl
	i_Text = i_Text & "Regards" & nl
	i_Text = i_Text & c_AdminEmail

	Set i_Message = CreateObject("hMailServer.Message")
	i_Message.From = c_AdminName & " <" & c_AdminEmail & ">"
	i_Message.FromAddress = c_AdminEmail
	i_Message.AddRecipient c_AdminName, c_AdminEmail
	i_Message.Subject = "Warning: Account limits will be reached soon"
	i_Message.Body = i_Text
	i_Message.Save
End Sub
