
<script language="C#" runat="server">

  HttpRequest    yasl_Request;
  HttpResponse  yasl_Response;
  XmlDocument    yasl_Xml;
  XmlElement    yasl_XmlRoot;


  void yasl_HitInitialise()
  {
    yasl_Request            = System.Web.HttpContext.Current.Request;
    yasl_Response           = System.Web.HttpContext.Current.Response;
    yasl_Xml                = new XmlDocument();
    yasl_XmlRoot            = yasl_Xml.CreateElement("yasl");
  }
  

  string yasl_GetString(string p_Name)
  {
    string          i_Param             = "";
    string          i_Test              = "";
    string[]        i_Banned            = { "<script", "<iframe" };
    int             i_Sub               = 0;
    bool            i_Dirty             = false;

    try
    {
      i_Param               = ((yasl_Request[p_Name] == null) ? "" : yasl_Request[p_Name].ToString() );
      if (i_Param != "") {
        i_Dirty             = false;
        i_Test              = i_Param.ToLower();
        i_Test              = Regex.Replace(i_Test, "\\s+", "");
        for (i_Sub = 0; i_Sub < i_Banned.Length; i_Sub++) {
          if (i_Test.IndexOf(i_Banned[i_Sub]) != -1)  {
            i_Dirty         = true; } }
        if (i_Dirty) {
          i_Param           = ""; }
      }
    }
    catch { i_Param  = ""; }
    return i_Param;
  }

  int yasl_GetInt(string p_Name)
  {
    int             i_Param             = 0;

    try
    {
      i_Param               = Int32.Parse(yasl_GetString(p_Name));
    }
    catch { i_Param = 0; }
    return i_Param;
  }


  bool yasl_GetBool(string p_Name)
  {
    bool            i_Param             = false;
    string          i_Text;

    try
    {
      i_Param               = false;
      i_Text                = yasl_GetString(p_Name).ToLower();
      if (i_Text == "true" || i_Text == "yes" || i_Text == "1") {
        i_Param             = true; }
    }
    catch { i_Param = false; }
    return i_Param;
  }

</script>
