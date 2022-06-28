<%@ Page language="C#" Debug="true" %>
<%@ Import Namespace="System"%>
<%@ Import Namespace="System.Xml"%>


<!--#include file="yasl_Routines.aspx"-->


<script language="C#" runat="server">

  void Page_Load(object sender, System.EventArgs e)
  {
    string          i_Address           = "";
    int             i_Qty               = 0;
    bool            i_FromSame          = true;
    int             i_Result            =  yasl.Result_Error;

    try
      {
      yasl_HitInitialise();

      i_Address             = yasl_GetString("Address");
      i_Qty                 = yasl_GetInt("Qty");
      i_FromSame            = yasl_GetBool("FromSame");
      if (i_Address != "") {
        i_Result            = yasl.Instance.NewMessage(yasl_XmlRoot, i_Address, i_Qty, i_FromSame); }
      yasl_XmlRoot.SetAttribute("Result",  i_Result.ToString());
      yasl_Xml.AppendChild(yasl_XmlRoot);

      yasl_Response.Expires      = -1;
      yasl_Response.ContentType  = "text/xml";
      yasl_Response.Write(yasl_Xml.InnerXml);
    }
    catch(Exception ex) {
      yasl_Response.Expires      = -1;
      yasl_Response.ContentType  = "text/xml";
      yasl_Response.Write("<yasl Error='Unknown'/>"); }
  }


</script>
