<%@ Page language="C#" Debug="true" %>
<%@ Import Namespace="System"%>
<%@ Import Namespace="System.IO"%>
<%@ Import Namespace="System.Xml"%>
<%@ Import Namespace="System.Xml.Xsl"%>


<!--#include file="yasl_Routines.aspx"-->


<script language="C#" runat="server">

  XslTransform      c_Xslt;
  StringWriter      c_XsltOutput;
  string            c_Action            = "";
  string            c_Page              = "";

  void Page_Load(object sender, System.EventArgs e)
  {
    string          i_ResultStatus;

    yasl_HitInitialise();
    c_Action                = yasl_GetString("Action").ToLower();

    switch (c_Action) {
      case "showsent":    act_ShowSent("");               break;
      case "savesent":    act_ShowSent("Save");           break;
      case "loadconfig":  act_ShowConfig("Load");         break;
      case "clearerrors": act_ShowConfig("ClearErrors");  break;
      default:            act_ShowConfig("");             break; }

    i_ResultStatus          = yasl.Instance.GetStatus(yasl_XmlRoot);
    yasl_XmlRoot.SetAttribute("ResultStatus",  i_ResultStatus);
    yasl_Xml.AppendChild(yasl_XmlRoot);

    c_Xslt                  = new XslTransform();
    c_XsltOutput            = new StringWriter();
    c_Xslt.Load(yasl.yasl_DataPath + c_Page);
    c_Xslt.Transform(yasl_Xml, null, c_XsltOutput);

    yasl_Response.Expires      = -1;
    yasl_Response.ContentType  = "text/html";
    yasl_Response.Write(c_XsltOutput.ToString());
  }


  void act_ShowConfig(string p_Initial)
  {
    string          i_Result            = "";
    string          i_ResultInit        = "";

    switch (p_Initial) {
      case "Load":         i_ResultInit    = yasl.Instance.ManualLoadConfig();
                           yasl_XmlRoot.SetAttribute("ResultLoad",    i_ResultInit);
                           break;
      case "ClearErrors":  i_ResultInit    = yasl.Instance.ManualClearErrors();
                           yasl_XmlRoot.SetAttribute("ResultClearErrors",  i_ResultInit);
                           break; }

    c_Page                  = "yasl_ShowConfig.xslt";
    i_Result                = yasl.Instance.GetConfig(yasl_XmlRoot);
    yasl_XmlRoot.SetAttribute("Result", i_Result);
  }


  void act_ShowSent(string p_Initial)
  {
    string          i_Result            = "";
    string          i_ResultInit        = "";

    switch (p_Initial) {
      case "Save":  i_ResultInit    = yasl.Instance.ManualSaveSent();
                    yasl_XmlRoot.SetAttribute("ResultSave", i_ResultInit);
                    break; }

    c_Page                  = "yasl_ShowSent.xslt";
    i_Result                = yasl.Instance.GetSent(yasl_XmlRoot);
    yasl_XmlRoot.SetAttribute("Result", i_Result);
  }


</script>
